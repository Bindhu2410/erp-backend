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
    public class CsCompanyController : ControllerBase
    {
        private readonly ICsCompanyService _companyService;
        private readonly ILogger<CsCompanyController> _logger;

        public CsCompanyController(ICsCompanyService companyService, ILogger<CsCompanyController> logger)
        {
            _companyService = companyService;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new company
        /// </summary>
        /// <param name="createDto">The company information</param>
        /// <returns>The ID of the created company</returns>
        /// <response code="201">Returns the ID of the created company</response>
        /// <response code="400">If the request data is invalid</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateCompany([FromBody] CreateCsCompanyDto createDto)
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
                var companyId = await _companyService.CreateCompanyAsync(createDto);
                return CreatedAtAction(
                    nameof(GetCompanyById),
                    new { id = companyId },
                    new {
                        message = "Company created successfully",
                        data = new { CompanyId = companyId }
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating company: {LegalCompanyName}", createDto.LegalCompanyName);
                return StatusCode(500, new
                {
                    message = "An error occurred while creating the company",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        /// <summary>
        /// Updates an existing company
        /// </summary>
        /// <param name="updateDto">The updated company information</param>
        /// <returns>Success status</returns>
        /// <response code="200">If the company was updated successfully</response>
        /// <response code="400">If the request data is invalid</response>
        /// <response code="404">If the company was not found</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateCompany([FromBody] UpdateCsCompanyDto updateDto)
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
                var result = await _companyService.UpdateCompanyAsync(updateDto);
                if (!result)
                {
                    return NotFound(new
                    {
                        message = $"Company with ID {updateDto.CompanyId} not found",
                        data = (object?)null
                    });
                }
                return Ok(new
                {
                    message = "Company updated successfully",
                    data = (object?)null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating company: {CompanyId}", updateDto.CompanyId);
                return StatusCode(500, new
                {
                    message = "An error occurred while updating the company",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        /// <summary>
        /// Deletes a company
        /// </summary>
        /// <param name="id">The company ID</param>
        /// <param name="forceDelete">Whether to force delete (ignores child companies)</param>
        /// <returns>Success status</returns>
        /// <response code="200">If the company was deleted successfully</response>
        /// <response code="400">If the company has child companies and force delete is false</response>
        /// <response code="404">If the company was not found</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteCompany(int id, [FromQuery] bool forceDelete = false)
        {
            try
            {
                var result = await _companyService.DeleteCompanyAsync(id, forceDelete);
                if (!result)
                {
                    return NotFound(new
                    {
                        message = $"Company with ID {id} not found",
                        data = (object?)null
                    });
                }
                return Ok(new
                {
                    message = "Company deleted successfully",
                    data = (object?)null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting company: {CompanyId}", id);
                if (ex.Message.Contains("child companies"))
                {
                    return BadRequest(new
                    {
                        message = ex.Message,
                        error = ex.Message,
                        stackTrace = ex.StackTrace
                    });
                }
                return StatusCode(500, new
                {
                    message = "An error occurred while deleting the company",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        /// <summary>
        /// Gets a company by ID
        /// </summary>
        /// <param name="id">The company ID</param>
        /// <returns>The company details</returns>
        /// <response code="200">Returns the company details</response>
        /// <response code="404">If the company was not found</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCompanyById(int id)
        {
            try
            {
                var company = await _companyService.GetCompanyByIdAsync(id);
                if (company == null)
                {
                    return NotFound(new
                    {
                        message = $"Company with ID {id} not found",
                        data = (object?)null
                    });
                }
                return Ok(new
                {
                    message = "Company retrieved successfully",
                    data = company
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting company by ID: {CompanyId}", id);
                return StatusCode(500, new
                {
                    message = "An error occurred while retrieving the company",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        /// <summary>
        /// Gets all companies
        /// </summary>
        /// <returns>List of all companies</returns>
        /// <response code="200">Returns the list of companies</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllCompanies()
        {
            try
            {
                var companies = await _companyService.GetAllCompaniesAsync();
                return Ok(new
                {
                    message = "Companies retrieved successfully",
                    data = companies,
                    count = companies.Count()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all companies");
                return StatusCode(500, new
                {
                    message = "An error occurred while retrieving companies",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        /// <summary>
        /// Searches companies based on criteria
        /// </summary>
        /// <param name="searchDto">Search criteria</param>
        /// <returns>List of matching companies</returns>
        /// <response code="200">Returns the list of matching companies</response>
        /// <response code="400">If no search criteria is provided</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpPost("search")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SearchCompanies([FromBody] CsCompanySearchDto searchDto)
        {
            try
            {
                // Validate that at least one search parameter is provided
                if (string.IsNullOrEmpty(searchDto.SearchTerm) && 
                    searchDto.ParentCompanyId == null && 
                    string.IsNullOrEmpty(searchDto.LegalEntityType))
                {
                    return BadRequest(new
                    {
                        message = "At least one search parameter must be provided"
                    });
                }
                var companies = await _companyService.SearchCompaniesAsync(searchDto);
                return Ok(new
                {
                    message = "Companies search completed successfully",
                    data = companies,
                    count = companies.Count()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching companies");
                return StatusCode(500, new
                {
                    message = "An error occurred while searching companies",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        /// <summary>
        /// Gets company hierarchy
        /// </summary>
        /// <returns>Hierarchical list of companies</returns>
        /// <response code="200">Returns the company hierarchy</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpGet("hierarchy")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCompanyHierarchy()
        {
            try
            {
                var hierarchy = await _companyService.GetCompanyHierarchyAsync();
                return Ok(new
                {
                    message = "Company hierarchy retrieved successfully",
                    data = hierarchy,
                    count = hierarchy.Count()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting company hierarchy");
                return StatusCode(500, new
                {
                    message = "An error occurred while retrieving company hierarchy",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }
    }
}
