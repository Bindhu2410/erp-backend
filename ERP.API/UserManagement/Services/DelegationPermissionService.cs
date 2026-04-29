using System.Data;
using Npgsql;
using Microsoft.Extensions.Configuration;
using ERP.API.UserManagement.DTOs;
using ERP.API.UserManagement.Services;

namespace ERP.API.UserManagement.Services
{
    /// <summary>
    /// Delegation Permission service implementation using PostgreSQL stored procedures
    /// </summary>
    public class DelegationPermissionService : IDelegationPermissionService
    {
        private readonly string _connectionString;
        private readonly ILogger<DelegationPermissionService> _logger;

        public DelegationPermissionService(
            IConfiguration configuration,
            ILogger<DelegationPermissionService> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new ArgumentException("Default connection string not found");
            _logger = logger;
        }

        /// <summary>
        /// Creates a new delegation permission
        /// </summary>
        public async Task<DelegationPermissionResultDto> CreateDelegationPermissionAsync(CreateDelegationPermissionDto createDto)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                using var command = new NpgsqlCommand("SELECT * FROM sp_create_um_delegation_permission(@p_delegation_id, @p_permission_id)", connection)
                {
                    CommandType = CommandType.Text
                };

                command.Parameters.AddWithValue("p_delegation_id", createDto.DelegationId);
                command.Parameters.AddWithValue("p_permission_id", createDto.PermissionId);

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return new DelegationPermissionResultDto
                    {
                        Success = reader.GetBoolean("success"),
                        Message = reader.GetString("message")
                    };
                }

                return new DelegationPermissionResultDto
                {
                    Success = false,
                    Message = "No response from stored procedure"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating delegation permission for DelegationId: {DelegationId}, PermissionId: {PermissionId}", 
                    createDto.DelegationId, createDto.PermissionId);
                
                return new DelegationPermissionResultDto
                {
                    Success = false,
                    Message = $"Error creating delegation permission: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Gets permissions for a specific delegation
        /// </summary>
        public async Task<List<DelegationPermissionDto>> GetDelegationPermissionsAsync(int delegationId)
        {
            try
            {
                var permissions = new List<DelegationPermissionDto>();

                using var connection = new NpgsqlConnection(_connectionString);
                using var command = new NpgsqlCommand("SELECT * FROM sp_get_um_delegation_permissions(@p_delegation_id)", connection)
                {
                    CommandType = CommandType.Text
                };

                command.Parameters.AddWithValue("p_delegation_id", delegationId);

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    permissions.Add(new DelegationPermissionDto
                    {
                        DelegationId = reader.GetInt32("delegation_id"),
                        PermissionId = reader.GetInt32("permission_id"),
                        PermissionName = reader.GetString("permission_name"),
                        Description = reader.IsDBNull("description") ? null : reader.GetString("description"),
                        Category = reader.IsDBNull("category") ? null : reader.GetString("category"),
                        IsActive = reader.GetBoolean("is_active")
                    });
                }

                return permissions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting delegation permissions for DelegationId: {DelegationId}", delegationId);
                throw;
            }
        }

        /// <summary>
        /// Gets all delegation permissions with pagination
        /// </summary>
        public async Task<PagedDelegationPermissionsResponseDto> GetAllDelegationPermissionsAsync(PagedDelegationPermissionsRequestDto request)
        {
            try
            {
                var items = new List<DetailedDelegationPermissionDto>();
                long totalCount = 0;

                using var connection = new NpgsqlConnection(_connectionString);
                using var command = new NpgsqlCommand("SELECT * FROM sp_get_um_all_delegation_permissions(@p_page_number, @p_page_size, @p_delegation_id)", connection)
                {
                    CommandType = CommandType.Text
                };

                command.Parameters.AddWithValue("p_page_number", request.PageNumber);
                command.Parameters.AddWithValue("p_page_size", request.PageSize);
                command.Parameters.AddWithValue("p_delegation_id", request.DelegationId ?? (object)DBNull.Value);

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    if (totalCount == 0)
                        totalCount = reader.GetInt64("total_count");

                    items.Add(new DetailedDelegationPermissionDto
                    {
                        DelegationId = reader.GetInt32("delegation_id"),
                        PermissionId = reader.GetInt32("permission_id"),
                        PermissionName = reader.GetString("permission_name"),
                        Description = reader.IsDBNull("description") ? null : reader.GetString("description"),
                        Category = reader.IsDBNull("category") ? null : reader.GetString("category"),
                        IsActive = reader.GetBoolean("is_active"),
                        DelegationName = reader.GetString("delegation_name"),
                        DelegatorUsername = reader.GetString("delegator_username"),
                        DelegateUsername = reader.GetString("delegate_username")
                    });
                }

                return new PagedDelegationPermissionsResponseDto
                {
                    Items = items,
                    TotalCount = (int)totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all delegation permissions");
                throw;
            }
        }

        /// <summary>
        /// Checks if a permission exists for a delegation
        /// </summary>
        public async Task<bool> CheckDelegationPermissionExistsAsync(int delegationId, int permissionId)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                using var command = new NpgsqlCommand("SELECT sp_check_um_delegation_permission_exists(@p_delegation_id, @p_permission_id)", connection)
                {
                    CommandType = CommandType.Text
                };

                command.Parameters.AddWithValue("p_delegation_id", delegationId);
                command.Parameters.AddWithValue("p_permission_id", permissionId);

                await connection.OpenAsync();
                var result = await command.ExecuteScalarAsync();
                
