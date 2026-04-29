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
    [Route("api/UmUserRole")]
    [ApiController]
    // [Authorize]
    public class UserRoleController : ControllerBase
    {
        private readonly IUserRoleService _userRoleService;
        private readonly ILogger<UserRoleController> _logger;

        public UserRoleController(IUserRoleService userRoleService, ILogger<UserRoleController> logger)
        {
            _userRoleService = userRoleService;
            _logger = logger;
        }

        /// <summary>
        /// Assign a role to a user
        /// </summary>
        [HttpPost("assign")]
        // [Authorize(Policy = "RequireAdminRole")]
        public async Task<IActionResult> AssignRole([FromBody] AssignRoleDto dto)
        {
            try
            {
                var (success, message) = await _userRoleService.AssignRoleToUserAsync(dto);

                if (!success)
                {
                    return BadRequest(new { status = false, message, data = (object)null });
                }

                return Ok(new { status = true, message, data = dto });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning role {RoleId} to user {UserId}", dto.RoleId, dto.UserId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { status = false, message = "An error occurred while assigning the role.", data = (object)null });
            }
        }

        /// <summary>
        /// Assign multiple roles to a user
        /// </summary>
        [HttpPost("assign/roles")]
        // [Authorize(Policy = "RequireAdminRole")]
        public async Task<IActionResult> AssignRolesToUser([FromBody] AssignRolesToUserDto dto)
        {
            try
            {
                var result = await _userRoleService.AssignRolesToUserAsync(dto);

                if (!result.Success)
                {
                    return BadRequest(new { status = false, message = result.Message, data = (object)null });
                }

                return Ok(new { status = true, message = result.Message, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error batch assigning roles to user {UserId}", dto.UserId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { status = false, message = "An error occurred while assigning roles.", data = (object)null });
            }
        }

        /// <summary>
        /// Assign a role to multiple users
        /// </summary>
        [HttpPost("assign/users")]
        // [Authorize(Policy = "RequireAdminRole")]
        public async Task<IActionResult> AssignRoleToUsers([FromBody] AssignRoleToUsersDto dto)
        {
            try
            {
                var result = await _userRoleService.AssignRoleToUsersAsync(dto);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error batch assigning role {RoleId} to users", dto.RoleId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while assigning role to users." });
            }
        }


        [HttpGet("user-roles")]
        // [Authorize(Policy = "RequireAdminRole")]
        public async Task<IActionResult> GetAllUserRoles()
        {
            try
            {
                var result = await _userRoleService.GetAllUserRolesAsync();
                return Ok(new
                {
                    status = true,
                    message = "User roles retrieved successfully",
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all user roles");
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    status = false,
                    message = "An error occurred while retrieving user roles.",
                    data = (object)null
                });
            }
        }

        /// <summary>
        /// Revoke a role from a user
        /// </summary>
        [HttpPost("revoke")]
        // [Authorize(Policy = "RequireAdminRole")]
        public async Task<IActionResult> RevokeRole([FromBody] RevokeRoleDto dto)
        {
            try
            {
                var (success, message) = await _userRoleService.RevokeRoleFromUserAsync(dto);

                if (!success)
                {
                    return BadRequest(new { message });
                }

                return Ok(new { message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking role {RoleId} from user {UserId}",
                    dto.RoleId, dto.UserId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while revoking the role." });
            }
        }

        /// <summary>
        /// Revoke all roles from a user
        /// </summary>
        [HttpPost("revoke/all/user/{userId}")]
        // [Authorize(Policy = "RequireAdminRole")]
        public async Task<IActionResult> RevokeAllRolesFromUser(int userId)
        {
            try
            {
                var result = await _userRoleService.RevokeAllRolesFromUserAsync(userId);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking all roles from user {UserId}", userId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while revoking roles." });
            }
        }

        /// <summary>
        /// Revoke a role from all users
        /// </summary>
        [HttpPost("revoke/all/role/{roleId}")]
        // [Authorize(Policy = "RequireAdminRole")]
        public async Task<IActionResult> RevokeRoleFromAllUsers(int roleId)
        {
            try
            {
                var result = await _userRoleService.RevokeRoleFromAllUsersAsync(roleId);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking role {RoleId} from all users", roleId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while revoking role from users." });
            }
        }

        /// <summary>
        /// Get all roles assigned to a user
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserRoles(int userId)
        {
            try
            {
                var roles = await _userRoleService.GetUserRolesAsync(userId);
                return Ok(new
                {
                    status = true,
                    message = "User roles retrieved successfully",
                    data = roles
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting roles for user {UserId}", userId);
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    status = false,
                    message = "An error occurred while retrieving user roles.",
                    data = (object)null
                });
            }
        }

        /// <summary>
        /// Get all users assigned to a role
        /// </summary>
        [HttpGet("role/{roleId}")]
        public async Task<IActionResult> GetUsersWithRole(int roleId)
        {
            try
            {
                var users = await _userRoleService.GetUsersWithRoleAsync(roleId);
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users for role {RoleId}", roleId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving role users." });
            }
        }

        /// <summary>
        /// Get paginated list of users assigned to a role
        /// </summary>
        [HttpGet("role/{roleId}/paginated")]
        public async Task<IActionResult> GetUsersWithRolePaginated(
            int roleId,
            [FromQuery] UserRoleQueryParametersDto parameters)
        {
            try
            {
                var users = await _userRoleService.GetUsersWithRolePaginatedAsync(roleId, parameters);
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paginated users for role {RoleId}", roleId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving role users." });
            }
        }

        /// <summary>
        /// Check if a user has a specific role
        /// </summary>
        [HttpGet("check/{userId}/{roleId}")]
        public async Task<IActionResult> CheckUserHasRole(int userId, int roleId)
        {
            try
            {
                var hasRole = await _userRoleService.UserHasRoleAsync(userId, roleId);
                return Ok(new UserHasRoleResultDto { HasRole = hasRole });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if user {UserId} has role {RoleId}",
                    userId, roleId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while checking the role." });
            }
        }

        /// <summary>
        /// Check if a user has a specific role by role name
        /// </summary>
        [HttpGet("check/{userId}/byname/{roleName}")]
        public async Task<IActionResult> CheckUserHasRoleByName(int userId, string roleName)
        {
            try
            {
                var hasRole = await _userRoleService.UserHasRoleByNameAsync(userId, roleName);
                return Ok(new UserHasRoleResultDto { HasRole = hasRole });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if user {UserId} has role {RoleName}",
                    userId, roleName);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while checking the role." });
            }
        }

        /// <summary>
        /// Get all roles not assigned to a user
        /// </summary>
        [HttpGet("unassigned/roles/{userId}")]
        public async Task<IActionResult> GetUnassignedRolesForUser(int userId, [FromQuery] bool? isActive = true)
        {
            try
            {
                var roles = await _userRoleService.GetUnassignedRolesForUserAsync(userId, isActive);
                return Ok(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unassigned roles for user {UserId}", userId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving unassigned roles." });
            }
        }

        /// <summary>
        /// Get all users not assigned to a role
        /// </summary>
        [HttpGet("unassigned/users/{roleId}")]
        public async Task<IActionResult> GetUsersWithoutRole(int roleId, [FromQuery] bool? isActive = true)
        {
            try
            {
                var users = await _userRoleService.GetUsersWithoutRoleAsync(roleId, isActive);
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users without role {RoleId}", roleId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving users without role." });
            }
        }

        /// <summary>
        /// Sync user roles by replacing all current roles with the provided list
        /// </summary>
        [HttpPost("sync")]
        // [Authorize(Policy = "RequireAdminRole")]
        public async Task<IActionResult> SyncUserRoles([FromBody] SyncUserRolesDto dto)
        {
            try
            {
                var result = await _userRoleService.SyncUserRolesAsync(dto);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing roles for user {UserId}", dto.UserId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while syncing user roles." });
            }
        }

        /// <summary>
        /// Get user role statistics
        /// </summary>
        [HttpGet("statistics")]
        // [Authorize(Policy = "RequireAdminRole")]
        public async Task<IActionResult> GetStatistics()
        {
            try
            {
                var statistics = await _userRoleService.GetUserRoleStatisticsAsync();
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user role statistics");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving statistics." });
            }
        }

        /// <summary>
        /// Check if user has any permissions from their roles
        /// </summary>
        [HttpPost("permissions/any")]
        public async Task<IActionResult> UserHasAnyPermission([FromBody] UserPermissionCheckDto dto)
        {
            try
            {
                var hasPermission = await _userRoleService.UserHasAnyPermissionAsync(dto.UserId, dto.PermissionIds);
                return Ok(new UserHasPermissionResultDto { HasPermission = hasPermission });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if user {UserId} has any permissions", dto.UserId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while checking permissions." });
            }
        }

        /// <summary>
        /// Check if user has all specified permissions from their roles
        /// </summary>
        [HttpPost("permissions/all")]
        public async Task<IActionResult> UserHasAllPermissions([FromBody] UserPermissionCheckDto dto)
        {
            try
            {
                var hasAllPermissions = await _userRoleService.UserHasAllPermissionsAsync(dto.UserId, dto.PermissionIds);
                return Ok(new UserHasPermissionResultDto { HasPermission = hasAllPermissions });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if user {UserId} has all permissions", dto.UserId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while checking permissions." });
            }
        }

        /// <summary>
        /// Get all permissions a user has through their roles
        /// </summary>
        [HttpGet("permissions/{userId}")]
        public async Task<IActionResult> GetAllUserPermissions(int userId)
        {
            try
            {
                var permissions = await _userRoleService.GetAllUserPermissionsAsync(userId);
                return Ok(new
                {
                    status = true,
                    message = "User permissions retrieved successfully",
                    data = permissions
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all permissions for user {UserId}", userId);
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    status = false,
                    message = "An error occurred while retrieving user permissions.",
                    data = (object)null
                });
            }
        }

        /// <summary>
        /// Get all users not assigned to any role
        /// </summary>
        [HttpGet("unassigned/users")]
        public async Task<IActionResult> GetAllUnassignedUsers([FromQuery] bool? isActive = true)
        {
            try
            {
                var users = await _userRoleService.GetAllUnassignedUsersAsync(isActive);
                return Ok(new
                {
                    status = true,
                    message = "Unassigned users retrieved successfully",
                    data = users
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unassigned users");
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    status = false,
                    message = "An error occurred while retrieving unassigned users.",
                    data = (object)null
                });
            }
        }

        [HttpPut("assignment/by-id/{id}")]
        public async Task<IActionResult> UpdateUserRoleById(int id, [FromBody] UpdateUserRoleByIdDto dto)
        {
            try
            {
                dto.Id = id; // Ensure route param is used
                var success = await _userRoleService.UpdateUserRoleByIdAsync(dto);
                if (!success)
                    return StatusCode(StatusCodes.Status500InternalServerError, new
                    {
                        status = false,
                        message = "An error occurred while updating the user role assignment by id.",
                        data = (object)null
                    });

                return Ok(new
                {
                    status = true,
                    message = "User role assignment updated successfully by id.",
                    data = dto
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user role assignment by id {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    status = false,
                    message = "An error occurred while updating the user role assignment by id.",
                    data = (object)null
                });
            }
        }

[HttpDelete("assignment/by-id/{id}")]
public async Task<IActionResult> DeleteUserRoleById(int id)
{
    try
    {
        var success = await _userRoleService.DeleteUserRoleByIdAsync(id);
        if (!success)
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                status = false,
                message = "An error occurred while deleting the user role assignment by id.",
                data = (object)null
            });

        return Ok(new
        {
            status = true,
            message = "User role assignment deleted successfully by id.",
            data = success
        });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error deleting user role assignment by id {Id}", id);
        return StatusCode(StatusCodes.Status500InternalServerError, new
        {
            status = false,
            message = "An error occurred while deleting the user role assignment by id.",
            data = (object)null
        });
    }
}
    }
}
