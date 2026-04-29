using System.Threading.Tasks;

namespace ERP.API.UserManagement.Services
{
    /// <summary>
    /// Interface for password management operations
    /// </summary>
    public interface IPasswordService
    {
        /// <summary>
        /// Generates a random salt for password hashing
        /// </summary>
        /// <returns>Base64 encoded salt string</returns>
        string GenerateSalt();

        /// <summary>
        /// Hashes a password with the given salt using SHA512
        /// </summary>
        /// <param name="password">Plain text password</param>
        /// <param name="salt">Base64 encoded salt</param>
        /// <returns>Base64 encoded hash</returns>
        string HashPassword(string password, string salt);

        /// <summary>
        /// Verifies a password against stored hash and salt
        /// </summary>
        /// <param name="providedPassword">Plain text password to verify</param>
        /// <param name="storedHash">Stored password hash</param>
        /// <param name="storedSalt">Stored password salt</param>
        /// <returns>True if password matches</returns>
        bool VerifyPassword(string providedPassword, string storedHash, string storedSalt);

        /// <summary>
        /// Validates password strength according to policy
        /// </summary>
        /// <param name="password">Password to validate</param>
        /// <returns>True if password meets strength requirements</returns>
        bool ValidatePasswordStrength(string password);

        /// <summary>
        /// Generates a secure random token for password reset
        /// </summary>
        /// <returns>Reset token string</returns>
        string GenerateResetToken();

        /// <summary>
        /// Generates a secure random token for email verification
        /// </summary>
        /// <returns>Verification token string</returns>
        string GenerateVerificationToken();

        /// <summary>
        /// Gets password strength description for user feedback
        /// </summary>
        /// <param name="password">Password to evaluate</param>
        /// <returns>Password strength description</returns>
        string GetPasswordStrengthDescription(string password);

        /// <summary>
        /// Verifies a user's password by user ID (async method for database operations)
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="password">Plain text password to verify</param>
        /// <returns>True if password matches</returns>
        Task<bool> VerifyPasswordAsync(int userId, string password);
    }
}
