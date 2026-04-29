using System.Data;
using Npgsql;
using Microsoft.Extensions.Configuration;
using ERP.API.UserManagement.DTOs;
using ERP.API.UserManagement.Services;

namespace ERP.API.UserManagement.Services
{
    /// <summary>
    /// Access delegation service implementation using PostgreSQL stored procedures
    /// </summary>
    public class AccessDelegationService : IAccessDelegationService
    {
        private readonly string _connectionString;
        private readonly ILogger<AccessDelegationService> _logger;

        public AccessDelegationService(
            IConfiguration configuration,
            ILogger<AccessDelegationService> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new ArgumentException("Default connection string not found");
            _logger = logger;
        }

        /// <summary>
        /// Creates a new access delegation
        /// </summary>
        public async Task<int> CreateDelegationAsync(CreateAccessDelegationDto createDto)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                using var command = new NpgsqlCommand("sp_um_insert_access_delegation", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                var delegationIdParam = new NpgsqlParameter("p_delegation_id", NpgsqlTypes.NpgsqlDbType.Integer)
                {
                    Direction = ParameterDirection.InputOutput,
                    Value = DBNull.Value
                };

                command.Parameters.AddWithValue("p_from_user_id", createDto.FromUserId);
                command.Parameters.AddWithValue("p_to_user_id", createDto.ToUserId);
                command.Parameters.AddWithValue("p_start_date", createDto.StartDate);
                command.Parameters.AddWithValue("p_end_date", createDto.EndDate);
                command.Parameters.AddWithValue("p_created_by", createDto.CreatedBy);
                command.Parameters.AddWithValue("p_reason", createDto.Reason ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("p_is_active", createDto.IsActive);
                command.Parameters.Add(delegationIdParam);

                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();

                return Convert.ToInt32(delegationIdParam.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating access delegation");
                throw;
            }
        }

        /// <summary>
        /// Gets an access delegation by ID
        /// </summary>
        public async Task<AccessDelegationDto?> GetDelegationByIdAsync(int delegationId)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                using var command = new NpgsqlCommand("SELECT * FROM sp_um_get_access_delegation_by_id(@p_delegation_id)", connection);
                
                command.Parameters.AddWithValue("p_delegation_id", delegationId);

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return new AccessDelegationDto
                    {
                        DelegationId = reader.GetInt32("DelegationId"),
                        FromUserId = reader.GetInt32("FromUserId"),
                        ToUserId = reader.GetInt32("ToUserId"),
                        StartDate = reader.GetDateTime("StartDate"),
                        EndDate = reader.GetDateTime("EndDate"),
                        Reason = reader.IsDBNull("Reason") ? null : reader.GetString("Reason"),
                        IsActive = reader.GetBoolean("IsActive"),
                        CreatedBy = reader.GetInt32("CreatedBy"),
                        DateCreated = reader.GetDateTime("DateCreated")
                    };
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting access delegation by ID: {DelegationId}", delegationId);
                throw;
            }
        }

