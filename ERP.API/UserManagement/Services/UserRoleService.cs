using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using ERP.API.UserManagement.DTOs;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace ERP.API.UserManagement.Services
{
    public class UserRoleService : IUserRoleService
    {
        private readonly string _connectionString;
        private readonly ILogger<UserRoleService>? _logger;

        public UserRoleService(string connectionString, ILogger<UserRoleService>? logger = null)
        {
            _connectionString = connectionString;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<(bool success, string message)> AssignRoleToUserAsync(AssignRoleDto dto)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                var parameters = new
                {
                    p_userid = dto.UserId,
                    p_roleid = dto.RoleId,
                    p_assignedby = dto.AssignedBy
                };

                var result = await connection.QueryFirstAsync<dynamic>(
                    "SELECT * FROM sp_um_assign_role_to_user(@p_userid, @p_roleid, @p_assignedby)",
                    parameters);

                return (result.success, result.message?.ToString() ?? string.Empty);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error assigning role {RoleId} to user {UserId}", dto.RoleId, dto.UserId);
                return (false, ex.Message);
            }
        }

        /// <inheritdoc />
        public async Task<UserRoleBatchAssignResultDto> AssignRolesToUserAsync(AssignRolesToUserDto dto)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                var parameters = new
                {
                    p_userid = dto.UserId,
                    p_roleids = dto.RoleIds.ToArray(),
                    p_assignedby = dto.AssignedBy
                };

                var result = await connection.QueryFirstAsync<dynamic>(
                    "SELECT * FROM sp_um_assign_roles_to_user(@p_userid, @p_roleids, @p_assignedby)",
                    parameters);

                return new UserRoleBatchAssignResultDto
                {
                    Success = result.success,
                    Message = result.message?.ToString() ?? string.Empty,
                    AssignedCount = Convert.ToInt32(result.assigned_count),
                    FailedCount = Convert.ToInt32(result.failed_count)
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error batch assigning roles to user {UserId}", dto.UserId);
                return new UserRoleBatchAssignResultDto
                {
                    Success = false,
                    Message = ex.Message,
                    AssignedCount = 0,
                    FailedCount = dto.RoleIds.Count
                };
            }
        }

        /// <inheritdoc />
        public async Task<UserRoleBatchAssignResultDto> AssignRoleToUsersAsync(AssignRoleToUsersDto dto)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                var parameters = new
                {
                    p_userids = dto.UserIds.ToArray(),
                    p_roleid = dto.RoleId,
                    p_assignedby = dto.AssignedBy
                };

                var result = await connection.QueryFirstAsync<dynamic>(
                    "SELECT * FROM sp_um_assign_role_to_users(@p_userids, @p_roleid, @p_assignedby)",
                    parameters);

                return new UserRoleBatchAssignResultDto
                {
                    Success = result.success,
                    Message = result.message?.ToString() ?? string.Empty,
                    AssignedCount = Convert.ToInt32(result.assigned_count),
                    FailedCount = Convert.ToInt32(result.failed_count)
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error batch assigning role {RoleId} to users", dto.RoleId);
                return new UserRoleBatchAssignResultDto
                {
                    Success = false,
                    Message = ex.Message,
                    AssignedCount = 0,
                    FailedCount = dto.UserIds.Count
                };
            }
        }


        public async Task<List<UserRoleAssignmentDto>> GetAllUserRolesAsync()
        {
            try
            {
                using var connection = new Npgsql.NpgsqlConnection(_connectionString);
                var result = (await connection.QueryAsync<UserRoleAssignmentDto>(
                    "SELECT * FROM sp_um_get_all_user_roles()")).ToList();
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting all user roles");
                return new List<UserRoleAssignmentDto>();
            }
        }
        /// <inheritdoc />
        public async Task<(bool success, string message)> RevokeRoleFromUserAsync(RevokeRoleDto dto)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                var parameters = new
                {
                    p_userid = dto.UserId,
                    p_roleid = dto.RoleId
                };

                var result = await connection.QueryFirstAsync<dynamic>(
                    "SELECT * FROM sp_um_revoke_role_from_user(@p_userid, @p_roleid)",
                    parameters);

                return (result.success, result.message?.ToString() ?? string.Empty);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error revoking role {RoleId} from user {UserId}", dto.RoleId, dto.UserId);
                return (false, ex.Message);
            }
        }

        /// <inheritdoc />
        public async Task<RevokeAllResultDto> RevokeAllRolesFromUserAsync(int userId)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                var parameters = new
                {
                    p_userid = userId
                };

                var result = await connection.QueryFirstAsync<dynamic>(
                    "SELECT * FROM sp_um_revoke_all_roles_from_user(@p_userid)",
                    parameters);

                return new RevokeAllResultDto
                {
                    Success = result.success,
                    Message = result.message?.ToString() ?? string.Empty,
                    RevokedCount = Convert.ToInt32(result.revoked_count)
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error revoking all roles from user {UserId}", userId);
                return new RevokeAllResultDto
                {
                    Success = false,
                    Message = ex.Message,
                    RevokedCount = 0
                };
            }
        }

        /// <inheritdoc />
        public async Task<RevokeAllResultDto> RevokeRoleFromAllUsersAsync(int roleId)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                var parameters = new
                {
                    p_roleid = roleId
                };

                var result = await connection.QueryFirstAsync<dynamic>(
                    "SELECT * FROM sp_um_revoke_role_from_all_users(@p_roleid)",
                    parameters);

                return new RevokeAllResultDto
                {
                    Success = result.success,
                    Message = result.message?.ToString() ?? string.Empty,
                    RevokedCount = Convert.ToInt32(result.revoked_count)
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error revoking role {RoleId} from all users", roleId);
                return new RevokeAllResultDto
                {
                    Success = false,
                    Message = ex.Message,
                    RevokedCount = 0
                };
            }
        }

        /// <inheritdoc />
        public async Task<List<UserRoleDto>> GetUserRolesAsync(int userId)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                var parameters = new
                {
                    p_userid = userId
                };

                var roles = await connection.QueryAsync<UserRoleDto>(
                    "SELECT * FROM sp_um_get_user_roles(@p_userid)",
                    parameters);

                return roles.ToList();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting roles for user {UserId}", userId);
                return new List<UserRoleDto>();
            }
        }

        /// <inheritdoc />
        public async Task<List<RoleUserDto>> GetUsersWithRoleAsync(int roleId)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                var parameters = new
                {
                    p_roleid = roleId
                };

                var users = await connection.QueryAsync<RoleUserDto>(
                    "SELECT * FROM sp_um_get_users_with_role(@p_roleid)",
                    parameters);

                return users.ToList();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting users with role {RoleId}", roleId);
                return new List<RoleUserDto>();
            }
        }

        /// <inheritdoc />
        public async Task<RoleUsersResponseDto> GetUsersWithRolePaginatedAsync(int roleId, UserRoleQueryParametersDto parameters)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                var queryParams = new
                {
                    p_roleid = roleId,
                    p_page_number = parameters.PageNumber,
                    p_page_size = parameters.PageSize,
                    p_search_term = parameters.SearchTerm,
                    p_is_active = parameters.IsActive
                };

                var users = await connection.QueryAsync<RoleUserDto>(
                    "SELECT * FROM sp_um_get_users_with_role_paginated(@p_roleid, @p_page_number, @p_page_size, @p_search_term, @p_is_active)",
                    queryParams);

                var usersList = users.ToList();
                // Get total count from a separate property or query
                int totalCount = 0;

                if (usersList.Any() && users.First() is IDictionary<string, object> firstRow)
                {
                    if (firstRow.TryGetValue("total_count", out object? value) && value != null)
                    {
                        totalCount = Convert.ToInt32(value);
                    }
                }

                return new RoleUsersResponseDto
                {
                    Users = usersList,
                    TotalCount = totalCount,
                    PageNumber = parameters.PageNumber,
                    PageSize = parameters.PageSize
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting users with role {RoleId} with pagination", roleId);
                return new RoleUsersResponseDto
                {
                    Users = new List<RoleUserDto>(),
                    TotalCount = 0,
                    PageNumber = parameters.PageNumber,
                    PageSize = parameters.PageSize
                };
            }
        }

        /// <inheritdoc />
        public async Task<bool> UserHasRoleAsync(int userId, int roleId)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                var parameters = new
                {
                    p_userid = userId,
                    p_roleid = roleId
                };

                return await connection.QueryFirstOrDefaultAsync<bool>(
                    "SELECT * FROM sp_um_user_has_role(@p_userid, @p_roleid)",
                    parameters);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error checking if user {UserId} has role {RoleId}", userId, roleId);
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<bool> UserHasRoleByNameAsync(int userId, string roleName)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                var parameters = new
                {
                    p_userid = userId,
                    p_rolename = roleName
                };

                return await connection.QueryFirstOrDefaultAsync<bool>(
                    "SELECT * FROM sp_um_user_has_role_by_name(@p_userid, @p_rolename)",
                    parameters);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error checking if user {UserId} has role {RoleName}", userId, roleName);
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<List<RoleDto>> GetUnassignedRolesForUserAsync(int userId, bool? isActive = true)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                var parameters = new
                {
                    p_userid = userId,
                    p_is_active = isActive
                };

                var roles = await connection.QueryAsync<RoleDto>(
                    "SELECT * FROM sp_um_get_unassigned_roles_for_user(@p_userid, @p_is_active)",
                    parameters);

                return roles.ToList();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting unassigned roles for user {UserId}", userId);
                return new List<RoleDto>();
            }
        }

        /// <inheritdoc />
        public async Task<List<UserBasicDto>> GetUsersWithoutRoleAsync(int roleId, bool? isActive = true)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                var parameters = new
                {
                    p_roleid = roleId,
                    p_is_active = isActive
                };

                var users = await connection.QueryAsync<UserBasicDto>(
                    "SELECT * FROM sp_um_get_users_without_role(@p_roleid, @p_is_active)",
                    parameters);

                return users.ToList();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting users without role {RoleId}", roleId);
                return new List<UserBasicDto>();
            }
        }

        /// <inheritdoc />
        public async Task<UserRoleSyncResultDto> SyncUserRolesAsync(SyncUserRolesDto dto)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                var parameters = new
                {
                    p_userid = dto.UserId,
                    p_roleids = dto.RoleIds.ToArray(),
                    p_assignedby = dto.AssignedBy
                };

                var result = await connection.QueryFirstAsync<dynamic>(
                    "SELECT * FROM sp_um_sync_user_roles(@p_userid, @p_roleids, @p_assignedby)",
                    parameters);

                return new UserRoleSyncResultDto
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
                _logger?.LogError(ex, "Error syncing roles for user {UserId}", dto.UserId);
                return new UserRoleSyncResultDto
                {
                    Success = false,
                    Message = ex.Message,
                    AddedCount = 0,
                    RemovedCount = 0,
                    UnchangedCount = 0
                };
            }
        }

        /// <inheritdoc />
        public async Task<UserRoleStatisticsDto> GetUserRoleStatisticsAsync()
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);

                var stats = await connection.QueryFirstOrDefaultAsync<UserRoleStatisticsDto>(
                    "SELECT * FROM sp_um_get_user_roles_statistics()");

                return stats ?? new UserRoleStatisticsDto();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting user role statistics");
                return new UserRoleStatisticsDto();
            }
        }

        /// <inheritdoc />
        public async Task<bool> UserHasAnyPermissionAsync(int userId, List<int> permissionIds)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                var parameters = new
                {
                    p_userid = userId,
                    p_permissionids = permissionIds.ToArray()
                };

                return await connection.QueryFirstOrDefaultAsync<bool>(
                    "SELECT * FROM sp_um_user_has_any_permission_from_role(@p_userid, @p_permissionids)",
                    parameters);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error checking if user {UserId} has any of the specified permissions", userId);
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<bool> UserHasAllPermissionsAsync(int userId, List<int> permissionIds)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                var parameters = new
                {
                    p_userid = userId,
                    p_permissionids = permissionIds.ToArray()
                };

                return await connection.QueryFirstOrDefaultAsync<bool>(
                    "SELECT * FROM sp_um_user_has_all_permissions_from_roles(@p_userid, @p_permissionids)",
                    parameters);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error checking if user {UserId} has all specified permissions", userId);
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<List<UserPermissionDto>> GetAllUserPermissionsAsync(int userId)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                var parameters = new
                {
                    p_userid = userId
                };

                var permissions = await connection.QueryAsync<UserPermissionDto>(
                    "SELECT * FROM sp_um_get_all_user_permissions(@p_userid)",
                    parameters);

                return permissions.ToList();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting all permissions for user {UserId}", userId);
                return new List<UserPermissionDto>();
            }
        }

        public async Task<List<UnassignedUserDto>> GetAllUnassignedUsersAsync(bool? isActive = true)
        {
            var result = new List<UnassignedUserDto>();
            try
            {
                await using var connection = new Npgsql.NpgsqlConnection(_connectionString);
                await connection.OpenAsync();
                await using var command = new NpgsqlCommand("SELECT * FROM sp_um_get_all_unassigned_users(@p_is_active)", connection);
                command.Parameters.AddWithValue("p_is_active", (object?)isActive ?? DBNull.Value);

                await using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    result.Add(new UnassignedUserDto
                    {
                        UserId = reader.GetInt32(reader.GetOrdinal("userid")),
                        Username = reader.GetString(reader.GetOrdinal("username")),
                        Email = reader.GetString(reader.GetOrdinal("email")),
                        IsActive = reader.GetBoolean(reader.GetOrdinal("isactive"))
                    });
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting unassigned users");
            }
            return result;
        }


        public async Task<bool> UpdateUserRoleByIdAsync(UpdateUserRoleByIdDto dto)
        {
            try
            {
                using var connection = new Npgsql.NpgsqlConnection(_connectionString);
                await connection.OpenAsync();
                using var command = new NpgsqlCommand("SELECT public.sp_um_userroles_update_by_id(@p_id, @p_userid, @p_roleid, @p_assignedby, @p_dateassigned)", connection);

                command.Parameters.AddWithValue("p_id", dto.Id);
                command.Parameters.AddWithValue("p_userid", dto.UserId);
                command.Parameters.AddWithValue("p_roleid", dto.RoleId);
                command.Parameters.AddWithValue("p_assignedby", dto.AssignedBy);
                command.Parameters.Add(new NpgsqlParameter("p_dateassigned", NpgsqlDbType.TimestampTz)
                {
                    Value = (object?)dto.DateAssigned ?? DBNull.Value
                });

                await command.ExecuteNonQueryAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error updating user role by id {Id}", dto.Id);
                return false;
            }
        }

        public async Task<bool> DeleteUserRoleByIdAsync(int id)
        {
            try
            {
                using var connection = new Npgsql.NpgsqlConnection(_connectionString);
                await connection.OpenAsync();
                using var command = new NpgsqlCommand("SELECT public.sp_um_userroles_delete_by_id(@p_id)", connection);
                command.Parameters.AddWithValue("p_id", id);

                await command.ExecuteNonQueryAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error deleting user role by id {Id}", id);
                return false;
            }
        }
    }
}
