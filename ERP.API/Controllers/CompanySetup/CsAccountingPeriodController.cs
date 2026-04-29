using Microsoft.AspNetCore.Mvc;
using ERP.API.Models.DTOs.CompanySetup;
using ERP.API.Services.CompanySetup;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using System;
using Microsoft.AspNetCore.Http;

namespace ERP.API.Controllers.CompanySetup
{
    [ApiController]
    [Route("api/[controller]")]
    // [SwaggerTag("Manage company accounting periods")]
    public class CsAccountingPeriodController : ControllerBase
    {
        private readonly ICsAccountingPeriodService _service;
        private readonly ILogger<CsAccountingPeriodController> _logger;

        public CsAccountingPeriodController(ICsAccountingPeriodService service, ILogger<CsAccountingPeriodController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new accounting period
        /// </summary>
        /// <param name="createDto">The accounting period details</param>
        /// <returns>The created accounting period</returns>
        [HttpPost]
        [SwaggerOperation(Summary = "Creates a new accounting period")]
        [ProducesResponseType(typeof(CsAccountingPeriodResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)] // Added for specific error messages
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CsAccountingPeriodDto createDto)
        {
            try
            {
                var result = await _service.CreateAccountingPeriodAsync(createDto);
                return CreatedAtAction(nameof(GetById), new { periodId = result.PeriodId }, new {
                    message = "Accounting period created successfully.",
                    data = result
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Validation error creating accounting period");
                return BadRequest(new {
                    message = "Validation Error",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating accounting period");
                return StatusCode(StatusCodes.Status500InternalServerError, new {
                    message = "An error occurred while creating the accounting period.",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        /// <summary>
        /// Updates an existing accounting period
        /// </summary>
        /// <param name="periodId">The ID of the accounting period to update</param>
        /// <param name="updateDto">The updated accounting period details</param>
        /// <returns>The updated accounting period</returns>
        [HttpPut("{periodId}")]
        [SwaggerOperation(Summary = "Updates an existing accounting period")]
        [ProducesResponseType(typeof(CsAccountingPeriodResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)] // Added for specific error messages
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int periodId, [FromBody] CsAccountingPeriodDto updateDto)
        {
            try
            {
                var result = await _service.UpdateAccountingPeriodAsync(periodId, updateDto);
                if (result == null)
                    return NotFound(new {
                        message = "Accounting period not found.",
                        data = (object?)null
                    });

                return Ok(new {
                    message = "Accounting period updated successfully.",
                    data = result
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Validation error updating accounting period");
                return BadRequest(new {
                    message = "Validation Error",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating accounting period");
                return StatusCode(StatusCodes.Status500InternalServerError, new {
                    message = "An error occurred while updating the accounting period.",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        /// <summary>
        /// Deletes an accounting period
        /// </summary>
        /// <param name="periodId">The ID of the accounting period to delete</param>
        /// <returns>No content</returns>
        [HttpDelete("{periodId}")]
        [SwaggerOperation(Summary = "Deletes an accounting period")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int periodId)
        {
            try
            {
                var result = await _service.DeleteAccountingPeriodAsync(periodId);
                if (!result)
                    return NotFound(new {
                        message = "Accounting period not found.",
                        data = (object?)null
                    });

                return Ok(new {
                    message = "Accounting period deleted successfully.",
                    data = (object?)null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting accounting period");
                return StatusCode(StatusCodes.Status500InternalServerError, new {
                    message = "An error occurred while deleting the accounting period.",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        /// <summary>
        /// Gets an accounting period by ID
        /// </summary>
        /// <param name="periodId">The ID of the accounting period</param>
        /// <returns>The accounting period details</returns>
        [HttpGet("{periodId}")]
        [SwaggerOperation(Summary = "Gets an accounting period by ID")]
        [ProducesResponseType(typeof(CsAccountingPeriodResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int periodId)
        {
            try
            {
                var result = await _service.GetAccountingPeriodByIdAsync(periodId);
                if (result == null)
                    return NotFound(new {
                        message = "Accounting period not found.",
                        data = (object?)null
                    });

                return Ok(new {
                    message = "Accounting period retrieved successfully.",
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving accounting period");
                return StatusCode(StatusCodes.Status500InternalServerError, new {
                    message = "An error occurred while retrieving the accounting period.",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        /// <summary>
        /// Gets all accounting periods for a company
        /// </summary>
        /// <param name="companyId">The company ID</param>
        /// <param name="pageNumber">Page number (optional)</param>
        /// <param name="pageSize">Page size (optional)</param>
        /// <returns>List of accounting periods</returns>
        [HttpGet("company/{companyId}")]
        [SwaggerOperation(Summary = "Gets accounting periods for a company")]
        [ProducesResponseType(typeof(CsAccountingPeriodPagedResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByCompany(int companyId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _service.GetAccountingPeriodsByCompanyAsync(companyId, pageNumber, pageSize);
                return Ok(new {
                    message = "Accounting periods retrieved successfully.",
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving accounting periods by company");
                return StatusCode(StatusCodes.Status500InternalServerError, new {
                    message = "An error occurred while retrieving accounting periods by company.",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        /// <summary>
        /// Searches accounting periods
        /// </summary>
        /// <param name="companyId">The company ID</param>
        /// <param name="searchRequest">Search parameters</param>
        /// <returns>List of matching accounting periods</returns>
        [HttpPost("company/{companyId}/search")]
        [SwaggerOperation(Summary = "Searches accounting periods")]
        [ProducesResponseType(typeof(CsAccountingPeriodPagedResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> Search(int companyId, [FromBody] CsAccountingPeriodSearchRequest searchRequest)
        {
            try
            {
                var result = await _service.SearchAccountingPeriodsAsync(companyId, searchRequest);
                return Ok(new {
                    message = "Accounting periods search completed successfully.",
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching accounting periods");
                return StatusCode(StatusCodes.Status500InternalServerError, new {
                    message = "An error occurred while searching accounting periods.",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        /// <summary>
        /// Gets all accounting periods
        /// </summary>
        /// <returns>List of all accounting periods</returns>
        [HttpGet("all")]
        [SwaggerOperation(Summary = "Gets all accounting periods")]
        [ProducesResponseType(typeof(List<CsAccountingPeriodResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _service.GetAllAccountingPeriodsAsync();
                return Ok(new {
                    message = "All accounting periods retrieved successfully.",
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all accounting periods");
                return StatusCode(StatusCodes.Status500InternalServerError, new {
                    message = "An error occurred while retrieving all accounting periods.",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }
    }
}