                return result != null && (bool)result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking delegation permission existence for DelegationId: {DelegationId}, PermissionId: {PermissionId}", 
                    delegationId, permissionId);
                return false;
            }
        }

        /// <summary>
        /// Bulk updates delegation permissions
        /// </summary>
        public async Task<DelegationPermissionResultDto> UpdateDelegationPermissionsAsync(UpdateDelegationPermissionsDto updateDto)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                using var command = new NpgsqlCommand("SELECT * FROM sp_update_um_delegation_permissions(@p_delegation_id, @p_permission_ids)", connection)
                {
                    CommandType = CommandType.Text
                };

                command.Parameters.AddWithValue("p_delegation_id", updateDto.DelegationId);
                command.Parameters.AddWithValue("p_permission_ids", updateDto.PermissionIds.ToArray());

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return new DelegationPermissionResultDto
                    {
                        Success = reader.GetBoolean("success"),
                        Message = reader.GetString("message")
                    };
                }

                return new DelegationPermissionResultDto
                {
                    Success = false,
                    Message = "No response from stored procedure"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating delegation permissions for DelegationId: {DelegationId}", updateDto.DelegationId);
                
                return new DelegationPermissionResultDto
                {
                    Success = false,
                    Message = $"Error updating delegation permissions: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Deletes a specific delegation permission
        /// </summary>
        public async Task<DelegationPermissionResultDto> DeleteDelegationPermissionAsync(int delegationId, int permissionId)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                using var command = new NpgsqlCommand("SELECT * FROM sp_delete_um_delegation_permission(@p_delegation_id, @p_permission_id)", connection)
                {
                    CommandType = CommandType.Text
                };

                command.Parameters.AddWithValue("p_delegation_id", delegationId);
                command.Parameters.AddWithValue("p_permission_id", permissionId);

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return new DelegationPermissionResultDto
                    {
                        Success = reader.GetBoolean("success"),
                        Message = reader.GetString("message")
                    };
                }

                return new DelegationPermissionResultDto
                {
                    Success = false,
                    Message = "No response from stored procedure"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting delegation permission for DelegationId: {DelegationId}, PermissionId: {PermissionId}", 
                    delegationId, permissionId);
                
                return new DelegationPermissionResultDto
                {
                    Success = false,
                    Message = $"Error deleting delegation permission: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Deletes all permissions for a delegation
        /// </summary>
        public async Task<DeleteAllDelegationPermissionsResultDto> DeleteAllDelegationPermissionsAsync(int delegationId)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                using var command = new NpgsqlCommand("SELECT * FROM sp_delete_um_all_delegation_permissions(@p_delegation_id)", connection)
                {
                    CommandType = CommandType.Text
                };

                command.Parameters.AddWithValue("p_delegation_id", delegationId);

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return new DeleteAllDelegationPermissionsResultDto
                    {
                        Success = reader.GetBoolean("success"),
                        Message = reader.GetString("message"),
                        DeletedCount = reader.GetInt32("deleted_count")
                    };
                }

                return new DeleteAllDelegationPermissionsResultDto
                {
                    Success = false,
                    Message = "No response from stored procedure",
                    DeletedCount = 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting all delegation permissions for DelegationId: {DelegationId}", delegationId);
                
                return new DeleteAllDelegationPermissionsResultDto
                {
                    Success = false,
                    Message = $"Error deleting all delegation permissions: {ex.Message}",
                    DeletedCount = 0
                };
            }
        }

        /// <summary>
        /// Gets available permissions for a delegation (not yet assigned)
        /// </summary>
        public async Task<List<AvailablePermissionDto>> GetAvailablePermissionsForDelegationAsync(int delegationId)
        {
            try
            {
                var permissions = new List<AvailablePermissionDto>();

                using var connection = new NpgsqlConnection(_connectionString);
                using var command = new NpgsqlCommand("SELECT * FROM sp_get_um_available_permissions_for_delegation(@p_delegation_id)", connection)
                {
                    CommandType = CommandType.Text
                };

                command.Parameters.AddWithValue("p_delegation_id", delegationId);

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    permissions.Add(new AvailablePermissionDto
                    {
                        PermissionId = reader.GetInt32("permission_id"),
                        PermissionName = reader.GetString("permission_name"),
                        Description = reader.IsDBNull("description") ? null : reader.GetString("description"),
                        Category = reader.IsDBNull("category") ? null : reader.GetString("category"),
                        IsActive = reader.GetBoolean("is_active")
                    });
                }

                return permissions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available permissions for DelegationId: {DelegationId}", delegationId);
                throw;
            }
        }

        /// <summary>
        /// Gets delegation permission summary
        /// </summary>
        public async Task<DelegationPermissionSummaryDto?> GetDelegationPermissionSummaryAsync(int delegationId)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                using var command = new NpgsqlCommand("SELECT * FROM sp_get_um_delegation_permission_summary(@p_delegation_id)", connection)
                {
                    CommandType = CommandType.Text
                };

                command.Parameters.AddWithValue("p_delegation_id", delegationId);

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    var categoriesArray = reader.GetFieldValue<string[]>("categories");
                    
                    return new DelegationPermissionSummaryDto
                    {
                        DelegationId = reader.GetInt32("delegation_id"),
                        DelegationName = reader.GetString("delegation_name"),
                        DelegatorUsername = reader.GetString("delegator_username"),
                        DelegateUsername = reader.GetString("delegate_username"),
                        TotalPermissions = reader.GetInt64("total_permissions"),
                        ActivePermissions = reader.GetInt64("active_permissions"),
                        Categories = categoriesArray?.ToList() ?? new List<string>()
                    };
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting delegation permission summary for DelegationId: {DelegationId}", delegationId);
                throw;
            }
        }
    }
}
