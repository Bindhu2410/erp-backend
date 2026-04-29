using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ERP.API.UserManagement.DTOs;
using Microsoft.Extensions.Logging;

namespace ERP.API.UserManagement.Services
{
    /// <summary>
    /// Service implementation for managing user assignments to organizational units
    /// </summary>
    public class UserOrganizationalUnitService : IUserOrganizationalUnitService
    {
        private readonly IDbConnection _dbConnection;
        private readonly ILogger<UserOrganizationalUnitService> _logger;

        /// <summary>
        /// Constructor for UserOrganizationalUnitService
        /// </summary>
        /// <param name="dbConnection">Database connection</param>
        /// <param name="logger">Logger</param>
        public UserOrganizationalUnitService(IDbConnection dbConnection, ILogger<UserOrganizationalUnitService> logger)
        {
            _dbConnection = dbConnection ?? throw new ArgumentNullException(nameof(dbConnection));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<UserUnitOperationResultDTO> AssignUserToUnitAsync(CreateUserOrganizationalUnitDTO dto)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("p_user_id", dto.UserId);
                parameters.Add("p_unit_id", dto.UnitId);
                parameters.Add("p_is_primary", dto.IsPrimary);
                parameters.Add("p_assigned_by", dto.AssignedBy);

                var result = await _dbConnection.QueryFirstOrDefaultAsync<dynamic>(
                    "SELECT * FROM sp_um_assign_user_to_unit(@p_user_id, @p_unit_id, @p_is_primary, @p_assigned_by)",
                    parameters);

                return new UserUnitOperationResultDTO
                {
                    Success = result?.success ?? false,
                    Message = result?.message?.ToString() ?? "Error processing request"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning user {UserId} to unit {UnitId}", dto.UserId, dto.UnitId);
                return new UserUnitOperationResultDTO
                {
                    Success = false,
                    Message = $"Error assigning user to unit: {ex.Message}"
                };
            }
        }

        /// <inheritdoc />
        public async Task<UserOrganizationalUnitDTO?> GetUserUnitAssignmentAsync(int userId, int unitId)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("p_user_id", userId);
                parameters.Add("p_unit_id", unitId);

                var result = await _dbConnection.QueryFirstOrDefaultAsync<dynamic>(
                    "SELECT * FROM sp_um_get_user_unit_assignment(@p_user_id, @p_unit_id)",
                    parameters);

                if (result == null)
                {
                    return null;
                }

                return new UserOrganizationalUnitDTO
                {
                    UserId = result.UserId,
                    UnitId = result.UnitId,
                    IsPrimary = result.IsPrimary,
                    DateAssigned = result.DateAssigned,
                    AssignedBy = result.AssignedBy,
                    AssignedByName = result.AssignedByName,
                    UnitName = result.UnitName,
                    UnitType = result.UnitType
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving assignment for user {UserId} to unit {UnitId}", userId, unitId);
                return null;
            }
        }

        /// <inheritdoc />
        public async Task<UserUnitOperationResultDTO> UpdateUserUnitAssignmentAsync(int userId, int unitId, UpdateUserOrganizationalUnitDTO dto)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("p_user_id", userId);
                parameters.Add("p_unit_id", unitId);
                parameters.Add("p_is_primary", dto.IsPrimary);

                var result = await _dbConnection.QueryFirstOrDefaultAsync<dynamic>(
                    "SELECT * FROM sp_um_update_user_unit_assignment(@p_user_id, @p_unit_id, @p_is_primary)",
                    parameters);

