using Microsoft.AspNetCore.Mvc;
using ERP.API.Models.DTOs;
using ERP.API.Services;
using Dapper;
using System.Data;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmailController : ControllerBase
    {
        private readonly IGmailService _gmailService;
        private readonly string _connectionString;
        private readonly ILogger<EmailController> _logger;

        public EmailController(IGmailService gmailService, IConfiguration configuration, ILogger<EmailController> logger)
        {
            _gmailService = gmailService;
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
            _logger = logger;
        }

        [HttpPost("init-db")]
        public async Task<IActionResult> InitializeDb()
        {
            try
            {
                using var db = new Npgsql.NpgsqlConnection(_connectionString);
                await db.OpenAsync();

                _logger.LogInformation("Initializing Email System Database...");

                // 1. Create OAuth States table (Safe to drop as it's transient)
                string oauthStatesSql = @"
                DROP TABLE IF EXISTS oauth_states;
                CREATE TABLE oauth_states (
                    user_id INTEGER PRIMARY KEY,
                    state VARCHAR(255) NOT NULL,
                    expires_at TIMESTAMP NOT NULL,
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                );";

                // 2. Create Email Accounts and other system tables
                string emailTablesSql = @"
                CREATE TABLE IF NOT EXISTS email_accounts (
                    id SERIAL PRIMARY KEY,
                    email_address VARCHAR(255) UNIQUE NOT NULL,
                    display_name VARCHAR(255),
                    access_token TEXT,
                    refresh_token TEXT,
                    token_expiry TIMESTAMP,
                    is_connected BOOLEAN DEFAULT FALSE,
                    is_primary BOOLEAN DEFAULT FALSE,
                    is_active BOOLEAN DEFAULT TRUE,
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                );

                DO $$ 
                BEGIN 
                    IF NOT EXISTS (SELECT 1 FROM information_schema.table_constraints WHERE table_name = 'email_accounts' AND constraint_type = 'UNIQUE') THEN
                        ALTER TABLE email_accounts ADD CONSTRAINT email_accounts_email_address_key UNIQUE (email_address);
                    END IF;
                EXCEPTION WHEN OTHERS THEN
                    -- Ignore if already exists or other issues
                END $$;

                CREATE TABLE IF NOT EXISTS email_messages (
                    id SERIAL PRIMARY KEY,
                    gmail_message_id VARCHAR(255) UNIQUE,
                    thread_id VARCHAR(255),
                    subject TEXT,
                    body_html TEXT,
                    body_text TEXT,
                    from_email VARCHAR(255),
                    from_name VARCHAR(255),
                    to_emails TEXT[],
                    cc_emails TEXT[],
                    bcc_emails TEXT[],
                    status VARCHAR(50),
                    sent_at TIMESTAMP,
                    received_at TIMESTAMP,
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                );";

                await db.ExecuteAsync(oauthStatesSql);
                await db.ExecuteAsync(emailTablesSql);

                return Ok(new { success = true, message = "Database initialized successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing database");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("auth-url")]
        public async Task<ActionResult<OAuthUrlResponse>> GetAuthUrl([FromQuery] int userId)
        {
            try
            {
                var state = Guid.NewGuid().ToString();
                
                using var db = new Npgsql.NpgsqlConnection(_connectionString);
                await db.ExecuteAsync("DELETE FROM oauth_states WHERE user_id = @userId", new { userId });
                await db.ExecuteAsync(
                    "INSERT INTO oauth_states (user_id, state, expires_at) VALUES (@userId, @state, @expiresAt)",
                    new { userId, state, expiresAt = DateTime.UtcNow.AddMinutes(15) });

                var url = _gmailService.GetAuthorizationUrl(userId, state);
                return Ok(new OAuthUrlResponse { AuthorizationUrl = url, State = state });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating auth URL");
                return StatusCode(500, new { success = false, message = "An internal error occurred." });
            }
        }

        [HttpPost("callback")]
        public async Task<IActionResult> Callback([FromBody] OAuthCallbackRequest request)
        {
            using var db = new Npgsql.NpgsqlConnection(_connectionString);
            var stateInfo = await db.QueryFirstOrDefaultAsync<dynamic>(
                "SELECT * FROM oauth_states WHERE state = @State AND expires_at > CURRENT_TIMESTAMP",
                new { request.State });

            if (stateInfo == null)
                return BadRequest("Invalid or expired state");

            int userId = stateInfo.user_id;
            var success = await _gmailService.AuthenticateAsync(userId, request.Code);
            
            if (success)
            {
                await db.ExecuteAsync("DELETE FROM oauth_states WHERE state = @State", new { request.State });
                return Ok(new { success = true });
            }

            return BadRequest("Authentication failed");
        }

        [HttpPost("send")]
        public async Task<ActionResult<EmailResponse>> SendEmail([FromQuery] int userId, [FromBody] SendEmailRequest request)
        {
            var result = await _gmailService.SendEmailAsync(userId, request);
            if (result.Success) return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("accounts")]
        public async Task<ActionResult<List<EmailAccountDto>>> GetAccounts([FromQuery] int userId)
        {
            var account = await _gmailService.GetConnectedAccountAsync(userId);
            return Ok(account != null ? new List<EmailAccountDto> { account } : new List<EmailAccountDto>());
        }

        [HttpPost("disconnect")]
        public async Task<IActionResult> Disconnect([FromQuery] int userId)
        {
            await _gmailService.DisconnectAccountAsync(userId);
            return Ok();
        }
    }
}
