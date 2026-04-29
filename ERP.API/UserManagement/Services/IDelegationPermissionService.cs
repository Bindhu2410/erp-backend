using ERP.API.UserManagement.DTOs;

namespace ERP.API.UserManagement.Services
{
    /// <summary>
    /// Interface for Delegation Permission Service operations
    /// </summary>
    public interface IDelegationPermissionService
    {
        /// <summary>
        /// Creates a new delegation permission
        /// </summary>
        /// <param name="createDto">Delegation permission creation details</param>
        /// <returns>Operation result</returns>
        Task<DelegationPermissionResultDto> CreateDelegationPermissionAsync(CreateDelegationPermissionDto createDto);

        /// <summary>
        /// Gets permissions for a specific delegation
        /// </summary>
        /// <param name="delegationId">Delegation ID</param>
        /// <returns>List of delegation permissions</returns>
        Task<List<DelegationPermissionDto>> GetDelegationPermissionsAsync(int delegationId);

        /// <summary>
        /// Gets all delegation permissions with pagination
        /// </summary>
        /// <param name="request">Paged request parameters</param>
        /// <returns>Paged delegation permissions</returns>
        Task<PagedDelegationPermissionsResponseDto> GetAllDelegationPermissionsAsync(PagedDelegationPermissionsRequestDto request);

        /// <summary>
        /// Checks if a permission exists for a delegation
        /// </summary>
        /// <param name="delegationId">Delegation ID</param>
        /// <param name="permissionId">Permission ID</param>
        /// <returns>True if exists, false otherwise</returns>
        Task<bool> CheckDelegationPermissionExistsAsync(int delegationId, int permissionId);

        /// <summary>
        /// Bulk updates delegation permissions
        /// </summary>
        /// <param name="updateDto">Bulk update details</param>
        /// <returns>Operation result</returns>
        Task<DelegationPermissionResultDto> UpdateDelegationPermissionsAsync(UpdateDelegationPermissionsDto updateDto);

        /// <summary>
        /// Deletes a specific delegation permission
        /// </summary>
        /// <param name="delegationId">Delegation ID</param>
        /// <param name="permissionId">Permission ID</param>
        /// <returns>Operation result</returns>
        Task<DelegationPermissionResultDto> DeleteDelegationPermissionAsync(int delegationId, int permissionId);

        /// <summary>
        /// Deletes all permissions for a delegation
        /// </summary>
        /// <param name="delegationId">Delegation ID</param>
        /// <returns>Operation result with deletion count</returns>
        Task<DeleteAllDelegationPermissionsResultDto> DeleteAllDelegationPermissionsAsync(int delegationId);

        /// <summary>
        /// Gets available permissions for a delegation (not yet assigned)
        /// </summary>
        /// <param name="delegationId">Delegation ID</param>
        /// <returns>List of available permissions</returns>
        Task<List<AvailablePermissionDto>> GetAvailablePermissionsForDelegationAsync(int delegationId);

        /// <summary>
        /// Gets delegation permission summary
        /// </summary>
        /// <param name="delegationId">Delegation ID</param>
        /// <returns>Delegation permission summary</returns>
        Task<DelegationPermissionSummaryDto?> GetDelegationPermissionSummaryAsync(int delegationId);
    }
}
