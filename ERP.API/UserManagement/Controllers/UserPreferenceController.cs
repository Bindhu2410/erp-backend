using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ERP.API.UserManagement.Services;
using ERP.API.UserManagement.DTOs;
using System.Security.Claims;

namespace ERP.API.UserManagement.Controllers
{
    /// <summary>
    /// API Controller for User Preference Management operations
    /// </summary>
    [ApiController]
    [Route("api/UmUserPreference")]
    [Produces("application/json")]
    public class UserPreferenceController : ControllerBase
    {
        private readonly IUserPreferenceService _userPreferenceService;
        private readonly ILogger<UserPreferenceController> _logger;

        public UserPreferenceController(
            IUserPreferenceService userPreferenceService,
            ILogger<UserPreferenceController> logger)
        {
            _userPreferenceService = userPreferenceService;
            _logger = logger;
        }

        #region Basic CRUD Operations

        /// <summary>
        /// Creates or updates a user preference (UPSERT)
        /// </summary>
        /// <param name="createDto">User preference creation details</param>
        /// <returns>Operation result</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponseDto<UserPreferenceResultDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponseDto<UserPreferenceResultDto>>> CreateUserPreference([FromBody] CreateUserPreferenceDto createDto)
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

                var result = await _userPreferenceService.CreateUserPreferenceAsync(createDto);

                if (result.Success)
                {
                    return CreatedAtAction(
                        nameof(GetUserPreference),
                        new { userId = createDto.UserId, preferenceKey = createDto.PreferenceKey },
                        new ApiResponseDto<UserPreferenceResultDto>
                        {
                            Success = true,
                            Message = "User preference created/updated successfully",
                            Data = result
                        });
                }

