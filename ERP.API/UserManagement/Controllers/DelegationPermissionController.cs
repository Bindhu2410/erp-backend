using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ERP.API.UserManagement.Services;
using ERP.API.UserManagement.DTOs;
using System.Security.Claims;

namespace ERP.API.UserManagement.Controllers
{
    /// <summary>
    /// API Controller for Delegation Permission Management operations
    /// </summary>
    [ApiController]
    [Route("api/UmDelegationPermission")]
    [Produces("application/json")]
    public class DelegationPermissionController : ControllerBase
    {
        private readonly IDelegationPermissionService _delegationPermissionService;
        private readonly ILogger<DelegationPermissionController> _logger;

        public DelegationPermissionController(
            IDelegationPermissionService delegationPermissionService,
            ILogger<DelegationPermissionController> logger)
        {
            _delegationPermissionService = delegationPermissionService;
            _logger = logger;
        }

        #region Basic CRUD Operations

        /// <summary>
        /// Creates a new delegation permission
        /// </summary>
        /// <param name="createDto">Delegation permission creation details</param>
        /// <returns>Operation result</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponseDto<DelegationPermissionResultDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponseDto<DelegationPermissionResultDto>>> CreateDelegationPermission([FromBody] CreateDelegationPermissionDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = "Validation failed",
                        Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                    });
                }

                var result = await _delegationPermissionService.CreateDelegationPermissionAsync(createDto);

                if (result.Success)
                {
                    return CreatedAtAction(
                        nameof(GetDelegationPermissions),
                        new { delegationId = createDto.DelegationId },
                        new ApiResponseDto<DelegationPermissionResultDto>
                        {
                            Success = true,
                            Message = "Delegation permission created successfully",
                            Data = result
                        });
                }

                return BadRequest(new ApiResponseDto<DelegationPermissionResultDto>
                {
                    Success = false,
                    Message = result.Message,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating delegation permission");
                return StatusCode(500, new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "An error occurred while creating the delegation permission",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Gets permissions for a specific delegation
        /// </summary>
        /// <param name="delegationId">Delegation ID</param>
        /// <returns>List of delegation permissions</returns>
        [HttpGet("{delegationId:int}")]
        [ProducesResponseType(typeof(ApiResponseDto<List<DelegationPermissionDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponseDto<List<DelegationPermissionDto>>>> GetDelegationPermissions(int delegationId)
        {
            try
            {
                var permissions = await _delegationPermissionService.GetDelegationPermissionsAsync(delegationId);

                return Ok(new ApiResponseDto<List<DelegationPermissionDto>>
                {
                    Success = true,
                    Message = "Delegation permissions retrieved successfully",
                    Data = permissions
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving delegation permissions for DelegationId: {DelegationId}", delegationId);
                return StatusCode(500, new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving delegation permissions",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Gets all delegation permissions with pagination
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 20)</param>
        /// <param name="delegationId">Optional delegation ID filter</param>
        /// <returns>Paged delegation permissions</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponseDto<PagedDelegationPermissionsResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponseDto<PagedDelegationPermissionsResponseDto>>> GetAllDelegationPermissions(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] int? delegationId = null)
        {
            try
            {
                var request = new PagedDelegationPermissionsRequestDto
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    DelegationId = delegationId
                };

                var result = await _delegationPermissionService.GetAllDelegationPermissionsAsync(request);

                return Ok(new ApiResponseDto<PagedDelegationPermissionsResponseDto>
                {
                    Success = true,
                    Message = "Delegation permissions retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all delegation permissions");
                return StatusCode(500, new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving delegation permissions",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Checks if a permission exists for a delegation
        /// </summary>
        /// <param name="delegationId">Delegation ID</param>
        /// <param name="permissionId">Permission ID</param>
        /// <returns>True if exists, false otherwise</returns>
        [HttpGet("{delegationId:int}/permissions/{permissionId:int}/exists")]
        [ProducesResponseType(typeof(ApiResponseDto<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponseDto<bool>>> CheckDelegationPermissionExists(int delegationId, int permissionId)
        {
            try
            {
                var exists = await _delegationPermissionService.CheckDelegationPermissionExistsAsync(delegationId, permissionId);

                return Ok(new ApiResponseDto<bool>
                {
                    Success = true,
                    Message = "Permission existence checked successfully",
                    Data = exists
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking delegation permission existence for DelegationId: {DelegationId}, PermissionId: {PermissionId}", 
                    delegationId, permissionId);
                return StatusCode(500, new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "An error occurred while checking permission existence",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Gets available permissions for a delegation (not yet assigned)
        /// </summary>
        /// <param name="delegationId">Delegation ID</param>
        /// <returns>List of available permissions</returns>
        [HttpGet("{delegationId:int}/available-permissions")]
        [ProducesResponseType(typeof(ApiResponseDto<List<AvailablePermissionDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponseDto<List<AvailablePermissionDto>>>> GetAvailablePermissionsForDelegation(int delegationId)
        {
            try
            {
                var permissions = await _delegationPermissionService.GetAvailablePermissionsForDelegationAsync(delegationId);

                return Ok(new ApiResponseDto<List<AvailablePermissionDto>>
                {
                    Success = true,
                    Message = "Available permissions retrieved successfully",
                    Data = permissions
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving available permissions for DelegationId: {DelegationId}", delegationId);
                return StatusCode(500, new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving available permissions",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Gets delegation permission summary
        /// </summary>
        /// <param name="delegationId">Delegation ID</param>
        /// <returns>Delegation permission summary</returns>
        [HttpGet("{delegationId:int}/summary")]
        [ProducesResponseType(typeof(ApiResponseDto<DelegationPermissionSummaryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponseDto<DelegationPermissionSummaryDto>>> GetDelegationPermissionSummary(int delegationId)
        {
            try
            {
                var summary = await _delegationPermissionService.GetDelegationPermissionSummaryAsync(delegationId);

                if (summary == null)
                {
                    return NotFound(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = "Delegation not found or has no permissions"
                    });
                }

                return Ok(new ApiResponseDto<DelegationPermissionSummaryDto>
                {
                    Success = true,
                    Message = "Delegation permission summary retrieved successfully",
                    Data = summary
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving delegation permission summary for DelegationId: {DelegationId}", delegationId);
                return StatusCode(500, new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving delegation permission summary",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        #endregion

        #region Update Operations

        /// <summary>
        /// Bulk updates delegation permissions
        /// </summary>
        /// <param name="updateDto">Bulk update details</param>
        /// <returns>Operation result</returns>
        [HttpPut]
        [ProducesResponseType(typeof(ApiResponseDto<DelegationPermissionResultDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponseDto<DelegationPermissionResultDto>>> UpdateDelegationPermissions([FromBody] UpdateDelegationPermissionsDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = "Validation failed",
                        Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                    });
                }

                var result = await _delegationPermissionService.UpdateDelegationPermissionsAsync(updateDto);

                if (result.Success)
                {
                    return Ok(new ApiResponseDto<DelegationPermissionResultDto>
                    {
                        Success = true,
                        Message = "Delegation permissions updated successfully",
                        Data = result
                    });
                }

                return BadRequest(new ApiResponseDto<DelegationPermissionResultDto>
                {
                    Success = false,
                    Message = result.Message,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating delegation permissions");
                return StatusCode(500, new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "An error occurred while updating delegation permissions",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        #endregion

        #region Delete Operations

        /// <summary>
        /// Deletes a specific delegation permission
        /// </summary>
        /// <param name="delegationId">Delegation ID</param>
        /// <param name="permissionId">Permission ID</param>
        /// <returns>Operation result</returns>
        [HttpDelete("{delegationId:int}/permissions/{permissionId:int}")]
        [ProducesResponseType(typeof(ApiResponseDto<DelegationPermissionResultDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponseDto<DelegationPermissionResultDto>>> DeleteDelegationPermission(int delegationId, int permissionId)
        {
            try
            {
                var result = await _delegationPermissionService.DeleteDelegationPermissionAsync(delegationId, permissionId);

                if (result.Success)
                {
                    return Ok(new ApiResponseDto<DelegationPermissionResultDto>
                    {
                        Success = true,
                        Message = "Delegation permission deleted successfully",
                        Data = result
                    });
                }

                return NotFound(new ApiResponseDto<DelegationPermissionResultDto>
                {
                    Success = false,
                    Message = result.Message,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting delegation permission for DelegationId: {DelegationId}, PermissionId: {PermissionId}", 
                    delegationId, permissionId);
                return StatusCode(500, new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "An error occurred while deleting delegation permission",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Deletes all permissions for a delegation
        /// </summary>
        /// <param name="delegationId">Delegation ID</param>
        /// <returns>Operation result with deletion count</returns>
        [HttpDelete("{delegationId:int}/permissions")]
        [ProducesResponseType(typeof(ApiResponseDto<DeleteAllDelegationPermissionsResultDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponseDto<DeleteAllDelegationPermissionsResultDto>>> DeleteAllDelegationPermissions(int delegationId)
        {
            try
            {
                var result = await _delegationPermissionService.DeleteAllDelegationPermissionsAsync(delegationId);

                if (result.Success)
                {
                    return Ok(new ApiResponseDto<DeleteAllDelegationPermissionsResultDto>
                    {
                        Success = true,
                        Message = "All delegation permissions deleted successfully",
                        Data = result
                    });
                }

                return NotFound(new ApiResponseDto<DeleteAllDelegationPermissionsResultDto>
                {
                    Success = false,
                    Message = result.Message,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting all delegation permissions for DelegationId: {DelegationId}", delegationId);
                return StatusCode(500, new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "An error occurred while deleting all delegation permissions",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        #endregion
    }
}
