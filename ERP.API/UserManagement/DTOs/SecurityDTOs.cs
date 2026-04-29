using System.ComponentModel.DataAnnotations;

namespace ERP.API.UserManagement.DTOs
{
    /// <summary>
    /// Data Transfer Object for Two-Factor Authentication setup
    /// </summary>
    public class TwoFactorSetupDto
    {
        public string SecretKey { get; set; } = string.Empty;
        public string QrCodeUri { get; set; } = string.Empty;
        public List<string> BackupCodes { get; set; } = new List<string>();
        public string ManualEntryKey { get; set; } = string.Empty;
    }

    /// <summary>
    /// Data Transfer Object for Two-Factor Authentication verification
    /// </summary>
    public class TwoFactorVerificationDto
    {
        public int UserId { get; set; }
        
        [Required(ErrorMessage = "Verification code is required")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Verification code must be 6 digits")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Verification code must be 6 digits")]
        public string Code { get; set; } = string.Empty;
    }

    /// <summary>
    /// Data Transfer Object for security audit log
    /// </summary>
    public class SecurityAuditDto
    {
        public int UserId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public bool Success { get; set; }
        public string? FailureReason { get; set; }
        public Dictionary<string, object>? AdditionalData { get; set; }
    }

    /// <summary>
    /// Data Transfer Object for user statistics
    /// </summary>
    public class UserStatisticsDto
    {
        public long TotalUsers { get; set; }
        public long ActiveUsers { get; set; }
        public long InactiveUsers { get; set; }
        public long LockedUsers { get; set; }
        public long UsersWithTwoFactor { get; set; }
        public long UsersRequiringPasswordChange { get; set; }
        public Dictionary<string, long> UsersByTimeZone { get; set; } = new Dictionary<string, long>();
        public Dictionary<string, long> UsersByLanguage { get; set; } = new Dictionary<string, long>();
        public Dictionary<string, long> LoginsByMonth { get; set; } = new Dictionary<string, long>();
    }

    /// <summary>
    /// Data Transfer Object for account lockout information
    /// </summary>
    public class AccountLockoutDto
    {
        public int UserId { get; set; }
        public bool IsLocked { get; set; }
        public int FailedLoginAttempts { get; set; }
        public DateTime? LockoutEndTime { get; set; }
        public string? LockoutReason { get; set; }
        public DateTime? LastFailedAttempt { get; set; }
    }

    /// <summary>
    /// Data Transfer Object for session information
    /// </summary>
    public class UserSessionDto
    {
        public string SessionId { get; set; } = string.Empty;
        public int UserId { get; set; }
        public DateTime LoginTime { get; set; }
        public DateTime? LastActivity { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        public string DeviceInfo { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string Location { get; set; } = string.Empty;
    }

    /// <summary>
    /// Data Transfer Object for bulk user operations
    /// </summary>
    public class BulkUserOperationDto
    {
        public List<int> UserIds { get; set; } = new List<int>();
        public string Operation { get; set; } = string.Empty; // activate, deactivate, lock, unlock, delete
        public string? Reason { get; set; }
        public bool SendNotification { get; set; } = true;
    }

    /// <summary>
    /// Data Transfer Object for password policy validation result
    /// </summary>
    public class PasswordValidationResultDto
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public int Score { get; set; } // 0-100 password strength score
        public string StrengthLevel { get; set; } = string.Empty; // Weak, Fair, Good, Strong, Very Strong
        public List<string> Suggestions { get; set; } = new List<string>();
    }
}
