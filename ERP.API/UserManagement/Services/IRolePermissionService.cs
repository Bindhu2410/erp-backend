using System.Collections.Generic;
using System.Threading.Tasks;
using ERP.API.UserManagement.DTOs;

namespace ERP.API.UserManagement.Services
{
    public interface IRolePermissionService
    {
        /// <summary>
        /// Assigns a single permission to a role
        /// </summary>
        Task<(bool success, string message)> AssignPermissionToRoleAsync(AssignPermissionDto dto);
        
        /// <summary>
        /// Assigns multiple permissions to a role
        /// </summary>
        Task<BatchAssignResultDto> AssignPermissionsToRoleAsync(BatchAssignPermissionsDto dto);
        
        /// <summary>
        /// Revokes a permission from a role
        /// </summary>
        Task<(bool success, string message)> RevokePermissionFromRoleAsync(RevokePermissionDto dto);
        
        /// <summary>
        /// Revokes all permissions from a role
        /// </summary>
        Task<RevokeAllPermissionsResultDto> RevokeAllPermissionsFromRoleAsync(int roleId);
        
        /// <summary>
        /// Gets all permissions assigned to a role
        /// </summary>
        Task<List<RolePermissionDto>> GetRolePermissionsAsync(int roleId);

        Task<List<RolePermissionDto>> GetAllRolesWithPermissionsAsync();
        
        /// <summary>
        /// Gets all roles that have a specific permission
        /// </summary>
        Task<List<PermissionRoleDto>> GetPermissionRolesAsync(int permissionId);
        
        /// <summary>
        /// Checks if a role has a specific permission by permission ID
        /// </summary>
        Task<bool> RoleHasPermissionAsync(int roleId, int permissionId);
        
        /// <summary>
        /// Checks if a role has a specific permission by permission name
        /// </summary>
        Task<bool> RoleHasPermissionByNameAsync(int roleId, string permissionName);
        
        /// <summary>
        /// Gets all permissions that are not assigned to a role
        /// </summary>
        Task<List<PermissionDto>> GetUnassignedPermissionsForRoleAsync(int roleId, bool? isActive = true);
        
        /// <summary>
        /// Syncs role permissions by replacing all current permissions with the provided list
        /// </summary>
        Task<SyncResultDto> SyncRolePermissionsAsync(SyncRolePermissionsDto dto);
        
        /// <summary>
        /// Gets statistics about role-permission assignments
        /// </summary>
        Task<RolePermissionStatisticsDto> GetRolePermissionsStatisticsAsync();
    }
}
