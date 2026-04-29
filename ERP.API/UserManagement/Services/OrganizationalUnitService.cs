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
    public class OrganizationalUnitService : IOrganizationalUnitService
    {
        private readonly string _connectionString;
        private readonly ILogger<OrganizationalUnitService>? _logger;

        public OrganizationalUnitService(string connectionString, ILogger<OrganizationalUnitService>? logger = null)
        {
            _connectionString = connectionString;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<CreateUnitResultDto> CreateOrganizationalUnitAsync(CreateOrganizationalUnitDto dto, int? createdBy)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                var parameters = new
                {
                    p_unit_name = dto.UnitName,
                    p_unit_type = dto.UnitType,
                    p_description = dto.Description,
                    p_parent_unit_id = dto.ParentUnitId,
                    p_manager_id = dto.ManagerId,
                    p_is_active = dto.IsActive,
                    p_created_by = createdBy
                };

                var result = await connection.QueryFirstAsync<dynamic>(
                    "SELECT * FROM sp_um_create_organizational_unit(@p_unit_name, @p_unit_type, @p_description, " +
                    "@p_parent_unit_id, @p_manager_id, @p_is_active, @p_created_by)",
                    parameters);

                return new CreateUnitResultDto
                {
                    Success = result.success,
                    Message = result.message?.ToString() ?? string.Empty,
                    UnitId = Convert.ToInt32(result.unit_id)
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error creating organizational unit {UnitName}", dto.UnitName);
                return new CreateUnitResultDto
                {
                    Success = false,
                    Message = ex.Message,
                    UnitId = 0
                };
            }
        }

        /// <inheritdoc />
        public async Task<OrganizationalUnitDto?> GetOrganizationalUnitByIdAsync(int unitId)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                var parameters = new
                {
                    p_unit_id = unitId
                };

                var result = await connection.QueryFirstOrDefaultAsync<OrganizationalUnitDto>(
                    "SELECT * FROM sp_um_get_organizational_unit_by_id(@p_unit_id)",
                    parameters);

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting organizational unit with ID {UnitId}", unitId);
                return null;
            }
        }

        /// <inheritdoc />
        public async Task<OperationResultDto> UpdateOrganizationalUnitAsync(int unitId, UpdateOrganizationalUnitDto dto)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                var parameters = new
                {
                    p_unit_id = unitId,
                    p_unit_name = dto.UnitName,
                    p_unit_type = dto.UnitType,
                    p_description = dto.Description,
                    p_parent_unit_id = dto.ParentUnitId,
                    p_manager_id = dto.ManagerId,
                    p_is_active = dto.IsActive
                };

                var result = await connection.QueryFirstAsync<dynamic>(
                    "SELECT * FROM sp_um_update_organizational_unit(@p_unit_id, @p_unit_name, @p_unit_type, " +
                    "@p_description, @p_parent_unit_id, @p_manager_id, @p_is_active)",
                    parameters);

                return new OperationResultDto
                {
                    Success = result.success,
                    Message = result.message?.ToString() ?? string.Empty
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error updating organizational unit with ID {UnitId}", unitId);
                return new OperationResultDto
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        /// <inheritdoc />
        public async Task<OperationResultDto> DeleteOrganizationalUnitAsync(int unitId)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                var parameters = new
                {
                    p_unit_id = unitId
                };

                var result = await connection.QueryFirstAsync<dynamic>(
                    "SELECT * FROM sp_um_delete_organizational_unit(@p_unit_id)",
                    parameters);

                return new OperationResultDto
                {
                    Success = result.success,
                    Message = result.message?.ToString() ?? string.Empty
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error deleting organizational unit with ID {UnitId}", unitId);
                return new OperationResultDto
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        /// <inheritdoc />
        public async Task<OperationResultDto> SetOrganizationalUnitStatusAsync(int unitId, bool isActive)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                var parameters = new
                {
                    p_unit_id = unitId,
                    p_is_active = isActive
                };

                var result = await connection.QueryFirstAsync<dynamic>(
                    "SELECT * FROM sp_um_set_organizational_unit_status(@p_unit_id, @p_is_active)",
                    parameters);

                return new OperationResultDto
                {
                    Success = result.success,
                    Message = result.message?.ToString() ?? string.Empty
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error setting status for organizational unit with ID {UnitId}", unitId);
                return new OperationResultDto
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        /// <inheritdoc />
        public async Task<List<OrganizationalUnitDto>> GetAllOrganizationalUnitsAsync(bool? isActive = null)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                var parameters = new
                {
                    p_is_active = isActive
                };

                var units = await connection.QueryAsync<OrganizationalUnitDto>(
                    "SELECT * FROM sp_um_get_all_organizational_units(@p_is_active)",
                    parameters);

                return units.ToList();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting all organizational units");
                return new List<OrganizationalUnitDto>();
            }
        }

        /// <inheritdoc />
        public async Task<OrganizationalUnitPaginatedResponseDto> GetOrganizationalUnitsPaginatedAsync(OrganizationalUnitQueryParametersDto parameters)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                var queryParams = new
                {
                    p_page_number = parameters.PageNumber,
                    p_page_size = parameters.PageSize,
                    p_search_term = parameters.SearchTerm,
                    p_unit_type = parameters.UnitType,
                    p_is_active = parameters.IsActive
                };

                var units = await connection.QueryAsync<OrganizationalUnitDto>(
                    "SELECT * FROM sp_um_get_organizational_units_paginated(@p_page_number, @p_page_size, @p_search_term, @p_unit_type, @p_is_active)",
                    queryParams);

                var unitsList = units.ToList();
                long totalCount = 0;

                // Extract total count from first row if available
                if (unitsList.Any())
                {
                    // Try to get TotalCount from dynamic result
                    var firstRow = units.First() as IDictionary<string, object>;
                    if (firstRow != null && firstRow.TryGetValue("TotalCount", out var totalCountObj))
                    {
                        totalCount = Convert.ToInt64(totalCountObj);
                    }
                }

                return new OrganizationalUnitPaginatedResponseDto
                {
                    Units = unitsList,
                    TotalCount = totalCount,
                    PageNumber = parameters.PageNumber,
                    PageSize = parameters.PageSize
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting paginated organizational units");
                return new OrganizationalUnitPaginatedResponseDto
                {
                    Units = new List<OrganizationalUnitDto>(),
                    TotalCount = 0,
                    PageNumber = parameters.PageNumber,
                    PageSize = parameters.PageSize
                };
            }
        }

        /// <inheritdoc />
        public async Task<List<OrganizationalUnitChildDto>> GetChildUnitsAsync(int parentUnitId, bool? isActive = null)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                var parameters = new
                {
                    p_parent_unit_id = parentUnitId,
                    p_is_active = isActive
                };

                var units = await connection.QueryAsync<OrganizationalUnitChildDto>(
                    "SELECT * FROM sp_um_get_child_units(@p_parent_unit_id, @p_is_active)",
                    parameters);

                return units.ToList();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting child units for parent ID {ParentUnitId}", parentUnitId);
                return new List<OrganizationalUnitChildDto>();
            }
        }

        /// <inheritdoc />
        public async Task<List<OrganizationalUnitChildDto>> GetTopLevelUnitsAsync(bool? isActive = null)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                var parameters = new
                {
                    p_is_active = isActive
                };

                var units = await connection.QueryAsync<OrganizationalUnitChildDto>(
                    "SELECT * FROM sp_um_get_top_level_units(@p_is_active)",
                    parameters);

                return units.ToList();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting top-level organizational units");
                return new List<OrganizationalUnitChildDto>();
            }
        }

        /// <inheritdoc />
        public async Task<List<OrganizationalUnitHierarchyDto>> GetUnitHierarchyAsync(int? unitId = null, bool? isActive = true)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                var parameters = new
                {
                    p_unit_id = unitId,
                    p_is_active = isActive
                };

                var units = await connection.QueryAsync<OrganizationalUnitHierarchyDto>(
                    "SELECT * FROM sp_um_get_unit_hierarchy(@p_unit_id, @p_is_active)",
                    parameters);

                return units.ToList();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting organizational unit hierarchy");
                return new List<OrganizationalUnitHierarchyDto>();
            }
        }

        /// <inheritdoc />
        public async Task<List<OrganizationalUnitSearchResultDto>> SearchOrganizationalUnitsAsync(string searchTerm, string? unitType = null, bool? isActive = null)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                var parameters = new
                {
                    p_search_term = searchTerm,
                    p_unit_type = unitType,
                    p_is_active = isActive
                };

                var units = await connection.QueryAsync<OrganizationalUnitSearchResultDto>(
                    "SELECT * FROM sp_um_search_organizational_units(@p_search_term, @p_unit_type, @p_is_active)",
                    parameters);

                return units.ToList();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error searching organizational units with term {SearchTerm}", searchTerm);
                return new List<OrganizationalUnitSearchResultDto>();
            }
        }

        /// <inheritdoc />
        public async Task<List<OrganizationalUnitDto>> GetUnitsByManagerAsync(int managerId, bool? isActive = null)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                var parameters = new
                {
                    p_manager_id = managerId,
                    p_is_active = isActive
                };

                var units = await connection.QueryAsync<OrganizationalUnitDto>(
                    "SELECT * FROM sp_um_get_units_by_manager(@p_manager_id, @p_is_active)",
                    parameters);

                return units.ToList();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting units managed by manager ID {ManagerId}", managerId);
                return new List<OrganizationalUnitDto>();
            }
        }

        /// <inheritdoc />
        public async Task<List<OrganizationalUnitTypeDto>> GetUnitTypesAsync()
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);

                var types = await connection.QueryAsync<OrganizationalUnitTypeDto>(
                    "SELECT * FROM sp_um_get_unit_types()");

                return types.ToList();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting organizational unit types");
                return new List<OrganizationalUnitTypeDto>();
            }
        }

        /// <inheritdoc />
        public async Task<OrganizationalUnitStatisticsDto> GetOrganizationalUnitStatisticsAsync()
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);

                var stats = await connection.QueryFirstOrDefaultAsync<OrganizationalUnitStatisticsDto>(
                    "SELECT * FROM sp_um_get_organizational_unit_statistics()");

                return stats ?? new OrganizationalUnitStatisticsDto();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting organizational unit statistics");
                return new OrganizationalUnitStatisticsDto();
            }
        }

        /// <inheritdoc />
        public async Task<OperationResultDto> AssignManagerToUnitAsync(int unitId, int managerId)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                var parameters = new
                {
                    p_unit_id = unitId,
                    p_manager_id = managerId
                };

                var result = await connection.QueryFirstAsync<dynamic>(
                    "SELECT * FROM sp_um_assign_manager_to_unit(@p_unit_id, @p_manager_id)",
                    parameters);

                return new OperationResultDto
                {
                    Success = result.success,
                    Message = result.message?.ToString() ?? string.Empty
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error assigning manager {ManagerId} to unit {UnitId}", managerId, unitId);
                return new OperationResultDto
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        /// <inheritdoc />
        public async Task<OperationResultDto> MoveOrganizationalUnitAsync(int unitId, int? newParentId)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                var parameters = new
                {
                    p_unit_id = unitId,
                    p_new_parent_id = newParentId
                };

                var result = await connection.QueryFirstAsync<dynamic>(
                    "SELECT * FROM sp_um_move_organizational_unit(@p_unit_id, @p_new_parent_id)",
                    parameters);

                return new OperationResultDto
                {
                    Success = result.success,
                    Message = result.message?.ToString() ?? string.Empty
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error moving unit {UnitId} to parent {NewParentId}", unitId, newParentId);
                return new OperationResultDto
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }
    }
}
