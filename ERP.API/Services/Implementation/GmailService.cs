using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using ERP.API.Models.DTOs;
using System.Data;
using Dapper;
using Google.Apis.Auth.OAuth2.Requests;

namespace ERP.API.Services.Implementation
{
    public class GmailService : IGmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<GmailService> _logger;
        private readonly string _connectionString;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _redirectUri;

        public GmailService(IConfiguration configuration, ILogger<GmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "";
            _clientId = _configuration["GmailApp:ClientId"] ?? "";
            _clientSecret = _configuration["GmailApp:ClientSecret"] ?? "";
            _redirectUri = _configuration["GmailApp:RedirectUri"] ?? "";
        }

        private async Task<GoogleAuthorizationCodeFlow> GetFlowAsync()
        {
            return new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = _clientId,
                    ClientSecret = _clientSecret
                },
                Scopes = new[] { Google.Apis.Gmail.v1.GmailService.Scope.GmailSend, Google.Apis.Gmail.v1.GmailService.Scope.GmailReadonly, Google.Apis.Gmail.v1.GmailService.Scope.GmailCompose },
                DataStore = new NullDataStore()
            });
        }

        public string GetAuthorizationUrl(int userId, string state)
        {
            if (string.IsNullOrEmpty(_clientId) || _clientId.StartsWith("REPLACE_"))
            {
                throw new InvalidOperationException("Gmail ClientId is not configured in appsettings.json. Please add GmailApp:ClientId.");
            }

            var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets { ClientId = _clientId, ClientSecret = _clientSecret },
                Scopes = new[] { Google.Apis.Gmail.v1.GmailService.Scope.GmailSend, Google.Apis.Gmail.v1.GmailService.Scope.GmailReadonly, Google.Apis.Gmail.v1.GmailService.Scope.GmailCompose }
            });

            var request = (GoogleAuthorizationCodeRequestUrl)flow.CreateAuthorizationCodeRequest(_redirectUri);
            request.State = state;
            request.AccessType = "offline";
            request.Prompt = "consent";
            return request.Build().ToString();
        }

        public async Task<bool> AuthenticateAsync(int userId, string code)
        {
            try
            {
                var flow = await GetFlowAsync();
                var tokenResponse = await flow.ExchangeCodeForTokenAsync(userId.ToString(), code, _redirectUri, CancellationToken.None);

                var credential = new UserCredential(flow, userId.ToString(), tokenResponse);

                // Get user info to get email address
                var service = new Google.Apis.Gmail.v1.GmailService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "ERP Integration"
                });

                var profile = await service.Users.GetProfile("me").ExecuteAsync();
                var emailAddress = profile.EmailAddress;

                using var db = new Npgsql.NpgsqlConnection(_connectionString);
                await db.OpenAsync();

                var existingAccount = await db.QueryFirstOrDefaultAsync<int?>(
                    "SELECT id FROM email_accounts WHERE email_address = @EmailAddress",
                    new { EmailAddress = emailAddress });

                var accountData = new
                {
                    UserId = userId,
                    EmailAddress = emailAddress,
                    AccessToken = tokenResponse.AccessToken,
                    RefreshToken = tokenResponse.RefreshToken,
                    ExpiresIn = tokenResponse.ExpiresInSeconds ?? 3600,
                    TokenType = tokenResponse.TokenType,
                    Scope = tokenResponse.Scope,
                    IssuedUtc = tokenResponse.IssuedUtc
                };

                if (existingAccount.HasValue)
                {
                    await db.ExecuteAsync(@"
                        UPDATE email_accounts 
                        SET access_token = @AccessToken, 
                            refresh_token = @RefreshToken, 
                            token_expiry = @IssuedUtc + interval '1 second' * @ExpiresIn,
                            is_connected = true,
                            updated_at = CURRENT_TIMESTAMP
                        WHERE id = @Id",
                        new { accountData.AccessToken, accountData.RefreshToken, accountData.IssuedUtc, accountData.ExpiresIn, Id = existingAccount.Value });
                }
                else
                {
                    await db.ExecuteAsync(@"
                        INSERT INTO email_accounts (email_address, access_token, refresh_token, token_expiry, is_connected, is_primary)
                        VALUES (@EmailAddress, @AccessToken, @RefreshToken, @IssuedUtc + interval '1 second' * @ExpiresIn, true, true)",
                        accountData);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error authenticating with Gmail for user {UserId}", userId);
                return false;
            }
        }

        private async Task<UserCredential> GetCredentialAsync(int userId)
        {
            using var db = new Npgsql.NpgsqlConnection(_connectionString);
            var account = await db.QueryFirstOrDefaultAsync<dynamic>(
                "SELECT * FROM email_accounts WHERE is_connected = true LIMIT 1");

            if (account == null) return null;

            var token = new TokenResponse
            {
                AccessToken = account.access_token,
                RefreshToken = account.refresh_token,
                ExpiresInSeconds = (long)(account.token_expiry - DateTime.UtcNow).TotalSeconds,
                TokenType = "Bearer",
                IssuedUtc = DateTime.UtcNow.AddSeconds(-(3600 - (long)(account.token_expiry - DateTime.UtcNow).TotalSeconds))
            };

            var flow = await GetFlowAsync();
            return new UserCredential(flow, userId.ToString(), token);
        }

        public async Task<EmailResponse> SendEmailAsync(int userId, SendEmailRequest request)
        {
            try
            {
                var credential = await GetCredentialAsync(userId);
                if (credential == null)
                {
                    return new EmailResponse { Success = false, ErrorMessage = "No Gmail account connected." };
                }

                var service = new Google.Apis.Gmail.v1.GmailService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "ERP Integration"
                });

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("ERP System", "me"));
                message.To.Add(MailboxAddress.Parse(request.To));
                message.Subject = request.Subject;

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = request.BodyHtml,
                    TextBody = request.BodyText
                };

                foreach (var att in request.Attachments)
                {
                    bodyBuilder.Attachments.Add(att.Filename, att.Content, ContentType.Parse(att.ContentType));
                }

                message.Body = bodyBuilder.ToMessageBody();

                using var ms = new MemoryStream();
                await message.WriteToAsync(ms);
                var rawMessage = Convert.ToBase64String(ms.ToArray())
                    .Replace('+', '-')
                    .Replace('/', '_')
                    .Replace("=", "");

                var gmailMessage = new Message { Raw = rawMessage };
                var result = await service.Users.Messages.Send(gmailMessage, "me").ExecuteAsync();

                return new EmailResponse
                {
                    Success = true,
                    GmailMessageId = result.Id,
                    SentAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email for user {UserId}", userId);
                return new EmailResponse { Success = false, ErrorMessage = ex.Message };
            }
        }

        public async Task<EmailAccountDto?> GetConnectedAccountAsync(int userId)
        {
            using var db = new Npgsql.NpgsqlConnection(_connectionString);
            var account = await db.QueryFirstOrDefaultAsync<EmailAccountDto>(
                "SELECT id, email_address as EmailAddress, is_connected as IsConnected, token_expiry as TokenExpiry FROM email_accounts WHERE is_connected = true LIMIT 1");
            return account;
        }

        public async Task<bool> DisconnectAccountAsync(int userId)
        {
            using var db = new Npgsql.NpgsqlConnection(_connectionString);
            await db.ExecuteAsync("UPDATE email_accounts SET is_connected = false WHERE is_connected = true");
            return true;
        }

        public async Task<EmailListResponse> ListMessagesAsync(int userId, EmailSearchRequest request)
        {
            // Implementation for listing messages if needed
            return new EmailListResponse();
        }

        public async Task<EmailMessageDto?> GetMessageAsync(int userId, string messageId)
        {
            // Implementation for getting a message if needed
            return null;
        }
    }
}
