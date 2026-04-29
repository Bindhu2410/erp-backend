using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ERP.API.UserManagement.DTOs
{
    /// <summary>
    /// Data transfer object for creating a new user session
    /// </summary>
    public class CreateUserSessionDTO
    {
        /// <summary>
        /// Unique identifier for the session
        /// </summary>
        [Required]
        public string SessionId { get; set; }

        /// <summary>
        /// User ID associated with this session
        /// </summary>
        [Required]
        public int UserId { get; set; }

        /// <summary>
        /// IP address of the client
        /// </summary>
        public string? IpAddress { get; set; }

        /// <summary>
        /// Information about the device used
        /// </summary>
        public string? DeviceInfo { get; set; }

        /// <summary>
        /// User agent string from the client browser/application
        /// </summary>
        public string? UserAgent { get; set; }
    }

    /// <summary>
    /// Data transfer object for a user session
    /// </summary>
    public class UserSessionDTO
    {
        /// <summary>
        /// Unique identifier for the session
        /// </summary>
        public string SessionId { get; set; }

        /// <summary>
        /// User ID associated with this session
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Username associated with this session
        /// </summary>
        public string? Username { get; set; }

        /// <summary>
        /// Login timestamp
        /// </summary>
        public DateTime LoginTime { get; set; }

        /// <summary>
        /// Logout timestamp, null if session is still active
        /// </summary>
        public DateTime? LogoutTime { get; set; }

        /// <summary>
        /// IP address of the client
        /// </summary>
        public string? IpAddress { get; set; }

        /// <summary>
        /// Information about the device used
        /// </summary>
        public string? DeviceInfo { get; set; }

        /// <summary>
        /// User agent string from the client browser/application
        /// </summary>
        public string? UserAgent { get; set; }

        /// <summary>
        /// Whether the session is currently active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Duration of the session (calculated property)
        /// </summary>
        public TimeSpan? SessionDuration { get; set; }
    }

    /// <summary>
    /// Data transfer object for updating a user session
    /// </summary>
    public class UpdateUserSessionDTO
    {
        /// <summary>
        /// Logout timestamp, null if session is still active
        /// </summary>
        public DateTime? LogoutTime { get; set; }

        /// <summary>
        /// Whether the session is currently active
        /// </summary>
        public bool? IsActive { get; set; }
    }

    /// <summary>
    /// Data transfer object for user session history with pagination
    /// </summary>
    public class UserSessionHistoryDTO
    {
        /// <summary>
        /// List of user sessions
        /// </summary>
        public List<UserSessionDTO> Sessions { get; set; } = new List<UserSessionDTO>();

        /// <summary>
        /// Total count of available sessions
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
    /// Data transfer object for session statistics by date
    /// </summary>
    public class SessionStatisticsDTO
    {
        /// <summary>
        /// Date for the statistics
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Total number of sessions on this date
        /// </summary>
        public long TotalSessions { get; set; }

        /// <summary>
        /// Number of unique users on this date
        /// </summary>
        public long UniqueUsers { get; set; }

        /// <summary>
        /// Average session duration on this date
        /// </summary>
        public TimeSpan AverageSessionDuration { get; set; }
    }

    /// <summary>
    /// Data transfer object for user login frequency statistics
    /// </summary>
    public class UserLoginFrequencyDTO
    {
        /// <summary>
        /// User ID
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Username
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// User's full name
        /// </summary>
        public string? FullName { get; set; }

        /// <summary>
        /// Number of logins in the period
        /// </summary>
        public long LoginCount { get; set; }

        /// <summary>
        /// Last login timestamp
        /// </summary>
        public DateTime LastLogin { get; set; }
    }

    /// <summary>
    /// Data transfer object for device statistics
    /// </summary>
    public class DeviceStatisticsDTO
    {
        /// <summary>
        /// Device information
        /// </summary>
        public string DeviceInfo { get; set; }

        /// <summary>
        /// Number of users using this device type
        /// </summary>
        public long UserCount { get; set; }

        /// <summary>
        /// Number of sessions from this device type
        /// </summary>
        public long SessionCount { get; set; }

        /// <summary>
        /// Average session duration for this device type
        /// </summary>
        public TimeSpan AverageSessionDuration { get; set; }
    }

    /// <summary>
    /// Parameters for querying user session history
    /// </summary>
    public class SessionHistoryQueryParams
    {
        /// <summary>
        /// Optional start date for filtering
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Optional end date for filtering
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Page number for pagination (default: 1)
        /// </summary>
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// Page size for pagination (default: 10)
        /// </summary>
        public int PageSize { get; set; } = 10;
    }

    /// <summary>
    /// Parameters for concurrent sessions query
    /// </summary>
    public class ConcurrentSessionsQueryParams
    {
        /// <summary>
        /// Start date for the analysis period
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// End date for the analysis period
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Interval in minutes for the time slots (default: 60 minutes)
        /// </summary>
        public int IntervalMinutes { get; set; } = 60;
    }

    /// <summary>
    /// Data transfer object for concurrent sessions time slot
    /// </summary>
    public class ConcurrentSessionsDTO
    {
        /// <summary>
        /// Time slot
        /// </summary>
        public DateTime TimeSlot { get; set; }

        /// <summary>
        /// Number of concurrent sessions at this time slot
        /// </summary>
        public long ConcurrentSessions { get; set; }
    }
}