                return BadRequest(new ApiResponseDto<UserPreferenceResultDto>
                {
                    Success = false,
                    Message = result.Message,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user preference");
                return StatusCode(500, new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "An error occurred while creating the user preference",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Gets a specific user preference by key
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="preferenceKey">Preference key</param>
        /// <returns>User preference</returns>
        [HttpGet("{userId:int}/{preferenceKey}")]
        [ProducesResponseType(typeof(ApiResponseDto<UserPreferenceDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponseDto<UserPreferenceDto>>> GetUserPreference(int userId, string preferenceKey)
        {
            try
            {
                var preference = await _userPreferenceService.GetUserPreferenceAsync(userId, preferenceKey);

                if (preference == null)
                {
                    return NotFound(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = "User preference not found"
                    });
                }

                return Ok(new ApiResponseDto<UserPreferenceDto>
                {
                    Success = true,
                    Message = "User preference retrieved successfully",
                    Data = preference
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user preference for UserId: {UserId}, Key: {Key}", userId, preferenceKey);
                return StatusCode(500, new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving user preference",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Gets all preferences for a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of user preferences</returns>
        [HttpGet("user/{userId:int}")]
        [ProducesResponseType(typeof(ApiResponseDto<List<UserPreferenceDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponseDto<List<UserPreferenceDto>>>> GetUserPreferences(int userId)
        {
            try
            {
                var preferences = await _userPreferenceService.GetUserPreferencesAsync(userId);

                return Ok(new ApiResponseDto<List<UserPreferenceDto>>
                {
                    Success = true,
                    Message = "User preferences retrieved successfully",
                    Data = preferences
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user preferences for UserId: {UserId}", userId);
                return StatusCode(500, new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving user preferences",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Gets all user preferences with pagination
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 20)</param>
        /// <param name="userId">Optional user ID filter</param>
        /// <returns>Paged user preferences</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponseDto<PagedUserPreferencesResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponseDto<PagedUserPreferencesResponseDto>>> GetAllUserPreferences(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] int? userId = null)
        {
            try
            {
                var request = new PagedUserPreferencesRequestDto
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    UserId = userId
                };

                var result = await _userPreferenceService.GetAllUserPreferencesAsync(request);

                return Ok(new ApiResponseDto<PagedUserPreferencesResponseDto>
                {
                    Success = true,
                    Message = "User preferences retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all user preferences");
                return StatusCode(500, new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving user preferences",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Checks if a user preference exists
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="preferenceKey">Preference key</param>
        /// <returns>True if exists, false otherwise</returns>
        [HttpGet("{userId:int}/{preferenceKey}/exists")]
        [ProducesResponseType(typeof(ApiResponseDto<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponseDto<bool>>> CheckUserPreferenceExists(int userId, string preferenceKey)
        {
            try
            {
                var exists = await _userPreferenceService.CheckUserPreferenceExistsAsync(userId, preferenceKey);

                return Ok(new ApiResponseDto<bool>
                {
                    Success = true,
                    Message = "User preference existence checked successfully",
                    Data = exists
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking user preference existence for UserId: {UserId}, Key: {Key}", 
                    userId, preferenceKey);
                return StatusCode(500, new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "An error occurred while checking user preference existence",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Gets user preferences by key pattern
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="keyPattern">Key pattern to search for</param>
        /// <returns>List of matching user preferences</returns>
        [HttpGet("user/{userId:int}/pattern/{keyPattern}")]
        [ProducesResponseType(typeof(ApiResponseDto<List<UserPreferenceDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponseDto<List<UserPreferenceDto>>>> GetUserPreferencesByPattern(int userId, string keyPattern)
        {
            try
            {
                var preferences = await _userPreferenceService.GetUserPreferencesByPatternAsync(userId, keyPattern);

                return Ok(new ApiResponseDto<List<UserPreferenceDto>>
                {
                    Success = true,
                    Message = "User preferences by pattern retrieved successfully",
                    Data = preferences
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user preferences by pattern for UserId: {UserId}, Pattern: {Pattern}", 
                    userId, keyPattern);
                return StatusCode(500, new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving user preferences by pattern",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Gets user preferences summary
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>User preferences summary</returns>
        [HttpGet("user/{userId:int}/summary")]
        [ProducesResponseType(typeof(ApiResponseDto<UserPreferencesSummaryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponseDto<UserPreferencesSummaryDto>>> GetUserPreferencesSummary(int userId)
        {
            try
            {
                var summary = await _userPreferenceService.GetUserPreferencesSummaryAsync(userId);

                if (summary == null)
                {
                    return NotFound(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = "User not found"
                    });
                }

                return Ok(new ApiResponseDto<UserPreferencesSummaryDto>
                {
                    Success = true,
                    Message = "User preferences summary retrieved successfully",
                    Data = summary
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user preferences summary for UserId: {UserId}", userId);
                return StatusCode(500, new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving user preferences summary",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Gets users with a specific preference
        /// </summary>
        /// <param name="preferenceKey">Preference key</param>
        /// <param name="preferenceValue">Optional preference value filter</param>
        /// <returns>List of users with the preference</returns>
        [HttpGet("by-preference/{preferenceKey}")]
        [ProducesResponseType(typeof(ApiResponseDto<List<UserWithPreferenceDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponseDto<List<UserWithPreferenceDto>>>> GetUsersWithPreference(
            string preferenceKey, 
            [FromQuery] string? preferenceValue = null)
        {
            try
            {
                var users = await _userPreferenceService.GetUsersWithPreferenceAsync(preferenceKey, preferenceValue);

                return Ok(new ApiResponseDto<List<UserWithPreferenceDto>>
                {
                    Success = true,
                    Message = "Users with preference retrieved successfully",
                    Data = users
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users with preference Key: {Key}, Value: {Value}", 
                    preferenceKey, preferenceValue);
                return StatusCode(500, new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving users with preference",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        #endregion

        #region Update Operations

        /// <summary>
        /// Updates a user preference
        /// </summary>
        /// <param name="updateDto">Update details</param>
        /// <returns>Operation result</returns>
        [HttpPut]
        [ProducesResponseType(typeof(ApiResponseDto<UserPreferenceResultDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponseDto<UserPreferenceResultDto>>> UpdateUserPreference([FromBody] UpdateUserPreferenceDto updateDto)
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

                var result = await _userPreferenceService.UpdateUserPreferenceAsync(updateDto);

                if (result.Success)
                {
                    return Ok(new ApiResponseDto<UserPreferenceResultDto>
                    {
                        Success = true,
                        Message = "User preference updated successfully",
                        Data = result
                    });
                }

                return BadRequest(new ApiResponseDto<UserPreferenceResultDto>
                {
                    Success = false,
                    Message = result.Message,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user preference");
                return StatusCode(500, new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "An error occurred while updating user preference",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Bulk updates user preferences
        /// </summary>
        /// <param name="bulkUpdateDto">Bulk update details</param>
        /// <returns>Operation result</returns>
        [HttpPut("bulk")]
        [ProducesResponseType(typeof(ApiResponseDto<BulkUserPreferenceResultDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponseDto<BulkUserPreferenceResultDto>>> BulkUpdateUserPreferences([FromBody] BulkUpdateUserPreferencesDto bulkUpdateDto)
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

                var result = await _userPreferenceService.BulkUpdateUserPreferencesAsync(bulkUpdateDto);

                if (result.Success)
                {
                    return Ok(new ApiResponseDto<BulkUserPreferenceResultDto>
                    {
                        Success = true,
                        Message = "User preferences bulk updated successfully",
                        Data = result
                    });
                }

                return BadRequest(new ApiResponseDto<BulkUserPreferenceResultDto>
                {
                    Success = false,
                    Message = result.Message,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk updating user preferences");
                return StatusCode(500, new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "An error occurred while bulk updating user preferences",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        #endregion

        #region Delete Operations

        /// <summary>
        /// Deletes a specific user preference
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="preferenceKey">Preference key</param>
        /// <returns>Operation result</returns>
        [HttpDelete("{userId:int}/{preferenceKey}")]
        [ProducesResponseType(typeof(ApiResponseDto<UserPreferenceResultDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponseDto<UserPreferenceResultDto>>> DeleteUserPreference(int userId, string preferenceKey)
        {
            try
            {
                var result = await _userPreferenceService.DeleteUserPreferenceAsync(userId, preferenceKey);

                if (result.Success)
                {
                    return Ok(new ApiResponseDto<UserPreferenceResultDto>
                    {
                        Success = true,
                        Message = "User preference deleted successfully",
                        Data = result
                    });
                }

                return NotFound(new ApiResponseDto<UserPreferenceResultDto>
                {
                    Success = false,
                    Message = result.Message,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user preference for UserId: {UserId}, Key: {Key}", 
                    userId, preferenceKey);
                return StatusCode(500, new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "An error occurred while deleting user preference",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Deletes all preferences for a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Operation result with deletion count</returns>
        [HttpDelete("user/{userId:int}")]
        [ProducesResponseType(typeof(ApiResponseDto<DeleteAllUserPreferencesResultDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponseDto<DeleteAllUserPreferencesResultDto>>> DeleteAllUserPreferences(int userId)
        {
            try
            {
                var result = await _userPreferenceService.DeleteAllUserPreferencesAsync(userId);

                if (result.Success)
                {
                    return Ok(new ApiResponseDto<DeleteAllUserPreferencesResultDto>
                    {
                        Success = true,
                        Message = "All user preferences deleted successfully",
                        Data = result
                    });
                }

                return NotFound(new ApiResponseDto<DeleteAllUserPreferencesResultDto>
                {
                    Success = false,
                    Message = result.Message,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting all user preferences for UserId: {UserId}", userId);
                return StatusCode(500, new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "An error occurred while deleting all user preferences",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        #endregion
    }
}
