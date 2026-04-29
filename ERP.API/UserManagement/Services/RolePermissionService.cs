using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using ERP.API.UserManagement.DTOs;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace ERP.API.UserManagement.Services
{
    public class RolePermissionService : IRolePermissionService
    {
        private readonly string _connectionString;
        private readonly ILogger<RolePermissionService>? _logger;

        public RolePermissionService(string connectionString, ILogger<RolePermissionService>? logger = null)
        {
            _connectionString = connectionString;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<(bool success, string message)> AssignPermissionToRoleAsync(AssignPermissionDto dto)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                var parameters = new
                {
                    p_roleid = dto.RoleId,
                    p_permissionid = dto.PermissionId,
                    p_assignedby = dto.AssignedBy
                };

                var result = await connection.QueryFirstAsync<dynamic>(
                    "SELECT * FROM sp_um_assign_permission_to_role(@p_roleid, @p_permissionid, @p_assignedby)",
                    parameters);

                return (result.success, result.message?.ToString() ?? string.Empty);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error assigning permission {PermissionId} to role {RoleId}", dto.PermissionId, dto.RoleId);
                return (false, ex.Message);
            }
        }

        /// <inheritdoc />
        public async Task<BatchAssignResultDto> AssignPermissionsToRoleAsync(BatchAssignPermissionsDto dto)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                var parameters = new
                {
                    p_roleid = dto.RoleId,
                    p_permissionids = dto.PermissionIds.ToArray(),
                    p_assignedby = dto.AssignedBy
                };

                var result = await connection.QueryFirstAsync<dynamic>(
                    "SELECT * FROM sp_um_assign_permissions_to_role(@p_roleid, @p_permissionids, @p_assignedby)",
                    parameters);

                return new BatchAssignResultDto
                {
                    Success = result.success,
                    Message = result.message?.ToString() ?? string.Empty,
                    AssignedCount = Convert.ToInt32(result.assigned_count),
                    FailedCount = Convert.ToInt32(result.failed_count)
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error batch assigning permissions to role {RoleId}", dto.RoleId);
                return new BatchAssignResultDto
                {
                    Success = false,
                    Message = ex.Message,
                    AssignedCount = 0,
                    FailedCount = dto.PermissionIds.Count
                };
            }
        }

        /// <inheritdoc />
        public async Task<(bool success, string message)> RevokePermissionFromRoleAsync(RevokePermissionDto dto)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                var parameters = new
                {
                    p_roleid = dto.RoleId,
                    p_permissionid = dto.PermissionId
                };

                var result = await connection.QueryFirstAsync<dynamic>(
                    "SELECT * FROM sp_um_revoke_permission_from_role(@p_roleid, @p_permissionid)",
                    parameters);

                return (result.success, result.message?.ToString() ?? string.Empty);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error revoking permission {PermissionId} from role {RoleId}", dto.PermissionId, dto.RoleId);
                return (false, ex.Message);
            }
        }

        /// <inheritdoc />
        public async Task<RevokeAllPermissionsResultDto> RevokeAllPermissionsFromRoleAsync(int roleId)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                var parameters = new
                {
                    p_roleid = roleId
                };

                var result = await connection.QueryFirstAsync<dynamic>(
                    "SELECT * FROM sp_um_revoke_all_permissions_from_role(@p_roleid)",
                    parameters);

                return new RevokeAllPermissionsResultDto
                {
                    Success = result.success,
                    Message = result.message?.ToString() ?? string.Empty,
                    RevokedCount = Convert.ToInt32(result.revoked_count)
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error revoking all permissions from role {RoleId}", roleId);
                return new RevokeAllPermissionsResultDto
                {
                    Success = false,
                    Message = ex.Message,
                    RevokedCount = 0
                };
            }
        }

        /// <inheritdoc />
        public async Task<List<RolePermissionDto>> GetRolePermissionsAsync(int roleId)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                var parameters = new
                {
                    p_roleid = roleId
                };

                var permissions = await connection.QueryAsync<RolePermissionDto>(
                    "SELECT * FROM sp_um_get_role_permissions(@p_roleid)",
                    parameters);

                return permissions.ToList();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting permissions for role {RoleId}", roleId);
                return new List<RolePermissionDto>();
            }
        }

        /// <inheritdoc />
        public async Task<List<PermissionRoleDto>> GetPermissionRolesAsync(int permissionId)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                var parameters = new
                {
                    p_permissionid = permissionId
                };

                var roles = await connection.QueryAsync<PermissionRoleDto>(
                    "SELECT * FROM sp_um_get_permission_roles(@p_permissionid)",
                    parameters);

                return roles.ToList();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting roles for permission {PermissionId}", permissionId);
                return new List<PermissionRoleDto>();
            }
        }

        /// <inheritdoc />
        public async Task<bool> RoleHasPermissionAsync(int roleId, int permissionId)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                var parameters = new
                {
                    p_roleid = roleId,
                    p_permissionid = permissionId
                };

                return await connection.QueryFirstOrDefaultAsync<bool>(
                    "SELECT * FROM sp_um_role_has_permission(@p_roleid, @p_permissionid)",
                    parameters);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error checking if role {RoleId} has permission {PermissionId}", roleId, permissionId);
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<bool> RoleHasPermissionByNameAsync(int roleId, string permissionName)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                var parameters = new
                {
                    p_roleid = roleId,
                    p_permissionname = permissionName
                };

                return await connection.QueryFirstOrDefaultAsync<bool>(
                    "SELECT * FROM sp_um_role_has_permission_by_name(@p_roleid, @p_permissionname)",
                    parameters);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error checking if role {RoleId} has permission {PermissionName}", roleId, permissionName);
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<List<PermissionDto>> GetUnassignedPermissionsForRoleAsync(int roleId, bool? isActive = true)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                var parameters = new
                {
                    p_roleid = roleId,
                    p_is_active = isActive
                };

                var permissions = await connection.QueryAsync<PermissionDto>(
                    "SELECT * FROM sp_um_get_unassigned_permissions_for_role(@p_roleid, @p_is_active)",
                    parameters);

                return permissions.ToList();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting unassigned permissions for role {RoleId}", roleId);
                return new List<PermissionDto>();
            }
        }

        /// <inheritdoc />
        public async Task<SyncResultDto> SyncRolePermissionsAsync(SyncRolePermissionsDto dto)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                var parameters = new
                {
                    p_roleid = dto.RoleId,
                    p_permissionids = dto.PermissionIds.ToArray(),
                    p_assignedby = dto.AssignedBy
                };

                var result = await connection.QueryFirstAsync<dynamic>(
                    "SELECT * FROM sp_um_sync_role_permissions(@p_roleid, @p_permissionids, @p_assignedby)",
                    parameters);

                return new SyncResultDto
                {
                    Success = result.success,
                    Message = result.message?.ToString() ?? string.Empty,
                    AddedCount = Convert.ToInt32(result.added_count),
                    RemovedCount = Convert.ToInt32(result.removed_count),
                    UnchangedCount = Convert.ToInt32(result.unchanged_count)
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error syncing permissions for role {RoleId}", dto.RoleId);
                return new SyncResultDto
                {
                    Success = false,
                    Message = ex.Message,
                    AddedCount = 0,
                    RemovedCount = 0,
                    UnchangedCount = 0
                };
            }
        }
        
        public async Task<List<RolePermissionDto>> GetAllRolesWithPermissionsAsync()
{
    try
    {
        using var connection = new Npgsql.NpgsqlConnection(_connectionString);
        var result = (await connection.QueryAsync<RolePermissionDto>(
            "SELECT * FROM sp_um_get_all_roles_with_permissions()")).ToList();
        return result;
    }
    catch (Exception ex)
    {
        _logger?.LogError(ex, "Error getting roles with permissions");
        return new List<RolePermissionDto>();
    }
}

        /// <inheritdoc />
        public async Task<RolePermissionStatisticsDto> GetRolePermissionsStatisticsAsync()
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);

                var stats = await connection.QueryFirstOrDefaultAsync<RolePermissionStatisticsDto>(
                    "SELECT * FROM sp_um_get_role_permissions_statistics()");

                return stats ?? new RolePermissionStatisticsDto();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting role permission statistics");
                return new RolePermissionStatisticsDto();
            }
        }
    }
}
