using System.ComponentModel.DataAnnotations;

namespace ERP.API.UserManagement.DTOs
{
    /// <summary>
    /// DTO for creating/updating a user preference
    /// </summary>
    public class CreateUserPreferenceDto
    {
        [Required(ErrorMessage = "UserId is required")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "PreferenceKey is required")]
        [StringLength(50, ErrorMessage = "PreferenceKey cannot exceed 50 characters")]
        public string PreferenceKey { get; set; } = string.Empty;

        public string? PreferenceValue { get; set; }
    }

    /// <summary>
    /// DTO for updating a user preference
    /// </summary>
    public class UpdateUserPreferenceDto
    {
        [Required(ErrorMessage = "UserId is required")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "PreferenceKey is required")]
        [StringLength(50, ErrorMessage = "PreferenceKey cannot exceed 50 characters")]
        public string PreferenceKey { get; set; } = string.Empty;

        public string? PreferenceValue { get; set; }
    }

    /// <summary>
    /// DTO for bulk updating user preferences
    /// </summary>
    public class BulkUpdateUserPreferencesDto
    {
        [Required(ErrorMessage = "UserId is required")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Preferences is required")]
        public List<UserPreferenceItemDto> Preferences { get; set; } = new();
    }

    /// <summary>
    /// DTO for individual preference item in bulk operations
    /// </summary>
    public class UserPreferenceItemDto
    {
        [Required(ErrorMessage = "Key is required")]
        [StringLength(50, ErrorMessage = "Key cannot exceed 50 characters")]
        public string Key { get; set; } = string.Empty;

        public string? Value { get; set; }
    }

    /// <summary>
    /// DTO for user preference response
    /// </summary>
    public class UserPreferenceDto
    {
        public int UserId { get; set; }
        public string PreferenceKey { get; set; } = string.Empty;
        public string? PreferenceValue { get; set; }
        public DateTime DateModified { get; set; }
    }

    /// <summary>
    /// DTO for detailed user preference with user info
    /// </summary>
    public class DetailedUserPreferenceDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PreferenceKey { get; set; } = string.Empty;
        public string? PreferenceValue { get; set; }
        public DateTime DateModified { get; set; }
    }

    /// <summary>
    /// DTO for paged user preferences request
    /// </summary>
    public class PagedUserPreferencesRequestDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "PageNumber must be greater than 0")]
        public int PageNumber { get; set; } = 1;

        [Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100")]
        public int PageSize { get; set; } = 20;

        public int? UserId { get; set; }
    }

    /// <summary>
    /// DTO for paged user preferences response
    /// </summary>
    public class PagedUserPreferencesResponseDto
    {
        public List<DetailedUserPreferenceDto> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasNextPage => PageNumber < TotalPages;
        public bool HasPreviousPage => PageNumber > 1;
    }

    /// <summary>
    /// DTO for user preferences summary
    /// </summary>
    public class UserPreferencesSummaryDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public long TotalPreferences { get; set; }
        public DateTime? LastModified { get; set; }
    }

    /// <summary>
    /// DTO for users with specific preference
    /// </summary>
    public class UserWithPreferenceDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? PreferenceValue { get; set; }
        public DateTime DateModified { get; set; }
    }

    /// <summary>
    /// DTO for user preference operation result
    /// </summary>
    public class UserPreferenceResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for bulk user preference operation result
    /// </summary>
    public class BulkUserPreferenceResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int UpdatedCount { get; set; }
    }

    /// <summary>
    /// DTO for delete all user preferences result
    /// </summary>
    public class DeleteAllUserPreferencesResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int DeletedCount { get; set; }
    }

    /// <summary>
    /// DTO for user preference search by pattern
    /// </summary>
    public class UserPreferencePatternSearchDto
    {
        [Required(ErrorMessage = "UserId is required")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "KeyPattern is required")]
        [StringLength(50, ErrorMessage = "KeyPattern cannot exceed 50 characters")]
        public string KeyPattern { get; set; } = string.Empty;
    }
}
