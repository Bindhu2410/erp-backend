using ERP.API.UserManagement.DTOs;

namespace ERP.API.UserManagement.Services
{
    /// <summary>
    /// Interface for access delegation management operations
    /// </summary>
    public interface IAccessDelegationService
    {
        /// <summary>
        /// Creates a new access delegation
        /// </summary>
        /// <param name="createDto">Delegation creation details</param>
        /// <returns>Created delegation ID</returns>
        Task<int> CreateDelegationAsync(CreateAccessDelegationDto createDto);

        /// <summary>
        /// Gets an access delegation by ID
        /// </summary>
        /// <param name="delegationId">Delegation ID</param>
        /// <returns>Delegation details or null if not found</returns>
        Task<AccessDelegationDto?> GetDelegationByIdAsync(int delegationId);

        /// <summary>
        /// Updates an existing access delegation
        /// </summary>
        /// <param name="updateDto">Delegation update details</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> UpdateDelegationAsync(UpdateAccessDelegationDto updateDto);

        /// <summary>
        /// Deletes an access delegation
        /// </summary>
        /// <param name="delegationId">Delegation ID</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> DeleteDelegationAsync(int delegationId);

        /// <summary>
        /// Gets paginated list of all access delegations
        /// </summary>
        /// <param name="pageNumber">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Paginated delegation list</returns>
        Task<AccessDelegationPagedDto> GetDelegationsPagedAsync(int pageNumber = 1, int pageSize = 10);

        /// <summary>
        /// Gets active delegations for a specific user as delegator
        /// </summary>
        /// <param name="fromUserId">User ID who delegated</param>
        /// <returns>List of active delegations</returns>
        Task<List<AccessDelegationDto>> GetActiveDelegationsByFromUserAsync(int fromUserId);

        /// <summary>
        /// Gets active delegations for a specific user as delegate
        /// </summary>
        /// <param name="toUserId">User ID who received delegation</param>
        /// <returns>List of active delegations</returns>
        Task<List<AccessDelegationDto>> GetActiveDelegationsByToUserAsync(int toUserId);

        /// <summary>
        /// Gets paginated delegations by from user
        /// </summary>
        /// <param name="queryDto">Query parameters</param>
        /// <returns>Paginated delegation list</returns>
        Task<AccessDelegationPagedDto> GetDelegationsByFromUserPagedAsync(UserDelegationQueryDto queryDto);

        /// <summary>
        /// Gets paginated delegations by to user
        /// </summary>
        /// <param name="queryDto">Query parameters</param>
        /// <returns>Paginated delegation list</returns>
        Task<AccessDelegationPagedDto> GetDelegationsByToUserPagedAsync(UserDelegationQueryDto queryDto);

        /// <summary>
        /// Gets delegations by date range with pagination
        /// </summary>
        /// <param name="queryDto">Date range query parameters</param>
        /// <returns>Paginated delegation list</returns>
        Task<AccessDelegationPagedDto> GetDelegationsByDateRangePagedAsync(DateRangeQueryDto queryDto);

        /// <summary>
        /// Gets current active delegations with pagination
        /// </summary>
        /// <param name="pageNumber">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Paginated active delegation list</returns>
        Task<AccessDelegationPagedDto> GetCurrentActiveDelegationsAsync(int pageNumber = 1, int pageSize = 10);

        /// <summary>
        /// Deactivates expired delegations
        /// </summary>
        /// <returns>Number of delegations updated</returns>
        Task<int> DeactivateExpiredDelegationsAsync();

        /// <summary>
        /// Checks if a user has active delegation to another user
        /// </summary>
        /// <param name="fromUserId">Delegator user ID</param>
        /// <param name="toUserId">Delegate user ID</param>
        /// <returns>True if active delegation exists, false otherwise</returns>
        Task<bool> CheckUserHasActiveDelegationAsync(int fromUserId, int toUserId);

        /// <summary>
        /// Gets delegation history for a user (both as delegator and delegate)
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="pageNumber">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Paginated delegation history</returns>
        Task<DelegationHistoryPagedDto> GetDelegationHistoryByUserAsync(int userId, int pageNumber = 1, int pageSize = 10);

        /// <summary>
        /// Extends the end date of a delegation
        /// </summary>
        /// <param name="extendDto">Extension details</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> ExtendDelegationAsync(ExtendDelegationDto extendDto);

        /// <summary>
        /// Searches delegations with advanced filters
        /// </summary>
        /// <param name="searchDto">Search criteria</param>
        /// <returns>Paginated search results</returns>
        Task<AccessDelegationPagedDto> SearchDelegationsAsync(DelegationSearchDto searchDto);

        /// <summary>
        /// Gets delegation statistics
        /// </summary>
        /// <param name="queryDto">Statistics query parameters</param>
        /// <returns>Delegation statistics</returns>
        Task<DelegationStatisticsDto> GetDelegationStatisticsAsync(DelegationStatsQueryDto queryDto);

        /// <summary>
        /// Gets most active delegators
        /// </summary>
        /// <param name="queryDto">Query parameters</param>
        /// <returns>List of most active delegators</returns>
        Task<List<UserDelegationActivityDto>> GetMostActiveDelegatorsAsync(MostActiveUsersQueryDto queryDto);

        /// <summary>
        /// Gets most popular delegates
        /// </summary>
        /// <param name="queryDto">Query parameters</param>
        /// <returns>List of most popular delegates</returns>
        Task<List<UserDelegationActivityDto>> GetMostPopularDelegatesAsync(MostActiveUsersQueryDto queryDto);
    }
}
