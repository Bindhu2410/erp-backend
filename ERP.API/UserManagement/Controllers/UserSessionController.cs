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
    /// API controller for managing user sessions
    /// </summary>
    [ApiController]
    [Route("api/UmUserSession")]
    [Authorize]
    public class UserSessionController : ControllerBase
    {
        private readonly IUserSessionService _userSessionService;
        private readonly ILogger<UserSessionController> _logger;

        /// <summary>
        /// Constructor for the user session controller
        /// </summary>
        /// <param name="userSessionService">The user session service</param>
        /// <param name="logger">The logger</param>
        public UserSessionController(IUserSessionService userSessionService, ILogger<UserSessionController> logger)
        {
            _userSessionService = userSessionService ?? throw new ArgumentNullException(nameof(userSessionService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Creates a new user session
        /// </summary>
        /// <param name="createDTO">Session creation data</param>
        /// <returns>The created session</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserSessionDTO>> CreateSession(CreateUserSessionDTO createDTO)
        {
            try
            {
                var result = await _userSessionService.CreateSessionAsync(createDTO);
                return CreatedAtAction(nameof(GetSessionById), new { sessionId = result.SessionId }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating session for user {UserId}", createDTO.UserId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the session");
            }
        }

        /// <summary>
        /// Gets a session by its ID
        /// </summary>
        /// <param name="sessionId">The session ID</param>
        /// <returns>The session if found</returns>
        [HttpGet("{sessionId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserSessionDTO>> GetSessionById(string sessionId)
        {
            try
            {
                var result = await _userSessionService.GetSessionByIdAsync(sessionId);
                if (result == null)
                {
                    return NotFound($"Session with ID {sessionId} not found");
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving session {SessionId}", sessionId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the session");
            }
        }

        /// <summary>
        /// Updates a user session
        /// </summary>
        /// <param name="sessionId">The session ID</param>
        /// <param name="updateDTO">The update data</param>
        /// <returns>The updated session</returns>
        [HttpPut("{sessionId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserSessionDTO>> UpdateSession(string sessionId, UpdateUserSessionDTO updateDTO)
        {
            try
            {
                var result = await _userSessionService.UpdateSessionAsync(sessionId, updateDTO);
                if (result == null)
                {
                    return NotFound($"Session with ID {sessionId} not found");
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating session {SessionId}", sessionId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the session");
            }
        }

        /// <summary>
        /// Deletes a user session
        /// </summary>
        /// <param name="sessionId">The session ID to delete</param>
        /// <returns>No content if successful</returns>
        [HttpDelete("{sessionId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> DeleteSession(string sessionId)
        {
            try
            {
                var result = await _userSessionService.DeleteSessionAsync(sessionId);
                if (!result)
                {
                    return NotFound($"Session with ID {sessionId} not found");
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting session {SessionId}", sessionId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while deleting the session");
            }
        }

        /// <summary>
        /// Ends a user session (marks as inactive and sets logout time)
        /// </summary>
        /// <param name="sessionId">The session ID to end</param>
        /// <returns>No content if successful</returns>
        [HttpPost("{sessionId}/end")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> EndSession(string sessionId)
        {
            try
            {
                var result = await _userSessionService.EndSessionAsync(sessionId);
                if (!result)
                {
                    return NotFound($"Session with ID {sessionId} not found or already inactive");
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ending session {SessionId}", sessionId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while ending the session");
            }
        }

        /// <summary>
        /// Ends all active sessions for a user (except optionally the current session)
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="currentSessionId">Optional current session ID to exclude</param>
        /// <returns>Number of sessions ended</returns>
        [HttpPost("users/{userId}/end-all")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<int>> EndAllUserSessions(int userId, [FromQuery] string? currentSessionId = null)
        {
            try
            {
                var result = await _userSessionService.EndAllUserSessionsAsync(userId, currentSessionId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ending all sessions for user {UserId}", userId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while ending user sessions");
            }
        }

        /// <summary>
        /// Gets all active sessions with pagination
        /// </summary>
        /// <param name="pageNumber">The page number (default: 1)</param>
        /// <param name="pageSize">The page size (default: 10)</param>
        /// <returns>List of active sessions with pagination info</returns>
        [HttpGet("active")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserSessionHistoryDTO>> GetActiveSessions(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _userSessionService.GetActiveSessionsAsync(pageNumber, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active sessions");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving active sessions");
            }
        }

        /// <summary>
        /// Gets session history for a user with pagination and optional date filtering
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="startDate">Optional start date for filtering</param>
        /// <param name="endDate">Optional end date for filtering</param>
        /// <param name="pageNumber">The page number (default: 1)</param>
        /// <param name="pageSize">The page size (default: 10)</param>
        /// <returns>User session history with pagination info</returns>
        [HttpGet("users/{userId}/history")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserSessionHistoryDTO>> GetUserSessionHistory(
            int userId,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var queryParams = new SessionHistoryQueryParams
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                var result = await _userSessionService.GetUserSessionHistoryAsync(userId, queryParams);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving session history for user {UserId}", userId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving session history");
            }
        }

        /// <summary>
        /// Gets session statistics by date range
        /// </summary>
        /// <param name="startDate">Optional start date</param>
        /// <param name="endDate">Optional end date</param>
        /// <returns>List of daily session statistics</returns>
        [HttpGet("statistics")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<SessionStatisticsDTO>>> GetSessionStatistics(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var result = await _userSessionService.GetSessionStatisticsAsync(startDate, endDate);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving session statistics");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving session statistics");
            }
        }

        /// <summary>
        /// Gets active sessions for a user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>List of active user sessions</returns>
        [HttpGet("users/{userId}/active")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<UserSessionDTO>>> GetUserActiveSessions(int userId)
        {
            try
            {
                var result = await _userSessionService.GetUserActiveSessionsAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active sessions for user {UserId}", userId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving user's active sessions");
            }
        }

        /// <summary>
        /// Cleans up old inactive sessions
        /// </summary>
        /// <param name="daysOld">Age in days for sessions to remove (default: 90)</param>
        /// <returns>Number of sessions removed</returns>
        [HttpPost("cleanup")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<int>> CleanupOldSessions([FromQuery] int daysOld = 90)
        {
            try
            {
                var result = await _userSessionService.CleanupOldSessionsAsync(daysOld);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up old sessions");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while cleaning up old sessions");
            }
        }

        /// <summary>
        /// Fixes incomplete sessions (sessions still marked active but older than 24 hours)
        /// </summary>
        /// <returns>Number of sessions fixed</returns>
        [HttpPost("fix-incomplete")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<int>> FixIncompleteSessions()
        {
            try
            {
                var result = await _userSessionService.FixIncompleteSessionsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fixing incomplete sessions");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while fixing incomplete sessions");
            }
        }

        /// <summary>
        /// Gets concurrent sessions count by time slots
        /// </summary>
        /// <param name="startDate">Optional start date</param>
        /// <param name="endDate">Optional end date</param>
        /// <param name="intervalMinutes">Interval in minutes for the time slots (default: 60)</param>
        /// <returns>List of time slots with concurrent session counts</returns>
        [HttpGet("concurrent")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<ConcurrentSessionsDTO>>> GetConcurrentSessions(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int intervalMinutes = 60)
        {
            try
            {
                var queryParams = new ConcurrentSessionsQueryParams
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    IntervalMinutes = intervalMinutes
                };

                var result = await _userSessionService.GetConcurrentSessionsAsync(queryParams);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving concurrent sessions");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving concurrent sessions");
            }
        }

        /// <summary>
        /// Gets user login frequency statistics
        /// </summary>
        /// <param name="topN">Number of users to return (default: 10)</param>
        /// <param name="startDate">Optional start date</param>
        /// <param name="endDate">Optional end date</param>
        /// <returns>List of user login statistics</returns>
        [HttpGet("login-frequency")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<UserLoginFrequencyDTO>>> GetUserLoginFrequency(
            [FromQuery] int topN = 10,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var result = await _userSessionService.GetUserLoginFrequencyAsync(topN, startDate, endDate);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user login frequency");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving user login frequency");
            }
        }

        /// <summary>
        /// Gets sessions by IP address with pagination
        /// </summary>
        /// <param name="ipAddress">The IP address to search for</param>
        /// <param name="pageNumber">The page number (default: 1)</param>
        /// <param name="pageSize">The page size (default: 10)</param>
        /// <returns>List of sessions from the IP address with pagination info</returns>
        [HttpGet("ip/{ipAddress}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserSessionHistoryDTO>> GetSessionsByIp(
            string ipAddress,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _userSessionService.GetSessionsByIpAsync(ipAddress, pageNumber, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving sessions by IP {IpAddress}", ipAddress);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving sessions by IP");
            }
        }

        /// <summary>
        /// Gets device usage statistics
        /// </summary>
        /// <param name="startDate">Optional start date</param>
        /// <param name="endDate">Optional end date</param>
        /// <returns>List of device statistics</returns>
        [HttpGet("device-statistics")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<DeviceStatisticsDTO>>> GetDeviceStatistics(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var result = await _userSessionService.GetDeviceStatisticsAsync(startDate, endDate);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving device statistics");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving device statistics");
            }
        }

        /// <summary>
        /// Gets long-running active sessions
        /// </summary>
        /// <param name="hoursThreshold">Minimum duration in hours (default: 8)</param>
        /// <returns>List of long-running sessions</returns>
        [HttpGet("long-running")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<UserSessionDTO>>> GetLongRunningSessions(
            [FromQuery] int hoursThreshold = 8)
        {
            try
            {
                var result = await _userSessionService.GetLongRunningSessionsAsync(hoursThreshold);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving long running sessions");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving long running sessions");
            }
        }
    }
}
