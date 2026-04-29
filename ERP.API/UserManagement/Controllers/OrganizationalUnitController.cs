using System;
using System.Threading.Tasks;
using ERP.API.UserManagement.DTOs;
using ERP.API.UserManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ERP.API.UserManagement.Controllers
{
    [Route("api/UmOrganizationalUnit")]
    [ApiController]
    [Authorize]
    public class OrganizationalUnitController : ControllerBase
    {
        private readonly IOrganizationalUnitService _organizationalUnitService;
        private readonly ILogger<OrganizationalUnitController> _logger;

        public OrganizationalUnitController(IOrganizationalUnitService organizationalUnitService, ILogger<OrganizationalUnitController> logger)
        {
            _organizationalUnitService = organizationalUnitService;
            _logger = logger;
        }

        /// <summary>
        /// Create a new organizational unit
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "RequireAdminRole")]
        public async Task<IActionResult> CreateOrganizationalUnit([FromBody] CreateOrganizationalUnitDto dto)
        {
            try
            {
                // Get current user ID from claims if available
                var userIdClaim = User.FindFirst("sub")?.Value;
                int? userId = userIdClaim != null ? int.Parse(userIdClaim) : null;

                var result = await _organizationalUnitService.CreateOrganizationalUnitAsync(dto, userId);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return CreatedAtAction(nameof(GetOrganizationalUnitById), new { id = result.UnitId }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating organizational unit");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "An error occurred while creating the organizational unit." });
            }
        }

        /// <summary>
        /// Get an organizational unit by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrganizationalUnitById(int id)
        {
            try
            {
                var unit = await _organizationalUnitService.GetOrganizationalUnitByIdAsync(id);

                if (unit == null)
                {
                    return NotFound(new { message = $"Organizational unit with ID {id} not found." });
                }

                return Ok(unit);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting organizational unit with ID {UnitId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "An error occurred while retrieving the organizational unit." });
            }
        }

        /// <summary>
        /// Update an organizational unit
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Policy = "RequireAdminRole")]
        public async Task<IActionResult> UpdateOrganizationalUnit(int id, [FromBody] UpdateOrganizationalUnitDto dto)
        {
            try
            {
                var result = await _organizationalUnitService.UpdateOrganizationalUnitAsync(id, dto);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating organizational unit with ID {UnitId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "An error occurred while updating the organizational unit." });
            }
        }

        /// <summary>
        /// Delete an organizational unit
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Policy = "RequireAdminRole")]
        public async Task<IActionResult> DeleteOrganizationalUnit(int id)
        {
            try
            {
                var result = await _organizationalUnitService.DeleteOrganizationalUnitAsync(id);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting organizational unit with ID {UnitId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "An error occurred while deleting the organizational unit." });
            }
        }

        /// <summary>
        /// Set the active status of an organizational unit
        /// </summary>
        [HttpPatch("{id}/status")]
        [Authorize(Policy = "RequireAdminRole")]
        public async Task<IActionResult> SetOrganizationalUnitStatus(int id, [FromQuery] bool isActive)
        {
            try
            {
                var result = await _organizationalUnitService.SetOrganizationalUnitStatusAsync(id, isActive);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting status for organizational unit with ID {UnitId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "An error occurred while setting the organizational unit status." });
            }
        }

        /// <summary>
        /// Get all organizational units
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllOrganizationalUnits([FromQuery] bool? isActive = null)
        {
            try
            {
                var units = await _organizationalUnitService.GetAllOrganizationalUnitsAsync(isActive);
                return Ok(units);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all organizational units");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "An error occurred while retrieving organizational units." });
            }
        }

        /// <summary>
        /// Get paginated organizational units with filtering
        /// </summary>
        [HttpGet("paginated")]
        public async Task<IActionResult> GetOrganizationalUnitsPaginated([FromQuery] OrganizationalUnitQueryParametersDto parameters)
        {
            try
            {
                var response = await _organizationalUnitService.GetOrganizationalUnitsPaginatedAsync(parameters);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paginated organizational units");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "An error occurred while retrieving paginated organizational units." });
            }
        }

        /// <summary>
        /// Get child units for a parent unit
        /// </summary>
        [HttpGet("{id}/children")]
        public async Task<IActionResult> GetChildUnits(int id, [FromQuery] bool? isActive = null)
        {
            try
            {
                var children = await _organizationalUnitService.GetChildUnitsAsync(id, isActive);
                return Ok(children);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting child units for parent ID {ParentUnitId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "An error occurred while retrieving child units." });
            }
        }

        /// <summary>
        /// Get top-level units (units without a parent)
        /// </summary>
        [HttpGet("top-level")]
        public async Task<IActionResult> GetTopLevelUnits([FromQuery] bool? isActive = null)
        {
            try
            {
                var units = await _organizationalUnitService.GetTopLevelUnitsAsync(isActive);
                return Ok(units);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top-level organizational units");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "An error occurred while retrieving top-level units." });
            }
        }

        /// <summary>
        /// Get the organizational unit hierarchy
        /// </summary>
        [HttpGet("hierarchy")]
        public async Task<IActionResult> GetUnitHierarchy([FromQuery] int? unitId = null, [FromQuery] bool? isActive = true)
        {
            try
            {
                var hierarchy = await _organizationalUnitService.GetUnitHierarchyAsync(unitId, isActive);
                return Ok(hierarchy);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting organizational unit hierarchy");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "An error occurred while retrieving unit hierarchy." });
            }
        }

        /// <summary>
        /// Search for organizational units
        /// </summary>
        [HttpGet("search")]
        public async Task<IActionResult> SearchOrganizationalUnits(
            [FromQuery] string searchTerm,
            [FromQuery] string? unitType = null,
            [FromQuery] bool? isActive = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return BadRequest(new { message = "Search term is required." });
                }

                var results = await _organizationalUnitService.SearchOrganizationalUnitsAsync(searchTerm, unitType, isActive);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching organizational units with term {SearchTerm}", searchTerm);
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "An error occurred while searching organizational units." });
            }
        }

        /// <summary>
        /// Get units managed by a specific manager
        /// </summary>
        [HttpGet("by-manager/{managerId}")]
        public async Task<IActionResult> GetUnitsByManager(int managerId, [FromQuery] bool? isActive = null)
        {
            try
            {
                var units = await _organizationalUnitService.GetUnitsByManagerAsync(managerId, isActive);
                return Ok(units);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting units managed by manager ID {ManagerId}", managerId);
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "An error occurred while retrieving units by manager." });
            }
        }

        /// <summary>
        /// Get all available organizational unit types
        /// </summary>
        [HttpGet("types")]
        public async Task<IActionResult> GetUnitTypes()
        {
            try
            {
                var types = await _organizationalUnitService.GetUnitTypesAsync();
                return Ok(types);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting organizational unit types");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "An error occurred while retrieving unit types." });
            }
        }

        /// <summary>
        /// Get organizational unit statistics
        /// </summary>
        [HttpGet("statistics")]
        [Authorize(Policy = "RequireAdminRole")]
        public async Task<IActionResult> GetOrganizationalUnitStatistics()
        {
            try
            {
                var statistics = await _organizationalUnitService.GetOrganizationalUnitStatisticsAsync();
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting organizational unit statistics");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "An error occurred while retrieving unit statistics." });
            }
        }

        /// <summary>
        /// Assign a manager to an organizational unit
        /// </summary>
        [HttpPost("assign-manager")]
        [Authorize(Policy = "RequireAdminRole")]
        public async Task<IActionResult> AssignManagerToUnit([FromBody] AssignManagerDto dto)
        {
            try
            {
                var result = await _organizationalUnitService.AssignManagerToUnitAsync(dto.UnitId, dto.ManagerId);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning manager {ManagerId} to unit {UnitId}", dto.ManagerId, dto.UnitId);
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "An error occurred while assigning the manager to the unit." });
            }
        }

        /// <summary>
        /// Move an organizational unit to a new parent
        /// </summary>
        [HttpPost("move")]
        [Authorize(Policy = "RequireAdminRole")]
        public async Task<IActionResult> MoveOrganizationalUnit([FromBody] MoveUnitDto dto)
        {
            try
            {
                var result = await _organizationalUnitService.MoveOrganizationalUnitAsync(dto.UnitId, dto.NewParentId);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error moving unit {UnitId} to parent {NewParentId}", dto.UnitId, dto.NewParentId);
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "An error occurred while moving the organizational unit." });
            }
        }
    }
}
