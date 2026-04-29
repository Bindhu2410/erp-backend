using Dapper;
using ERP.API.UserManagement.DTOs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ERP.API.UserManagement.Services
{
    public class RoleService : IRoleService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<RoleService> _logger;
        private readonly string _connectionString;

        public RoleService(IConfiguration configuration, ILogger<RoleService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
        }

        /// <summary>
        /// Creates a new role
        /// </summary>
        public async Task<(bool Success, string Message, int RoleId)> CreateRoleAsync(CreateRoleDto roleDto)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var parameters = new DynamicParameters();
                    parameters.Add("p_rolename", roleDto.RoleName);
                    // Now expects permission names (string[]), not IDs
                    parameters.Add("p_permission_names", roleDto.PermissionNames?.ToArray() ?? new string[0]);
                    parameters.Add("p_description", roleDto.Description);
                    parameters.Add("p_issystemrole", roleDto.IsSystemRole);
                    parameters.Add("p_createdby", roleDto.CreatedBy);

                    var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                        "SELECT * FROM sp_um_create_role(@p_rolename, @p_permission_names, @p_description, @p_issystemrole, @p_createdby)",
                parameters);

                    return (result.success, result.message, result.roleid);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating role {RoleName}", roleDto.RoleName);
                return (false, $"Error creating role: {ex.Message}", 0);
            }
        }

        /// <summary>
        /// Gets a role by ID
        /// </summary>
        public async Task<RoleDto> GetRoleByIdAsync(int roleId)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var parameters = new DynamicParameters();
                    parameters.Add("p_roleid", roleId);

                    var role = await connection.QueryFirstOrDefaultAsync<RoleDto>(
                        "SELECT roleid as RoleId, rolename as RoleName, description as Description, " +
                        "issystemrole as IsSystemRole, datecreated as DateCreated, createdby as CreatedBy, " +
                        "isactive as IsActive, createdby_username as CreatedByUsername " +
                        "FROM sp_um_get_role_by_id(@p_roleid)",
                        parameters);

                    return role;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting role by ID {RoleId}", roleId);
                return null;
            }
        }

        /// <summary>
        /// Gets a role by name
        /// </summary>
        public async Task<RoleDto> GetRoleByNameAsync(string roleName)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var parameters = new DynamicParameters();
                    parameters.Add("p_rolename", roleName);

                    var role = await connection.QueryFirstOrDefaultAsync<RoleDto>(
                        "SELECT roleid as RoleId, rolename as RoleName, description as Description, " +
                        "issystemrole as IsSystemRole, datecreated as DateCreated, createdby as CreatedBy, " +
                        "isactive as IsActive, createdby_username as CreatedByUsername " +
                        "FROM sp_um_get_role_by_name(@p_rolename)",
                        parameters);

                    return role;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting role by name {RoleName}", roleName);
                return null;
            }
        }

        /// <summary>
        /// Gets a paginated list of roles with optional filtering
        /// </summary>
        public async Task<PaginatedRolesDto> GetAllRolesAsync(int pageNumber = 1, int pageSize = 10,
            string searchTerm = null, bool? isActive = null, bool? isSystemRole = null)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var parameters = new DynamicParameters();
                    parameters.Add("p_page_number", pageNumber);
                    parameters.Add("p_page_size", pageSize);
                    parameters.Add("p_search_term", searchTerm);
                    parameters.Add("p_is_active", isActive);
                    parameters.Add("p_is_system_role", isSystemRole);

                    var roles = await connection.QueryAsync<RoleListDto>(
                        "SELECT roleid as RoleId, rolename as RoleName, description as Description, " +
                        "issystemrole as IsSystemRole, datecreated as DateCreated, createdby as CreatedBy, " +
                        "isactive as IsActive, createdby_username as CreatedByUsername, total_count as TotalCount " +
                        "FROM sp_um_get_all_roles(@p_page_number, @p_page_size, @p_search_term, @p_is_active, @p_is_system_role)",
                        parameters);

                    var rolesList = roles.ToList();
                    var totalCount = rolesList.Any() ? rolesList.First().TotalCount : 0;

                    return new PaginatedRolesDto
                    {
                        Roles = rolesList,
                        TotalCount = totalCount,
                        PageNumber = pageNumber,
                        PageSize = pageSize
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting roles list");
                return new PaginatedRolesDto
                {
                    Roles = new List<RoleListDto>(),
                    TotalCount = 0,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
            }
        }

        /// <summary>
        /// Updates a role
        /// </summary>
        public async Task<(bool Success, string Message, int RoleId)> UpdateRoleAsync(int roleId, UpdateRoleDto roleDto)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var parameters = new DynamicParameters();
                    parameters.Add("p_roleid", roleId);
                    parameters.Add("p_rolename", roleDto.RoleName);
                    parameters.Add("p_description", roleDto.Description);
                    parameters.Add("p_issystemrole", roleDto.IsSystemRole);
                    parameters.Add("p_isactive", roleDto.IsActive);

                    var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                        "SELECT * FROM sp_um_update_role(@p_roleid, @p_rolename, @p_description, @p_issystemrole, @p_isactive)",
                        parameters);

                    return (result.success, result.message, result.updated_roleid);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating role {RoleId}", roleId);
                return (false, $"Error updating role: {ex.Message}", 0);
            }
        }

        /// <summary>
        /// Soft deletes (deactivates) a role
        /// </summary>
        public async Task<(bool Success, string Message)> SoftDeleteRoleAsync(int roleId)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var parameters = new DynamicParameters();
                    parameters.Add("p_roleid", roleId);

                    var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                        "SELECT * FROM sp_um_soft_delete_role(@p_roleid)",
                        parameters);

                    return (result.success, result.message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error soft deleting role {RoleId}", roleId);
                return (false, $"Error deactivating role: {ex.Message}");
            }
        }

        /// <summary>
        /// Hard deletes (permanently removes) a role
        /// </summary>
        public async Task<(bool Success, string Message)> HardDeleteRoleAsync(int roleId)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var parameters = new DynamicParameters();
                    parameters.Add("p_roleid", roleId);

                    var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                        "SELECT * FROM sp_um_hard_delete_role(@p_roleid)",
                        parameters);

                    return (result.success, result.message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error hard deleting role {RoleId}", roleId);
                return (false, $"Error deleting role: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets statistics about roles
        /// </summary>
        public async Task<RoleStatisticsDto> GetRoleStatisticsAsync()
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var stats = await connection.QueryFirstOrDefaultAsync<RoleStatisticsDto>(
                        "SELECT total_roles as TotalRoles, active_roles as ActiveRoles, " +
                        "inactive_roles as InactiveRoles, system_roles as SystemRoles, " +
                        "custom_roles as CustomRoles FROM sp_um_get_roles_statistics()");

                    return stats;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting role statistics");
                return new RoleStatisticsDto
                {
                    TotalRoles = 0,
                    ActiveRoles = 0,
                    InactiveRoles = 0,
                    SystemRoles = 0,
                    CustomRoles = 0
                };
            }
        }
    }
}
