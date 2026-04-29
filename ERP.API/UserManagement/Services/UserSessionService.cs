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
    /// Implementation of the user session management service
    /// </summary>
    public class UserSessionService : IUserSessionService
    {
        private readonly string _connectionString;
        private readonly ILogger<UserSessionService> _logger;

        /// <summary>
        /// Constructor for the user session service
        /// </summary>
        /// <param name="connectionString">Database connection string</param>
        /// <param name="logger">Logger for logging</param>
        public UserSessionService(string connectionString, ILogger<UserSessionService> logger)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<UserSessionDTO> CreateSessionAsync(CreateUserSessionDTO sessionDTO)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var parameters = new
                {
                    p_session_id = sessionDTO.SessionId,
                    p_user_id = sessionDTO.UserId,
                    p_ip_address = sessionDTO.IpAddress,
                    p_device_info = sessionDTO.DeviceInfo,
                    p_user_agent = sessionDTO.UserAgent
                };

                var result = await connection.QueryFirstOrDefaultAsync<UserSessionDTO>(
                    "SELECT * FROM public.sp_create_user_session(@p_session_id, @p_user_id, @p_ip_address, @p_device_info, @p_user_agent)",
                    parameters);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user session for user {UserId}", sessionDTO.UserId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<UserSessionDTO?> GetSessionByIdAsync(string sessionId)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var parameters = new
                {
                    p_session_id = sessionId
                };

                var result = await connection.QueryFirstOrDefaultAsync<UserSessionDTO>(
                    "SELECT * FROM public.sp_get_user_session_by_id(@p_session_id)",
                    parameters);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting session by ID {SessionId}", sessionId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<UserSessionDTO?> UpdateSessionAsync(string sessionId, UpdateUserSessionDTO updateDTO)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var parameters = new
                {
                    p_session_id = sessionId,
                    p_logout_time = updateDTO.LogoutTime,
                    p_is_active = updateDTO.IsActive
                };

                var result = await connection.QueryFirstOrDefaultAsync<UserSessionDTO>(
                    "SELECT * FROM public.sp_update_user_session(@p_session_id, @p_logout_time, @p_is_active)",
                    parameters);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating session {SessionId}", sessionId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteSessionAsync(string sessionId)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var parameters = new
                {
                    p_session_id = sessionId
                };

                var result = await connection.ExecuteScalarAsync<bool>(
                    "SELECT * FROM public.sp_delete_user_session(@p_session_id)",
                    parameters);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting session {SessionId}", sessionId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> EndSessionAsync(string sessionId)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var parameters = new
                {
                    p_session_id = sessionId
                };

                var result = await connection.ExecuteScalarAsync<bool>(
                    "SELECT * FROM public.sp_end_user_session(@p_session_id)",
                    parameters);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ending session {SessionId}", sessionId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<int> EndAllUserSessionsAsync(int userId, string? currentSessionId = null)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var parameters = new
                {
                    p_user_id = userId,
                    p_current_session_id = currentSessionId
                };

                var result = await connection.ExecuteScalarAsync<int>(
                    "SELECT * FROM public.sp_end_all_user_sessions(@p_user_id, @p_current_session_id)",
                    parameters);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ending all sessions for user {UserId}", userId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<UserSessionHistoryDTO> GetActiveSessionsAsync(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var parameters = new
                {
                    p_page_number = pageNumber,
                    p_page_size = pageSize
                };

                var sessions = await connection.QueryAsync<UserSessionDTO>(
                    "SELECT * FROM public.sp_get_active_sessions(@p_page_number, @p_page_size)",
                    parameters);

                // Extract total count from the first record
                var sessionList = sessions.AsList();
                var totalCount = sessionList.Count > 0 
                    ? Convert.ToInt64(sessionList[0].GetType().GetProperty("TotalCount")?.GetValue(sessionList[0], null) ?? 0)
                    : 0L;

                return new UserSessionHistoryDTO
                {
                    Sessions = sessionList,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active sessions");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<UserSessionHistoryDTO> GetUserSessionHistoryAsync(int userId, SessionHistoryQueryParams queryParams)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var parameters = new
                {
                    p_user_id = userId,
                    p_start_date = queryParams.StartDate,
                    p_end_date = queryParams.EndDate,
                    p_page_number = queryParams.PageNumber,
                    p_page_size = queryParams.PageSize
                };

                var sessions = await connection.QueryAsync<UserSessionDTO>(
                    "SELECT * FROM public.sp_get_user_session_history(@p_user_id, @p_start_date, @p_end_date, @p_page_number, @p_page_size)",
                    parameters);

                // Extract total count from the first record
                var sessionList = sessions.AsList();
                var totalCount = sessionList.Count > 0 
                    ? Convert.ToInt64(sessionList[0].GetType().GetProperty("TotalCount")?.GetValue(sessionList[0], null) ?? 0)
                    : 0L;

                return new UserSessionHistoryDTO
                {
                    Sessions = sessionList,
                    TotalCount = totalCount,
                    PageNumber = queryParams.PageNumber,
                    PageSize = queryParams.PageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting session history for user {UserId}", userId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<SessionStatisticsDTO>> GetSessionStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var parameters = new
                {
                    p_start_date = startDate,
                    p_end_date = endDate
                };

                var result = await connection.QueryAsync<SessionStatisticsDTO>(
                    "SELECT * FROM public.sp_get_session_statistics(@p_start_date, @p_end_date)",
                    parameters);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting session statistics");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<UserSessionDTO>> GetUserActiveSessionsAsync(int userId)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var parameters = new
                {
                    p_user_id = userId
                };

                var result = await connection.QueryAsync<UserSessionDTO>(
                    "SELECT * FROM public.sp_get_user_active_sessions(@p_user_id)",
                    parameters);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active sessions for user {UserId}", userId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<int> CleanupOldSessionsAsync(int daysOld = 90)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var parameters = new
                {
                    p_days_old = daysOld
                };

                var result = await connection.ExecuteScalarAsync<int>(
                    "SELECT * FROM public.sp_cleanup_old_sessions(@p_days_old)",
                    parameters);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up old sessions");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<int> FixIncompleteSessionsAsync()
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var result = await connection.ExecuteScalarAsync<int>(
                    "SELECT * FROM public.sp_fix_incomplete_sessions()");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fixing incomplete sessions");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ConcurrentSessionsDTO>> GetConcurrentSessionsAsync(ConcurrentSessionsQueryParams queryParams)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var parameters = new
                {
                    p_start_date = queryParams.StartDate,
                    p_end_date = queryParams.EndDate,
                    p_interval_minutes = queryParams.IntervalMinutes
                };

                var result = await connection.QueryAsync<ConcurrentSessionsDTO>(
                    "SELECT * FROM public.sp_get_concurrent_sessions(@p_start_date, @p_end_date, @p_interval_minutes)",
                    parameters);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting concurrent sessions");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<UserLoginFrequencyDTO>> GetUserLoginFrequencyAsync(int topN = 10, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var parameters = new
                {
                    p_top_n = topN,
                    p_start_date = startDate,
                    p_end_date = endDate
                };

                var result = await connection.QueryAsync<UserLoginFrequencyDTO>(
                    "SELECT * FROM public.sp_get_user_login_frequency(@p_top_n, @p_start_date, @p_end_date)",
                    parameters);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user login frequency");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<UserSessionHistoryDTO> GetSessionsByIpAsync(string ipAddress, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var parameters = new
                {
                    p_ip_address = ipAddress,
                    p_page_number = pageNumber,
                    p_page_size = pageSize
                };

                var sessions = await connection.QueryAsync<UserSessionDTO>(
                    "SELECT * FROM public.sp_get_sessions_by_ip(@p_ip_address, @p_page_number, @p_page_size)",
                    parameters);

                // Extract total count from the first record
                var sessionList = sessions.AsList();
                var totalCount = sessionList.Count > 0 
                    ? Convert.ToInt64(sessionList[0].GetType().GetProperty("TotalCount")?.GetValue(sessionList[0], null) ?? 0)
                    : 0L;

                return new UserSessionHistoryDTO
                {
                    Sessions = sessionList,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sessions by IP {IpAddress}", ipAddress);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<DeviceStatisticsDTO>> GetDeviceStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var parameters = new
                {
                    p_start_date = startDate,
                    p_end_date = endDate
                };

                var result = await connection.QueryAsync<DeviceStatisticsDTO>(
                    "SELECT * FROM public.sp_get_device_statistics(@p_start_date, @p_end_date)",
                    parameters);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting device statistics");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<UserSessionDTO>> GetLongRunningSessionsAsync(int hoursThreshold = 8)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var parameters = new
                {
                    p_hours_threshold = hoursThreshold
                };

                var result = await connection.QueryAsync<UserSessionDTO>(
                    "SELECT * FROM public.sp_get_long_running_sessions(@p_hours_threshold)",
                    parameters);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting long running sessions");
                throw;
            }
        }
    }
}
