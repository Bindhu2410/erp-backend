using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using ERP.API.UserManagement.DTOs;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace ERP.API.UserManagement.Services
{
    /// <summary>
    /// Implementation of the user audit log service
    /// </summary>
    public class UserAuditLogService : IUserAuditLogService
    {
        private readonly string _connectionString;
        private readonly ILogger<UserAuditLogService> _logger;

        /// <summary>
        /// Constructor for the user audit log service
        /// </summary>
        /// <param name="connectionString">Database connection string</param>
        /// <param name="logger">Logger for logging</param>
        public UserAuditLogService(string connectionString, ILogger<UserAuditLogService> logger)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<UserAuditLogDTO> CreateAuditLogAsync(CreateUserAuditLogDTO auditLogDTO)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var parameters = new DynamicParameters();
                parameters.Add("p_user_id", auditLogDTO.UserId);
                parameters.Add("p_action_type", auditLogDTO.ActionType);
                parameters.Add("p_entity_type", auditLogDTO.EntityType);
                parameters.Add("p_entity_id", auditLogDTO.EntityId);
                parameters.Add("p_description", auditLogDTO.Description);
                parameters.Add("p_old_value", auditLogDTO.OldValue);
                parameters.Add("p_new_value", auditLogDTO.NewValue);
                parameters.Add("p_ip_address", auditLogDTO.IpAddress);
                parameters.Add("p_audit_id", dbType: DbType.Int32, direction: ParameterDirection.InputOutput);

                await connection.ExecuteAsync(
                    "CALL public.sp_insert_user_audit_log(@p_user_id, @p_action_type, @p_entity_type, @p_entity_id, @p_description, @p_old_value, @p_new_value, @p_ip_address, @p_audit_id)",
                    parameters);

                int auditId = parameters.Get<int>("p_audit_id");

                return await GetAuditLogByIdAsync(auditId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating audit log entry for user {UserId}", auditLogDTO.UserId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<UserAuditLogDTO?> GetAuditLogByIdAsync(int auditId)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var result = await connection.QueryFirstOrDefaultAsync<UserAuditLogDTO>(
                    "SELECT * FROM public.sp_get_user_audit_log_by_id(@p_audit_id)",
                    new { p_audit_id = auditId });

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving audit log entry with ID {AuditId}", auditId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<UserAuditLogDTO?> UpdateAuditLogAsync(int auditId, UpdateUserAuditLogDTO updateDTO)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                await connection.ExecuteAsync(
                    "CALL public.sp_update_user_audit_log(@p_audit_id, @p_description, @p_old_value, @p_new_value)",
                    new
                    {
                        p_audit_id = auditId,
                        p_description = updateDTO.Description,
                        p_old_value = updateDTO.OldValue,
                        p_new_value = updateDTO.NewValue
                    });

                return await GetAuditLogByIdAsync(auditId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating audit log entry with ID {AuditId}", auditId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAuditLogAsync(int auditId)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                await connection.ExecuteAsync(
                    "CALL public.sp_delete_user_audit_log(@p_audit_id)",
                    new { p_audit_id = auditId });

                // Check if the record still exists
                var exists = await connection.QueryFirstOrDefaultAsync<int>(
                    "SELECT COUNT(*) FROM public.\"UserAuditLog\" WHERE \"AuditId\" = @auditId",
                    new { auditId });

                return exists == 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting audit log entry with ID {AuditId}", auditId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<UserAuditLogListDTO> GetAllAuditLogsAsync(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var results = await connection.QueryAsync<UserAuditLogDTO>(
                    "SELECT * FROM public.sp_get_user_audit_logs_paged(@p_page_number, @p_page_size)",
                    new { p_page_number = pageNumber, p_page_size = pageSize });

                long totalCount = 0;
                if (results.Any())
                {
                    // Get total count from the first result
                    var firstResult = results.First();
                    var resultType = firstResult.GetType();
                    var totalCountProperty = resultType.GetProperty("TotalCount");
                    if (totalCountProperty != null)
                    {
                        totalCount = (long)totalCountProperty.GetValue(firstResult, null);
                    }
                }

                return new UserAuditLogListDTO
                {
                    AuditLogs = results.ToList(),
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving audit logs for page {PageNumber}", pageNumber);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<UserAuditLogListDTO> GetUserAuditLogsAsync(int userId, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var results = await connection.QueryAsync<UserAuditLogDTO>(
                    "SELECT * FROM public.sp_get_user_audit_logs_by_user_id_paged(@p_user_id, @p_page_number, @p_page_size)",
                    new { p_user_id = userId, p_page_number = pageNumber, p_page_size = pageSize });

                long totalCount = 0;
                if (results.Any())
                {
                    // Get total count from the first result
                    var firstResult = results.First();
                    var resultType = firstResult.GetType();
                    var totalCountProperty = resultType.GetProperty("TotalCount");
                    if (totalCountProperty != null)
                    {
                        totalCount = (long)totalCountProperty.GetValue(firstResult, null);
                    }
                }

                return new UserAuditLogListDTO
                {
                    AuditLogs = results.ToList(),
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving audit logs for user {UserId} on page {PageNumber}", userId, pageNumber);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<UserAuditLogListDTO> GetEntityAuditLogsAsync(string entityType, int entityId, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var results = await connection.QueryAsync<UserAuditLogDTO>(
                    "SELECT * FROM public.sp_get_user_audit_logs_by_entity_paged(@p_entity_type, @p_entity_id, @p_page_number, @p_page_size)",
                    new { p_entity_type = entityType, p_entity_id = entityId, p_page_number = pageNumber, p_page_size = pageSize });

                long totalCount = 0;
                if (results.Any())
                {
                    // Get total count from the first result
                    var firstResult = results.First();
                    var resultType = firstResult.GetType();
                    var totalCountProperty = resultType.GetProperty("TotalCount");
                    if (totalCountProperty != null)
                    {
                        totalCount = (long)totalCountProperty.GetValue(firstResult, null);
                    }
                }

                return new UserAuditLogListDTO
                {
                    AuditLogs = results.ToList(),
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving audit logs for entity {EntityType} with ID {EntityId} on page {PageNumber}", 
                    entityType, entityId, pageNumber);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<UserAuditLogListDTO> GetAuditLogsByActionTypeAsync(string actionType, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var results = await connection.QueryAsync<UserAuditLogDTO>(
                    "SELECT * FROM public.sp_get_user_audit_logs_by_action_type_paged(@p_action_type, @p_page_number, @p_page_size)",
                    new { p_action_type = actionType, p_page_number = pageNumber, p_page_size = pageSize });

                long totalCount = 0;
                if (results.Any())
                {
                    // Get total count from the first result
                    var firstResult = results.First();
                    var resultType = firstResult.GetType();
                    var totalCountProperty = resultType.GetProperty("TotalCount");
                    if (totalCountProperty != null)
                    {
                        totalCount = (long)totalCountProperty.GetValue(firstResult, null);
                    }
                }

                return new UserAuditLogListDTO
                {
                    AuditLogs = results.ToList(),
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving audit logs for action type {ActionType} on page {PageNumber}", 
                    actionType, pageNumber);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<UserAuditLogListDTO> GetAuditLogsByDateRangeAsync(DateTime startDate, DateTime endDate, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var results = await connection.QueryAsync<UserAuditLogDTO>(
                    "SELECT * FROM public.sp_get_user_audit_logs_by_date_range_paged(@p_start_date, @p_end_date, @p_page_number, @p_page_size)",
                    new { p_start_date = startDate, p_end_date = endDate, p_page_number = pageNumber, p_page_size = pageSize });

                long totalCount = 0;
                if (results.Any())
                {
                    // Get total count from the first result
                    var firstResult = results.First();
                    var resultType = firstResult.GetType();
                    var totalCountProperty = resultType.GetProperty("TotalCount");
                    if (totalCountProperty != null)
                    {
                        totalCount = (long)totalCountProperty.GetValue(firstResult, null);
                    }
                }

                return new UserAuditLogListDTO
                {
                    AuditLogs = results.ToList(),
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving audit logs between {StartDate} and {EndDate} on page {PageNumber}", 
                    startDate, endDate, pageNumber);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<UserAuditLogListDTO> GetAuditLogsByIpAddressAsync(string ipAddress, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var results = await connection.QueryAsync<UserAuditLogDTO>(
                    "SELECT * FROM public.sp_get_user_audit_logs_by_ip_address_paged(@p_ip_address, @p_page_number, @p_page_size)",
                    new { p_ip_address = ipAddress, p_page_number = pageNumber, p_page_size = pageSize });

                long totalCount = 0;
                if (results.Any())
                {
                    // Get total count from the first result
                    var firstResult = results.First();
                    var resultType = firstResult.GetType();
                    var totalCountProperty = resultType.GetProperty("TotalCount");
                    if (totalCountProperty != null)
                    {
                        totalCount = (long)totalCountProperty.GetValue(firstResult, null);
                    }
                }

                return new UserAuditLogListDTO
                {
                    AuditLogs = results.ToList(),
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving audit logs for IP address {IpAddress} on page {PageNumber}", 
                    ipAddress, pageNumber);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<UserAuditLogListDTO> SearchAuditLogsAsync(UserAuditLogSearchDTO searchDTO)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var parameters = new
                {
                    p_user_id = searchDTO.UserId,
                    p_action_type = searchDTO.ActionType,
                    p_entity_type = searchDTO.EntityType,
                    p_entity_id = searchDTO.EntityId,
                    p_start_date = searchDTO.StartDate,
                    p_end_date = searchDTO.EndDate,
                    p_ip_address = searchDTO.IpAddress,
                    p_search_text = searchDTO.SearchText,
                    p_page_number = searchDTO.PageNumber,
                    p_page_size = searchDTO.PageSize
                };

                var results = await connection.QueryAsync<UserAuditLogDTO>(
                    "SELECT * FROM public.sp_get_user_audit_logs_advanced_search(@p_user_id, @p_action_type, @p_entity_type, @p_entity_id, " +
                    "@p_start_date, @p_end_date, @p_ip_address, @p_search_text, @p_page_number, @p_page_size)",
                    parameters);

                long totalCount = 0;
                if (results.Any())
                {
                    // Get total count from the first result
                    var firstResult = results.First();
                    var resultType = firstResult.GetType();
                    var totalCountProperty = resultType.GetProperty("TotalCount");
                    if (totalCountProperty != null)
                    {
                        totalCount = (long)totalCountProperty.GetValue(firstResult, null);
                    }
                }

                return new UserAuditLogListDTO
                {
                    AuditLogs = results.ToList(),
                    TotalCount = totalCount,
                    PageNumber = searchDTO.PageNumber,
                    PageSize = searchDTO.PageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing advanced search on audit logs");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<DailyAuditActivityDTO>> GetDailyAuditActivityAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                startDate ??= DateTime.UtcNow.AddDays(-30);
                endDate ??= DateTime.UtcNow;

                var results = await connection.QueryAsync<DailyAuditActivityDTO>(
                    "SELECT * FROM public.sp_get_daily_audit_log_activity(@p_start_date, @p_end_date)",
                    new { p_start_date = startDate, p_end_date = endDate });

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving daily audit activity between {StartDate} and {EndDate}", 
                    startDate, endDate);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<UserActivityDTO>> GetMostActiveUsersAsync(DateTime? startDate = null, DateTime? endDate = null, int limit = 10)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                startDate ??= DateTime.UtcNow.AddDays(-30);
                endDate ??= DateTime.UtcNow;

                var results = await connection.QueryAsync<UserActivityDTO>(
                    "SELECT * FROM public.sp_get_most_active_users(@p_start_date, @p_end_date, @p_limit)",
                    new { p_start_date = startDate, p_end_date = endDate, p_limit = limit });

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving most active users between {StartDate} and {EndDate}", 
                    startDate, endDate);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ActionTypeDistributionDTO>> GetActionTypeDistributionAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                startDate ??= DateTime.UtcNow.AddDays(-30);
                endDate ??= DateTime.UtcNow;

                var results = await connection.QueryAsync<ActionTypeDistributionDTO>(
                    "SELECT * FROM public.sp_get_action_type_distribution(@p_start_date, @p_end_date)",
                    new { p_start_date = startDate, p_end_date = endDate });

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving action type distribution between {StartDate} and {EndDate}", 
                    startDate, endDate);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<EntityActivityDTO>> GetEntityActivitySummaryAsync(string entityType, DateTime? startDate = null, DateTime? endDate = null, int limit = 10)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                startDate ??= DateTime.UtcNow.AddDays(-30);
                endDate ??= DateTime.UtcNow;

                var results = await connection.QueryAsync<EntityActivityDTO>(
                    "SELECT * FROM public.sp_get_entity_activity_summary(@p_entity_type, @p_start_date, @p_end_date, @p_limit)",
                    new { p_entity_type = entityType, p_start_date = startDate, p_end_date = endDate, p_limit = limit });

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving activity summary for entity type {EntityType}", entityType);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<RecentUserActivityDTO>> GetRecentUserActivityAsync(int userId, int limit = 10)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var results = await connection.QueryAsync<RecentUserActivityDTO>(
                    "SELECT * FROM public.sp_get_recent_user_activity(@p_user_id, @p_limit)",
                    new { p_user_id = userId, p_limit = limit });

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recent activity for user {UserId}", userId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<EntityHistoryDTO>> GetEntityHistoryAsync(string entityType, int entityId)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var results = await connection.QueryAsync<EntityHistoryDTO>(
                    "SELECT * FROM public.sp_get_entity_history(@p_entity_type, @p_entity_id)",
                    new { p_entity_type = entityType, p_entity_id = entityId });

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving history for entity {EntityType} with ID {EntityId}", 
                    entityType, entityId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<UserSessionActivityDTO>> GetUserSessionActivityAsync(int? userId = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                startDate ??= DateTime.UtcNow.AddDays(-30);
                endDate ??= DateTime.UtcNow;

                var results = await connection.QueryAsync<UserSessionActivityDTO>(
                    "SELECT * FROM public.sp_get_user_session_activity(@p_user_id, @p_start_date, @p_end_date)",
                    new { p_user_id = userId, p_start_date = startDate, p_end_date = endDate });

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving session activity for user {UserId}", userId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FailedLoginDTO>> GetFailedLoginAttemptsAsync(int? userId = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                startDate ??= DateTime.UtcNow.AddDays(-7);
                endDate ??= DateTime.UtcNow;

                var results = await connection.QueryAsync<FailedLoginDTO>(
                    "SELECT * FROM public.sp_get_failed_login_attempts(@p_user_id, @p_start_date, @p_end_date)",
                    new { p_user_id = userId, p_start_date = startDate, p_end_date = endDate });

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving failed login attempts for user {UserId}", userId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<int> CleanupOldAuditLogsAsync(int daysOld = 365)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var parameters = new DynamicParameters();
                parameters.Add("p_days_old", daysOld);
                parameters.Add("p_rows_deleted", dbType: DbType.Int32, direction: ParameterDirection.InputOutput);

                await connection.ExecuteAsync("CALL public.sp_cleanup_old_audit_logs(@p_days_old, @p_rows_deleted)", parameters);

                return parameters.Get<int>("p_rows_deleted");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up old audit logs older than {DaysOld} days", daysOld);
                throw;
            }
        }
    }
}
