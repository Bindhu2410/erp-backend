using System.Security.Cryptography;
using System.Text;

namespace ERP.API.UserManagement.Services
{
    /// <summary>
    /// Interface for Two-Factor Authentication operations
    /// </summary>
    public interface ITwoFactorService
    {
        /// <summary>
        /// Generates a new secret key for TOTP
        /// </summary>
        /// <returns>Base32 encoded secret key</returns>
        string GenerateSecretKey();

        /// <summary>
        /// Generates QR code URI for authenticator apps
        /// </summary>
        /// <param name="secretKey">User's secret key</param>
        /// <param name="userEmail">User's email</param>
        /// <param name="issuer">Service issuer name</param>
        /// <returns>QR code URI</returns>
        string GenerateQrCodeUri(string secretKey, string userEmail, string issuer = "ERP System");

        /// <summary>
        /// Verifies TOTP code against secret key
        /// </summary>
        /// <param name="secretKey">User's secret key</param>
        /// <param name="code">6-digit TOTP code</param>
        /// <param name="timeStep">Time step window (default: 30 seconds)</param>
        /// <returns>True if code is valid</returns>
        bool VerifyTotpCode(string secretKey, string code, int timeStep = 30);

        /// <summary>
        /// Generates backup codes for recovery
        /// </summary>
        /// <param name="count">Number of backup codes to generate</param>
        /// <returns>List of backup codes</returns>
        List<string> GenerateBackupCodes(int count = 10);
    }

    /// <summary>
    /// Implementation of Two-Factor Authentication service
    /// </summary>
    public class TwoFactorService : ITwoFactorService
    {
        /// <summary>
        /// Generates a new secret key for TOTP
        /// </summary>
        public string GenerateSecretKey()
        {
            // Generate 20 bytes (160 bits) of random data
            byte[] secretBytes = new byte[20];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(secretBytes);
            }
            return ToBase32String(secretBytes);
        }

        /// <summary>
        /// Generates QR code URI for authenticator apps
        /// </summary>
        public string GenerateQrCodeUri(string secretKey, string userEmail, string issuer = "ERP System")
        {
            var encodedIssuer = Uri.EscapeDataString(issuer);
            var encodedUser = Uri.EscapeDataString(userEmail);
            
            return $"otpauth://totp/{encodedIssuer}:{encodedUser}?secret={secretKey}&issuer={encodedIssuer}&algorithm=SHA1&digits=6&period=30";
        }

        /// <summary>
        /// Verifies TOTP code against secret key
        /// </summary>
        public bool VerifyTotpCode(string secretKey, string code, int timeStep = 30)
        {
            if (string.IsNullOrEmpty(secretKey) || string.IsNullOrEmpty(code))
                return false;

            if (code.Length != 6 || !code.All(char.IsDigit))
                return false;

            byte[] secretBytes = FromBase32String(secretKey);
            long currentTimeStep = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / timeStep;

            // Check current time window and adjacent windows (±1) for clock drift
            for (int i = -1; i <= 1; i++)
            {
                long timeWindow = currentTimeStep + i;
                string expectedCode = GenerateTotpCode(secretBytes, timeWindow);
                
                if (code == expectedCode)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Generates backup codes for recovery
        /// </summary>
        public List<string> GenerateBackupCodes(int count = 10)
        {
            var backupCodes = new List<string>();
            
            for (int i = 0; i < count; i++)
            {
                byte[] codeBytes = new byte[4]; // 8 digit codes
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(codeBytes);
                }
                
                // Convert to 8-digit number
                uint codeNumber = BitConverter.ToUInt32(codeBytes, 0) % 100000000;
                backupCodes.Add(codeNumber.ToString("D8"));
            }
            
            return backupCodes;
        }

        /// <summary>
        /// Generates TOTP code for given secret and time window
        /// </summary>
        private string GenerateTotpCode(byte[] secret, long timeWindow)
        {
            byte[] timeBytes = BitConverter.GetBytes(timeWindow);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(timeBytes);

            using (var hmac = new HMACSHA1(secret))
            {
                byte[] hash = hmac.ComputeHash(timeBytes);
                
                int offset = hash[hash.Length - 1] & 0x0F;
                int binaryCode = ((hash[offset] & 0x7F) << 24) |
                                ((hash[offset + 1] & 0xFF) << 16) |
                                ((hash[offset + 2] & 0xFF) << 8) |
                                (hash[offset + 3] & 0xFF);
                
                int code = binaryCode % 1000000;
                return code.ToString("D6");
            }
        }

        /// <summary>
        /// Converts byte array to Base32 string
        /// </summary>
        private string ToBase32String(byte[] bytes)
        {
            const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
            string result = "";
            
            for (int i = 0; i < bytes.Length; i += 5)
            {
                int count = Math.Min(5, bytes.Length - i);
                ulong buffer = 0;
                
                for (int j = 0; j < count; j++)
                {
                    buffer = (buffer << 8) | bytes[i + j];
                }
                
                for (int j = 0; j < (count * 8 + 4) / 5; j++)
                {
                    result = alphabet[(int)((buffer >> (35 - 5 * j)) & 0x1F)] + result;
                }
            }
            
            return new string(result.Reverse().ToArray());
        }

        /// <summary>
        /// Converts Base32 string to byte array
        /// </summary>
        private byte[] FromBase32String(string base32)
        {
            const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
            base32 = base32.ToUpper();
            
            var result = new List<byte>();
            int buffer = 0;
            int bitsLeft = 0;
            
            foreach (char c in base32)
            {
                int value = alphabet.IndexOf(c);
                if (value < 0) continue;
                
                buffer = (buffer << 5) | value;
                bitsLeft += 5;
                
                if (bitsLeft >= 8)
                {
                    result.Add((byte)(buffer >> (bitsLeft - 8)));
                    bitsLeft -= 8;
                }
            }
            
            return result.ToArray();
        }
    }
}
