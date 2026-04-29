using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ERP.API.UserManagement.Services;
using ERP.API.UserManagement.DTOs;
using System.Security.Claims;

namespace ERP.API.UserManagement.Controllers
{
    /// <summary>
    /// API Controller for Access Delegation Management operations
    /// </summary>
    [ApiController]
    [Route("api/UmAccessDelegation")]
    [Produces("application/json")]
    public class AccessDelegationController : ControllerBase
    {
        private readonly IAccessDelegationService _accessDelegationService;
        private readonly ILogger<AccessDelegationController> _logger;

        public AccessDelegationController(
            IAccessDelegationService accessDelegationService,
            ILogger<AccessDelegationController> logger)
        {
            _accessDelegationService = accessDelegationService;
            _logger = logger;
        }

        #region Basic CRUD Operations

        /// <summary>
        /// Creates a new access delegation
        /// </summary>
        /// <param name="createDto">Delegation creation details</param>
        /// <returns>Created delegation ID</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponseDto<int>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponseDto<int>>> CreateDelegation([FromBody] CreateAccessDelegationDto createDto)
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

                // Validate dates
                if (createDto.StartDate >= createDto.EndDate)
                {
                    return BadRequest(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = "Start date must be before end date",
                        Errors = new List<string> { "Invalid date range" }
                    });
                }

