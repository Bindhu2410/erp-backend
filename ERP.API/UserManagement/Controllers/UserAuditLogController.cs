using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ERP.API.UserManagement.DTOs;
using ERP.API.UserManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ERP.API.UserManagement.Controllers
{
    /// <summary>
    /// API controller for managing user audit logs
    /// </summary>
    [ApiController]
    [Route("api/UmUserAuditLog")]
    [Authorize]
    public class UserAuditLogController : ControllerBase
    {
        private readonly IUserAuditLogService _userAuditLogService;
        private readonly ILogger<UserAuditLogController> _logger;

        /// <summary>
        /// Constructor for the user audit log controller
        /// </summary>
        /// <param name="userAuditLogService">The user audit log service</param>
        /// <param name="logger">The logger</param>
        public UserAuditLogController(IUserAuditLogService userAuditLogService, ILogger<UserAuditLogController> logger)
        {
            _userAuditLogService = userAuditLogService ?? throw new ArgumentNullException(nameof(userAuditLogService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Creates a new audit log entry
        /// </summary>
        /// <param name="createDTO">Audit log creation data</param>
        /// <returns>The created audit log</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserAuditLogDTO>> CreateAuditLog(CreateUserAuditLogDTO createDTO)
        {
            try
            {
                var result = await _userAuditLogService.CreateAuditLogAsync(createDTO);
                return CreatedAtAction(nameof(GetAuditLogById), new { auditId = result.AuditId }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating audit log entry for user {UserId}", createDTO.UserId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the audit log entry");
            }
        }

        /// <summary>
        /// Gets an audit log entry by its ID
        /// </summary>
        /// <param name="auditId">The audit log ID</param>
        /// <returns>The audit log if found</returns>
        [HttpGet("{auditId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserAuditLogDTO>> GetAuditLogById(int auditId)
        {
            try
            {
                var result = await _userAuditLogService.GetAuditLogByIdAsync(auditId);
                if (result == null)
                {
                    return NotFound($"Audit log with ID {auditId} not found");
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving audit log {AuditId}", auditId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the audit log");
            }
        }

        /// <summary>
        /// Updates an audit log entry (limited fields only)
        /// </summary>
        /// <param name="auditId">The audit log ID</param>
        /// <param name="updateDTO">The update data</param>
        /// <returns>The updated audit log</returns>
        [HttpPut("{auditId}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserAuditLogDTO>> UpdateAuditLog(int auditId, UpdateUserAuditLogDTO updateDTO)
        {
            try
            {
                var result = await _userAuditLogService.UpdateAuditLogAsync(auditId, updateDTO);
                if (result == null)
                {
                    return NotFound($"Audit log with ID {auditId} not found");
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating audit log {AuditId}", auditId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the audit log");
            }
        }

        /// <summary>
        /// Deletes an audit log entry
        /// </summary>
        /// <param name="auditId">The audit log ID to delete</param>
        /// <returns>No content if successful</returns>
        [HttpDelete("{auditId}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> DeleteAuditLog(int auditId)
        {
            try
            {
                var result = await _userAuditLogService.DeleteAuditLogAsync(auditId);
                if (!result)
                {
                    return NotFound($"Audit log with ID {auditId} not found");
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting audit log {AuditId}", auditId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while deleting the audit log");
            }
        }

        /// <summary>
        /// Gets all audit logs with pagination
        /// </summary>
        /// <param name="pageNumber">The page number (default: 1)</param>
        /// <param name="pageSize">The page size (default: 10)</param>
        /// <returns>List of audit logs with pagination info</returns>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserAuditLogListDTO>> GetAllAuditLogs(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _userAuditLogService.GetAllAuditLogsAsync(pageNumber, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving audit logs");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving audit logs");
            }
        }

        /// <summary>
        /// Gets audit logs for a specific user with pagination
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="pageNumber">The page number (default: 1)</param>
        /// <param name="pageSize">The page size (default: 10)</param>
        /// <returns>List of audit logs for the user with pagination info</returns>
        [HttpGet("users/{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserAuditLogListDTO>> GetUserAuditLogs(
            int userId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _userAuditLogService.GetUserAuditLogsAsync(userId, pageNumber, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving audit logs for user {UserId}", userId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving user audit logs");
            }
        }

        /// <summary>
        /// Gets audit logs for a specific entity with pagination
        /// </summary>
        /// <param name="entityType">The entity type</param>
        /// <param name="entityId">The entity ID</param>
        /// <param name="pageNumber">The page number (default: 1)</param>
        /// <param name="pageSize">The page size (default: 10)</param>
        /// <returns>List of audit logs for the entity with pagination info</returns>
        [HttpGet("entities/{entityType}/{entityId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserAuditLogListDTO>> GetEntityAuditLogs(
            string entityType,
            int entityId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _userAuditLogService.GetEntityAuditLogsAsync(entityType, entityId, pageNumber, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving audit logs for entity {EntityType} with ID {EntityId}", entityType, entityId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving entity audit logs");
            }
        }

        /// <summary>
        /// Gets audit logs by action type with pagination
        /// </summary>
        /// <param name="actionType">The action type</param>
        /// <param name="pageNumber">The page number (default: 1)</param>
        /// <param name="pageSize">The page size (default: 10)</param>
        /// <returns>List of audit logs for the action type with pagination info</returns>
        [HttpGet("actions/{actionType}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserAuditLogListDTO>> GetAuditLogsByActionType(
            string actionType,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _userAuditLogService.GetAuditLogsByActionTypeAsync(actionType, pageNumber, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving audit logs for action type {ActionType}", actionType);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving action type audit logs");
            }
        }

        /// <summary>
        /// Gets audit logs by date range with pagination
        /// </summary>
        /// <param name="startDate">The start date</param>
        /// <param name="endDate">The end date</param>
        /// <param name="pageNumber">The page number (default: 1)</param>
        /// <param name="pageSize">The page size (default: 10)</param>
        /// <returns>List of audit logs in the date range with pagination info</returns>
        [HttpGet("date-range")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserAuditLogListDTO>> GetAuditLogsByDateRange(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _userAuditLogService.GetAuditLogsByDateRangeAsync(startDate, endDate, pageNumber, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving audit logs between {StartDate} and {EndDate}", startDate, endDate);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving date range audit logs");
            }
        }

        /// <summary>
        /// Gets audit logs by IP address with pagination
        /// </summary>
        /// <param name="ipAddress">The IP address</param>
        /// <param name="pageNumber">The page number (default: 1)</param>
        /// <param name="pageSize">The page size (default: 10)</param>
        /// <returns>List of audit logs for the IP address with pagination info</returns>
        [HttpGet("ip/{ipAddress}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserAuditLogListDTO>> GetAuditLogsByIpAddress(
            string ipAddress,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _userAuditLogService.GetAuditLogsByIpAddressAsync(ipAddress, pageNumber, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving audit logs for IP address {IpAddress}", ipAddress);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving IP address audit logs");
            }
        }

        /// <summary>
        /// Performs an advanced search on audit logs with multiple filter criteria
        /// </summary>
        /// <param name="searchDTO">The search criteria</param>
        /// <returns>List of matching audit logs with pagination info</returns>
        [HttpPost("search")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserAuditLogListDTO>> SearchAuditLogs(UserAuditLogSearchDTO searchDTO)
        {
            try
            {
                var result = await _userAuditLogService.SearchAuditLogsAsync(searchDTO);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching audit logs");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while searching audit logs");
            }
        }

        /// <summary>
        /// Gets daily audit log activity counts
        /// </summary>
        /// <param name="startDate">Optional start date</param>
        /// <param name="endDate">Optional end date</param>
        /// <returns>Daily activity counts</returns>
        [HttpGet("daily-activity")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<DailyAuditActivityDTO>>> GetDailyAuditActivity(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var result = await _userAuditLogService.GetDailyAuditActivityAsync(startDate, endDate);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving daily audit activity");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving daily audit activity");
            }
        }

        /// <summary>
        /// Gets most active users based on audit logs
        /// </summary>
        /// <param name="startDate">Optional start date</param>
        /// <param name="endDate">Optional end date</param>
        /// <param name="limit">Maximum number of users to return (default: 10)</param>
        /// <returns>List of most active users</returns>
        [HttpGet("most-active-users")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<UserActivityDTO>>> GetMostActiveUsers(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int limit = 10)
        {
            try
            {
                var result = await _userAuditLogService.GetMostActiveUsersAsync(startDate, endDate, limit);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving most active users");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving most active users");
            }
        }

        /// <summary>
        /// Gets action type distribution statistics
        /// </summary>
        /// <param name="startDate">Optional start date</param>
        /// <param name="endDate">Optional end date</param>
        /// <returns>Action type distribution statistics</returns>
        [HttpGet("action-distribution")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<ActionTypeDistributionDTO>>> GetActionTypeDistribution(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var result = await _userAuditLogService.GetActionTypeDistributionAsync(startDate, endDate);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving action type distribution");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving action type distribution");
            }
        }

        /// <summary>
        /// Gets entity activity summary
        /// </summary>
        /// <param name="entityType">The entity type</param>
        /// <param name="startDate">Optional start date</param>
        /// <param name="endDate">Optional end date</param>
        /// <param name="limit">Maximum number of entities to return (default: 10)</param>
        /// <returns>Entity activity summary</returns>
        [HttpGet("entity-activity/{entityType}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<EntityActivityDTO>>> GetEntityActivitySummary(
            string entityType,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int limit = 10)
        {
            try
            {
                var result = await _userAuditLogService.GetEntityActivitySummaryAsync(entityType, startDate, endDate, limit);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving entity activity for {EntityType}", entityType);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving entity activity");
            }
        }

        /// <summary>
        /// Gets recent activity for a user with relative time information
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="limit">Maximum number of activities to return (default: 10)</param>
        /// <returns>List of recent user activities</returns>
        [HttpGet("users/{userId}/recent")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<RecentUserActivityDTO>>> GetRecentUserActivity(
            int userId,
            [FromQuery] int limit = 10)
        {
            try
            {
                var result = await _userAuditLogService.GetRecentUserActivityAsync(userId, limit);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recent activity for user {UserId}", userId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving recent user activity");
            }
        }

        /// <summary>
        /// Gets history for a specific entity
        /// </summary>
        /// <param name="entityType">The entity type</param>
        /// <param name="entityId">The entity ID</param>
        /// <returns>List of audit log entries for the entity</returns>
        [HttpGet("history/{entityType}/{entityId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<EntityHistoryDTO>>> GetEntityHistory(
            string entityType,
            int entityId)
        {
            try
            {
                var result = await _userAuditLogService.GetEntityHistoryAsync(entityType, entityId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving history for entity {EntityType} with ID {EntityId}", entityType, entityId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving entity history");
            }
        }

        /// <summary>
        /// Gets user session activity summary (for security monitoring)
        /// </summary>
        /// <param name="userId">Optional user ID to filter by</param>
        /// <param name="startDate">Optional start date</param>
        /// <param name="endDate">Optional end date</param>
        /// <returns>Session activity summary</returns>
        [HttpGet("session-activity")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<UserSessionActivityDTO>>> GetUserSessionActivity(
            [FromQuery] int? userId = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var result = await _userAuditLogService.GetUserSessionActivityAsync(userId, startDate, endDate);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user session activity");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving session activity");
            }
        }

        /// <summary>
        /// Gets failed login attempts
        /// </summary>
        /// <param name="userId">Optional user ID to filter by</param>
        /// <param name="startDate">Optional start date</param>
        /// <param name="endDate">Optional end date</param>
        /// <returns>Failed login attempts summary</returns>
        [HttpGet("failed-logins")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<FailedLoginDTO>>> GetFailedLoginAttempts(
            [FromQuery] int? userId = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var result = await _userAuditLogService.GetFailedLoginAttemptsAsync(userId, startDate, endDate);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving failed login attempts");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving failed login attempts");
            }
        }

        /// <summary>
        /// Cleans up old audit logs
        /// </summary>
        /// <param name="daysOld">Age in days for logs to remove (default: 365)</param>
        /// <returns>Number of logs removed</returns>
        [HttpPost("cleanup")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<int>> CleanupOldAuditLogs([FromQuery] int daysOld = 365)
        {
            try
            {
                var result = await _userAuditLogService.CleanupOldAuditLogsAsync(daysOld);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up old audit logs");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while cleaning up old audit logs");
            }
        }
    }
}
