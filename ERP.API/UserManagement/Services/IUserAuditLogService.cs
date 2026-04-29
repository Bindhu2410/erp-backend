using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ERP.API.UserManagement.DTOs;

namespace ERP.API.UserManagement.Services
{
    /// <summary>
    /// Interface for user audit log management services
    /// </summary>
    public interface IUserAuditLogService
    {
        /// <summary>
        /// Creates a new audit log entry
        /// </summary>
        /// <param name="auditLogDTO">The audit log data to create</param>
        /// <returns>The created audit log details</returns>
        Task<UserAuditLogDTO> CreateAuditLogAsync(CreateUserAuditLogDTO auditLogDTO);

        /// <summary>
        /// Gets an audit log entry by its ID
        /// </summary>
        /// <param name="auditId">The audit log ID</param>
        /// <returns>The audit log details if found, null otherwise</returns>
        Task<UserAuditLogDTO?> GetAuditLogByIdAsync(int auditId);

        /// <summary>
        /// Updates an audit log entry (limited fields only)
        /// </summary>
        /// <param name="auditId">The audit log ID to update</param>
        /// <param name="updateDTO">The audit log data to update</param>
        /// <returns>The updated audit log details</returns>
        Task<UserAuditLogDTO?> UpdateAuditLogAsync(int auditId, UpdateUserAuditLogDTO updateDTO);

        /// <summary>
        /// Deletes an audit log entry (should be restricted to admin use)
        /// </summary>
        /// <param name="auditId">The audit log ID to delete</param>
        /// <returns>True if deleted, false if not found</returns>
        Task<bool> DeleteAuditLogAsync(int auditId);

        /// <summary>
        /// Gets all audit logs with pagination
        /// </summary>
        /// <param name="pageNumber">The page number</param>
        /// <param name="pageSize">The page size</param>
        /// <returns>List of audit logs with pagination info</returns>
        Task<UserAuditLogListDTO> GetAllAuditLogsAsync(int pageNumber = 1, int pageSize = 10);

        /// <summary>
        /// Gets audit logs for a user with pagination
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="pageNumber">The page number</param>
        /// <param name="pageSize">The page size</param>
        /// <returns>List of audit logs for the user with pagination info</returns>
        Task<UserAuditLogListDTO> GetUserAuditLogsAsync(int userId, int pageNumber = 1, int pageSize = 10);

        /// <summary>
        /// Gets audit logs for an entity with pagination
        /// </summary>
        /// <param name="entityType">The entity type</param>
        /// <param name="entityId">The entity ID</param>
        /// <param name="pageNumber">The page number</param>
        /// <param name="pageSize">The page size</param>
        /// <returns>List of audit logs for the entity with pagination info</returns>
        Task<UserAuditLogListDTO> GetEntityAuditLogsAsync(string entityType, int entityId, int pageNumber = 1, int pageSize = 10);

        /// <summary>
        /// Gets audit logs by action type with pagination
        /// </summary>
        /// <param name="actionType">The action type</param>
        /// <param name="pageNumber">The page number</param>
        /// <param name="pageSize">The page size</param>
        /// <returns>List of audit logs for the action type with pagination info</returns>
        Task<UserAuditLogListDTO> GetAuditLogsByActionTypeAsync(string actionType, int pageNumber = 1, int pageSize = 10);

        /// <summary>
        /// Gets audit logs by date range with pagination
        /// </summary>
        /// <param name="startDate">The start date</param>
        /// <param name="endDate">The end date</param>
        /// <param name="pageNumber">The page number</param>
        /// <param name="pageSize">The page size</param>
        /// <returns>List of audit logs in the date range with pagination info</returns>
        Task<UserAuditLogListDTO> GetAuditLogsByDateRangeAsync(DateTime startDate, DateTime endDate, int pageNumber = 1, int pageSize = 10);

