using ERP.API.UserManagement.DTOs;
using System.Threading.Tasks;

namespace ERP.API.UserManagement.Services
{
    public interface IRoleService
    {
        /// <summary>
        /// Creates a new role
        /// </summary>
        Task<(bool Success, string Message, int RoleId)> CreateRoleAsync(CreateRoleDto roleDto);
        
        /// <summary>
        /// Gets a role by ID
        /// </summary>
        Task<RoleDto> GetRoleByIdAsync(int roleId);
        
        /// <summary>
        /// Gets a role by name
        /// </summary>
        Task<RoleDto> GetRoleByNameAsync(string roleName);
        
        /// <summary>
        /// Gets a paginated list of roles with optional filtering
        /// </summary>
        Task<PaginatedRolesDto> GetAllRolesAsync(int pageNumber = 1, int pageSize = 10, 
            string searchTerm = null, bool? isActive = null, bool? isSystemRole = null);
        
        /// <summary>
        /// Updates a role
        /// </summary>
        Task<(bool Success, string Message, int RoleId)> UpdateRoleAsync(int roleId, UpdateRoleDto roleDto);
        
        /// <summary>
        /// Soft deletes (deactivates) a role
        /// </summary>
        Task<(bool Success, string Message)> SoftDeleteRoleAsync(int roleId);
        
        /// <summary>
        /// Hard deletes (permanently removes) a role
        /// </summary>
        Task<(bool Success, string Message)> HardDeleteRoleAsync(int roleId);
        
        /// <summary>
        /// Gets statistics about roles
        /// </summary>
        Task<RoleStatisticsDto> GetRoleStatisticsAsync();
    }
}