        /// <summary>
        /// Updates an existing access delegation
        /// </summary>
        public async Task<bool> UpdateDelegationAsync(UpdateAccessDelegationDto updateDto)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                using var command = new NpgsqlCommand("sp_um_update_access_delegation", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.AddWithValue("p_delegation_id", updateDto.DelegationId);
                command.Parameters.AddWithValue("p_from_user_id", updateDto.FromUserId ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("p_to_user_id", updateDto.ToUserId ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("p_start_date", updateDto.StartDate ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("p_end_date", updateDto.EndDate ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("p_reason", updateDto.Reason ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("p_is_active", updateDto.IsActive ?? (object)DBNull.Value);

                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating access delegation: {DelegationId}", updateDto.DelegationId);
                return false;
            }
        }

        /// <summary>
        /// Deletes an access delegation
        /// </summary>
        public async Task<bool> DeleteDelegationAsync(int delegationId)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                using var command = new NpgsqlCommand("sp_um_delete_access_delegation", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.AddWithValue("p_delegation_id", delegationId);

                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting access delegation: {DelegationId}", delegationId);
                return false;
            }
        }

        /// <summary>
        /// Gets paginated list of all access delegations
        /// </summary>
        public async Task<AccessDelegationPagedDto> GetDelegationsPagedAsync(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                using var command = new NpgsqlCommand("SELECT * FROM sp_um_get_access_delegations_paged(@p_page_number, @p_page_size)", connection);
                
                command.Parameters.AddWithValue("p_page_number", pageNumber);
                command.Parameters.AddWithValue("p_page_size", pageSize);

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();

                var delegations = new List<AccessDelegationDto>();
                long totalCount = 0;

                while (await reader.ReadAsync())
                {
                    delegations.Add(new AccessDelegationDto
                    {
                        DelegationId = reader.GetInt32("DelegationId"),
                        FromUserId = reader.GetInt32("FromUserId"),
                        ToUserId = reader.GetInt32("ToUserId"),
                        StartDate = reader.GetDateTime("StartDate"),
                        EndDate = reader.GetDateTime("EndDate"),
                        Reason = reader.IsDBNull("Reason") ? null : reader.GetString("Reason"),
                        IsActive = reader.GetBoolean("IsActive"),
                        CreatedBy = reader.GetInt32("CreatedBy"),
                        DateCreated = reader.GetDateTime("DateCreated")
                    });

                    totalCount = reader.GetInt64("TotalCount");
                }

                return new AccessDelegationPagedDto
                {
                    Delegations = delegations,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paginated delegations");
                throw;
            }
        }

        /// <summary>
        /// Gets active delegations for a specific user as delegator
        /// </summary>
        public async Task<List<AccessDelegationDto>> GetActiveDelegationsByFromUserAsync(int fromUserId)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                using var command = new NpgsqlCommand("SELECT * FROM sp_um_get_active_delegations_by_from_user(@p_from_user_id)", connection);
                
                command.Parameters.AddWithValue("p_from_user_id", fromUserId);

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();

                var delegations = new List<AccessDelegationDto>();

                while (await reader.ReadAsync())
                {
                    delegations.Add(new AccessDelegationDto
                    {
                        DelegationId = reader.GetInt32("DelegationId"),
                        FromUserId = reader.GetInt32("FromUserId"),
                        ToUserId = reader.GetInt32("ToUserId"),
                        StartDate = reader.GetDateTime("StartDate"),
                        EndDate = reader.GetDateTime("EndDate"),
                        Reason = reader.IsDBNull("Reason") ? null : reader.GetString("Reason"),
                        IsActive = reader.GetBoolean("IsActive"),
                        CreatedBy = reader.GetInt32("CreatedBy"),
                        DateCreated = reader.GetDateTime("DateCreated")
                    });
                }

                return delegations;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active delegations by from user: {FromUserId}", fromUserId);
                throw;
            }
        }

        /// <summary>
        /// Gets active delegations for a specific user as delegate
        /// </summary>
        public async Task<List<AccessDelegationDto>> GetActiveDelegationsByToUserAsync(int toUserId)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                using var command = new NpgsqlCommand("SELECT * FROM sp_um_get_active_delegations_by_to_user(@p_to_user_id)", connection);
                
                command.Parameters.AddWithValue("p_to_user_id", toUserId);

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();

                var delegations = new List<AccessDelegationDto>();

                while (await reader.ReadAsync())
                {
                    delegations.Add(new AccessDelegationDto
                    {
                        DelegationId = reader.GetInt32("DelegationId"),
                        FromUserId = reader.GetInt32("FromUserId"),
                        ToUserId = reader.GetInt32("ToUserId"),
                        StartDate = reader.GetDateTime("StartDate"),
                        EndDate = reader.GetDateTime("EndDate"),
                        Reason = reader.IsDBNull("Reason") ? null : reader.GetString("Reason"),
                        IsActive = reader.GetBoolean("IsActive"),
                        CreatedBy = reader.GetInt32("CreatedBy"),
                        DateCreated = reader.GetDateTime("DateCreated")
                    });
                }

                return delegations;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active delegations by to user: {ToUserId}", toUserId);
                throw;
            }
        }

        /// <summary>
        /// Gets paginated delegations by from user
        /// </summary>
        public async Task<AccessDelegationPagedDto> GetDelegationsByFromUserPagedAsync(UserDelegationQueryDto queryDto)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                using var command = new NpgsqlCommand("SELECT * FROM sp_um_get_delegations_by_from_user_paged(@p_from_user_id, @p_page_number, @p_page_size, @p_include_inactive)", connection);
                
                command.Parameters.AddWithValue("p_from_user_id", queryDto.UserId);
                command.Parameters.AddWithValue("p_page_number", queryDto.PageNumber);
                command.Parameters.AddWithValue("p_page_size", queryDto.PageSize);
                command.Parameters.AddWithValue("p_include_inactive", queryDto.IncludeInactive);

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();

                var delegations = new List<AccessDelegationDto>();
                long totalCount = 0;

                while (await reader.ReadAsync())
                {
                    delegations.Add(new AccessDelegationDto
                    {
                        DelegationId = reader.GetInt32("DelegationId"),
                        FromUserId = reader.GetInt32("FromUserId"),
                        ToUserId = reader.GetInt32("ToUserId"),
                        StartDate = reader.GetDateTime("StartDate"),
                        EndDate = reader.GetDateTime("EndDate"),
                        Reason = reader.IsDBNull("Reason") ? null : reader.GetString("Reason"),
                        IsActive = reader.GetBoolean("IsActive"),
                        CreatedBy = reader.GetInt32("CreatedBy"),
                        DateCreated = reader.GetDateTime("DateCreated")
                    });

                    totalCount = reader.GetInt64("TotalCount");
                }

                return new AccessDelegationPagedDto
                {
                    Delegations = delegations,
                    PageNumber = queryDto.PageNumber,
                    PageSize = queryDto.PageSize,
                    TotalCount = totalCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paginated delegations by from user: {UserId}", queryDto.UserId);
                throw;
            }
        }

        /// <summary>
        /// Gets paginated delegations by to user
        /// </summary>
        public async Task<AccessDelegationPagedDto> GetDelegationsByToUserPagedAsync(UserDelegationQueryDto queryDto)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                using var command = new NpgsqlCommand("SELECT * FROM sp_um_get_delegations_by_to_user_paged(@p_to_user_id, @p_page_number, @p_page_size, @p_include_inactive)", connection);
                
                command.Parameters.AddWithValue("p_to_user_id", queryDto.UserId);
                command.Parameters.AddWithValue("p_page_number", queryDto.PageNumber);
                command.Parameters.AddWithValue("p_page_size", queryDto.PageSize);
                command.Parameters.AddWithValue("p_include_inactive", queryDto.IncludeInactive);

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();

                var delegations = new List<AccessDelegationDto>();
                long totalCount = 0;

                while (await reader.ReadAsync())
                {
                    delegations.Add(new AccessDelegationDto
                    {
                        DelegationId = reader.GetInt32("DelegationId"),
                        FromUserId = reader.GetInt32("FromUserId"),
                        ToUserId = reader.GetInt32("ToUserId"),
                        StartDate = reader.GetDateTime("StartDate"),
                        EndDate = reader.GetDateTime("EndDate"),
                        Reason = reader.IsDBNull("Reason") ? null : reader.GetString("Reason"),
                        IsActive = reader.GetBoolean("IsActive"),
                        CreatedBy = reader.GetInt32("CreatedBy"),
                        DateCreated = reader.GetDateTime("DateCreated")
                    });

                    totalCount = reader.GetInt64("TotalCount");
                }

                return new AccessDelegationPagedDto
                {
                    Delegations = delegations,
                    PageNumber = queryDto.PageNumber,
                    PageSize = queryDto.PageSize,
                    TotalCount = totalCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paginated delegations by to user: {UserId}", queryDto.UserId);
                throw;
            }
        }

        /// <summary>
        /// Gets delegations by date range with pagination
        /// </summary>
        public async Task<AccessDelegationPagedDto> GetDelegationsByDateRangePagedAsync(DateRangeQueryDto queryDto)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                using var command = new NpgsqlCommand("SELECT * FROM sp_um_get_delegations_by_date_range_paged(@p_start_date, @p_end_date, @p_page_number, @p_page_size, @p_active_only)", connection);
                
                command.Parameters.AddWithValue("p_start_date", queryDto.StartDate ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("p_end_date", queryDto.EndDate ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("p_page_number", queryDto.PageNumber);
                command.Parameters.AddWithValue("p_page_size", queryDto.PageSize);
                command.Parameters.AddWithValue("p_active_only", queryDto.ActiveOnly);

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();

                var delegations = new List<AccessDelegationDto>();
                long totalCount = 0;

                while (await reader.ReadAsync())
                {
                    delegations.Add(new AccessDelegationDto
                    {
                        DelegationId = reader.GetInt32("DelegationId"),
                        FromUserId = reader.GetInt32("FromUserId"),
                        ToUserId = reader.GetInt32("ToUserId"),
                        StartDate = reader.GetDateTime("StartDate"),
                        EndDate = reader.GetDateTime("EndDate"),
                        Reason = reader.IsDBNull("Reason") ? null : reader.GetString("Reason"),
                        IsActive = reader.GetBoolean("IsActive"),
                        CreatedBy = reader.GetInt32("CreatedBy"),
                        DateCreated = reader.GetDateTime("DateCreated")
                    });

                    totalCount = reader.GetInt64("TotalCount");
                }

                return new AccessDelegationPagedDto
                {
                    Delegations = delegations,
                    PageNumber = queryDto.PageNumber,
                    PageSize = queryDto.PageSize,
                    TotalCount = totalCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting delegations by date range");
                throw;
            }
        }

        /// <summary>
        /// Gets current active delegations with pagination
        /// </summary>
        public async Task<AccessDelegationPagedDto> GetCurrentActiveDelegationsAsync(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                using var command = new NpgsqlCommand("SELECT * FROM sp_um_get_current_active_delegations(@p_page_number, @p_page_size)", connection);
                
                command.Parameters.AddWithValue("p_page_number", pageNumber);
                command.Parameters.AddWithValue("p_page_size", pageSize);

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();

                var delegations = new List<AccessDelegationDto>();
                long totalCount = 0;

                while (await reader.ReadAsync())
                {
                    delegations.Add(new AccessDelegationDto
                    {
                        DelegationId = reader.GetInt32("DelegationId"),
                        FromUserId = reader.GetInt32("FromUserId"),
                        ToUserId = reader.GetInt32("ToUserId"),
                        StartDate = reader.GetDateTime("StartDate"),
                        EndDate = reader.GetDateTime("EndDate"),
                        Reason = reader.IsDBNull("Reason") ? null : reader.GetString("Reason"),
                        IsActive = reader.GetBoolean("IsActive"),
                        CreatedBy = reader.GetInt32("CreatedBy"),
                        DateCreated = reader.GetDateTime("DateCreated")
                    });

                    totalCount = reader.GetInt64("TotalCount");
                }

                return new AccessDelegationPagedDto
                {
                    Delegations = delegations,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current active delegations");
                throw;
            }
        }

        /// <summary>
        /// Deactivates expired delegations
        /// </summary>
        public async Task<int> DeactivateExpiredDelegationsAsync()
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                using var command = new NpgsqlCommand("sp_um_deactivate_expired_delegations", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                var updatedCountParam = new NpgsqlParameter("p_delegations_updated", NpgsqlTypes.NpgsqlDbType.Integer)
                {
                    Direction = ParameterDirection.InputOutput,
                    Value = 0
                };

                command.Parameters.Add(updatedCountParam);

                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();

                return Convert.ToInt32(updatedCountParam.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating expired delegations");
                throw;
            }
        }

        /// <summary>
        /// Checks if a user has active delegation to another user
        /// </summary>
        public async Task<bool> CheckUserHasActiveDelegationAsync(int fromUserId, int toUserId)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                using var command = new NpgsqlCommand("SELECT sp_um_check_user_has_active_delegation(@p_from_user_id, @p_to_user_id)", connection);
                
                command.Parameters.AddWithValue("p_from_user_id", fromUserId);
                command.Parameters.AddWithValue("p_to_user_id", toUserId);

                await connection.OpenAsync();
                var result = await command.ExecuteScalarAsync();

                return Convert.ToBoolean(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking user has active delegation: {FromUserId} to {ToUserId}", fromUserId, toUserId);
                throw;
            }
        }

        /// <summary>
        /// Gets delegation history for a user (both as delegator and delegate)
        /// </summary>
        public async Task<DelegationHistoryPagedDto> GetDelegationHistoryByUserAsync(int userId, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                using var command = new NpgsqlCommand("SELECT * FROM sp_um_get_delegation_history_by_user(@p_user_id, @p_page_number, @p_page_size)", connection);
                
                command.Parameters.AddWithValue("p_user_id", userId);
                command.Parameters.AddWithValue("p_page_number", pageNumber);
                command.Parameters.AddWithValue("p_page_size", pageSize);

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();

                var history = new List<DelegationHistoryDto>();
                long totalCount = 0;

                while (await reader.ReadAsync())
                {
                    history.Add(new DelegationHistoryDto
                    {
                        DelegationId = reader.GetInt32("DelegationId"),
                        FromUserId = reader.GetInt32("FromUserId"),
                        ToUserId = reader.GetInt32("ToUserId"),
                        StartDate = reader.GetDateTime("StartDate"),
                        EndDate = reader.GetDateTime("EndDate"),
                        Reason = reader.IsDBNull("Reason") ? null : reader.GetString("Reason"),
                        IsActive = reader.GetBoolean("IsActive"),
                        CreatedBy = reader.GetInt32("CreatedBy"),
                        DateCreated = reader.GetDateTime("DateCreated"),
                        DelegationRole = reader.GetString("DelegationRole")
                    });

                    totalCount = reader.GetInt64("TotalCount");
                }

                return new DelegationHistoryPagedDto
                {
                    History = history,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting delegation history by user: {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Extends the end date of a delegation
        /// </summary>
        public async Task<bool> ExtendDelegationAsync(ExtendDelegationDto extendDto)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                using var command = new NpgsqlCommand("sp_um_extend_delegation", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.AddWithValue("p_delegation_id", extendDto.DelegationId);
                command.Parameters.AddWithValue("p_new_end_date", extendDto.NewEndDate);

                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extending delegation: {DelegationId}", extendDto.DelegationId);
                return false;
            }
        }

        /// <summary>
        /// Searches delegations with advanced filters
        /// </summary>
        public async Task<AccessDelegationPagedDto> SearchDelegationsAsync(DelegationSearchDto searchDto)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                using var command = new NpgsqlCommand("SELECT * FROM sp_um_search_delegations(@p_from_user_id, @p_to_user_id, @p_start_date, @p_end_date, @p_is_active, @p_search_text, @p_page_number, @p_page_size)", connection);
                
                command.Parameters.AddWithValue("p_from_user_id", searchDto.FromUserId ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("p_to_user_id", searchDto.ToUserId ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("p_start_date", searchDto.StartDate ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("p_end_date", searchDto.EndDate ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("p_is_active", searchDto.IsActive ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("p_search_text", searchDto.SearchText ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("p_page_number", searchDto.PageNumber);
                command.Parameters.AddWithValue("p_page_size", searchDto.PageSize);

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();

                var delegations = new List<AccessDelegationDto>();
                long totalCount = 0;

                while (await reader.ReadAsync())
                {
                    delegations.Add(new AccessDelegationDto
                    {
                        DelegationId = reader.GetInt32("DelegationId"),
                        FromUserId = reader.GetInt32("FromUserId"),
                        ToUserId = reader.GetInt32("ToUserId"),
                        StartDate = reader.GetDateTime("StartDate"),
                        EndDate = reader.GetDateTime("EndDate"),
                        Reason = reader.IsDBNull("Reason") ? null : reader.GetString("Reason"),
                        IsActive = reader.GetBoolean("IsActive"),
                        CreatedBy = reader.GetInt32("CreatedBy"),
                        DateCreated = reader.GetDateTime("DateCreated")
                    });

                    totalCount = reader.GetInt64("TotalCount");
                }

                return new AccessDelegationPagedDto
                {
                    Delegations = delegations,
                    PageNumber = searchDto.PageNumber,
                    PageSize = searchDto.PageSize,
                    TotalCount = totalCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching delegations");
                throw;
            }
        }

        /// <summary>
        /// Gets delegation statistics
        /// </summary>
        public async Task<DelegationStatisticsDto> GetDelegationStatisticsAsync(DelegationStatsQueryDto queryDto)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                using var command = new NpgsqlCommand("SELECT * FROM sp_um_get_delegation_stats(@p_start_date, @p_end_date)", connection);
                
                command.Parameters.AddWithValue("p_start_date", queryDto.StartDate ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("p_end_date", queryDto.EndDate ?? (object)DBNull.Value);

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return new DelegationStatisticsDto
                    {
                        TotalDelegations = reader.GetInt64("TotalDelegations"),
                        ActiveDelegations = reader.GetInt64("ActiveDelegations"),
                        ExpiredDelegations = reader.GetInt64("ExpiredDelegations"),
                        UpcomingDelegations = reader.GetInt64("UpcomingDelegations"),
                        AverageDurationDays = reader.GetDecimal("AverageDurationDays")
                    };
                }

                return new DelegationStatisticsDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting delegation statistics");
                throw;
            }
        }

        /// <summary>
        /// Gets most active delegators
        /// </summary>
        public async Task<List<UserDelegationActivityDto>> GetMostActiveDelegatorsAsync(MostActiveUsersQueryDto queryDto)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                using var command = new NpgsqlCommand("SELECT * FROM sp_um_get_most_active_delegators(@p_limit, @p_start_date, @p_end_date)", connection);
                
                command.Parameters.AddWithValue("p_limit", queryDto.Limit);
                command.Parameters.AddWithValue("p_start_date", queryDto.StartDate ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("p_end_date", queryDto.EndDate ?? (object)DBNull.Value);

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();

                var delegators = new List<UserDelegationActivityDto>();

                while (await reader.ReadAsync())
                {
                    delegators.Add(new UserDelegationActivityDto
                    {
                        UserId = reader.GetInt32("UserId"),
                        DelegationCount = reader.GetInt64("DelegationCount")
                    });
                }

                return delegators;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting most active delegators");
                throw;
            }
        }

        /// <summary>
        /// Gets most popular delegates
        /// </summary>
        public async Task<List<UserDelegationActivityDto>> GetMostPopularDelegatesAsync(MostActiveUsersQueryDto queryDto)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                using var command = new NpgsqlCommand("SELECT * FROM sp_um_get_most_popular_delegates(@p_limit, @p_start_date, @p_end_date)", connection);
                
                command.Parameters.AddWithValue("p_limit", queryDto.Limit);
                command.Parameters.AddWithValue("p_start_date", queryDto.StartDate ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("p_end_date", queryDto.EndDate ?? (object)DBNull.Value);

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();

                var delegates = new List<UserDelegationActivityDto>();

                while (await reader.ReadAsync())
                {
                    delegates.Add(new UserDelegationActivityDto
                    {
                        UserId = reader.GetInt32("UserId"),
                        DelegationCount = reader.GetInt64("DelegationCount")
                    });
                }

                return delegates;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting most popular delegates");
                throw;
            }
        }
    }
}
