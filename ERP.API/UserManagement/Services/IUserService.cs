using ERP.API.UserManagement.DTOs;

namespace ERP.API.UserManagement.Services
{
    /// <summary>
    /// Interface for user management operations
    /// </summary>
    public interface IUserService
    {
        // User Registration and Authentication
        Task<AuthResultDto> RegisterUserAsync(RegisterUserDto registerDto);
        Task<AuthResultDto> LoginAsync(LoginDto loginDto);
        Task<bool> LogoutAsync(string sessionId);
        
        // User Profile Management
        Task<UserProfileDto?> GetUserByIdAsync(int userId);
        Task<UserProfileDto?> GetUserByUsernameAsync(string username);
        Task<UserProfileDto?> GetUserByEmailAsync(string email);
        Task<UserSearchResultDto> GetAllUsersAsync(UserSearchDto searchDto);
        Task<bool> UpdateUserAsync(UpdateUserDto updateDto);
        Task<bool> UpdateUserStatusAsync(int userId, bool isActive, bool? isLocked = null);
        
        // Password Management
        Task<bool> ChangePasswordAsync(ChangePasswordDto changePasswordDto);
        Task<bool> RequestPasswordResetAsync(PasswordResetRequestDto requestDto);
        Task<bool> ResetPasswordAsync(PasswordResetDto resetDto);
        Task<bool> ValidateResetTokenAsync(string resetToken);
        
        // User Account Management
        Task<bool> LockUserAccountAsync(int userId);
        Task<bool> UnlockUserAccountAsync(int userId);
        Task<bool> DeactivateUserAsync(int userId);
        Task<bool> ActivateUserAsync(int userId);
        Task<bool> DeleteUserAsync(int userId, bool hardDelete = false);
        
        // Authentication Tracking
        Task<bool> UpdateLoginInfoAsync(int userId, bool success, string? ipAddress = null, string? deviceInfo = null, string? userAgent = null, string? location = null, string? sessionId = null);
        Task<bool> ResetFailedLoginAttemptsAsync(int userId);
        
        // Two-Factor Authentication
        Task<string> EnableTwoFactorAsync(int userId);
        Task<bool> DisableTwoFactorAsync(int userId);
        Task<bool> VerifyTwoFactorCodeAsync(int userId, string code);
        
        // User Statistics
        Task<object> GetUserStatisticsAsync();
        
        // Validation
        Task<bool> IsUsernameAvailableAsync(string username, int? excludeUserId = null);
        Task<bool> IsEmailAvailableAsync(string email, int? excludeUserId = null);
    }
}
