using System.Collections.Generic;
using System.Threading.Tasks;
using ERP.API.UserManagement.DTOs;

namespace ERP.API.UserManagement.Services
{
    public interface IUserRoleService
    {
        /// <summary>
        /// Assigns a single role to a user
        /// </summary>
        Task<(bool success, string message)> AssignRoleToUserAsync(AssignRoleDto dto);

        Task<List<UserRoleAssignmentDto>> GetAllUserRolesAsync();

        Task<bool> UpdateUserRoleByIdAsync(UpdateUserRoleByIdDto dto);
        Task<bool> DeleteUserRoleByIdAsync(int id);
        
        /// <summary>
        /// Assigns multiple roles to a user
        /// </summary>
        Task<UserRoleBatchAssignResultDto> AssignRolesToUserAsync(AssignRolesToUserDto dto);
        
        Task<List<UnassignedUserDto>> GetAllUnassignedUsersAsync(bool? isActive = true);
        /// <summary>
        /// Assigns a role to multiple users
        /// </summary>
        Task<UserRoleBatchAssignResultDto> AssignRoleToUsersAsync(AssignRoleToUsersDto dto);
        
        /// <summary>
        /// Revokes a role from a user
        /// </summary>
        Task<(bool success, string message)> RevokeRoleFromUserAsync(RevokeRoleDto dto);
        
        /// <summary>
        /// Revokes all roles from a user
        /// </summary>
        Task<RevokeAllResultDto> RevokeAllRolesFromUserAsync(int userId);
        
        /// <summary>
        /// Revokes a role from all users
        /// </summary>
        Task<RevokeAllResultDto> RevokeRoleFromAllUsersAsync(int roleId);
        
        /// <summary>
        /// Gets all roles assigned to a user
        /// </summary>
        Task<List<UserRoleDto>> GetUserRolesAsync(int userId);
        
        /// <summary>
        /// Gets all users assigned to a role
        /// </summary>
        Task<List<RoleUserDto>> GetUsersWithRoleAsync(int roleId);
        
        /// <summary>
        /// Gets all users assigned to a role with pagination
        /// </summary>
        Task<RoleUsersResponseDto> GetUsersWithRolePaginatedAsync(int roleId, UserRoleQueryParametersDto parameters);
        
        /// <summary>
        /// Checks if a user has a specific role by role ID
        /// </summary>
        Task<bool> UserHasRoleAsync(int userId, int roleId);
        
        /// <summary>
        /// Checks if a user has a specific role by role name
        /// </summary>
        Task<bool> UserHasRoleByNameAsync(int userId, string roleName);
        
        /// <summary>
        /// Gets all roles that are not assigned to a user
        /// </summary>
        Task<List<RoleDto>> GetUnassignedRolesForUserAsync(int userId, bool? isActive = true);
        
        /// <summary>
        /// Gets all users that do not have a specific role
        /// </summary>
        Task<List<UserBasicDto>> GetUsersWithoutRoleAsync(int roleId, bool? isActive = true);
        
        /// <summary>
        /// Syncs user roles by replacing all current roles with the provided list
        /// </summary>
        Task<UserRoleSyncResultDto> SyncUserRolesAsync(SyncUserRolesDto dto);
        
        /// <summary>
        /// Gets statistics about user-role assignments
        /// </summary>
        Task<UserRoleStatisticsDto> GetUserRoleStatisticsAsync();
        
        /// <summary>
        /// Checks if a user has any of the specified permissions through their roles
        /// </summary>
        Task<bool> UserHasAnyPermissionAsync(int userId, List<int> permissionIds);
        
        /// <summary>
        /// Checks if a user has all of the specified permissions through their roles
        /// </summary>
        Task<bool> UserHasAllPermissionsAsync(int userId, List<int> permissionIds);
        
        /// <summary>
        /// Gets all permissions a user has through their roles
        /// </summary>
        Task<List<UserPermissionDto>> GetAllUserPermissionsAsync(int userId);
    }
}
