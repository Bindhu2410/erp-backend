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
    [Route("api/UmPermission")]
    [ApiController]
    // [Authorize]
    public class PermissionController : ControllerBase
    {
        private readonly IPermissionService _permissionService;
        private readonly ILogger<PermissionController> _logger;

        public PermissionController(IPermissionService permissionService, ILogger<PermissionController> logger)
        {
            _permissionService = permissionService;
            _logger = logger;
        }

        [HttpPost]
        [Authorize(Policy = "RequireAdminRole")]
        public async Task<IActionResult> CreatePermission([FromBody] CreatePermissionDto dto)
        {
            try
            {
                var (success, message, permissionId) = await _permissionService.CreatePermissionAsync(dto);

                if (!success)
                {
                    return BadRequest(new { message });
                }

                return CreatedAtAction(nameof(GetPermissionById), new { id = permissionId }, 
                    new { permissionId, message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating permission");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "An error occurred while creating the permission." });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPermissionById(int id)
        {
            try
            {
                var permission = await _permissionService.GetPermissionByIdAsync(id);

                if (permission == null)
                {
                    return NotFound(new { message = "Permission not found." });
                }

                return Ok(permission);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting permission by ID {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "An error occurred while retrieving the permission." });
            }
        }

        [HttpGet("name/{name}")]
        public async Task<IActionResult> GetPermissionByName(string name)
        {
            try
            {
                var permission = await _permissionService.GetPermissionByNameAsync(name);

                if (permission == null)
                {
                    return NotFound(new { message = "Permission not found." });
                }

                return Ok(permission);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting permission by name {Name}", name);
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "An error occurred while retrieving the permission." });
            }
        }

        [HttpGet]
public async Task<IActionResult> GetAllPermissions(
    [FromQuery] int pageNumber = 1, 
    [FromQuery] int pageSize = 10, 
    [FromQuery] string searchTerm = null,
    [FromQuery] string category = null,
    [FromQuery] bool? isActive = null)
{
    try
    {
        var result = await _permissionService.GetAllPermissionsAsync(
            pageNumber, pageSize, searchTerm, category, isActive);

        return Ok(new
        {
            status = true,
            message = "Permissions retrieved successfully",
            data = result
        });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error getting permissions");
        return StatusCode(StatusCodes.Status500InternalServerError, new
        {
            status = false,
            message = "An error occurred while retrieving permissions.",
            data = (object)null
        });
    }
}

        [HttpGet("category/{category}")]
        public async Task<IActionResult> GetPermissionsByCategory(string category, [FromQuery] bool? isActive = true)
        {
            try
            {
                var permissions = await _permissionService.GetPermissionsByCategoryAsync(category, isActive);
                return Ok(permissions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting permissions by category {Category}", category);
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "An error occurred while retrieving permissions." });
            }
        }

        [HttpGet("categories")]
        public async Task<IActionResult> GetPermissionCategories()
        {
            try
            {
                var categories = await _permissionService.GetPermissionCategoriesAsync();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting permission categories");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "An error occurred while retrieving permission categories." });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "RequireAdminRole")]
        public async Task<IActionResult> UpdatePermission(int id, [FromBody] UpdatePermissionDto dto)
        {
            try
            {
                var (success, message) = await _permissionService.UpdatePermissionAsync(id, dto);

                if (!success)
                {
                    return BadRequest(new { message });
                }

                return Ok(new { message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating permission {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "An error occurred while updating the permission." });
            }
        }

        [HttpPatch("{id}/deactivate")]
        [Authorize(Policy = "RequireAdminRole")]
        public async Task<IActionResult> SoftDeletePermission(int id)
        {
            try
            {
                var (success, message) = await _permissionService.SoftDeletePermissionAsync(id);

                if (!success)
                {
                    return BadRequest(new { message });
                }

                return Ok(new { message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating permission {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "An error occurred while deactivating the permission." });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "RequireAdminRole")]
        public async Task<IActionResult> HardDeletePermission(int id)
        {
            try
            {
                var (success, message) = await _permissionService.HardDeletePermissionAsync(id);

                if (!success)
                {
                    return BadRequest(new { message });
                }

                return Ok(new { message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting permission {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "An error occurred while deleting the permission." });
            }
        }

        [HttpGet("statistics")]
        [Authorize(Policy = "RequireAdminRole")]
        public async Task<IActionResult> GetPermissionStatistics()
        {
            try
            {
                var statistics = await _permissionService.GetPermissionStatisticsAsync();
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting permission statistics");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "An error occurred while retrieving permission statistics." });
            }
        }

        

        [HttpPost("batch")]
        [Authorize(Policy = "RequireAdminRole")]
        public async Task<IActionResult> BatchCreatePermissions([FromBody] BatchCreatePermissionsDto dto)
        {
            try
            {
                var result = await _permissionService.BatchCreatePermissionsAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error batch creating permissions");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "An error occurred while creating permissions in batch." });
            }
        }
    }
}
