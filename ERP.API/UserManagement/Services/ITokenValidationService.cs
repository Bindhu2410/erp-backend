using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Npgsql;

namespace ERP.API.UserManagement.Services
{
    public interface ITokenValidationService
    {
        bool IsTokenValid(string token);
        Task<bool> IsTokenRevokedAsync(string jti);
    }

    public class TokenValidationService : ITokenValidationService
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        private readonly ILogger<TokenValidationService> _logger;

        public TokenValidationService(IConfiguration configuration, ILogger<TokenValidationService> logger)
        {
            _configuration = configuration;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _logger = logger;
        }

        public bool IsTokenValid(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);

                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> IsTokenRevokedAsync(string jti)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = "SELECT 1 FROM revoked_tokens WHERE jti = @jti";
                using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("jti", jti);

                var result = await command.ExecuteScalarAsync();
                return result != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking token revocation");
                return false;
            }
        }
    }
}