        /// <summary>
        /// Gets audit logs by IP address with pagination
        /// </summary>
        /// <param name="ipAddress">The IP address</param>
        /// <param name="pageNumber">The page number</param>
        /// <param name="pageSize">The page size</param>
        /// <returns>List of audit logs for the IP address with pagination info</returns>
        Task<UserAuditLogListDTO> GetAuditLogsByIpAddressAsync(string ipAddress, int pageNumber = 1, int pageSize = 10);

        /// <summary>
        /// Performs an advanced search on audit logs with multiple filter criteria
        /// </summary>
        /// <param name="searchDTO">The search criteria</param>
        /// <returns>List of matching audit logs with pagination info</returns>
        Task<UserAuditLogListDTO> SearchAuditLogsAsync(UserAuditLogSearchDTO searchDTO);

        /// <summary>
        /// Gets daily audit log activity counts
        /// </summary>
        /// <param name="startDate">Optional start date, defaults to 30 days ago</param>
        /// <param name="endDate">Optional end date, defaults to current date</param>
        /// <returns>Daily activity counts</returns>
        Task<IEnumerable<DailyAuditActivityDTO>> GetDailyAuditActivityAsync(DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Gets most active users based on audit logs
        /// </summary>
        /// <param name="startDate">Optional start date, defaults to 30 days ago</param>
        /// <param name="endDate">Optional end date, defaults to current date</param>
        /// <param name="limit">Maximum number of users to return</param>
        /// <returns>List of most active users</returns>
        Task<IEnumerable<UserActivityDTO>> GetMostActiveUsersAsync(DateTime? startDate = null, DateTime? endDate = null, int limit = 10);

        /// <summary>
        /// Gets action type distribution statistics
        /// </summary>
        /// <param name="startDate">Optional start date, defaults to 30 days ago</param>
        /// <param name="endDate">Optional end date, defaults to current date</param>
        /// <returns>Action type distribution statistics</returns>
        Task<IEnumerable<ActionTypeDistributionDTO>> GetActionTypeDistributionAsync(DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Gets entity activity summary
        /// </summary>
        /// <param name="entityType">The entity type</param>
        /// <param name="startDate">Optional start date, defaults to 30 days ago</param>
        /// <param name="endDate">Optional end date, defaults to current date</param>
        /// <param name="limit">Maximum number of entities to return</param>
        /// <returns>Entity activity summary</returns>
        Task<IEnumerable<EntityActivityDTO>> GetEntityActivitySummaryAsync(string entityType, DateTime? startDate = null, DateTime? endDate = null, int limit = 10);

        /// <summary>
        /// Gets recent activity for a user with relative time information
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="limit">Maximum number of activities to return</param>
        /// <returns>List of recent user activities</returns>
        Task<IEnumerable<RecentUserActivityDTO>> GetRecentUserActivityAsync(int userId, int limit = 10);

        /// <summary>
        /// Gets history for a specific entity
        /// </summary>
        /// <param name="entityType">The entity type</param>
        /// <param name="entityId">The entity ID</param>
        /// <returns>List of audit log entries for the entity</returns>
        Task<IEnumerable<EntityHistoryDTO>> GetEntityHistoryAsync(string entityType, int entityId);

        /// <summary>
        /// Gets user session activity summary (for security monitoring)
        /// </summary>
        /// <param name="userId">Optional user ID to filter by</param>
        /// <param name="startDate">Optional start date</param>
        /// <param name="endDate">Optional end date</param>
        /// <returns>Session activity summary</returns>
        Task<IEnumerable<UserSessionActivityDTO>> GetUserSessionActivityAsync(int? userId = null, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Gets failed login attempts
        /// </summary>
        /// <param name="userId">Optional user ID to filter by</param>
        /// <param name="startDate">Optional start date</param>
        /// <param name="endDate">Optional end date</param>
        /// <returns>Failed login attempts summary</returns>
        Task<IEnumerable<FailedLoginDTO>> GetFailedLoginAttemptsAsync(int? userId = null, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Cleans up old audit logs
        /// </summary>
        /// <param name="daysOld">Age in days for logs to remove</param>
        /// <returns>Number of logs removed</returns>
        Task<int> CleanupOldAuditLogsAsync(int daysOld = 365);
    }
}
