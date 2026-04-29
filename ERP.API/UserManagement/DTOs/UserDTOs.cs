using System.ComponentModel.DataAnnotations;

namespace ERP.API.UserManagement.DTOs
{
    /// <summary>
    /// Data Transfer Object for user registration
    /// </summary>
    public class RegisterUserDto
    {
        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
        public string LastName { get; set; } = string.Empty;


        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password confirmation is required")]
        [Compare("Password", ErrorMessage = "Password and confirmation password do not match")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        public string? PhoneNumber { get; set; }

        // [Url(ErrorMessage = "Invalid URL format")]
        // [StringLength(255, ErrorMessage = "Profile image URL cannot exceed 255 characters")]
        public string? ProfileImageUrl { get; set; }

        [StringLength(10, ErrorMessage = "Preferred language cannot exceed 10 characters")]
        public string PreferredLanguage { get; set; } = "en-US";

        [StringLength(50, ErrorMessage = "Timezone cannot exceed 50 characters")]
        public string TimeZone { get; set; } = "UTC";

        public bool TwoFactorEnabled { get; set; } = false;

        [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Data Transfer Object for user login
    /// </summary>
    public class LoginDto
    {
        [Required(ErrorMessage = "Email or Username is required")]
        [StringLength(100, ErrorMessage = "Email or Username cannot exceed 100 characters")]
        public string EmailOrUsername { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(255, ErrorMessage = "Password cannot exceed 255 characters")]
        public string Password { get; set; } = string.Empty;
        public int? RoleId { get; set; }
        public string? RoleName { get; set; }
        public bool RememberMe { get; set; } = false;

        // Optional fields for security tracking
        public string? IpAddress { get; set; }
        public string? DeviceInfo { get; set; }
        public string? UserAgent { get; set; }

        // Two-factor authentication code
        [StringLength(10, ErrorMessage = "Two-factor code cannot exceed 10 characters")]
        public string? TwoFactorCode { get; set; }
    }

    /// <summary>
    /// Data Transfer Object for authentication result
    /// </summary>
    public class AuthResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Token { get; set; }
        public int UserId { get; set; }
        public string? SessionId { get; set; }
        public bool RequiresTwoFactor { get; set; } = false;
        public DateTime? TokenExpiry { get; set; }
        public UserProfileDto? UserProfile { get; set; }
        public RoleDto? RoleDto { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiry { get; set; }
    }

    /// <summary>
    /// Data Transfer Object for user profile information
    /// </summary>
    public class UserProfileDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? ProfileImageUrl { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsLocked { get; set; }
        public int FailedLoginAttempts { get; set; }
        public string PreferredLanguage { get; set; } = string.Empty;
        public string TimeZone { get; set; } = string.Empty;
        public bool TwoFactorEnabled { get; set; }
        public DateTime? LastPasswordChangeDate { get; set; }
        public bool RequirePasswordChange { get; set; }
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Data Transfer Object for updating user information
    /// </summary>
        public class UpdateUserDto
        {
            [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
            public string? Password { get; set; }

            public int UserId { get; set; }

            [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
            public string? Username { get; set; }

            [EmailAddress(ErrorMessage = "Invalid email format")]
            [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
            public string? Email { get; set; }

            [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
            public string? FirstName { get; set; }

            [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
            public string? LastName { get; set; }

            [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
            public string? PhoneNumber { get; set; }

            [StringLength(255, ErrorMessage = "Profile image URL cannot exceed 255 characters")]
            public string? ProfileImageUrl { get; set; }

            [StringLength(10, ErrorMessage = "Preferred language cannot exceed 10 characters")]
            public string? PreferredLanguage { get; set; }

            [StringLength(50, ErrorMessage = "Timezone cannot exceed 50 characters")]
            public string? TimeZone { get; set; }

            public bool? TwoFactorEnabled { get; set; }

            [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
            public string? Notes { get; set; }
        }

    /// <summary>
    /// Data Transfer Object for password change
    /// </summary>
    public class ChangePasswordDto
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "Current password is required")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password confirmation is required")]
        [Compare("NewPassword", ErrorMessage = "New password and confirmation do not match")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }

    /// <summary>
    /// Data Transfer Object for password reset request
    /// </summary>
    public class PasswordResetRequestDto
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;
    }

    /// <summary>
    /// Data Transfer Object for password reset
    /// </summary>
    public class PasswordResetDto
    {
        [Required(ErrorMessage = "Reset token is required")]
        public string ResetToken { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password confirmation is required")]
        [Compare("NewPassword", ErrorMessage = "New password and confirmation do not match")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }

    /// <summary>
    /// Data Transfer Object for user search and listing
    /// </summary>
    public class UserSearchDto
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsLocked { get; set; }
        public string? SortBy { get; set; }
        public bool SortDescending { get; set; } = false;
    }

    /// <summary>
    /// Data Transfer Object for paginated user results
    /// </summary>
    public class UserSearchResultDto
    {
        public List<UserProfileDto> Users { get; set; } = new List<UserProfileDto>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }

    /// <summary>
    /// Data Transfer Object for password verification
    /// </summary>
    public class VerifyPasswordDto
    {
        [Required(ErrorMessage = "User ID is required")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// Data Transfer Object for updating login information
    /// </summary>
    public class UpdateLoginInfoDto
    {
        [Required(ErrorMessage = "User ID is required")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Success status is required")]
        public bool Success { get; set; }

        [StringLength(45, ErrorMessage = "IP address cannot exceed 45 characters")]
        public string? IpAddress { get; set; }

        [StringLength(255, ErrorMessage = "Device information cannot exceed 255 characters")]
        public string? DeviceInfo { get; set; }

        [StringLength(255, ErrorMessage = "User agent cannot exceed 255 characters")]
        public string? UserAgent { get; set; }

        [StringLength(100, ErrorMessage = "Location information cannot exceed 100 characters")]
        public string? Location { get; set; }

        [StringLength(255, ErrorMessage = "Session ID cannot exceed 255 characters")]
        public string? SessionId { get; set; }

        public DateTime LoginAttemptTime { get; set; } = DateTime.UtcNow;
    }
    /// <summary>
    /// Data Transfer Object for token response
    /// </summary>
    public class TokenResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime AccessTokenExpiry { get; set; }
        public DateTime RefreshTokenExpiry { get; set; }
        public string TokenType { get; set; } = "Bearer";
    }

    /// <summary>
    /// Data Transfer Object for token refresh request
    /// </summary>
    public class RefreshTokenRequestDto
    {
        [Required(ErrorMessage = "Access token is required")]
        public string AccessToken { get; set; } = string.Empty;

        [Required(ErrorMessage = "Refresh token is required")]
        public string RefreshToken { get; set; } = string.Empty;
    }

    /// <summary>
    /// Data Transfer Object for refresh token information
    /// </summary>
    public class RefreshTokenDto
    {
        public int RefreshTokenId { get; set; }
        public int UserId { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }
        public bool IsRevoked { get; set; }
        public string? RevokedReason { get; set; }
        public DateTime? RevokedDate { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
