using System.Collections.Generic;
using System.Threading.Tasks;
using ERP.API.UserManagement.DTOs;

namespace ERP.API.UserManagement.Services
{
    public interface IPermissionService
    {
        Task<(bool success, string message, int permissionId)> CreatePermissionAsync(CreatePermissionDto dto);
        Task<PermissionDto?> GetPermissionByIdAsync(int permissionId);
        Task<PermissionDto?> GetPermissionByNameAsync(string permissionName);
        
        
        Task<PaginatedPermissionsDto> GetAllPermissionsAsync(int pageNumber = 1, int pageSize = 10, 
            string? searchTerm = null, string? category = null, bool? isActive = null);
        Task<List<PermissionDto>> GetPermissionsByCategoryAsync(string category, bool? isActive = true);
        Task<List<string>> GetPermissionCategoriesAsync();
        Task<(bool success, string message)> UpdatePermissionAsync(int permissionId, UpdatePermissionDto dto);
        Task<(bool success, string message)> SoftDeletePermissionAsync(int permissionId);
        Task<(bool success, string message)> HardDeletePermissionAsync(int permissionId);
        Task<PermissionStatisticsDto> GetPermissionStatisticsAsync();
        Task<BatchCreatePermissionsResultDto> BatchCreatePermissionsAsync(BatchCreatePermissionsDto dto);
    }
}
