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
    [Route("api/UmRolePermission")]
    [ApiController]
    // [Authorize]
    public class RolePermissionController : ControllerBase
    {
        private readonly IRolePermissionService _rolePermissionService;
        private readonly ILogger<RolePermissionController> _logger;

        public RolePermissionController(IRolePermissionService rolePermissionService, ILogger<RolePermissionController> logger)
        {
            _rolePermissionService = rolePermissionService;
            _logger = logger;
        }

        /// <summary>
        /// Assign a permission to a role
        /// </summary>
        [HttpPost("assign")]
        // [Authorize(Policy = "RequireAdminRole")]
        public async Task<IActionResult> AssignPermission([FromBody] AssignPermissionDto dto)
        {
            try
            {
                var (success, message) = await _rolePermissionService.AssignPermissionToRoleAsync(dto);

                if (!success)
                    return BadRequest(new { status = false, message, data = (object)null });

                return Ok(new { status = true, message, data = dto });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning permission {PermissionId} to role {RoleId}", dto.PermissionId, dto.RoleId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { status = false, message = "An error occurred while assigning the permission.", data = (object)null });
            }
        }

        [HttpGet("roles-with-permissions")]
        public async Task<IActionResult> GetAllRolesWithPermissions()
        {
            try
            {
                var result = await _rolePermissionService.GetAllRolesWithPermissionsAsync();
                return Ok(new
                {
                    status = true,
                    message = "Roles with permissions retrieved successfully",
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting roles with permissions");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new
                    {
                        status = false,
                        message = "An error occurred while retrieving roles with permissions.",
                        data = (object)null
                    });
            }
        }
        /// <summary>
        /// Assign multiple permissions to a role
        /// </summary>
        [HttpPost("assign/batch")]
        // [Authorize(Policy = "RequireAdminRole")]
        public async Task<IActionResult> BatchAssignPermissions([FromBody] BatchAssignPermissionsDto dto)
        {
            try
            {
                var result = await _rolePermissionService.AssignPermissionsToRoleAsync(dto);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error batch assigning permissions to role {RoleId}", dto.RoleId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while assigning permissions." });
            }
        }

        /// <summary>
        /// Revoke a permission from a role
        /// </summary>
        [HttpPost("revoke")]
        // [Authorize(Policy = "RequireAdminRole")]
        public async Task<IActionResult> RevokePermission([FromBody] RevokePermissionDto dto)
        {
            try
            {
                var (success, message) = await _rolePermissionService.RevokePermissionFromRoleAsync(dto);

                if (!success)
                {
                    return BadRequest(new { message });
                }

                return Ok(new { message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking permission {PermissionId} from role {RoleId}",
                    dto.PermissionId, dto.RoleId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while revoking the permission." });
            }
        }

        /// <summary>
        /// Revoke all permissions from a role
        /// </summary>
        [HttpPost("revoke/all/{roleId}")]
        // [Authorize(Policy = "RequireAdminRole")]
        public async Task<IActionResult> RevokeAllPermissions(int roleId)
        {
            try
            {
                var result = await _rolePermissionService.RevokeAllPermissionsFromRoleAsync(roleId);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking all permissions from role {RoleId}", roleId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while revoking permissions." });
            }
        }

        /// <summary>
        /// Get all permissions assigned to a role
        /// </summary>
        [HttpGet("role/{roleId}")]
        public async Task<IActionResult> GetRolePermissions(int roleId)
        {
            try
            {
                var permissions = await _rolePermissionService.GetRolePermissionsAsync(roleId);
                return Ok(new
                {
                    status = true,
                    message = "Role permissions retrieved successfully",
                    data = permissions
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting permissions for role {RoleId}", roleId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { status = false, message = "An error occurred while retrieving role permissions.", data = (object)null });
            }
        }

        /// <summary>
        /// Get all roles that have a specific permission
        /// </summary>
        [HttpGet("permission/{permissionId}")]
        public async Task<IActionResult> GetPermissionRoles(int permissionId)
        {
            try
            {
                var roles = await _rolePermissionService.GetPermissionRolesAsync(permissionId);
                return Ok(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting roles for permission {PermissionId}", permissionId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving permission roles." });
            }
        }

        /// <summary>
        /// Check if a role has a specific permission
        /// </summary>
        [HttpGet("check/{roleId}/{permissionId}")]
        public async Task<IActionResult> CheckPermission(int roleId, int permissionId)
        {
            try
            {
                var hasPermission = await _rolePermissionService.RoleHasPermissionAsync(roleId, permissionId);
                return Ok(new
                {
                    status = true,
                    message = "Permission check completed",
                    data = new { hasPermission }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if role {RoleId} has permission {PermissionId}", roleId, permissionId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { status = false, message = "An error occurred while checking the permission.", data = (object)null });
            }
        }

        /// <summary>
        /// Check if a role has a specific permission by permission name
        /// </summary>
        [HttpGet("check/{roleId}/byname/{permissionName}")]
        public async Task<IActionResult> CheckPermissionByName(int roleId, string permissionName)
        {
            try
            {
                var hasPermission = await _rolePermissionService.RoleHasPermissionByNameAsync(roleId, permissionName);
                return Ok(new RoleHasPermissionResultDto { HasPermission = hasPermission });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if role {RoleId} has permission {PermissionName}",
                    roleId, permissionName);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while checking the permission." });
            }
        }

        /// <summary>
        /// Get all unassigned permissions for a role
        /// </summary>
        [HttpGet("unassigned/{roleId}")]
        public async Task<IActionResult> GetUnassignedPermissions(int roleId, [FromQuery] bool? isActive = true)
        {
            try
            {
                var permissions = await _rolePermissionService.GetUnassignedPermissionsForRoleAsync(roleId, isActive);
                return Ok(permissions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unassigned permissions for role {RoleId}", roleId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving unassigned permissions." });
            }
        }

        /// <summary>
        /// Sync role permissions by replacing all current permissions with the provided list
        /// </summary>
        [HttpPost("sync")]
        // [Authorize(Policy = "RequireAdminRole")]
        public async Task<IActionResult> SyncRolePermissions([FromBody] SyncRolePermissionsDto dto)
        {
            try
            {
                var result = await _rolePermissionService.SyncRolePermissionsAsync(dto);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing permissions for role {RoleId}", dto.RoleId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while syncing role permissions." });
            }
        }

        /// <summary>
        /// Get role permissions statistics
        /// </summary>
        [HttpGet("statistics")]
        // [Authorize(Policy = "RequireAdminRole")]
        public async Task<IActionResult> GetStatistics()
        {
            try
            {
                var statistics = await _rolePermissionService.GetRolePermissionsStatisticsAsync();
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting role permission statistics");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving statistics." });
            }
        }
    }
}
