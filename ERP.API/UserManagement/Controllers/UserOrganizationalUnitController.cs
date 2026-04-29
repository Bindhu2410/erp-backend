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
    /// Controller for managing user assignments to organizational units
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/UmUserOrganizationalUnit")]
    public class UserOrganizationalUnitController : ControllerBase
    {
        private readonly IUserOrganizationalUnitService _service;
        private readonly ILogger<UserOrganizationalUnitController> _logger;

        /// <summary>
        /// Constructor for UserOrganizationalUnitController
        /// </summary>
        /// <param name="service">The user organizational unit service</param>
        /// <param name="logger">Logger</param>
        public UserOrganizationalUnitController(
            IUserOrganizationalUnitService service,
            ILogger<UserOrganizationalUnitController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Assigns a user to an organizational unit
        /// </summary>
        /// <param name="dto">The assignment details</param>
        /// <returns>Result of the operation</returns>
        [HttpPost("assign")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserUnitOperationResultDTO>> AssignUserToUnit([FromBody] CreateUserOrganizationalUnitDTO dto)
        {
            _logger.LogInformation("Assigning user {UserId} to unit {UnitId}", dto.UserId, dto.UnitId);
            
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _service.AssignUserToUnitAsync(dto);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }
            
            return Ok(result);
        }

        /// <summary>
        /// Gets a specific user-to-unit assignment
        /// </summary>
        /// <param name="userId">ID of the user</param>
        /// <param name="unitId">ID of the organizational unit</param>
        /// <returns>The assignment details</returns>
        [HttpGet("{userId}/{unitId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserOrganizationalUnitDTO>> GetUserUnitAssignment(int userId, int unitId)
        {
            _logger.LogInformation("Getting assignment for user {UserId} to unit {UnitId}", userId, unitId);
            
            var assignment = await _service.GetUserUnitAssignmentAsync(userId, unitId);
            
            if (assignment == null)
            {
                return NotFound(new { message = "User assignment to this organizational unit not found" });
            }
            
            return Ok(assignment);
        }

        /// <summary>
        /// Updates a user's assignment to an organizational unit (primary status)
        /// </summary>
        /// <param name="userId">ID of the user</param>
        /// <param name="unitId">ID of the organizational unit</param>
        /// <param name="dto">The updated assignment details</param>
        /// <returns>Result of the operation</returns>
        [HttpPut("{userId}/{unitId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserUnitOperationResultDTO>> UpdateUserUnitAssignment(
            int userId, 
            int unitId, 
            [FromBody] UpdateUserOrganizationalUnitDTO dto)
        {
            _logger.LogInformation("Updating assignment for user {UserId} to unit {UnitId}", userId, unitId);
            
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _service.UpdateUserUnitAssignmentAsync(userId, unitId, dto);
            
            if (!result.Success)
            {
                if (result.Message.Contains("not found"))
                {
                    return NotFound(result);
                }
                
                return BadRequest(result);
            }
            
            return Ok(result);
        }

        /// <summary>
        /// Removes a user from an organizational unit
        /// </summary>
        /// <param name="userId">ID of the user</param>
        /// <param name="unitId">ID of the organizational unit</param>
        /// <returns>Result of the operation</returns>
        [HttpDelete("{userId}/{unitId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserUnitOperationResultDTO>> RemoveUserFromUnit(int userId, int unitId)
        {
            _logger.LogInformation("Removing user {UserId} from unit {UnitId}", userId, unitId);
            
            var result = await _service.RemoveUserFromUnitAsync(userId, unitId);
            
            if (!result.Success)
            {
                if (result.Message.Contains("not assigned"))
                {
                    return NotFound(result);
                }
                
                return BadRequest(result);
            }
            
            return Ok(result);
        }

        /// <summary>
        /// Gets all users assigned to a specific organizational unit
        /// </summary>
        /// <param name="unitId">ID of the organizational unit</param>
        /// <param name="includeChildUnits">Whether to include users from child units</param>
        /// <returns>List of users assigned to the unit</returns>
        [HttpGet("unit/{unitId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<UserOrganizationalUnitDTO>>> GetUsersInUnit(
            int unitId, 
            [FromQuery] bool includeChildUnits = false)
        {
            _logger.LogInformation("Getting users in unit {UnitId}, includeChildUnits: {IncludeChildUnits}", 
                unitId, includeChildUnits);
            
            var users = await _service.GetUsersInUnitAsync(unitId, includeChildUnits);
            return Ok(users);
        }

        /// <summary>
        /// Gets all organizational units a user is assigned to
        /// </summary>
        /// <param name="userId">ID of the user</param>
        /// <returns>List of organizational units the user is assigned to</returns>
        [HttpGet("user/{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<UserOrganizationalUnitDTO>>> GetUserUnits(int userId)
        {
            _logger.LogInformation("Getting units for user {UserId}", userId);
            
            var units = await _service.GetUserUnitsAsync(userId);
            return Ok(units);
        }

        /// <summary>
        /// Gets a user's primary organizational unit
        /// </summary>
        /// <param name="userId">ID of the user</param>
        /// <returns>The user's primary organizational unit</returns>
        [HttpGet("user/{userId}/primary")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserOrganizationalUnitDTO>> GetUserPrimaryUnit(int userId)
        {
            _logger.LogInformation("Getting primary unit for user {UserId}", userId);
            
            var unit = await _service.GetUserPrimaryUnitAsync(userId);
            
            if (unit == null)
            {
                return NotFound(new { message = "User has no primary organizational unit" });
            }
            
            return Ok(unit);
        }

        /// <summary>
        /// Sets a user's primary organizational unit
        /// </summary>
        /// <param name="userId">ID of the user</param>
        /// <param name="unitId">ID of the organizational unit to set as primary</param>
        /// <returns>Result of the operation</returns>
        [HttpPut("user/{userId}/primary/{unitId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserUnitOperationResultDTO>> SetUserPrimaryUnit(int userId, int unitId)
        {
            _logger.LogInformation("Setting primary unit {UnitId} for user {UserId}", unitId, userId);
            
            var result = await _service.SetUserPrimaryUnitAsync(userId, unitId);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }
            
            return Ok(result);
        }

        /// <summary>
        /// Assigns multiple users to an organizational unit
        /// </summary>
        /// <param name="dto">The bulk assignment details</param>
        /// <returns>Result of the operation</returns>
        [HttpPost("bulk-assign-to-unit")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserUnitOperationResultDTO>> AssignUsersToUnit([FromBody] BulkUserAssignmentDTO dto)
        {
            _logger.LogInformation("Bulk assigning {UserCount} users to unit {UnitId}", dto.UserIds.Count, dto.UnitId);
            
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _service.AssignUsersToUnitAsync(dto);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }
            
            return Ok(result);
        }

        /// <summary>
        /// Assigns a user to multiple organizational units
        /// </summary>
        /// <param name="dto">The multi-unit assignment details</param>
        /// <returns>Result of the operation</returns>
        [HttpPost("assign-to-multiple-units")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserUnitOperationResultDTO>> AssignUserToUnits([FromBody] UserMultiUnitAssignmentDTO dto)
        {
            _logger.LogInformation("Assigning user {UserId} to {UnitCount} units", dto.UserId, dto.UnitIds.Count);
            
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _service.AssignUserToUnitsAsync(dto);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }
            
            return Ok(result);
        }

        /// <summary>
        /// Gets users that aren't assigned to any organizational unit
        /// </summary>
        /// <returns>List of users without unit assignments</returns>
        [HttpGet("users-without-unit")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<UserOrganizationalUnitDTO>>> GetUsersWithoutUnit()
        {
            _logger.LogInformation("Getting users without unit assignments");
            
            var users = await _service.GetUsersWithoutUnitAsync();
            return Ok(users);
        }

        /// <summary>
        /// Gets organizational units that have no users assigned
        /// </summary>
        /// <returns>List of empty organizational units</returns>
        [HttpGet("empty-units")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<UserOrganizationalUnitDTO>>> GetEmptyUnits()
        {
            _logger.LogInformation("Getting empty organizational units");
            
            var units = await _service.GetEmptyUnitsAsync();
            return Ok(units);
        }

        /// <summary>
        /// Gets assignment statistics for all organizational units
        /// </summary>
        /// <returns>List of unit statistics</returns>
        [HttpGet("unit-stats")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<UnitAssignmentStatsDTO>>> GetUnitAssignmentStats()
        {
            _logger.LogInformation("Getting unit assignment statistics");
            
            var stats = await _service.GetUnitAssignmentStatsAsync();
            return Ok(stats);
        }

        /// <summary>
        /// Gets a paginated list of user-unit assignments with search capabilities
        /// </summary>
        /// <param name="parameters">Query parameters for pagination and filtering</param>
        /// <returns>Paginated list of assignments</returns>
        [HttpGet("paginated")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PaginatedUserUnitAssignmentsDTO>> GetUserUnitAssignmentsPaginated(
            [FromQuery] UserUnitAssignmentQueryParameters parameters)
        {
            _logger.LogInformation("Getting paginated user-unit assignments, page {PageNumber}, size {PageSize}", 
                parameters.PageNumber, parameters.PageSize);
            
            var assignments = await _service.GetUserUnitAssignmentsPaginatedAsync(parameters);
            return Ok(assignments);
        }

        /// <summary>
        /// Gets statistics about user assignments by unit type
        /// </summary>
        /// <returns>List of unit type statistics</returns>
        [HttpGet("unit-type-stats")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<UnitTypeStatisticsDTO>>> GetUserCountByUnitType()
        {
            _logger.LogInformation("Getting user count by unit type");
            
            var stats = await _service.GetUserCountByUnitTypeAsync();
            return Ok(stats);
        }

        /// <summary>
        /// Transfers all users from one organizational unit to another
        /// </summary>
        /// <param name="dto">The transfer details</param>
        /// <returns>Result of the operation</returns>
        [HttpPost("transfer")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserUnitOperationResultDTO>> TransferUsersBetweenUnits(
            [FromBody] TransferUsersDTO dto)
        {
            _logger.LogInformation("Transferring users from unit {SourceUnitId} to unit {TargetUnitId}", 
                dto.SourceUnitId, dto.TargetUnitId);
            
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _service.TransferUsersBetweenUnitsAsync(dto);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }
            
            return Ok(result);
        }

        /// <summary>
        /// Ensures all users have a primary organizational unit set
        /// </summary>
        /// <returns>Result of the operation</returns>
        [HttpPost("ensure-primary-units")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserUnitOperationResultDTO>> EnsureUsersPrimaryUnit()
        {
            _logger.LogInformation("Ensuring all users have a primary organizational unit");
            
            var result = await _service.EnsureUsersPrimaryUnitAsync();
            
            return Ok(result);
        }
    }
}
