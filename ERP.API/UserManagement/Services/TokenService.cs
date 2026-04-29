using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using ERP.API.UserManagement.DTOs;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace ERP.API.UserManagement.Services
{
    public interface ITokenService
    {
        string GenerateAccessToken(int userId, string username, string role);
        RefreshTokenDto GenerateRefreshToken(int userId, string ipAddress, string userAgent);
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
        bool ValidateRefreshToken(string refreshToken, int userId);
        Task<bool> RevokeRefreshToken(string refreshToken, string reason = null);
        Task<bool> SaveRefreshTokenAsync(RefreshTokenDto refreshTokenDto);
        Task<RefreshTokenDto?> GetRefreshTokenAsync(string token);
    }

    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        private readonly ILogger<TokenService> _logger;

        public TokenService(IConfiguration configuration, ILogger<TokenService> logger)
        {
            _configuration = configuration;
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentException("Default connection string not found");
            _logger = logger;
        }

        public string GenerateAccessToken(int userId, string username, string role)
        {
            try
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.Role, role),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
                };

                var jwtKey = _configuration["Jwt:Key"];
                if (string.IsNullOrEmpty(jwtKey))
                    throw new InvalidOperationException("JWT Key is not configured");

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var tokenExpiryMinutes = _configuration.GetValue<int>("Jwt:AccessTokenExpiryMinutes", 15);

                var token = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"],
                    audience: _configuration["Jwt:Audience"],
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(tokenExpiryMinutes),
                    signingCredentials: creds
                );

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating access token");
                throw;
            }
        }

        public RefreshTokenDto GenerateRefreshToken(int userId, string ipAddress, string userAgent)
        {
            try
            {
                var randomNumber = new byte[32];
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(randomNumber);
                }

                var refreshTokenExpiryDays = _configuration.GetValue<int>("Jwt:RefreshTokenExpiryDays", 7);

                return new RefreshTokenDto
                {
                    UserId = userId,
                    Token = Convert.ToBase64String(randomNumber),
                    ExpiryDate = DateTime.UtcNow.AddDays(refreshTokenExpiryDays),
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    CreatedDate = DateTime.UtcNow,
                    IsRevoked = false
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating refresh token");
                throw;
            }
        }

        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            try
            {
                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = false,
                    ValidateIssuer = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])),
                    ValidateLifetime = false
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

                if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                    !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                    throw new SecurityTokenException("Invalid token");

                return principal;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting principal from expired token");
                throw;
            }
        }

        public bool ValidateRefreshToken(string refreshToken, int userId)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                connection.Open();

                var query = @"
                    SELECT 1 FROM refresh_tokens 
                    WHERE token = @token 
                    AND user_id = @userId 
                    AND expiry_date > @now 
                    AND is_revoked = false";

                using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("token", refreshToken);
                command.Parameters.AddWithValue("userId", userId);
                command.Parameters.AddWithValue("now", DateTime.UtcNow);

                var result = command.ExecuteScalar();
                return result != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating refresh token");
                return false;
            }
        }

        public async Task<bool> SaveRefreshTokenAsync(RefreshTokenDto refreshTokenDto)
        {
            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = @"
                    INSERT INTO refresh_tokens (user_id, token, expiry_date, ip_address, user_agent, created_date)
                    VALUES (@userId, @token, @expiryDate, @ipAddress, @userAgent, @createdDate)
                    ON CONFLICT (token) DO NOTHING";

                await using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("userId", refreshTokenDto.UserId);
                command.Parameters.AddWithValue("token", refreshTokenDto.Token);
                command.Parameters.AddWithValue("expiryDate", refreshTokenDto.ExpiryDate);
                command.Parameters.AddWithValue("ipAddress", refreshTokenDto.IpAddress ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("userAgent", refreshTokenDto.UserAgent ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("createdDate", refreshTokenDto.CreatedDate);

                var result = await command.ExecuteNonQueryAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving refresh token");
                return false;
            }
        }

        public async Task<RefreshTokenDto?> GetRefreshTokenAsync(string token)
        {
            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = @"
                    SELECT refresh_token_id, user_id, token, expiry_date, is_revoked, 
                           revoked_reason, revoked_date, ip_address, user_agent, created_date
                    FROM refresh_tokens 
                    WHERE token = @token";

                await using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("token", token);

                await using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return new RefreshTokenDto
                    {
                        RefreshTokenId = reader.GetInt32(0),
                        UserId = reader.GetInt32(1),
                        Token = reader.GetString(2),
                        ExpiryDate = reader.GetDateTime(3),
                        IsRevoked = reader.GetBoolean(4),
                        RevokedReason = reader.IsDBNull(5) ? null : reader.GetString(5),
                        RevokedDate = reader.IsDBNull(6) ? null : reader.GetDateTime(6),
                        IpAddress = reader.IsDBNull(7) ? null : reader.GetString(7),
                        UserAgent = reader.IsDBNull(8) ? null : reader.GetString(8),
                        CreatedDate = reader.GetDateTime(9)
                    };
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting refresh token");
                return null;
            }
        }

        public async Task<bool> RevokeRefreshToken(string refreshToken, string reason = null)
        {
            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = @"
                    UPDATE refresh_tokens 
                    SET is_revoked = true, 
                        revoked_reason = @reason, 
                        revoked_date = @revokedDate
                    WHERE token = @token";

                await using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("token", refreshToken);
                command.Parameters.AddWithValue("reason", reason ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("revokedDate", DateTime.UtcNow);

                var result = await command.ExecuteNonQueryAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking refresh token");
                return false;
            }
        }
    }
}