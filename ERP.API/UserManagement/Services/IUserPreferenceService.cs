using ERP.API.UserManagement.DTOs;

namespace ERP.API.UserManagement.Services
{
    /// <summary>
    /// Interface for User Preference Service operations
    /// </summary>
    public interface IUserPreferenceService
    {
        /// <summary>
        /// Creates or updates a user preference (UPSERT)
        /// </summary>
        /// <param name="createDto">User preference creation details</param>
        /// <returns>Operation result</returns>
        Task<UserPreferenceResultDto> CreateUserPreferenceAsync(CreateUserPreferenceDto createDto);

        /// <summary>
        /// Gets a specific user preference by key
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="preferenceKey">Preference key</param>
        /// <returns>User preference or null if not found</returns>
        Task<UserPreferenceDto?> GetUserPreferenceAsync(int userId, string preferenceKey);

        /// <summary>
        /// Gets all preferences for a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of user preferences</returns>
        Task<List<UserPreferenceDto>> GetUserPreferencesAsync(int userId);

        /// <summary>
        /// Gets all user preferences with pagination
        /// </summary>
        /// <param name="request">Paged request parameters</param>
        /// <returns>Paged user preferences</returns>
        Task<PagedUserPreferencesResponseDto> GetAllUserPreferencesAsync(PagedUserPreferencesRequestDto request);

        /// <summary>
        /// Checks if a user preference exists
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="preferenceKey">Preference key</param>
        /// <returns>True if exists, false otherwise</returns>
        Task<bool> CheckUserPreferenceExistsAsync(int userId, string preferenceKey);

        /// <summary>
        /// Updates a user preference
        /// </summary>
        /// <param name="updateDto">Update details</param>
        /// <returns>Operation result</returns>
        Task<UserPreferenceResultDto> UpdateUserPreferenceAsync(UpdateUserPreferenceDto updateDto);

        /// <summary>
        /// Bulk updates user preferences
        /// </summary>
        /// <param name="bulkUpdateDto">Bulk update details</param>
        /// <returns>Operation result</returns>
        Task<BulkUserPreferenceResultDto> BulkUpdateUserPreferencesAsync(BulkUpdateUserPreferencesDto bulkUpdateDto);

        /// <summary>
        /// Deletes a specific user preference
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="preferenceKey">Preference key</param>
        /// <returns>Operation result</returns>
        Task<UserPreferenceResultDto> DeleteUserPreferenceAsync(int userId, string preferenceKey);

        /// <summary>
        /// Deletes all preferences for a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Operation result with deletion count</returns>
        Task<DeleteAllUserPreferencesResultDto> DeleteAllUserPreferencesAsync(int userId);

        /// <summary>
        /// Gets user preferences by key pattern
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="keyPattern">Key pattern to search for</param>
        /// <returns>List of matching user preferences</returns>
        Task<List<UserPreferenceDto>> GetUserPreferencesByPatternAsync(int userId, string keyPattern);

        /// <summary>
        /// Gets user preferences summary
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>User preferences summary</returns>
        Task<UserPreferencesSummaryDto?> GetUserPreferencesSummaryAsync(int userId);

        /// <summary>
        /// Gets users with a specific preference
        /// </summary>
        /// <param name="preferenceKey">Preference key</param>
        /// <param name="preferenceValue">Optional preference value filter</param>
        /// <returns>List of users with the preference</returns>
        Task<List<UserWithPreferenceDto>> GetUsersWithPreferenceAsync(string preferenceKey, string? preferenceValue = null);
    }
}
