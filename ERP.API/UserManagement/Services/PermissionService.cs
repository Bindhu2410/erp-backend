using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Dapper;
using ERP.API.UserManagement.DTOs;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace ERP.API.UserManagement.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly string _connectionString;
        private readonly ILogger<PermissionService>? _logger;

        public PermissionService(string connectionString, ILogger<PermissionService>? logger = null)
        {
            _connectionString = connectionString;
            _logger = logger;
        }

        public async Task<(bool success, string message, int permissionId)> CreatePermissionAsync(CreatePermissionDto dto)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("p_permissionname", dto.PermissionName);
                    parameters.Add("p_description", dto.Description);
                    parameters.Add("p_category", dto.Category);

                    var result = await connection.QueryFirstAsync<dynamic>(
                        "SELECT * FROM sp_um_create_permission(@p_permissionname, @p_description, @p_category)",
                        parameters);

                    return (result.success, result.message?.ToString() ?? "", Convert.ToInt32(result.permissionid));
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error creating permission");
                return (false, ex.Message, 0);
            }
        }

        public async Task<PermissionDto?> GetPermissionByIdAsync(int permissionId)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    return await connection.QueryFirstOrDefaultAsync<PermissionDto>(
                        "SELECT * FROM sp_um_get_permission_by_id(@p_permissionid)",
                        new { p_permissionid = permissionId });
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting permission by ID {PermissionId}", permissionId);
                return null;
            }
        }

        public async Task<PermissionDto?> GetPermissionByNameAsync(string permissionName)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    return await connection.QueryFirstOrDefaultAsync<PermissionDto>(
                        "SELECT * FROM sp_um_get_permission_by_name(@p_permissionname)",
                        new { p_permissionname = permissionName });
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting permission by name {PermissionName}", permissionName);
                return null;
            }
        }

        // public async Task<PaginatedPermissionsDto> GetAllPermissionsAsync(int pageNumber = 1, int pageSize = 10, 
        //     string? searchTerm = null, string? category = null, bool? isActive = null)
        // {
        //     try
        //     {
        //         using (var connection = new NpgsqlConnection(_connectionString))
        //         {
        //             var parameters = new DynamicParameters();
        //             parameters.Add("p_page_number", pageNumber);
        //             parameters.Add("p_page_size", pageSize);
        //             parameters.Add("p_search_term", searchTerm);
        //             parameters.Add("p_category", category);
        //             parameters.Add("p_is_active", isActive);

        //             var permissions = await connection.QueryAsync<PermissionListDto>(
        //                 "SELECT * FROM sp_um_get_all_permissions(@p_page_number, @p_page_size, @p_search_term, @p_category, @p_is_active)",
        //                 parameters);

        //             var permissionsList = permissions.ToList();
        //             long totalCount = permissionsList.Any() ? permissionsList.First().TotalCount : 0;

        //             return new PaginatedPermissionsDto
        //             {
        //                 Permissions = permissionsList,
        //                 TotalCount = totalCount,
        //                 PageNumber = pageNumber,
        //                 PageSize = pageSize
        //             };
        //         }
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger?.LogError(ex, "Error getting permissions");
        //         return new PaginatedPermissionsDto
        //         {
        //             Permissions = new List<PermissionListDto>(),
        //             TotalCount = 0,
        //             PageNumber = pageNumber,
        //             PageSize = pageSize
        //         };
        //     }
        // }

        public async Task<PaginatedPermissionsDto> GetAllPermissionsAsync(
    int pageNumber = 1, int pageSize = 10, string? searchTerm = null, string? category = null, bool? isActive = null)
{
    try
    {
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            var parameters = new DynamicParameters();
            parameters.Add("p_page_number", pageNumber);
            parameters.Add("p_page_size", pageSize);
            parameters.Add("p_search_term", searchTerm);
            parameters.Add("p_category", category);
            parameters.Add("p_is_active", isActive);

            var permissions = (await connection.QueryAsync<PermissionListDto>(
                "SELECT * FROM sp_um_get_all_permissions(@p_page_number, @p_page_size, @p_search_term, @p_category, @p_is_active)",
                parameters)).ToList();

            long totalCount = permissions.FirstOrDefault()?.TotalCount ?? 0;

            return new PaginatedPermissionsDto
            {
                Permissions = permissions,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
    }
    catch (Exception ex)
    {
        _logger?.LogError(ex, "Error getting permissions");
        return new PaginatedPermissionsDto
        {
            Permissions = new List<PermissionListDto>(),
            TotalCount = 0,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }
}



        public async Task<List<PermissionDto>> GetPermissionsByCategoryAsync(string category, bool? isActive = true)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("p_category", category);
                    parameters.Add("p_is_active", isActive);

                    var permissions = await connection.QueryAsync<PermissionDto>(
                        "SELECT * FROM sp_um_get_permissions_by_category(@p_category, @p_is_active)",
                        parameters);

                    return permissions.ToList();
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting permissions by category {Category}", category);
                return new List<PermissionDto>();
            }
        }

        public async Task<List<string>> GetPermissionCategoriesAsync()
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    var categories = await connection.QueryAsync<string>(
                        "SELECT * FROM sp_um_get_permission_categories()");

                    return categories.ToList();
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting permission categories");
                return new List<string>();
            }
        }

        public async Task<(bool success, string message)> UpdatePermissionAsync(int permissionId, UpdatePermissionDto dto)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("p_permissionid", permissionId);
                    parameters.Add("p_permissionname", dto.PermissionName);
                    parameters.Add("p_description", dto.Description);
                    parameters.Add("p_category", dto.Category);
                    parameters.Add("p_isactive", dto.IsActive);

                    var result = await connection.QueryFirstAsync<dynamic>(
                        "SELECT success, message FROM sp_um_update_permission(@p_permissionid, @p_permissionname, @p_description, @p_category, @p_isactive)",
                        parameters);

                    return (result.success, result.message?.ToString() ?? "");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error updating permission {PermissionId}", permissionId);
                return (false, ex.Message);
            }
        }

        public async Task<(bool success, string message)> SoftDeletePermissionAsync(int permissionId)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    var result = await connection.QueryFirstAsync<dynamic>(
                        "SELECT success, message FROM sp_um_soft_delete_permission(@p_permissionid)",
                        new { p_permissionid = permissionId });

                    return (result.success, result.message?.ToString() ?? "");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error deactivating permission {PermissionId}", permissionId);
                return (false, ex.Message);
            }
        }

        public async Task<(bool success, string message)> HardDeletePermissionAsync(int permissionId)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    var result = await connection.QueryFirstAsync<dynamic>(
                        "SELECT success, message FROM sp_um_hard_delete_permission(@p_permissionid)",
                        new { p_permissionid = permissionId });

                    return (result.success, result.message?.ToString() ?? "");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error deleting permission {PermissionId}", permissionId);
                return (false, ex.Message);
            }
        }

        public async Task<PermissionStatisticsDto> GetPermissionStatisticsAsync()
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    var result = await connection.QueryFirstOrDefaultAsync<PermissionStatisticsDto>(
                        "SELECT * FROM sp_um_get_permissions_statistics()");

                    return result ?? new PermissionStatisticsDto();
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting permission statistics");
                return new PermissionStatisticsDto();
            }
        }

        public async Task<BatchCreatePermissionsResultDto> BatchCreatePermissionsAsync(BatchCreatePermissionsDto dto)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    // Convert the list of DTOs to a JSON array
                    var permissionsJson = JsonSerializer.Serialize(dto.Permissions.Select(p => new
                    {
                        permissionname = p.PermissionName,
                        description = p.Description,
                        category = p.Category
                    }));

                    var result = await connection.QueryFirstOrDefaultAsync<BatchCreatePermissionsResultDto>(
                        "SELECT success, message, created_count AS CreatedCount, failed_count AS FailedCount FROM sp_um_batch_create_permissions(@p_permissions::jsonb)",
                        new { p_permissions = permissionsJson });

                    return result ?? new BatchCreatePermissionsResultDto 
                    { 
                        Success = false, 
                        Message = "Error processing batch create permissions",
                        CreatedCount = 0,
                        FailedCount = dto.Permissions.Count
                    };
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error batch creating permissions");
                return new BatchCreatePermissionsResultDto 
                { 
                    Success = false, 
                    Message = ex.Message,
                    CreatedCount = 0,
                    FailedCount = dto.Permissions.Count
                };
            }
        }
    }
}
