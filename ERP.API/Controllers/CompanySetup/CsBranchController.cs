using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using ERP.API.Models.DTOs.CompanySetup;
using ERP.API.Services.CompanySetup;
using Microsoft.Extensions.Logging;

namespace ERP.API.Controllers.CompanySetup
{
    [ApiController]
    [Route("api/[controller]")]
    public class CsBranchController : ControllerBase
    {
        private readonly ICsBranchService _branchService;
        private readonly ILogger<CsBranchController> _logger;

        public CsBranchController(ICsBranchService branchService, ILogger<CsBranchController> logger)
        {
            _branchService = branchService;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new branch
        /// </summary>
        /// <param name="createDto">The branch information</param>
        /// <returns>The creation result with branch ID if successful</returns>
        /// <response code="201">Returns the branch creation result</response>
        /// <response code="400">If the request data is invalid or branch code already exists</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateBranch([FromBody] CreateCsBranchDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        message = "Invalid model state",
                        errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                    });
                }
                var result = await _branchService.CreateBranchAsync(createDto);
                if (result.Success)
                {
                    return CreatedAtAction(
                        nameof(GetBranchesByCompany),
                        new { companyId = createDto.CompanyId },
                        new
                        {
                            message = result.OutMessage,
                            data = new 
                            { 
                                BranchId = result.OutBranchId,
                                BranchCode = result.OutBranchCode
                            }
                        });
                }
                else
                {
                    return BadRequest(new
                    {
                        message = result.OutMessage
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating branch: {BranchName}", createDto.BranchName);
                return StatusCode(500, new
                {
                    message = "An error occurred while creating the branch",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        /// <summary>
        /// Updates an existing branch
        /// </summary>
        /// <param name="updateDto">The updated branch information</param>
        /// <returns>Success status</returns>
        /// <response code="200">If the branch was updated successfully</response>
        /// <response code="400">If the request data is invalid</response>
        /// <response code="404">If the branch was not found</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateBranch([FromBody] UpdateCsBranchDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        message = "Invalid model state",
                        errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                    });
                }
                var result = await _branchService.UpdateBranchAsync(updateDto);
                if (result.Success)
                {
                    return Ok(new
                    {
                        message = result.Message,
                        data = (object?)null
                    });
                }
                else
                {
                    if (result.Message.Contains("not found"))
                    {
                        return NotFound(new
                        {
                            message = result.Message,
                            data = (object?)null
                        });
                    }
                    else
                    {
                        return BadRequest(new
                        {
                            message = result.Message
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating branch: {BranchId}", updateDto.BranchId);
                return StatusCode(500, new
                {
                    message = "An error occurred while updating the branch",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        /// <summary>
        /// Gets branches by company ID
        /// </summary>
        /// <param name="companyId">The company ID</param>
        /// <param name="includeInactive">Whether to include inactive branches</param>
        /// <returns>List of branches for the company</returns>
        /// <response code="200">Returns the list of branches</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpGet("company/{companyId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBranchesByCompany(int companyId, [FromQuery] bool includeInactive = false)
        {
            try
            {
                var branches = await _branchService.GetBranchesByCompanyAsync(companyId, includeInactive);
                return Ok(new
                {
                    message = "Branches retrieved successfully",
                    data = branches,
                    count = branches.Count()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting branches by company: {CompanyId}", companyId);
                return StatusCode(500, new
                {
                    message = "An error occurred while retrieving branches",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        /// <summary>
        /// Gets branches dropdown for a company
        /// </summary>
        /// <param name="companyId">The company ID</param>
        /// <param name="activeOnly">Whether to include only active branches</param>
        /// <returns>List of branches for dropdown</returns>
        /// <response code="200">Returns the list of branches for dropdown</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpGet("dropdown/company/{companyId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBranchesDropdown(int companyId, [FromQuery] bool activeOnly = true)
        {
            try
            {
                var branches = await _branchService.GetBranchesDropdownAsync(companyId, activeOnly);
                return Ok(new
                {
                    message = "Branches dropdown retrieved successfully",
                    data = branches,
                    count = branches.Count()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting branches dropdown: {CompanyId}", companyId);
                return StatusCode(500, new
                {
                    message = "An error occurred while retrieving branches dropdown",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        /// <summary>
        /// Gets all branches with pagination
        /// </summary>
        /// <param name="request">Pagination and filter parameters</param>
        /// <returns>Paged list of branches</returns>
        /// <response code="200">Returns the paged list of branches</response>
        /// <response code="400">If the request parameters are invalid</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpPost("paged")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllBranches([FromBody] CsBranchPagedRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        message = "Invalid model state",
                        errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                    });
                }
                if (request.PageNumber < 1 || request.PageSize < 1 || request.PageSize > 100)
                {
                    return BadRequest(new
                    {
                        message = "Page number must be >= 1 and page size must be between 1 and 100"
                    });
                }
                var result = await _branchService.GetAllBranchesAsync(request);
                return Ok(new
                {
                    message = "Branches retrieved successfully",
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all branches");
                return StatusCode(500, new
                {
                    message = "An error occurred while retrieving branches",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        /// <summary>
        /// Validates if a branch belongs to a specific company
        /// </summary>
        /// <param name="branchId">The branch ID</param>
        /// <param name="companyId">The company ID</param>
        /// <returns>Validation result</returns>
        /// <response code="200">Returns the validation result</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpGet("validate/{branchId}/company/{companyId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ValidateBranchCompany(int branchId, int companyId)
        {
            try
            {
                var isValid = await _branchService.ValidateBranchCompanyAsync(branchId, companyId);
                return Ok(new
                {
                    message = isValid ? "Branch belongs to the company" : "Branch does not belong to the company",
                    data = new { IsValid = isValid }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating branch company: BranchId={BranchId}, CompanyId={CompanyId}", branchId, companyId);
                return StatusCode(500, new
                {
                    message = "An error occurred while validating branch company",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        /// <summary>
        /// Deletes a branch
        /// </summary>
        /// <param name="branchId">The branch ID to delete</param>
        /// <param name="companyId">The company ID that owns the branch</param>
        /// <returns>Deletion result</returns>
        /// <response code="200">If the branch was deleted successfully</response>
        /// <response code="400">If the branch does not belong to the company</response>
        /// <response code="404">If the branch was not found</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpDelete("{branchId}/company/{companyId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteBranch(int branchId, int companyId)
        {
            try
            {
                var result = await _branchService.DeleteBranchAsync(branchId, companyId);
                if (result.Success)
                {
                    return Ok(new
                    {
                        message = result.Message,
                        data = (object?)null
                    });
                }
                else
                {
                    if (result.Message.Contains("not found"))
                    {
                        return NotFound(new
                        {
                            message = result.Message,
                            data = (object?)null
                        });
                    }
                    else
                    {
                        return BadRequest(new
                        {
                            message = result.Message
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting branch: BranchId={BranchId}, CompanyId={CompanyId}", branchId, companyId);
                return StatusCode(500, new
                {
                    message = "An error occurred while deleting the branch",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }
    }
}
