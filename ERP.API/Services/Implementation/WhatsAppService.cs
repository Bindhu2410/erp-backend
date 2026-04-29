using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dapper;
using ERP.API.Hubs;
using ERP.API.Models;
using Microsoft.AspNetCore.SignalR;

namespace ERP.API.Services.Implementation
{
    public class WhatsAppService : IWhatsAppService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<WhatsAppService> _logger;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly string _connectionString;
        private readonly string _graphApiBaseUrl;
        private readonly string _graphApiVersion;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public WhatsAppService(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<WhatsAppService> logger,
            IHubContext<ChatHub> hubContext)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
            _hubContext = hubContext;
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
            _graphApiBaseUrl = configuration["WhatsApp:GraphApiBaseUrl"] ?? "https://graph.facebook.com";
            _graphApiVersion = configuration["WhatsApp:GraphApiVersion"] ?? "v19.0";
        }

        // ── Account Management ────────────────────────────────────────────────

        public async Task<WhatsAppAccountDto> RegisterAccountAsync(RegisterWhatsAppAccountRequest request)
        {
            using var db = new Npgsql.NpgsqlConnection(_connectionString);

            // Upsert: one row per user per phone_number_id
            const string sql = @"
                INSERT INTO whatsapp_accounts
                    (user_id, phone_number_id, phone_number, display_name, access_token, waba_id, is_active, created_at, updated_at)
                VALUES
                    (@UserId, @PhoneNumberId, @PhoneNumber, @DisplayName, @AccessToken, @WabaId, TRUE, NOW(), NOW())
                ON CONFLICT (phone_number_id) DO UPDATE SET
                    user_id         = EXCLUDED.user_id,
                    phone_number    = EXCLUDED.phone_number,
                    display_name    = EXCLUDED.display_name,
                    access_token    = EXCLUDED.access_token,
                    waba_id         = EXCLUDED.waba_id,
                    is_active       = TRUE,
                    updated_at      = NOW()
                RETURNING id, user_id, phone_number_id, phone_number, display_name, waba_id, is_active, created_at;";

            var row = await db.QuerySingleAsync<dynamic>(sql, new
            {
                request.UserId,
                request.PhoneNumberId,
                request.PhoneNumber,
                request.DisplayName,
                request.AccessToken,
                request.WabaId
            });

            return MapAccountDto(row);
        }

        public async Task<List<WhatsAppAccountDto>> GetAccountsAsync(int userId)
        {
            using var db = new Npgsql.NpgsqlConnection(_connectionString);
            var rows = await db.QueryAsync<dynamic>(
                @"SELECT id, user_id, phone_number_id, phone_number, display_name, waba_id, is_active, created_at
                  FROM whatsapp_accounts
                  WHERE user_id = @UserId AND is_active = TRUE
                  ORDER BY created_at DESC",
                new { UserId = userId });

            return rows.Select<dynamic, WhatsAppAccountDto>(r => MapAccountDto(r)).ToList();
        }

        public async Task<bool> DeregisterAccountAsync(int accountId, int userId)
        {
            using var db = new Npgsql.NpgsqlConnection(_connectionString);
            var affected = await db.ExecuteAsync(
                "UPDATE whatsapp_accounts SET is_active = FALSE, updated_at = NOW() WHERE id = @Id AND user_id = @UserId",
                new { Id = accountId, UserId = userId });
            return affected > 0;
        }

        // ── Messaging ─────────────────────────────────────────────────────────

        public async Task<WhatsAppMessageResponse> SendTextMessageAsync(int accountId, SendWhatsAppTextRequest request)
        {
            var account = await GetAccountByIdAsync(accountId);
            if (account == null)
                return new WhatsAppMessageResponse { Success = false, ErrorMessage = "Account not found." };

            var conversation = await EnsureConversationAsync(accountId, request.To, request.ContactName);

            var payload = new
            {
                messaging_product = "whatsapp",
                recipient_type = "individual",
                to = request.To,
                type = "text",
                text = new { body = request.Message }
            };

            string phoneNumberId = (string)account!.phone_number_id;
            string accessToken = (string)account!.access_token;
            (bool success, string? waMessageId, string? error) = await CallMetaApiAsync(phoneNumberId, accessToken, payload);

            var savedId = await SaveMessageAsync(new WhatsAppMessage
            {
                ConversationId = conversation.Id,
                WaMessageId = waMessageId,
                Direction = "outbound",
                MessageType = "text",
                Content = request.Message,
                Status = success ? "sent" : "failed",
                ErrorMessage = error,
                SentAt = success ? DateTime.UtcNow : null,
                CreatedAt = DateTime.UtcNow
            });

            if (success)
                await UpdateConversationLastMessageAsync(conversation.Id);

            return new WhatsAppMessageResponse
            {
                Success = success,
                WaMessageId = waMessageId,
                ErrorMessage = error,
                SavedMessageId = savedId
            };
        }

        public async Task<WhatsAppMessageResponse> SendTemplateMessageAsync(int accountId, SendWhatsAppTemplateRequest request)
        {
            var account = await GetAccountByIdAsync(accountId);
            if (account == null)
                return new WhatsAppMessageResponse { Success = false, ErrorMessage = "Account not found." };

            var conversation = await EnsureConversationAsync(accountId, request.To, request.ContactName);

            var templatePayload = new
            {
                name = request.TemplateName,
                language = new { code = request.Language },
                components = request.Components
            };

            var payload = new
            {
                messaging_product = "whatsapp",
                to = request.To,
                type = "template",
                template = templatePayload
            };

            string phoneNumberId2 = (string)account!.phone_number_id;
            string accessToken2 = (string)account!.access_token;
            (bool success, string? waMessageId, string? error) = await CallMetaApiAsync(phoneNumberId2, accessToken2, payload);

            var savedId = await SaveMessageAsync(new WhatsAppMessage
            {
                ConversationId = conversation.Id,
                WaMessageId = waMessageId,
                Direction = "outbound",
                MessageType = "template",
                TemplateName = request.TemplateName,
                Status = success ? "sent" : "failed",
                ErrorMessage = error,
                SentAt = success ? DateTime.UtcNow : null,
                CreatedAt = DateTime.UtcNow
            });

            if (success)
                await UpdateConversationLastMessageAsync(conversation.Id);

            return new WhatsAppMessageResponse
            {
                Success = success,
                WaMessageId = waMessageId,
                ErrorMessage = error,
                SavedMessageId = savedId
            };
        }

        // ── Conversations ─────────────────────────────────────────────────────

        public async Task<List<WhatsAppConversationDto>> GetConversationsAsync(int userId)
        {
            using var db = new Npgsql.NpgsqlConnection(_connectionString);
            var rows = await db.QueryAsync<dynamic>(@"
                SELECT c.id, c.account_id, c.contact_phone, c.contact_name,
                       c.lead_id, c.contact_id, c.last_message_at, c.unread_count,
                       (SELECT content FROM whatsapp_messages m
                        WHERE m.conversation_id = c.id AND m.message_type = 'text'
                        ORDER BY m.created_at DESC LIMIT 1) AS last_message_preview
                FROM whatsapp_conversations c
                JOIN whatsapp_accounts a ON a.id = c.account_id
                WHERE a.user_id = @UserId AND a.is_active = TRUE
                ORDER BY c.last_message_at DESC NULLS LAST",
                new { UserId = userId });

            return rows.Select<dynamic, WhatsAppConversationDto>(r => new WhatsAppConversationDto
            {
                Id = (int)r.id,
                AccountId = (int)r.account_id,
                ContactPhone = (string)r.contact_phone,
                ContactName = r.contact_name,
                LeadId = r.lead_id,
                ContactId = r.contact_id,
                LastMessageAt = r.last_message_at,
                UnreadCount = (int)r.unread_count,
                LastMessagePreview = r.last_message_preview
            }).ToList();
        }

        public async Task<List<WhatsAppMessageDto>> GetMessagesAsync(int conversationId, int userId)
        {
            using var db = new Npgsql.NpgsqlConnection(_connectionString);

            // Verify ownership
            var owned = await db.QueryFirstOrDefaultAsync<int?>(
                @"SELECT c.id FROM whatsapp_conversations c
                  JOIN whatsapp_accounts a ON a.id = c.account_id
                  WHERE c.id = @ConversationId AND a.user_id = @UserId AND a.is_active = TRUE",
                new { ConversationId = conversationId, UserId = userId });

            if (owned == null) return new List<WhatsAppMessageDto>();

            var rows = await db.QueryAsync<dynamic>(@"
                SELECT id, direction, message_type, content, template_name, status,
                       sent_at, delivered_at, read_at, created_at
                FROM whatsapp_messages
                WHERE conversation_id = @ConversationId
                ORDER BY created_at ASC",
                new { ConversationId = conversationId });

            return rows.Select<dynamic, WhatsAppMessageDto>(r => new WhatsAppMessageDto
            {
                Id = (int)r.id,
                Direction = (string)r.direction,
                MessageType = (string)r.message_type,
                Content = r.content,
                TemplateName = r.template_name,
                Status = (string)r.status,
                SentAt = r.sent_at,
                DeliveredAt = r.delivered_at,
                ReadAt = r.read_at,
                CreatedAt = (DateTime)r.created_at
            }).ToList();
        }

        public async Task<bool> LinkToLeadAsync(int conversationId, int leadId, int userId)
        {
            using var db = new Npgsql.NpgsqlConnection(_connectionString);
            var affected = await db.ExecuteAsync(@"
                UPDATE whatsapp_conversations c
                SET lead_id = @LeadId, updated_at = NOW()
                FROM whatsapp_accounts a
                WHERE c.id = @ConversationId AND c.account_id = a.id AND a.user_id = @UserId",
                new { ConversationId = conversationId, LeadId = leadId, UserId = userId });
            return affected > 0;
        }

        public async Task<bool> LinkToContactAsync(int conversationId, int contactId, int userId)
        {
            using var db = new Npgsql.NpgsqlConnection(_connectionString);
            var affected = await db.ExecuteAsync(@"
                UPDATE whatsapp_conversations c
                SET contact_id = @ContactId, updated_at = NOW()
                FROM whatsapp_accounts a
                WHERE c.id = @ConversationId AND c.account_id = a.id AND a.user_id = @UserId",
                new { ConversationId = conversationId, ContactId = contactId, UserId = userId });
            return affected > 0;
        }

        public async Task MarkConversationReadAsync(int conversationId, int userId)
        {
            using var db = new Npgsql.NpgsqlConnection(_connectionString);
            await db.ExecuteAsync(@"
                UPDATE whatsapp_conversations c
                SET unread_count = 0, updated_at = NOW()
                FROM whatsapp_accounts a
                WHERE c.id = @ConversationId AND c.account_id = a.id AND a.user_id = @UserId",
                new { ConversationId = conversationId, UserId = userId });
        }

        // ── Templates ─────────────────────────────────────────────────────────

        public async Task<List<WhatsAppTemplate>> GetTemplatesAsync(int accountId)
        {
            using var db = new Npgsql.NpgsqlConnection(_connectionString);
            var rows = await db.QueryAsync<dynamic>(@"
                SELECT id, account_id, template_name, language, category,
                       components_json, approval_status, synced_at, created_at
                FROM whatsapp_templates
                WHERE account_id = @AccountId AND approval_status = 'APPROVED'
                ORDER BY template_name",
                new { AccountId = accountId });

            return rows.Select<dynamic, WhatsAppTemplate>(r => new WhatsAppTemplate
            {
                Id = (int)r.id,
                AccountId = (int)r.account_id,
                TemplateName = (string)r.template_name,
                Language = (string)r.language,
                Category = (string)r.category,
                ComponentsJson = r.components_json,
                ApprovalStatus = (string)r.approval_status,
                SyncedAt = r.synced_at
            }).ToList();
        }

        public async Task<int> SyncTemplatesAsync(int accountId)
        {
            var account = await GetAccountByIdAsync(accountId);
            if (account == null) return 0;

            string syncAccessToken = (string)account.access_token;
            string syncWabaId = (string)account.waba_id;

            var client = _httpClientFactory.CreateClient("WhatsAppGraph");
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", syncAccessToken);

            var url = $"{_graphApiBaseUrl}/{_graphApiVersion}/{syncWabaId}/message_templates?fields=name,language,category,components,status&limit=100";

            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to sync WhatsApp templates for account {AccountId}: {Status}", accountId, response.StatusCode);
                return 0;
            }

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var templates = doc.RootElement.GetProperty("data");

            using var db = new Npgsql.NpgsqlConnection(_connectionString);
            int synced = 0;

            foreach (var t in templates.EnumerateArray())
            {
                string name = t.GetProperty("name").GetString() ?? "";
                string lang = t.TryGetProperty("language", out var lp) ? lp.GetString() ?? "en_US" : "en_US";
                string cat = t.TryGetProperty("category", out var cp) ? cp.GetString() ?? "" : "";
                string status = t.TryGetProperty("status", out var sp) ? sp.GetString() ?? "PENDING" : "PENDING";
                string comps = t.TryGetProperty("components", out var compProp) ? compProp.GetRawText() : "[]";

                await db.ExecuteAsync(@"
                    INSERT INTO whatsapp_templates
                        (account_id, template_name, language, category, components_json, approval_status, synced_at, created_at)
                    VALUES
                        (@AccountId, @Name, @Lang, @Cat, @Comps, @Status, NOW(), NOW())
                    ON CONFLICT (account_id, template_name, language) DO UPDATE SET
                        category         = EXCLUDED.category,
                        components_json  = EXCLUDED.components_json,
                        approval_status  = EXCLUDED.approval_status,
                        synced_at        = NOW()",
                    new { AccountId = accountId, Name = name, Lang = lang, Cat = cat, Comps = comps, Status = status });
                synced++;
            }

            return synced;
        }

        // ── Webhook ───────────────────────────────────────────────────────────

        public async Task ProcessWebhookAsync(WhatsAppWebhookPayload payload)
        {
            if (payload.Entry == null) return;

            foreach (var entry in payload.Entry)
            {
                if (entry.Changes == null) continue;

                foreach (var change in entry.Changes)
                {
                    if (change.Field != "messages" || change.Value == null) continue;

                    var value = change.Value;
                    var phoneNumberId = value.Metadata?.PhoneNumberId;
                    if (string.IsNullOrEmpty(phoneNumberId)) continue;

                    // Resolve account
                    using var db = new Npgsql.NpgsqlConnection(_connectionString);
                    var account = await db.QueryFirstOrDefaultAsync<dynamic>(
                        "SELECT id, user_id FROM whatsapp_accounts WHERE phone_number_id = @PhoneNumberId AND is_active = TRUE",
                        new { PhoneNumberId = phoneNumberId });

                    if (account == null) continue;

                    int accountId = (int)account.id;
                    int userId = (int)account.user_id;

                    // ── Process inbound messages ────────────────────────────
                    if (value.Messages != null)
                    {
                        foreach (var msg in value.Messages)
                        {
                            // Deduplicate by wa_message_id
                            var exists = await db.QueryFirstOrDefaultAsync<int?>(
                                "SELECT id FROM whatsapp_messages WHERE wa_message_id = @WaMessageId",
                                new { WaMessageId = msg.Id });
                            if (exists != null) continue;

                            string? contactName = value.Contacts?
                                .FirstOrDefault(c => c.WaId == msg.From)?.Profile?.Name;

                            var conversation = await EnsureConversationAsync(accountId, msg.From, contactName);

                            string content = msg.Type == "text" && msg.Text != null ? msg.Text.Body : $"[{msg.Type}]";

                            var savedId = await SaveMessageAsync(new WhatsAppMessage
                            {
                                ConversationId = conversation.Id,
                                WaMessageId = msg.Id,
                                Direction = "inbound",
                                MessageType = msg.Type,
                                Content = content,
                                Status = "delivered",
                                SentAt = DateTimeOffset.FromUnixTimeSeconds(long.Parse(msg.Timestamp)).UtcDateTime,
                                CreatedAt = DateTime.UtcNow
                            });

                            // Increment unread count
                            await db.ExecuteAsync(
                                "UPDATE whatsapp_conversations SET unread_count = unread_count + 1, last_message_at = NOW(), updated_at = NOW() WHERE id = @Id",
                                new { Id = conversation.Id });

                            // Push real-time event via SignalR to the owning user's group
                            await _hubContext.Clients.Group($"whatsapp:{userId}")
                                .SendAsync("ReceiveWhatsAppMessage", new
                                {
                                    conversationId = conversation.Id,
                                    messageId = savedId,
                                    direction = "inbound",
                                    contactPhone = msg.From,
                                    contactName,
                                    content,
                                    timestamp = DateTime.UtcNow
                                });
                        }
                    }

                    // ── Process status updates ──────────────────────────────
                    if (value.Statuses != null)
                    {
                        foreach (var statusUpdate in value.Statuses)
                        {
                            var msgRow = await db.QueryFirstOrDefaultAsync<dynamic>(
                                "SELECT id FROM whatsapp_messages WHERE wa_message_id = @WaMessageId",
                                new { WaMessageId = statusUpdate.Id });

                            if (msgRow == null) continue;

                            string col = statusUpdate.Status switch
                            {
                                "delivered" => "delivered_at",
                                "read" => "read_at",
                                _ => ""
                            };

                            if (!string.IsNullOrEmpty(col))
                            {
                                await db.ExecuteAsync(
                                    $"UPDATE whatsapp_messages SET status = @Status, {col} = NOW() WHERE id = @Id",
                                    new { Status = statusUpdate.Status, Id = (int)msgRow.id });
                            }
                            else
                            {
                                await db.ExecuteAsync(
                                    "UPDATE whatsapp_messages SET status = @Status WHERE id = @Id",
                                    new { Status = statusUpdate.Status, Id = (int)msgRow.id });
                            }
                        }
                    }
                }
            }
        }

        public bool VerifyWebhookSignature(string rawBody, string signatureHeader, string appSecret)
        {
            // Meta sends: sha256=<hex_digest>
            if (!signatureHeader.StartsWith("sha256=", StringComparison.OrdinalIgnoreCase))
                return false;

            var receivedHash = signatureHeader["sha256=".Length..];
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(appSecret));
            var computedHash = Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(rawBody))).ToLowerInvariant();

            // Constant-time comparison to prevent timing attacks
            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(receivedHash.ToLowerInvariant()),
                Encoding.UTF8.GetBytes(computedHash));
        }

        // ── Private helpers ───────────────────────────────────────────────────

        private async Task<(bool Success, string? WaMessageId, string? Error)> CallMetaApiAsync(
            string phoneNumberId, string accessToken, object payload)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("WhatsAppGraph");
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken);

                var url = $"{_graphApiBaseUrl}/{_graphApiVersion}/{phoneNumberId}/messages";
                var json = JsonSerializer.Serialize(payload, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(url, content);
                var body = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Meta API error {Status}: {Body}", response.StatusCode, body);
                    return (false, null, body);
                }

                using var doc = JsonDocument.Parse(body);
                var messageId = doc.RootElement
                    .GetProperty("messages")[0]
                    .GetProperty("id")
                    .GetString();

                return (true, messageId, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception calling Meta WhatsApp API");
                return (false, null, ex.Message);
            }
        }

        private async Task<WhatsAppConversation> EnsureConversationAsync(
            int accountId, string contactPhone, string? contactName)
        {
            using var db = new Npgsql.NpgsqlConnection(_connectionString);

            var existing = await db.QueryFirstOrDefaultAsync<dynamic>(
                "SELECT id, account_id, contact_phone, contact_name, lead_id, contact_id, last_message_at, unread_count FROM whatsapp_conversations WHERE account_id = @AccountId AND contact_phone = @ContactPhone",
                new { AccountId = accountId, ContactPhone = contactPhone });

            if (existing != null)
                return MapConversation(existing);

            var id = await db.QuerySingleAsync<int>(@"
                INSERT INTO whatsapp_conversations
                    (account_id, contact_phone, contact_name, unread_count, created_at, updated_at)
                VALUES (@AccountId, @ContactPhone, @ContactName, 0, NOW(), NOW())
                RETURNING id",
                new { AccountId = accountId, ContactPhone = contactPhone, ContactName = contactName });

            return new WhatsAppConversation
            {
                Id = id,
                AccountId = accountId,
                ContactPhone = contactPhone,
                ContactName = contactName
            };
        }

        private async Task<int> SaveMessageAsync(WhatsAppMessage msg)
        {
            using var db = new Npgsql.NpgsqlConnection(_connectionString);
            return await db.QuerySingleAsync<int>(@"
                INSERT INTO whatsapp_messages
                    (conversation_id, wa_message_id, direction, message_type, content, template_name, status, error_message, sent_at, delivered_at, read_at, created_at)
                VALUES
                    (@ConversationId, @WaMessageId, @Direction, @MessageType, @Content, @TemplateName, @Status, @ErrorMessage, @SentAt, @DeliveredAt, @ReadAt, @CreatedAt)
                RETURNING id",
                new
                {
                    msg.ConversationId,
                    msg.WaMessageId,
                    msg.Direction,
                    msg.MessageType,
                    msg.Content,
                    msg.TemplateName,
                    msg.Status,
                    msg.ErrorMessage,
                    msg.SentAt,
                    msg.DeliveredAt,
                    msg.ReadAt,
                    msg.CreatedAt
                });
        }

        private async Task UpdateConversationLastMessageAsync(int conversationId)
        {
            using var db = new Npgsql.NpgsqlConnection(_connectionString);
            await db.ExecuteAsync(
                "UPDATE whatsapp_conversations SET last_message_at = NOW(), updated_at = NOW() WHERE id = @Id",
                new { Id = conversationId });
        }

        private async Task<dynamic?> GetAccountByIdAsync(int accountId)
        {
            using var db = new Npgsql.NpgsqlConnection(_connectionString);
            return await db.QueryFirstOrDefaultAsync<dynamic>(
                "SELECT id, user_id, phone_number_id, access_token, waba_id FROM whatsapp_accounts WHERE id = @Id AND is_active = TRUE",
                new { Id = accountId });
        }

        private static WhatsAppAccountDto MapAccountDto(dynamic r) => new()
        {
            Id = (int)r.id,
            UserId = (int)r.user_id,
            PhoneNumberId = (string)r.phone_number_id,
            PhoneNumber = (string)r.phone_number,
            DisplayName = r.display_name,
            WabaId = (string)r.waba_id,
            IsActive = (bool)r.is_active,
            CreatedAt = (DateTime)r.created_at
        };

        private static WhatsAppConversation MapConversation(dynamic r) => new()
        {
            Id = (int)r.id,
            AccountId = (int)r.account_id,
            ContactPhone = (string)r.contact_phone,
            ContactName = r.contact_name,
            LeadId = r.lead_id,
            ContactId = r.contact_id,
            LastMessageAt = r.last_message_at,
            UnreadCount = (int)r.unread_count
        };
    }
}