                // Validate that user is not delegating to themselves
                if (createDto.FromUserId == createDto.ToUserId)
                {
                    return BadRequest(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = "Cannot delegate to yourself",
                        Errors = new List<string> { "Invalid user delegation" }
                    });
                }

                var delegationId = await _accessDelegationService.CreateDelegationAsync(createDto);

                return CreatedAtAction(
                    nameof(GetDelegationById),
                    new { id = delegationId },
                    new ApiResponseDto<int>
                    {
                        Success = true,
                        Message = "Access delegation created successfully",
                        Data = delegationId
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating access delegation");
                return StatusCode(500, new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "An error occurred while creating the delegation",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Gets an access delegation by ID
        /// </summary>
        /// <param name="id">Delegation ID</param>
        /// <returns>Delegation details</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponseDto<AccessDelegationDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponseDto<AccessDelegationDto>>> GetDelegationById(int id)
        {
            try
            {
                var delegation = await _accessDelegationService.GetDelegationByIdAsync(id);

                if (delegation == null)
                {
                    return NotFound(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = "Access delegation not found",
                        Errors = new List<string> { $"Delegation with ID {id} not found" }
                    });
                }

                return Ok(new ApiResponseDto<AccessDelegationDto>
                {
                    Success = true,
                    Message = "Access delegation retrieved successfully",
                    Data = delegation
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving access delegation with ID: {Id}", id);
                return StatusCode(500, new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving the delegation",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Updates an existing access delegation
        /// </summary>
        /// <param name="id">Delegation ID</param>
        /// <param name="updateDto">Delegation update details</param>
        /// <returns>Update result</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponseDto<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponseDto<bool>>> UpdateDelegation(int id, [FromBody] UpdateAccessDelegationDto updateDto)
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

                // Ensure the ID in the URL matches the DTO
                updateDto.DelegationId = id;

                // Validate dates if provided
                if (updateDto.StartDate.HasValue && updateDto.EndDate.HasValue && 
                    updateDto.StartDate.Value >= updateDto.EndDate.Value)
                {
                    return BadRequest(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = "Start date must be before end date",
                        Errors = new List<string> { "Invalid date range" }
                    });
                }

                // Validate that user is not delegating to themselves
                if (updateDto.FromUserId.HasValue && updateDto.ToUserId.HasValue && 
                    updateDto.FromUserId == updateDto.ToUserId)
                {
                    return BadRequest(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = "Cannot delegate to yourself",
                        Errors = new List<string> { "Invalid user delegation" }
                    });
                }

                var result = await _accessDelegationService.UpdateDelegationAsync(updateDto);

                if (!result)
                {
                    return NotFound(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = "Access delegation not found or could not be updated",
                        Errors = new List<string> { $"Delegation with ID {id} not found" }
                    });
                }

                return Ok(new ApiResponseDto<bool>
                {
                    Success = true,
                    Message = "Access delegation updated successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating access delegation with ID: {Id}", id);
                return StatusCode(500, new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "An error occurred while updating the delegation",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Deletes an access delegation
        /// </summary>
        /// <param name="id">Delegation ID</param>
        /// <returns>Delete result</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponseDto<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponseDto<bool>>> DeleteDelegation(int id)
        {
            try
            {
                var result = await _accessDelegationService.DeleteDelegationAsync(id);

                if (!result)
                {
                    return NotFound(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = "Access delegation not found",
                        Errors = new List<string> { $"Delegation with ID {id} not found" }
                    });
                }

                return Ok(new ApiResponseDto<bool>
                {
                    Success = true,
                    Message = "Access delegation deleted successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting access delegation with ID: {Id}", id);
                return StatusCode(500, new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "An error occurred while deleting the delegation",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        #endregion

        #region Pagination and Listing

        /// <summary>
        /// Gets paginated list of all access delegations
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10)</param>
        /// <returns>Paginated delegation list</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponseDto<AccessDelegationPagedDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponseDto<AccessDelegationPagedDto>>> GetDelegations(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1) pageSize = 10;
                if (pageSize > 100) pageSize = 100; // Limit page size

                var delegations = await _accessDelegationService.GetDelegationsPagedAsync(pageNumber, pageSize);

                return Ok(new ApiResponseDto<AccessDelegationPagedDto>
                {
                    Success = true,
                    Message = "Access delegations retrieved successfully",
                    Data = delegations
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving paginated access delegations");
                return StatusCode(500, new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving delegations",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Gets current active delegations with pagination
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10)</param>
        /// <returns>Paginated active delegation list</returns>
        [HttpGet("active")]
        [ProducesResponseType(typeof(ApiResponseDto<AccessDelegationPagedDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponseDto<AccessDelegationPagedDto>>> GetCurrentActiveDelegations(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1) pageSize = 10;
                if (pageSize > 100) pageSize = 100;

                var delegations = await _accessDelegationService.GetCurrentActiveDelegationsAsync(pageNumber, pageSize);

                return Ok(new ApiResponseDto<AccessDelegationPagedDto>
                {
                    Success = true,
                    Message = "Current active delegations retrieved successfully",
                    Data = delegations
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving current active delegations");
                return StatusCode(500, new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving active delegations",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        #endregion

        #region User-specific Operations

        /// <summary>
        /// Gets active delegations for a specific user as delegator
        /// </summary>
        /// <param name="userId">User ID who delegated</param>
        /// <returns>List of active delegations</returns>
        [HttpGet("from-user/{userId}")]
        [ProducesResponseType(typeof(ApiResponseDto<List<AccessDelegationDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponseDto<List<AccessDelegationDto>>>> GetActiveDelegationsByFromUser(int userId)
        {
            try
            {
                var delegations = await _accessDelegationService.GetActiveDelegationsByFromUserAsync(userId);

                return Ok(new ApiResponseDto<List<AccessDelegationDto>>
                {
                    Success = true,
                    Message = "Delegations retrieved successfully",
                    Data = delegations
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving delegations for from user: {UserId}", userId);
                return StatusCode(500, new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving delegations",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Gets active delegations for a specific user as delegate
        /// </summary>
        /// <param name="userId">User ID who received delegation</param>
        /// <returns>List of active delegations</returns>
        [HttpGet("to-user/{userId}")]
        [ProducesResponseType(typeof(ApiResponseDto<List<AccessDelegationDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponseDto<List<AccessDelegationDto>>>> GetActiveDelegationsByToUser(int userId)
        {
            try
            {
                var delegations = await _accessDelegationService.GetActiveDelegationsByToUserAsync(userId);

                return Ok(new ApiResponseDto<List<AccessDelegationDto>>
                {
                    Success = true,
                    Message = "Delegations retrieved successfully",
                    Data = delegations
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving delegations for to user: {UserId}", userId);
                return StatusCode(500, new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving delegations",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Gets paginated delegations by from user
        /// </summary>
        /// <param name="userId">User ID who delegated</param>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10)</param>
        /// <param name="includeInactive">Include inactive delegations (default: false)</param>
        /// <returns>Paginated delegation list</returns>
        [HttpGet("from-user/{userId}/paged")]
        [ProducesResponseType(typeof(ApiResponseDto<AccessDelegationPagedDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponseDto<AccessDelegationPagedDto>>> GetDelegationsByFromUserPaged(
            int userId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] bool includeInactive = false)
        {
            try
            {
                var queryDto = new UserDelegationQueryDto
                {
                    UserId = userId,
                    PageNumber = pageNumber < 1 ? 1 : pageNumber,
                    PageSize = pageSize < 1 ? 10 : (pageSize > 100 ? 100 : pageSize),
                    IncludeInactive = includeInactive
                };

                var delegations = await _accessDelegationService.GetDelegationsByFromUserPagedAsync(queryDto);

                return Ok(new ApiResponseDto<AccessDelegationPagedDto>
                {
                    Success = true,
                    Message = "Delegations retrieved successfully",
                    Data = delegations
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving paged delegations for from user: {UserId}", userId);
                return StatusCode(500, new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving delegations",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Gets paginated delegations by to user
        /// </summary>
        /// <param name="userId">User ID who received delegation</param>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10)</param>
        /// <param name="includeInactive">Include inactive delegations (default: false)</param>
        /// <returns>Paginated delegation list</returns>
        [HttpGet("to-user/{userId}/paged")]
        [ProducesResponseType(typeof(ApiResponseDto<AccessDelegationPagedDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponseDto<AccessDelegationPagedDto>>> GetDelegationsByToUserPaged(
            int userId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] bool includeInactive = false)
        {
            try
            {
                var queryDto = new UserDelegationQueryDto
                {
                    UserId = userId,
                    PageNumber = pageNumber < 1 ? 1 : pageNumber,
                    PageSize = pageSize < 1 ? 10 : (pageSize > 100 ? 100 : pageSize),
                    IncludeInactive = includeInactive
                };

                var delegations = await _accessDelegationService.GetDelegationsByToUserPagedAsync(queryDto);

                return Ok(new ApiResponseDto<AccessDelegationPagedDto>
                {
                    Success = true,
                    Message = "Delegations retrieved successfully",
                    Data = delegations
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving paged delegations for to user: {UserId}", userId);
                return StatusCode(500, new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving delegations",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        #endregion

        #region Advanced Operations

        /// <summary>
        /// Searches delegations with advanced filtering
        /// </summary>
        /// <param name="searchDto">Search parameters</param>
        /// <returns>Paginated search results</returns>
        [HttpPost("search")]
        [ProducesResponseType(typeof(ApiResponseDto<AccessDelegationPagedDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponseDto<AccessDelegationPagedDto>>> SearchDelegations([FromBody] DelegationSearchDto searchDto)
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

                // Validate page parameters
                if (searchDto.PageNumber < 1) searchDto.PageNumber = 1;
                if (searchDto.PageSize < 1) searchDto.PageSize = 10;
                if (searchDto.PageSize > 100) searchDto.PageSize = 100;

                var delegations = await _accessDelegationService.SearchDelegationsAsync(searchDto);

                return Ok(new ApiResponseDto<AccessDelegationPagedDto>
                {
                    Success = true,
                    Message = "Search completed successfully",
                    Data = delegations
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching delegations");
                return StatusCode(500, new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "An error occurred while searching delegations",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Gets delegations by date range with pagination
        /// </summary>
        /// <param name="dateRangeDto">Date range query parameters</param>
        /// <returns>Paginated delegation list</returns>
        [HttpPost("date-range")]
        [ProducesResponseType(typeof(ApiResponseDto<AccessDelegationPagedDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponseDto<AccessDelegationPagedDto>>> GetDelegationsByDateRange([FromBody] DateRangeQueryDto dateRangeDto)
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

                // Validate date range
                if (dateRangeDto.StartDate.HasValue && dateRangeDto.EndDate.HasValue && 
                    dateRangeDto.StartDate.Value > dateRangeDto.EndDate.Value)
                {
                    return BadRequest(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = "Start date cannot be after end date",
                        Errors = new List<string> { "Invalid date range" }
                    });
                }

                // Validate page parameters
                if (dateRangeDto.PageNumber < 1) dateRangeDto.PageNumber = 1;
                if (dateRangeDto.PageSize < 1) dateRangeDto.PageSize = 10;
                if (dateRangeDto.PageSize > 100) dateRangeDto.PageSize = 100;

                var delegations = await _accessDelegationService.GetDelegationsByDateRangePagedAsync(dateRangeDto);

                return Ok(new ApiResponseDto<AccessDelegationPagedDto>
                {
                    Success = true,
                    Message = "Delegations retrieved successfully",
                    Data = delegations
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving delegations by date range");
                return StatusCode(500, new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving delegations",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Gets delegation history for a specific user (both as delegator and delegate)
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10)</param>
        /// <returns>Paginated delegation history</returns>
        [HttpGet("user/{userId}/history")]
        [ProducesResponseType(typeof(ApiResponseDto<DelegationHistoryPagedDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponseDto<DelegationHistoryPagedDto>>> GetDelegationHistoryByUser(
            int userId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1) pageSize = 10;
                if (pageSize > 100) pageSize = 100;

                var history = await _accessDelegationService.GetDelegationHistoryByUserAsync(userId, pageNumber, pageSize);

                return Ok(new ApiResponseDto<DelegationHistoryPagedDto>
                {
                    Success = true,
                    Message = "Delegation history retrieved successfully",
                    Data = history
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving delegation history for user: {UserId}", userId);
                return StatusCode(500, new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving delegation history",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Extends the end date of a delegation
        /// </summary>
        /// <param name="extendDto">Extension details</param>
        /// <returns>Extension result</returns>
        [HttpPost("extend")]
        [ProducesResponseType(typeof(ApiResponseDto<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponseDto<bool>>> ExtendDelegation([FromBody] ExtendDelegationDto extendDto)
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

                // Validate that new end date is in the future
                if (extendDto.NewEndDate <= DateTime.UtcNow)
                {
                    return BadRequest(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = "New end date must be in the future",
                        Errors = new List<string> { "Invalid end date" }
                    });
                }

                var result = await _accessDelegationService.ExtendDelegationAsync(extendDto);

                return Ok(new ApiResponseDto<bool>
                {
                    Success = true,
                    Message = "Delegation extended successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extending delegation: {DelegationId}", extendDto.DelegationId);
                return StatusCode(500, new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "An error occurred while extending the delegation",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Checks if a user has active delegation to another user
        /// </summary>
        /// <param name="fromUserId">Delegator user ID</param>
        /// <param name="toUserId">Delegate user ID</param>
        /// <returns>True if active delegation exists</returns>
        [HttpGet("check-active/{fromUserId}/{toUserId}")]
        [ProducesResponseType(typeof(ApiResponseDto<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponseDto<bool>>> CheckUserHasActiveDelegation(int fromUserId, int toUserId)
        {
            try
            {
                var hasActiveDelegation = await _accessDelegationService.CheckUserHasActiveDelegationAsync(fromUserId, toUserId);

                return Ok(new ApiResponseDto<bool>
                {
                    Success = true,
                    Message = "Delegation status checked successfully",
                    Data = hasActiveDelegation
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking active delegation between users: {FromUserId} -> {ToUserId}", fromUserId, toUserId);
                return StatusCode(500, new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "An error occurred while checking delegation status",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        #endregion

        #region Statistics and Analytics

        /// <summary>
        /// Gets delegation statistics
        /// </summary>
        /// <param name="statsDto">Statistics query parameters</param>
        /// <returns>Delegation statistics</returns>
        [HttpPost("statistics")]
        [ProducesResponseType(typeof(ApiResponseDto<DelegationStatisticsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponseDto<DelegationStatisticsDto>>> GetDelegationStatistics([FromBody] DelegationStatsQueryDto statsDto)
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

                // Validate date range
                if (statsDto.StartDate.HasValue && statsDto.EndDate.HasValue && 
                    statsDto.StartDate.Value > statsDto.EndDate.Value)
                {
                    return BadRequest(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = "Start date cannot be after end date",
                        Errors = new List<string> { "Invalid date range" }
                    });
                }

                var statistics = await _accessDelegationService.GetDelegationStatisticsAsync(statsDto);

                return Ok(new ApiResponseDto<DelegationStatisticsDto>
                {
                    Success = true,
                    Message = "Statistics retrieved successfully",
                    Data = statistics
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving delegation statistics");
                return StatusCode(500, new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving statistics",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Gets most active delegators
        /// </summary>
        /// <param name="queryDto">Query parameters</param>
        /// <returns>List of most active delegators</returns>
        [HttpPost("most-active-delegators")]
        [ProducesResponseType(typeof(ApiResponseDto<List<UserDelegationActivityDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponseDto<List<UserDelegationActivityDto>>>> GetMostActiveDelegators([FromBody] MostActiveUsersQueryDto queryDto)
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

                // Validate limit
                if (queryDto.Limit < 1) queryDto.Limit = 10;
                if (queryDto.Limit > 100) queryDto.Limit = 100;

                // Validate date range
                if (queryDto.StartDate.HasValue && queryDto.EndDate.HasValue && 
                    queryDto.StartDate.Value > queryDto.EndDate.Value)
                {
                    return BadRequest(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = "Start date cannot be after end date",
                        Errors = new List<string> { "Invalid date range" }
                    });
                }

                var delegators = await _accessDelegationService.GetMostActiveDelegatorsAsync(queryDto);

                return Ok(new ApiResponseDto<List<UserDelegationActivityDto>>
                {
                    Success = true,
                    Message = "Most active delegators retrieved successfully",
                    Data = delegators
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving most active delegators");
                return StatusCode(500, new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving most active delegators",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Gets most popular delegates
        /// </summary>
        /// <param name="queryDto">Query parameters</param>
        /// <returns>List of most popular delegates</returns>
        [HttpPost("most-popular-delegates")]
        [ProducesResponseType(typeof(ApiResponseDto<List<UserDelegationActivityDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponseDto<List<UserDelegationActivityDto>>>> GetMostPopularDelegates([FromBody] MostActiveUsersQueryDto queryDto)
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

                // Validate limit
                if (queryDto.Limit < 1) queryDto.Limit = 10;
                if (queryDto.Limit > 100) queryDto.Limit = 100;

                // Validate date range
                if (queryDto.StartDate.HasValue && queryDto.EndDate.HasValue && 
                    queryDto.StartDate.Value > queryDto.EndDate.Value)
                {
                    return BadRequest(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = "Start date cannot be after end date",
                        Errors = new List<string> { "Invalid date range" }
                    });
                }

                var delegates = await _accessDelegationService.GetMostPopularDelegatesAsync(queryDto);

                return Ok(new ApiResponseDto<List<UserDelegationActivityDto>>
                {
                    Success = true,
                    Message = "Most popular delegates retrieved successfully",
                    Data = delegates
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving most popular delegates");
                return StatusCode(500, new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving most popular delegates",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        #endregion

        #region Maintenance Operations

        /// <summary>
        /// Deactivates expired delegations
        /// </summary>
        /// <returns>Number of delegations deactivated</returns>
        [HttpPost("deactivate-expired")]
        [ProducesResponseType(typeof(ApiResponseDto<int>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponseDto<int>>> DeactivateExpiredDelegations()
        {
            try
            {
                var count = await _accessDelegationService.DeactivateExpiredDelegationsAsync();

                return Ok(new ApiResponseDto<int>
                {
                    Success = true,
                    Message = $"Successfully deactivated {count} expired delegations",
                    Data = count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating expired delegations");
                return StatusCode(500, new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "An error occurred while deactivating expired delegations",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        #endregion
    }
}
