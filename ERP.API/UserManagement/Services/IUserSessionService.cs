using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ERP.API.UserManagement.DTOs;

namespace ERP.API.UserManagement.Services
{
    /// <summary>
    /// Interface for user session management services
    /// </summary>
    public interface IUserSessionService
    {
        /// <summary>
        /// Creates a new user session
        /// </summary>
        /// <param name="sessionDTO">The session data to create</param>
        /// <returns>The created session details</returns>
        Task<UserSessionDTO> CreateSessionAsync(CreateUserSessionDTO sessionDTO);

        /// <summary>
        /// Gets a session by its ID
        /// </summary>
        /// <param name="sessionId">The session ID</param>
        /// <returns>The session details if found, null otherwise</returns>
        Task<UserSessionDTO?> GetSessionByIdAsync(string sessionId);

        /// <summary>
        /// Updates a user session (primarily for ending a session)
        /// </summary>
        /// <param name="sessionId">The session ID to update</param>
        /// <param name="updateDTO">The session data to update</param>
        /// <returns>The updated session details</returns>
        Task<UserSessionDTO?> UpdateSessionAsync(string sessionId, UpdateUserSessionDTO updateDTO);

        /// <summary>
        /// Deletes a user session
        /// </summary>
        /// <param name="sessionId">The session ID to delete</param>
        /// <returns>True if deleted, false if not found</returns>
        Task<bool> DeleteSessionAsync(string sessionId);

        /// <summary>
        /// Ends a user session (marks as inactive and sets logout time)
        /// </summary>
        /// <param name="sessionId">The session ID to end</param>
        /// <returns>True if ended, false if not found or already inactive</returns>
        Task<bool> EndSessionAsync(string sessionId);

        /// <summary>
        /// Ends all active sessions for a user (except optionally the current session)
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="currentSessionId">Optional current session ID to exclude</param>
        /// <returns>Number of sessions ended</returns>
        Task<int> EndAllUserSessionsAsync(int userId, string? currentSessionId = null);

        /// <summary>
        /// Gets all active sessions with pagination
        /// </summary>
        /// <param name="pageNumber">The page number</param>
        /// <param name="pageSize">The page size</param>
        /// <returns>List of active sessions with pagination info</returns>
        Task<UserSessionHistoryDTO> GetActiveSessionsAsync(int pageNumber = 1, int pageSize = 10);

        /// <summary>
        /// Gets session history for a user with pagination and optional date filtering
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="queryParams">Query parameters including pagination and date filters</param>
        /// <returns>User session history with pagination info</returns>
        Task<UserSessionHistoryDTO> GetUserSessionHistoryAsync(int userId, SessionHistoryQueryParams queryParams);

        /// <summary>
        /// Gets session statistics by date range
        /// </summary>
        /// <param name="startDate">Optional start date</param>
        /// <param name="endDate">Optional end date</param>
        /// <returns>List of daily session statistics</returns>
        Task<IEnumerable<SessionStatisticsDTO>> GetSessionStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Gets active sessions for a user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>List of active user sessions</returns>
        Task<IEnumerable<UserSessionDTO>> GetUserActiveSessionsAsync(int userId);

        /// <summary>
        /// Cleans up old inactive sessions
        /// </summary>
        /// <param name="daysOld">Age in days for sessions to remove</param>
        /// <returns>Number of sessions removed</returns>
        Task<int> CleanupOldSessionsAsync(int daysOld = 90);

        /// <summary>
        /// Fixes incomplete sessions (sessions still marked active but older than 24 hours)
        /// </summary>
        /// <returns>Number of sessions fixed</returns>
        Task<int> FixIncompleteSessionsAsync();

        /// <summary>
        /// Gets concurrent sessions count by time slots
        /// </summary>
        /// <param name="queryParams">Query parameters including date range and interval</param>
        /// <returns>List of time slots with concurrent session counts</returns>
        Task<IEnumerable<ConcurrentSessionsDTO>> GetConcurrentSessionsAsync(ConcurrentSessionsQueryParams queryParams);

        /// <summary>
        /// Gets user login frequency statistics
        /// </summary>
        /// <param name="topN">Number of users to return</param>
        /// <param name="startDate">Optional start date</param>
        /// <param name="endDate">Optional end date</param>
        /// <returns>List of user login statistics</returns>
        Task<IEnumerable<UserLoginFrequencyDTO>> GetUserLoginFrequencyAsync(int topN = 10, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Gets sessions by IP address with pagination
        /// </summary>
        /// <param name="ipAddress">The IP address to search for</param>
        /// <param name="pageNumber">The page number</param>
        /// <param name="pageSize">The page size</param>
        /// <returns>List of sessions from the IP address with pagination info</returns>
        Task<UserSessionHistoryDTO> GetSessionsByIpAsync(string ipAddress, int pageNumber = 1, int pageSize = 10);

        /// <summary>
        /// Gets device usage statistics
        /// </summary>
        /// <param name="startDate">Optional start date</param>
        /// <param name="endDate">Optional end date</param>
        /// <returns>List of device statistics</returns>
        Task<IEnumerable<DeviceStatisticsDTO>> GetDeviceStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Gets long-running active sessions
        /// </summary>
        /// <param name="hoursThreshold">Minimum duration in hours</param>
        /// <returns>List of long-running sessions</returns>
        Task<IEnumerable<UserSessionDTO>> GetLongRunningSessionsAsync(int hoursThreshold = 8);
    }
}