                return new UserUnitOperationResultDTO
                {
                    Success = result?.success ?? false,
                    Message = result?.message?.ToString() ?? "Error updating assignment"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating assignment for user {UserId} to unit {UnitId}", userId, unitId);
                return new UserUnitOperationResultDTO
                {
                    Success = false,
                    Message = $"Error updating assignment: {ex.Message}"
                };
            }
        }

        /// <inheritdoc />
        public async Task<UserUnitOperationResultDTO> RemoveUserFromUnitAsync(int userId, int unitId)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("p_user_id", userId);
                parameters.Add("p_unit_id", unitId);

                var result = await _dbConnection.QueryFirstOrDefaultAsync<dynamic>(
                    "SELECT * FROM sp_um_remove_user_from_unit(@p_user_id, @p_unit_id)",
                    parameters);

                return new UserUnitOperationResultDTO
                {
                    Success = result?.success ?? false,
                    Message = result?.message?.ToString() ?? "Error removing user from unit"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing user {UserId} from unit {UnitId}", userId, unitId);
                return new UserUnitOperationResultDTO
                {
                    Success = false,
                    Message = $"Error removing user from unit: {ex.Message}"
                };
            }
        }

        /// <inheritdoc />
        public async Task<List<UserOrganizationalUnitDTO>> GetUsersInUnitAsync(int unitId, bool includeChildUnits = false)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("p_unit_id", unitId);
                parameters.Add("p_include_child_units", includeChildUnits);

                var results = await _dbConnection.QueryAsync<dynamic>(
                    "SELECT * FROM sp_um_get_users_in_unit(@p_unit_id, @p_include_child_units)",
                    parameters);

                return results.Select(r => new UserOrganizationalUnitDTO
                {
                    UserId = r.UserId,
                    Username = r.Username,
                    UserFullName = $"{r.FirstName} {r.LastName}",
                    Email = r.Email,
                    UnitId = r.UnitId,
                    UnitName = r.UnitName,
                    UnitType = r.UnitType,
                    IsPrimary = r.IsPrimary,
                    DateAssigned = r.DateAssigned
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users in unit {UnitId}", unitId);
                return new List<UserOrganizationalUnitDTO>();
            }
        }

        /// <inheritdoc />
        public async Task<List<UserOrganizationalUnitDTO>> GetUserUnitsAsync(int userId)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("p_user_id", userId);

                var results = await _dbConnection.QueryAsync<dynamic>(
                    "SELECT * FROM sp_um_get_user_units(@p_user_id)",
                    parameters);

                return results.Select(r => new UserOrganizationalUnitDTO
                {
                    UserId = userId,
                    UnitId = r.UnitId,
                    UnitName = r.UnitName,
                    UnitType = r.UnitType,
                    IsPrimary = r.IsPrimary,
                    DateAssigned = r.DateAssigned
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving units for user {UserId}", userId);
                return new List<UserOrganizationalUnitDTO>();
            }
        }

        /// <inheritdoc />
        public async Task<UserOrganizationalUnitDTO?> GetUserPrimaryUnitAsync(int userId)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("p_user_id", userId);

                var result = await _dbConnection.QueryFirstOrDefaultAsync<dynamic>(
                    "SELECT * FROM sp_um_get_user_primary_unit(@p_user_id)",
                    parameters);

                if (result == null)
                {
                    return null;
                }

                return new UserOrganizationalUnitDTO
                {
                    UserId = userId,
                    UnitId = result.UnitId,
                    UnitName = result.UnitName,
                    UnitType = result.UnitType,
                    IsPrimary = true,
                    DateAssigned = result.DateAssigned
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving primary unit for user {UserId}", userId);
                return null;
            }
        }

        /// <inheritdoc />
        public async Task<UserUnitOperationResultDTO> SetUserPrimaryUnitAsync(int userId, int unitId)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("p_user_id", userId);
                parameters.Add("p_unit_id", unitId);

                var result = await _dbConnection.QueryFirstOrDefaultAsync<dynamic>(
                    "SELECT * FROM sp_um_set_user_primary_unit(@p_user_id, @p_unit_id)",
                    parameters);

                return new UserUnitOperationResultDTO
                {
                    Success = result?.success ?? false,
                    Message = result?.message?.ToString() ?? "Error setting primary unit"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting primary unit {UnitId} for user {UserId}", unitId, userId);
                return new UserUnitOperationResultDTO
                {
                    Success = false,
                    Message = $"Error setting primary unit: {ex.Message}"
                };
            }
        }

        /// <inheritdoc />
        public async Task<UserUnitOperationResultDTO> AssignUsersToUnitAsync(BulkUserAssignmentDTO dto)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("p_user_ids", dto.UserIds.ToArray());
                parameters.Add("p_unit_id", dto.UnitId);
                parameters.Add("p_make_primary", dto.MakePrimary);
                parameters.Add("p_assigned_by", dto.AssignedBy);

                var result = await _dbConnection.QueryFirstOrDefaultAsync<dynamic>(
                    "SELECT * FROM sp_um_assign_users_to_unit(@p_user_ids, @p_unit_id, @p_make_primary, @p_assigned_by)",
                    parameters);

                return new UserUnitOperationResultDTO
                {
                    Success = result?.success ?? false,
                    Message = result?.message?.ToString() ?? "Error bulk assigning users",
                    AffectedCount = result?.affected_users
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk assigning {UserCount} users to unit {UnitId}", 
                    dto.UserIds.Count, dto.UnitId);
                return new UserUnitOperationResultDTO
                {
                    Success = false,
                    Message = $"Error bulk assigning users: {ex.Message}"
                };
            }
        }

        /// <inheritdoc />
        public async Task<UserUnitOperationResultDTO> AssignUserToUnitsAsync(UserMultiUnitAssignmentDTO dto)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("p_user_id", dto.UserId);
                parameters.Add("p_unit_ids", dto.UnitIds.ToArray());
                parameters.Add("p_primary_unit_id", dto.PrimaryUnitId);
                parameters.Add("p_assigned_by", dto.AssignedBy);

                var result = await _dbConnection.QueryFirstOrDefaultAsync<dynamic>(
                    "SELECT * FROM sp_um_assign_user_to_units(@p_user_id, @p_unit_ids, @p_primary_unit_id, @p_assigned_by)",
                    parameters);

                return new UserUnitOperationResultDTO
                {
                    Success = result?.success ?? false,
                    Message = result?.message?.ToString() ?? "Error assigning user to multiple units",
                    AffectedCount = result?.affected_units
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning user {UserId} to multiple units", dto.UserId);
                return new UserUnitOperationResultDTO
                {
                    Success = false,
                    Message = $"Error assigning user to multiple units: {ex.Message}"
                };
            }
        }

        /// <inheritdoc />
        public async Task<List<UserOrganizationalUnitDTO>> GetUsersWithoutUnitAsync()
        {
            try
            {
                var results = await _dbConnection.QueryAsync<dynamic>(
                    "SELECT * FROM sp_um_get_users_without_unit()");

                return results.Select(r => new UserOrganizationalUnitDTO
                {
                    UserId = r.UserId,
                    Username = r.Username,
                    UserFullName = $"{r.FirstName} {r.LastName}",
                    Email = r.Email
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users without unit assignments");
                return new List<UserOrganizationalUnitDTO>();
            }
        }

        /// <inheritdoc />
        public async Task<List<UserOrganizationalUnitDTO>> GetEmptyUnitsAsync()
        {
            try
            {
                var results = await _dbConnection.QueryAsync<dynamic>(
                    "SELECT * FROM sp_um_get_empty_units()");

                return results.Select(r => new UserOrganizationalUnitDTO
                {
                    UnitId = r.UnitId,
                    UnitName = r.UnitName,
                    UnitType = r.UnitType
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving empty organizational units");
                return new List<UserOrganizationalUnitDTO>();
            }
        }

        /// <inheritdoc />
        public async Task<List<UnitAssignmentStatsDTO>> GetUnitAssignmentStatsAsync()
        {
            try
            {
                var results = await _dbConnection.QueryAsync<UnitAssignmentStatsDTO>(
                    "SELECT * FROM sp_um_get_unit_assignment_stats()");

                return results.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving unit assignment statistics");
                return new List<UnitAssignmentStatsDTO>();
            }
        }

        /// <inheritdoc />
        public async Task<PaginatedUserUnitAssignmentsDTO> GetUserUnitAssignmentsPaginatedAsync(UserUnitAssignmentQueryParameters parameters)
        {
            try
            {
                var dbParams = new DynamicParameters();
                dbParams.Add("p_page_number", parameters.PageNumber);
                dbParams.Add("p_page_size", parameters.PageSize);
                dbParams.Add("p_search_term", parameters.SearchTerm);
                dbParams.Add("p_unit_id", parameters.UnitId);
                dbParams.Add("p_is_primary", parameters.IsPrimary);

                var results = await _dbConnection.QueryAsync<dynamic>(
                    "SELECT * FROM sp_um_get_user_unit_assignments_paginated(@p_page_number, @p_page_size, @p_search_term, @p_unit_id, @p_is_primary)",
                    dbParams);

                if (!results.Any())
                {
                    return new PaginatedUserUnitAssignmentsDTO
                    {
                        PageNumber = parameters.PageNumber,
                        PageSize = parameters.PageSize,
                        TotalCount = 0,
                        Items = new List<UserOrganizationalUnitDTO>()
                    };
                }

                var items = results.Select(r => new UserOrganizationalUnitDTO
                {
                    UserId = r.UserId,
                    Username = r.Username,
                    UserFullName = r.UserFullName,
                    Email = r.Email,
                    UnitId = r.UnitId,
                    UnitName = r.UnitName,
                    UnitType = r.UnitType,
                    IsPrimary = r.IsPrimary,
                    DateAssigned = r.DateAssigned,
                    AssignedBy = r.AssignedBy,
                    AssignedByName = r.AssignedByName
                }).ToList();

                return new PaginatedUserUnitAssignmentsDTO
                {
                    PageNumber = parameters.PageNumber,
                    PageSize = parameters.PageSize,
                    TotalCount = results.First().TotalCount,
                    Items = items
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving paginated user-unit assignments");
                return new PaginatedUserUnitAssignmentsDTO
                {
                    PageNumber = parameters.PageNumber,
                    PageSize = parameters.PageSize,
                    TotalCount = 0,
                    Items = new List<UserOrganizationalUnitDTO>()
                };
            }
        }

        /// <inheritdoc />
        public async Task<List<UnitTypeStatisticsDTO>> GetUserCountByUnitTypeAsync()
        {
            try
            {
                var results = await _dbConnection.QueryAsync<UnitTypeStatisticsDTO>(
                    "SELECT * FROM sp_um_get_user_count_by_unit_type()");

                return results.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user count by unit type");
                return new List<UnitTypeStatisticsDTO>();
            }
        }

        /// <inheritdoc />
        public async Task<UserUnitOperationResultDTO> TransferUsersBetweenUnitsAsync(TransferUsersDTO dto)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("p_source_unit_id", dto.SourceUnitId);
                parameters.Add("p_target_unit_id", dto.TargetUnitId);
                parameters.Add("p_retain_primary", dto.RetainPrimary);
                parameters.Add("p_assigned_by", dto.AssignedBy);

                var result = await _dbConnection.QueryFirstOrDefaultAsync<dynamic>(
                    "SELECT * FROM sp_um_transfer_users_between_units(@p_source_unit_id, @p_target_unit_id, @p_retain_primary, @p_assigned_by)",
                    parameters);

                return new UserUnitOperationResultDTO
                {
                    Success = result?.success ?? false,
                    Message = result?.message?.ToString() ?? "Error transferring users",
                    AffectedCount = result?.transferred_users
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error transferring users from unit {SourceUnitId} to unit {TargetUnitId}", 
                    dto.SourceUnitId, dto.TargetUnitId);
                return new UserUnitOperationResultDTO
                {
                    Success = false,
                    Message = $"Error transferring users: {ex.Message}"
                };
            }
        }

        /// <inheritdoc />
        public async Task<UserUnitOperationResultDTO> EnsureUsersPrimaryUnitAsync()
        {
            try
            {
                var result = await _dbConnection.QueryFirstOrDefaultAsync<dynamic>(
                    "SELECT * FROM sp_um_ensure_users_have_primary_unit()");

                return new UserUnitOperationResultDTO
                {
                    Success = result?.success ?? false,
                    Message = result?.message?.ToString() ?? "Error ensuring users have primary unit",
                    AffectedCount = result?.users_updated
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ensuring users have primary unit");
                return new UserUnitOperationResultDTO
                {
                    Success = false,
                    Message = $"Error ensuring users have primary unit: {ex.Message}"
                };
            }
        }
    }
}
