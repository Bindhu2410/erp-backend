using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ERP.API.UserManagement.DTOs;
using ERP.API.UserManagement.Services;
using System;
using System.Threading.Tasks;

namespace ERP.API.UserManagement.Controllers
{
    [ApiController]
    [Route("api/UmRole")]
    // [Authorize]
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _roleService;
        private readonly ILogger<RoleController> _logger;

        public RoleController(IRoleService roleService, ILogger<RoleController> logger)
        {
            _roleService = roleService;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new role
        /// </summary>
        [HttpPost]
        // [Authorize(Policy = "ManageRoles")]
        // [Authorize]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleDto roleDto)
        {
            try
            {
                var (success, message, roleId) = await _roleService.CreateRoleAsync(roleDto);

                if (success)
                {
                    return CreatedAtAction(nameof(GetRoleById), new { id = roleId }, new { roleId, message });
                }

                return BadRequest(new { message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating role");
                return StatusCode(500, "An error occurred while creating the role");
            }
        }

        /// <summary>
        /// Gets a role by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetRoleById(int id)
        {
            try
            {
                var role = await _roleService.GetRoleByIdAsync(id);

                if (role == null)
                {
                    return NotFound(new { status = false, message = "Role not found", data = (object)null });
                }

                return Ok(new { status = true, message = "Role retrieved successfully", data = role });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting role by ID {RoleId}", id);
                return StatusCode(500, new { status = false, message = "An error occurred while retrieving the role", data = (object)null });
            }
        }

        /// <summary>
        /// Gets a role by name
        /// </summary>
        [HttpGet("name/{name}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetRoleByName(string name)
        {
            try
            {
                var role = await _roleService.GetRoleByNameAsync(name);

                if (role == null)
                {
                    return NotFound(new { status = false, message = "Role not found", data = (object)null });
                }

                return Ok(new { status = true, message = "Role retrieved successfully", data = role });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting role by name {RoleName}", name);
                return StatusCode(500, new { status = false, message = "An error occurred while retrieving the role", data = (object)null });
            }
        }

        /// <summary>
        /// Gets a paginated list of roles with optional filtering
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAllRoles(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string searchTerm = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] bool? isSystemRole = null)
        {
            try
            {
                var roles = await _roleService.GetAllRolesAsync(pageNumber, pageSize, searchTerm, isActive, isSystemRole);
                return Ok(new { status = true, message = "Roles retrieved successfully", data = roles });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting roles list");
                return StatusCode(500, new { status = false, message = "An error occurred while retrieving roles", data = (object)null });
            }
        }

        /// <summary>
        /// Updates a role
        /// </summary>
        [HttpPut("{id}")]
    // [Authorize(Policy = "ManageRoles")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateRole(int id, [FromBody] UpdateRoleDto roleDto)
        {
            try
            {
                var (success, message, roleId) = await _roleService.UpdateRoleAsync(id, roleDto);

                if (success)
                {
                    return Ok(new { roleId, message });
                }

                if (message.Contains("not found"))
                {
                    return NotFound(new { message });
                }

                return BadRequest(new { message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating role {RoleId}", id);
                return StatusCode(500, "An error occurred while updating the role");
            }
        }

        /// <summary>
        /// Soft deletes (deactivates) a role
        /// </summary>
        [HttpPut("{id}/deactivate")]
        // [Authorize(Policy = "ManageRoles")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SoftDeleteRole(int id)
        {
            try
            {
                var (success, message) = await _roleService.SoftDeleteRoleAsync(id);

                if (success)
                {
                    return Ok(new { message });
                }

                if (message.Contains("not found"))
                {
                    return NotFound(new { message });
                }

                return BadRequest(new { message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating role {RoleId}", id);
                return StatusCode(500, "An error occurred while deactivating the role");
            }
        }

        /// <summary>
        /// Hard deletes (permanently removes) a role
        /// </summary>
        [HttpDelete("{id}")]
        // [Authorize(Policy = "ManageRoles")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> HardDeleteRole(int id)
        {
            try
            {
                var (success, message) = await _roleService.HardDeleteRoleAsync(id);

                if (success)
                {
                    return Ok(new { message });
                }

                if (message.Contains("not found"))
                {
                    return NotFound(new { message });
                }

                return BadRequest(new { message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting role {RoleId}", id);
                return StatusCode(500, "An error occurred while deleting the role");
            }
        }

        /// <summary>
        /// Gets statistics about roles
        /// </summary>
        [HttpGet("statistics")]
        // [Authorize(Policy = "ViewRoleStatistics")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetRoleStatistics()
        {
            try
            {
                var stats = await _roleService.GetRoleStatisticsAsync();
                return Ok(new { status = true, message = "Role statistics retrieved successfully", data = stats });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting role statistics");
                return StatusCode(500, new { status = false, message = "An error occurred while retrieving role statistics", data = (object)null });
            }
        }
    }
}
