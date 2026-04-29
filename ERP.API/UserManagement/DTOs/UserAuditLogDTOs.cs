using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ERP.API.UserManagement.DTOs
{
    /// <summary>
    /// Data transfer object for creating a new user audit log entry
    /// </summary>
    public class CreateUserAuditLogDTO
    {
        /// <summary>
        /// User ID associated with this audit log
        /// </summary>
        [Required]
        public int UserId { get; set; }

        /// <summary>
        /// Type of action performed (Login, Logout, Create, Update, Delete, etc.)
        /// </summary>
        [Required]
        [StringLength(50)]
        public string ActionType { get; set; }

        /// <summary>
        /// Type of entity affected (User, Role, Permission, etc.)
        /// </summary>
        [StringLength(50)]
        public string? EntityType { get; set; }

        /// <summary>
        /// ID of the entity affected
        /// </summary>
        public int? EntityId { get; set; }

        /// <summary>
        /// Description of the action performed
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Previous value before the change (JSON or text representation)
        /// </summary>
        public string? OldValue { get; set; }

        /// <summary>
        /// New value after the change (JSON or text representation)
        /// </summary>
        public string? NewValue { get; set; }

        /// <summary>
        /// IP address of the client
        /// </summary>
        [StringLength(50)]
        public string? IpAddress { get; set; }
    }

    /// <summary>
    /// Data transfer object for a user audit log entry
    /// </summary>
    public class UserAuditLogDTO
    {
        /// <summary>
        /// Unique identifier for the audit log entry
        /// </summary>
        public int AuditId { get; set; }

        /// <summary>
        /// User ID associated with this audit log
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Username associated with this audit log
        /// </summary>
        public string? UserName { get; set; }

        /// <summary>
        /// Type of action performed (Login, Logout, Create, Update, Delete, etc.)
        /// </summary>
        public string ActionType { get; set; }

        /// <summary>
        /// Type of entity affected (User, Role, Permission, etc.)
        /// </summary>
        public string? EntityType { get; set; }

        /// <summary>
        /// ID of the entity affected
        /// </summary>
        public int? EntityId { get; set; }

        /// <summary>
        /// Description of the action performed
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Previous value before the change (JSON or text representation)
        /// </summary>
        public string? OldValue { get; set; }

        /// <summary>
        /// New value after the change (JSON or text representation)
        /// </summary>
        public string? NewValue { get; set; }

        /// <summary>
        /// IP address of the client
        /// </summary>
        public string? IpAddress { get; set; }

        /// <summary>
        /// Timestamp when the action occurred
        /// </summary>
        public DateTime ActionTime { get; set; }
    }

    /// <summary>
    /// Data transfer object for updating a user audit log entry
    /// </summary>
    public class UpdateUserAuditLogDTO
    {
        /// <summary>
        /// Description of the action performed
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Previous value before the change (JSON or text representation)
        /// </summary>
        public string? OldValue { get; set; }

        /// <summary>
        /// New value after the change (JSON or text representation)
        /// </summary>
        public string? NewValue { get; set; }
    }

    /// <summary>
    /// Data transfer object for user audit logs with pagination
    /// </summary>
    public class UserAuditLogListDTO
    {
        /// <summary>
        /// List of audit log entries
        /// </summary>
        public List<UserAuditLogDTO> AuditLogs { get; set; } = new List<UserAuditLogDTO>();

        /// <summary>
        /// Total count of available audit log entries
        /// </summary>
        public long TotalCount { get; set; }

        /// <summary>
        /// Current page number
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// Page size
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Total number of pages
        /// </summary>
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    }

    /// <summary>
    /// Data transfer object for audit log search filters
    /// </summary>
    public class UserAuditLogSearchDTO
    {
        /// <summary>
        /// User ID to filter by
        /// </summary>
        public int? UserId { get; set; }

        /// <summary>
        /// Action type to filter by
        /// </summary>
        public string? ActionType { get; set; }

        /// <summary>
        /// Entity type to filter by
        /// </summary>
        public string? EntityType { get; set; }

        /// <summary>
        /// Entity ID to filter by
        /// </summary>
        public int? EntityId { get; set; }

        /// <summary>
        /// Start date for filtering by date range
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// End date for filtering by date range
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// IP address to filter by
        /// </summary>
        public string? IpAddress { get; set; }

        /// <summary>
        /// Text to search for in description, old value, or new value fields
        /// </summary>
        public string? SearchText { get; set; }

        /// <summary>
        /// Page number for pagination
        /// </summary>
        [Range(1, int.MaxValue)]
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// Page size for pagination
        /// </summary>
        [Range(1, 100)]
        public int PageSize { get; set; } = 10;
    }

    /// <summary>
    /// Data transfer object for daily audit log activity
    /// </summary>
    public class DailyAuditActivityDTO
    {
        /// <summary>
        /// Date of the activity
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Count of actions performed on this date
        /// </summary>
        public long ActionCount { get; set; }
    }

    /// <summary>
    /// Data transfer object for most active users
    /// </summary>
    public class UserActivityDTO
    {
        /// <summary>
        /// User ID
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Username (optional, can be populated from User service)
        /// </summary>
        public string? UserName { get; set; }

        /// <summary>
        /// Count of actions performed by this user
        /// </summary>
        public long ActionCount { get; set; }
    }

    /// <summary>
    /// Data transfer object for action type distribution
    /// </summary>
    public class ActionTypeDistributionDTO
    {
        /// <summary>
        /// Type of action
        /// </summary>
        public string ActionType { get; set; }

        /// <summary>
        /// Count of this action type
        /// </summary>
        public long ActionCount { get; set; }

        /// <summary>
        /// Percentage of this action type among all actions
        /// </summary>
        public decimal Percentage { get; set; }
    }

    /// <summary>
    /// Data transfer object for entity activity summary
    /// </summary>
    public class EntityActivityDTO
    {
        /// <summary>
        /// Entity ID
        /// </summary>
        public int EntityId { get; set; }

        /// <summary>
        /// Count of actions performed on this entity
        /// </summary>
        public long ActionCount { get; set; }
    }

    /// <summary>
    /// Data transfer object for recent user activity with time information
    /// </summary>
    public class RecentUserActivityDTO
    {
        /// <summary>
        /// Audit log ID
        /// </summary>
        public int AuditId { get; set; }

        /// <summary>
        /// Type of action performed
        /// </summary>
        public string ActionType { get; set; }

        /// <summary>
        /// Type of entity affected
        /// </summary>
        public string? EntityType { get; set; }

        /// <summary>
        /// ID of the entity affected
        /// </summary>
        public int? EntityId { get; set; }

        /// <summary>
        /// Description of the action performed
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Timestamp when the action occurred
        /// </summary>
        public DateTime ActionTime { get; set; }

        /// <summary>
        /// Number of days ago this action occurred
        /// </summary>
        public int DaysAgo { get; set; }

        /// <summary>
        /// Number of hours ago this action occurred (modulo 24)
        /// </summary>
        public int HoursAgo { get; set; }

        /// <summary>
        /// Number of minutes ago this action occurred (modulo 60)
        /// </summary>
        public int MinutesAgo { get; set; }
    }

    /// <summary>
    /// Data transfer object for entity history
    /// </summary>
    public class EntityHistoryDTO
    {
        /// <summary>
        /// Audit log ID
        /// </summary>
        public int AuditId { get; set; }

        /// <summary>
        /// User ID who performed the action
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Type of action performed
        /// </summary>
        public string ActionType { get; set; }

        /// <summary>
        /// Description of the action performed
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Previous value before the change
        /// </summary>
        public string? OldValue { get; set; }

        /// <summary>
        /// New value after the change
        /// </summary>
        public string? NewValue { get; set; }

        /// <summary>
        /// Timestamp when the action occurred
        /// </summary>
        public DateTime ActionTime { get; set; }
    }

    /// <summary>
    /// Data transfer object for user session activity (for security monitoring)
    /// </summary>
    public class UserSessionActivityDTO
    {
        /// <summary>
        /// IP address
        /// </summary>
        public string IpAddress { get; set; }

        /// <summary>
        /// Number of logins from this IP address
        /// </summary>
        public long LoginCount { get; set; }

        /// <summary>
        /// Timestamp of the last login from this IP address
        /// </summary>
        public DateTime LastLogin { get; set; }
    }

    /// <summary>
    /// Data transfer object for failed login attempts
    /// </summary>
    public class FailedLoginDTO
    {
        /// <summary>
        /// User ID
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// IP address
        /// </summary>
        public string? IpAddress { get; set; }

        /// <summary>
        /// Number of failed login attempts
        /// </summary>
        public long FailureCount { get; set; }

        /// <summary>
        /// Timestamp of the last failed login attempt
        /// </summary>
        public DateTime LastFailure { get; set; }
    }
}
