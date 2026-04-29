using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ERP.API.Models;
using ERP.API.Services;
using Dapper;
using System.Text;
using System.Text.Json;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WhatsAppController : ControllerBase
    {
        private readonly IWhatsAppService _whatsAppService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<WhatsAppController> _logger;
        private readonly string _connectionString;

        public WhatsAppController(
            IWhatsAppService whatsAppService,
            IConfiguration configuration,
            ILogger<WhatsAppController> logger)
        {
            _whatsAppService = whatsAppService;
            _configuration = configuration;
            _logger = logger;
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
        }

        // ── Database Initialization ───────────────────────────────────────────

        /// <summary>Creates all required WhatsApp tables in the database.</summary>
        [Authorize]
        [HttpPost("init-db")]
        public async Task<IActionResult> InitializeDb()
        {
            try
            {
                using var db = new Npgsql.NpgsqlConnection(_connectionString);

                const string sql = @"
                    CREATE TABLE IF NOT EXISTS whatsapp_accounts (
                        id               SERIAL PRIMARY KEY,
                        user_id          INTEGER NOT NULL,
                        phone_number_id  VARCHAR(100) NOT NULL,
                        phone_number     VARCHAR(30)  NOT NULL,
                        display_name     VARCHAR(255),
                        access_token     TEXT        NOT NULL,
                        waba_id          VARCHAR(100) NOT NULL,
                        is_active        BOOLEAN     DEFAULT TRUE,
                        created_at       TIMESTAMP   DEFAULT NOW(),
                        updated_at       TIMESTAMP   DEFAULT NOW(),
                        CONSTRAINT uq_whatsapp_accounts_phone_number_id UNIQUE (phone_number_id)
                    );

                    CREATE TABLE IF NOT EXISTS whatsapp_conversations (
                        id                  SERIAL PRIMARY KEY,
                        account_id          INTEGER NOT NULL REFERENCES whatsapp_accounts(id) ON DELETE CASCADE,
                        contact_phone       VARCHAR(30)  NOT NULL,
                        contact_name        VARCHAR(255),
                        lead_id             INTEGER,
                        contact_id          INTEGER,
                        last_message_at     TIMESTAMP,
                        unread_count        INTEGER DEFAULT 0,
                        wa_conversation_id  VARCHAR(100),
                        created_at          TIMESTAMP DEFAULT NOW(),
                        updated_at          TIMESTAMP DEFAULT NOW(),
                        CONSTRAINT uq_whatsapp_conversation UNIQUE (account_id, contact_phone)
                    );

                    CREATE INDEX IF NOT EXISTS idx_whatsapp_conversations_account ON whatsapp_conversations(account_id);
                    CREATE INDEX IF NOT EXISTS idx_whatsapp_conversations_lead    ON whatsapp_conversations(lead_id);
                    CREATE INDEX IF NOT EXISTS idx_whatsapp_conversations_contact ON whatsapp_conversations(contact_id);

                    CREATE TABLE IF NOT EXISTS whatsapp_messages (
                        id               SERIAL PRIMARY KEY,
                        conversation_id  INTEGER NOT NULL REFERENCES whatsapp_conversations(id) ON DELETE CASCADE,
                        wa_message_id    VARCHAR(100) UNIQUE,
                        direction        VARCHAR(10)  NOT NULL DEFAULT 'outbound',
                        message_type     VARCHAR(30)  NOT NULL DEFAULT 'text',
                        content          TEXT,
                        template_name    VARCHAR(255),
                        status           VARCHAR(20)  NOT NULL DEFAULT 'pending',
                        error_message    TEXT,
                        sent_at          TIMESTAMP,
                        delivered_at     TIMESTAMP,
                        read_at          TIMESTAMP,
                        created_at       TIMESTAMP DEFAULT NOW()
                    );

                    CREATE INDEX IF NOT EXISTS idx_whatsapp_messages_conversation ON whatsapp_messages(conversation_id);
                    CREATE INDEX IF NOT EXISTS idx_whatsapp_messages_wa_id        ON whatsapp_messages(wa_message_id);

                    CREATE TABLE IF NOT EXISTS whatsapp_templates (
                        id               SERIAL PRIMARY KEY,
                        account_id       INTEGER NOT NULL REFERENCES whatsapp_accounts(id) ON DELETE CASCADE,
                        template_name    VARCHAR(255) NOT NULL,
                        language         VARCHAR(20)  NOT NULL DEFAULT 'en_US',
                        category         VARCHAR(50),
                        components_json  TEXT,
                        approval_status  VARCHAR(30)  DEFAULT 'PENDING',
                        synced_at        TIMESTAMP,
                        created_at       TIMESTAMP DEFAULT NOW(),
                        CONSTRAINT uq_whatsapp_template UNIQUE (account_id, template_name, language)
                    );";

                await db.ExecuteAsync(sql);
                return Ok(new { success = true, message = "WhatsApp tables created successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing WhatsApp database tables");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // ── Webhook (no auth — verified by HMAC signature) ────────────────────

        /// <summary>Meta webhook verification challenge (GET).</summary>
        [AllowAnonymous]
        [HttpGet("webhook")]
        public IActionResult VerifyWebhook(
            [FromQuery(Name = "hub.mode")] string mode,
            [FromQuery(Name = "hub.verify_token")] string token,
            [FromQuery(Name = "hub.challenge")] string challenge)
        {
            var expectedToken = _configuration["WhatsApp:WebhookVerifyToken"] ?? "";
            if (mode == "subscribe" && token == expectedToken)
                return Ok(int.Parse(challenge));

            return Forbid();
        }

        /// <summary>Receive incoming WhatsApp events from Meta (POST).</summary>
        [AllowAnonymous]
        [HttpPost("webhook")]
        public async Task<IActionResult> ReceiveWebhook()
        {
            // Read raw body for signature verification
            Request.EnableBuffering();
            using var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true);
            var rawBody = await reader.ReadToEndAsync();
            Request.Body.Position = 0;

            var appSecret = _configuration["WhatsApp:AppSecret"] ?? "";
            var signature = Request.Headers["X-Hub-Signature-256"].FirstOrDefault() ?? "";

            // Reject calls with no signature (unless AppSecret not configured yet)
            if (!string.IsNullOrEmpty(appSecret) && !string.IsNullOrEmpty(signature))
            {
                if (!_whatsAppService.VerifyWebhookSignature(rawBody, signature, appSecret))
                {
                    _logger.LogWarning("WhatsApp webhook: invalid signature from {IP}", HttpContext.Connection.RemoteIpAddress);
                    return Unauthorized(new { message = "Invalid webhook signature." });
                }
            }

            try
            {
                var payload = JsonSerializer.Deserialize<WhatsAppWebhookPayload>(rawBody, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (payload != null)
                    await _whatsAppService.ProcessWebhookAsync(payload);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing WhatsApp webhook payload");
                // Always return 200 to Meta to prevent repeated delivery
            }

            return Ok();
        }

        // ── Account Management ────────────────────────────────────────────────

        /// <summary>Register a WhatsApp phone number for a user.</summary>
        [Authorize]
        [HttpPost("accounts/register")]
        public async Task<IActionResult> RegisterAccount([FromBody] RegisterWhatsAppAccountRequest request)
        {
            try
            {
                var account = await _whatsAppService.RegisterAccountAsync(request);
                return Ok(new { success = true, data = account });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering WhatsApp account");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>List all active WhatsApp numbers registered to a user.</summary>
        [Authorize]
        [HttpGet("accounts")]
        public async Task<IActionResult> GetAccounts([FromQuery] int userId)
        {
            var accounts = await _whatsAppService.GetAccountsAsync(userId);
            return Ok(new { success = true, data = accounts });
        }

        /// <summary>Deactivate a WhatsApp number registration.</summary>
        [Authorize]
        [HttpDelete("accounts/{id:int}")]
        public async Task<IActionResult> DeregisterAccount(int id, [FromQuery] int userId)
        {
            var success = await _whatsAppService.DeregisterAccountAsync(id, userId);
            if (!success)
                return NotFound(new { success = false, message = "Account not found or access denied." });
            return Ok(new { success = true, message = "Account deactivated." });
        }
 
        /// <summary>Send a text message directly to a phone number (resolves/creates conversation).</summary>
        [Authorize]
        [HttpPost("send")]
        public async Task<IActionResult> SendDirect([FromBody] SendDirectRequest request, [FromQuery] int userId)
        {
            try
            {
                var accounts = await _whatsAppService.GetAccountsAsync(userId);
                var account = accounts.FirstOrDefault();
                if (account == null)
                    return BadRequest(new { success = false, message = "No active WhatsApp account found for this user." });

                var result = await _whatsAppService.SendTextMessageAsync(account.Id, new SendWhatsAppTextRequest
                {
                    To = request.PhoneNumber,
                    Message = request.Message
                });

                return result.Success
                    ? Ok(new { success = true, data = result })
                    : BadRequest(new { success = false, message = result.ErrorMessage });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SendDirect WhatsApp message");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // ── Conversations ─────────────────────────────────────────────────────

        /// <summary>List all WhatsApp conversations for a user.</summary>
        [Authorize]
        [HttpGet("conversations")]
        public async Task<IActionResult> GetConversations([FromQuery] int userId)
        {
            var conversations = await _whatsAppService.GetConversationsAsync(userId);
            return Ok(new { success = true, data = conversations });
        }

        /// <summary>Get messages in a conversation thread.</summary>
        [Authorize]
        [HttpGet("conversations/{id:int}/messages")]
        public async Task<IActionResult> GetMessages(int id, [FromQuery] int userId)
        {
            var messages = await _whatsAppService.GetMessagesAsync(id, userId);
            return Ok(new { success = true, data = messages });
        }

        /// <summary>Send a text message in a conversation.</summary>
        [Authorize]
        [HttpPost("conversations/{conversationId:int}/send")]
        public async Task<IActionResult> SendText(int conversationId, [FromBody] SendConversationTextRequest request, [FromQuery] int userId)
        {
            var account = await GetAccountForConversationAsync(conversationId, userId);
            if (account == null)
                return NotFound(new { success = false, message = "Conversation not found or access denied." });

            var result = await _whatsAppService.SendTextMessageAsync(account.AccountId, new SendWhatsAppTextRequest
            {
                To = account.ContactPhone,
                Message = request.Message
            });

            return result.Success
                ? Ok(new { success = true, data = result })
                : BadRequest(new { success = false, message = result.ErrorMessage });
        }

        /// <summary>Send a template message in a conversation.</summary>
        [Authorize]
        [HttpPost("conversations/{conversationId:int}/send-template")]
        public async Task<IActionResult> SendTemplate(int conversationId, [FromBody] SendConversationTemplateRequest request, [FromQuery] int userId)
        {
            var account = await GetAccountForConversationAsync(conversationId, userId);
            if (account == null)
                return NotFound(new { success = false, message = "Conversation not found or access denied." });

            var result = await _whatsAppService.SendTemplateMessageAsync(account.AccountId, new SendWhatsAppTemplateRequest
            {
                To = account.ContactPhone,
                TemplateName = request.TemplateName,
                Language = request.Language,
                Components = request.Components
            });

            return result.Success
                ? Ok(new { success = true, data = result })
                : BadRequest(new { success = false, message = result.ErrorMessage });
        }

        /// <summary>Link a conversation to a SalesLead.</summary>
        [Authorize]
        [HttpPut("conversations/{id:int}/link-lead")]
        public async Task<IActionResult> LinkLead(int id, [FromBody] LinkLeadRequest request, [FromQuery] int userId)
        {
            var success = await _whatsAppService.LinkToLeadAsync(id, request.LeadId, userId);
            return success
                ? Ok(new { success = true, message = "Conversation linked to lead." })
                : NotFound(new { success = false, message = "Conversation not found or access denied." });
        }

        /// <summary>Link a conversation to a SalesContact.</summary>
        [Authorize]
        [HttpPut("conversations/{id:int}/link-contact")]
        public async Task<IActionResult> LinkContact(int id, [FromBody] LinkContactRequest request, [FromQuery] int userId)
        {
            var success = await _whatsAppService.LinkToContactAsync(id, request.ContactId, userId);
            return success
                ? Ok(new { success = true, message = "Conversation linked to contact." })
                : NotFound(new { success = false, message = "Conversation not found or access denied." });
        }

        /// <summary>Mark all messages in a conversation as read (resets unread count).</summary>
        [Authorize]
        [HttpPut("conversations/{id:int}/mark-read")]
        public async Task<IActionResult> MarkRead(int id, [FromQuery] int userId)
        {
            await _whatsAppService.MarkConversationReadAsync(id, userId);
            return Ok(new { success = true });
        }

        // ── Templates ─────────────────────────────────────────────────────────

        /// <summary>List approved templates cached from Meta for an account.</summary>
        [Authorize]
        [HttpGet("templates")]
        public async Task<IActionResult> GetTemplates([FromQuery] int accountId)
        {
            var templates = await _whatsAppService.GetTemplatesAsync(accountId);
            return Ok(new { success = true, data = templates });
        }

        /// <summary>Sync approved templates from Meta for an account.</summary>
        [Authorize]
        [HttpPost("templates/sync")]
        public async Task<IActionResult> SyncTemplates([FromQuery] int accountId)
        {
            try
            {
                var count = await _whatsAppService.SyncTemplatesAsync(accountId);
                return Ok(new { success = true, message = $"Synced {count} template(s) from Meta." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing WhatsApp templates for account {AccountId}", accountId);
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // ── Private helpers ───────────────────────────────────────────────────

        private async Task<ConversationAccountInfo?> GetAccountForConversationAsync(int conversationId, int userId)
        {
            using var db = new Npgsql.NpgsqlConnection(_connectionString);
            var row = await db.QueryFirstOrDefaultAsync<dynamic>(@"
                SELECT c.account_id, c.contact_phone
                FROM whatsapp_conversations c
                JOIN whatsapp_accounts a ON a.id = c.account_id
                WHERE c.id = @ConversationId AND a.user_id = @UserId AND a.is_active = TRUE",
                new { ConversationId = conversationId, UserId = userId });

            if (row == null) return null;
            return new ConversationAccountInfo { AccountId = (int)row.account_id, ContactPhone = (string)row.contact_phone };
        }

        private sealed class ConversationAccountInfo
        {
            public int AccountId { get; init; }
            public string ContactPhone { get; init; } = string.Empty;
        }
    }

    // ── Supplemental DTOs used only by the controller ─────────────────────────

    public class SendConversationTextRequest
    {
        public string Message { get; set; } = string.Empty;
    }

    public class SendConversationTemplateRequest
    {
        public string TemplateName { get; set; } = string.Empty;
        public string Language { get; set; } = "en_US";
        public List<TemplateComponent>? Components { get; set; }
    }

    public class SendDirectRequest
    {
        public string PhoneNumber { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
