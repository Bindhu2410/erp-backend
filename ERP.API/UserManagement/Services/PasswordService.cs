using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using ERP.API.UserManagement.Services;
using System.Threading.Tasks;
using Dapper;
using Npgsql;
using Microsoft.Extensions.Logging;

namespace ERP.API.UserManagement.Services
{
    /// <summary>
    /// Implementation of password management service using SHA512 hashing
    /// </summary>
    public class PasswordService : IPasswordService
    {
        private readonly string _connectionString;
        private readonly ILogger<PasswordService> _logger;

        public PasswordService(string connectionString, ILogger<PasswordService> logger)
        {
            _connectionString = connectionString;
            _logger = logger;
        }

        /// <summary>
        /// Generates a cryptographically secure random salt
        /// </summary>
        /// <returns>Base64 encoded salt string (256 bits)</returns>
        public string GenerateSalt()
        {
            byte[] saltBytes = new byte[32]; // 256 bits
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
            }
            return Convert.ToBase64String(saltBytes);
        }

        /// <summary>
        /// Hashes a password with the given salt using SHA512
        /// </summary>
        /// <param name="password">Plain text password</param>
        /// <param name="salt">Base64 encoded salt</param>
        /// <returns>Base64 encoded hash</returns>
        public string HashPassword(string password, string salt)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be null or empty", nameof(password));
            
            if (string.IsNullOrEmpty(salt))
                throw new ArgumentException("Salt cannot be null or empty", nameof(salt));

            byte[] saltBytes = Convert.FromBase64String(salt);
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            
            // Combine password and salt
            byte[] combinedBytes = new byte[passwordBytes.Length + saltBytes.Length];
            Buffer.BlockCopy(passwordBytes, 0, combinedBytes, 0, passwordBytes.Length);
            Buffer.BlockCopy(saltBytes, 0, combinedBytes, passwordBytes.Length, saltBytes.Length);
            
            // Use SHA512 for hashing
            using (var sha512 = SHA512.Create())
            {
                byte[] hashBytes = sha512.ComputeHash(combinedBytes);
                return Convert.ToBase64String(hashBytes);
            }
        }

        /// <summary>
        /// Verifies a password against stored hash and salt
        /// </summary>
        /// <param name="providedPassword">Plain text password to verify</param>
        /// <param name="storedHash">Stored password hash</param>
        /// <param name="storedSalt">Stored password salt</param>
        /// <returns>True if password matches</returns>
        public bool VerifyPassword(string providedPassword, string storedHash, string storedSalt)
        {
            if (string.IsNullOrEmpty(providedPassword) || 
                string.IsNullOrEmpty(storedHash) || 
                string.IsNullOrEmpty(storedSalt))
            {
                return false;
            }

            try
            {
                string hashedProvidedPassword = HashPassword(providedPassword, storedSalt);
                return hashedProvidedPassword == storedHash;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Validates password strength according to security policy
        /// </summary>
        /// <param name="password">Password to validate</param>
        /// <returns>True if password meets strength requirements</returns>
        public bool ValidatePasswordStrength(string password)
        {
            if (string.IsNullOrEmpty(password))
                return false;

            // Only require minimum length of 6 characters
            if (password.Length < 6)
                return false;

            return true;
        }

        /// <summary>
        /// Generates a secure random token for password reset
        /// </summary>
        /// <returns>Reset token string (128 characters)</returns>
        public string GenerateResetToken()
        {
            return GenerateSecureToken(64); // 64 bytes = 128 character base64 string
        }

        /// <summary>
        /// Generates a secure random token for email verification
        /// </summary>
        /// <returns>Verification token string (64 characters)</returns>
        public string GenerateVerificationToken()
        {
            return GenerateSecureToken(32); // 32 bytes = 64 character base64 string
        }

        /// <summary>
        /// Generates a cryptographically secure random token
        /// </summary>
        /// <param name="byteLength">Length in bytes</param>
        /// <returns>Base64 encoded token</returns>
        private string GenerateSecureToken(int byteLength)
        {
            byte[] tokenBytes = new byte[byteLength];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(tokenBytes);
            }
            return Convert.ToBase64String(tokenBytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", ""); // URL-safe base64
        }

        /// <summary>
        /// Gets password strength description for user feedback
        /// </summary>
        /// <param name="password">Password to evaluate</param>
        /// <returns>Password strength description</returns>
        public string GetPasswordStrengthDescription(string password)
        {
            if (string.IsNullOrEmpty(password))
                return "Password is required";

            if (password.Length < 6)
                return "Password must be at least 6 characters";

            return "Strong password";
        }

        /// <summary>
        /// Verifies a user's password by user ID (async method for database operations)
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="password">Plain text password to verify</param>
        /// <returns>True if password matches</returns>
        public async Task<bool> VerifyPasswordAsync(int userId, string password)
        {
            try
            {
                // Get user's password hash and salt from database
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var sql = @"
                    SELECT passwordhash, passwordsalt 
                    FROM public.users 
                    WHERE userid = @userId AND isactive = true AND islocked = false";

                var userCredentials = await connection.QueryFirstOrDefaultAsync<dynamic>(
                    sql, new { userId });

                if (userCredentials == null)
                {
                    return false; // User not found or inactive/locked
                }

                string storedHash = userCredentials.passwordhash;
                string storedSalt = userCredentials.passwordsalt;

                // Verify the provided password against stored hash and salt
                return VerifyPassword(password, storedHash, storedSalt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying password for user {UserId}", userId);
                return false;
            }
        }
    }
}
