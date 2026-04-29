using Microsoft.AspNetCore.Mvc;
using ERP.API.Models.CompanySetup;
using ERP.API.Services.CompanySetup;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Swashbuckle.AspNetCore.Annotations;
using Npgsql;
using Microsoft.Extensions.Logging;

namespace ERP.API.Controllers.CompanySetup
{
    [ApiController]
    [Route("api/[controller]")]
    public class CsCostCentreController : ControllerBase
    {
        private readonly ICsCostCentreService _costCentreService;
        private readonly ILogger<CsCostCentreController> _logger;

        public CsCostCentreController(ICsCostCentreService costCentreService, ILogger<CsCostCentreController> logger)
        {
            _costCentreService = costCentreService;
            _logger = logger;
        }

        [HttpPost("company/{companyId}")]
        [SwaggerOperation(Summary = "Creates a new cost centre")]
        [ProducesResponseType(typeof(CsCostCentre), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateCostCentre(int companyId, [FromBody] CsCostCentreDto createDto)
        {
            try
            {
                var result = await _costCentreService.CreateCostCentreAsync(companyId, createDto);
                return CreatedAtAction(
                    nameof(GetCostCentreById),
                    new { costCentreId = result.CostCentreId },
                    new { message = "Cost centre created successfully", data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating cost centre");
                return StatusCode(500, new { message = "An error occurred while creating the cost centre", error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpPut("{costCentreId}")]
        [SwaggerOperation(Summary = "Updates an existing cost centre")]
        [ProducesResponseType(typeof(CsCostCentre), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateCostCentre(int costCentreId, [FromBody] CsCostCentreDto updateDto)
        {
            try
            {
                var result = await _costCentreService.UpdateCostCentreAsync(costCentreId, updateDto);
                if (result == null)
                    return NotFound(new { message = $"Cost centre with id {costCentreId} not found for update.", data = (object?)null });
                return Ok(new { message = "Cost centre updated successfully", data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cost centre");
                return StatusCode(500, new { message = "An error occurred while updating the cost centre", error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        // [HttpDelete("{costCentreId}")]
        // [SwaggerOperation(Summary = "Deletes a cost centre")]
        // [ProducesResponseType(StatusCodes.Status204NoContent)]
        // [ProducesResponseType(StatusCodes.Status404NotFound)]
        // public async Task<IActionResult> DeleteCostCentre(int costCentreId)
        // {
        //     var result = await _costCentreService.DeleteCostCentreAsync(costCentreId);
        //     if (!result)
        //     {
        //         return NotFound();
        //     }

        //     return NoContent();
        // }

        [HttpDelete("{costCentreId}")]
        public async Task<IActionResult> DeleteCostCentre(int costCentreId)
        {
            try
            {
                var success = await _costCentreService.DeleteCostCentreAsync(costCentreId);
                if (!success)
                    return NotFound(new { message = $"Cost centre with id {costCentreId} not found for deletion or cannot be deleted.", data = (object?)null });
                return Ok(new { message = "Cost centre deleted successfully", data = (object?)null });
            }
            catch (PostgresException ex) when (ex.SqlState == "P0001")
            {
                return BadRequest(new { message = ex.MessageText });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting cost centre");
                return StatusCode(500, new { message = "An error occurred while deleting the cost centre", error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpGet("{costCentreId}")]
        [SwaggerOperation(Summary = "Gets a cost centre by ID")]
        [ProducesResponseType(typeof(CsCostCentre), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCostCentreById(int costCentreId)
        {
            try
            {
                var result = await _costCentreService.GetCostCentreByIdAsync(costCentreId);
                if (result == null)
                    return NotFound(new { message = $"Cost centre with id {costCentreId} not found.", data = (object?)null });
                return Ok(new { message = "Cost centre retrieved successfully", data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cost centre by id");
                return StatusCode(500, new { message = "An error occurred while retrieving cost centre by id", error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpGet("company/{companyId}")]
        [SwaggerOperation(Summary = "Gets all cost centres for a company with pagination")]
        [ProducesResponseType(typeof(CsCostCentrePagedResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCostCentresByCompany(
            int companyId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _costCentreService.GetCostCentresByCompanyAsync(companyId, pageNumber, pageSize);
                return Ok(new { message = "Cost centres retrieved successfully by company", data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cost centres by company");
                return StatusCode(500, new { message = "An error occurred while retrieving cost centres by company", error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpGet("company/{companyId}/search")]
        [SwaggerOperation(Summary = "Searches cost centres with filters")]
        [ProducesResponseType(typeof(CsCostCentrePagedResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> SearchCostCentres(
            int companyId,
            [FromQuery] CsCostCentreSearchRequest searchRequest)
        {
            try
            {
                var result = await _costCentreService.SearchCostCentresAsync(companyId, searchRequest);
                return Ok(new { message = "Cost centres search completed successfully", data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching cost centres");
                return StatusCode(500, new { message = "An error occurred while searching cost centres", error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpGet("company/{companyId}/hierarchy")]
        [SwaggerOperation(Summary = "Gets the hierarchical structure of cost centres")]
        [ProducesResponseType(typeof(List<CsCostCentreHierarchyItem>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCostCentreHierarchy(int companyId)
        {
            try
            {
                var result = await _costCentreService.GetCostCentreHierarchyAsync(companyId);
                return Ok(new { message = "Cost centre hierarchy retrieved successfully", data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cost centre hierarchy");
                return StatusCode(500, new { message = "An error occurred while retrieving cost centre hierarchy", error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpGet("company/{companyId}/dropdown")]
        [SwaggerOperation(Summary = "Gets cost centres in a dropdown format")]
        [ProducesResponseType(typeof(List<CsCostCentreDropdownItem>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCostCentresDropdown(int companyId)
        {
            try
            {
                var result = await _costCentreService.GetCostCentresDropdownAsync(companyId);
                return Ok(new { message = "Cost centres dropdown retrieved successfully", data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cost centres dropdown");
                return StatusCode(500, new { message = "An error occurred while retrieving cost centres dropdown", error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpGet("all")]
        [SwaggerOperation(Summary = "Gets all cost centres across all companies")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllCostCentres()
        {
            try
            {
                var result = await _costCentreService.GetAllCostCentresAsync();
                return Ok(new { message = "Cost centres retrieved successfully", data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAllCostCentres endpoint");
                return StatusCode(500, new { message = "An error occurred while retrieving cost centres", error = ex.Message, stackTrace = ex.StackTrace });
            }
        }
    }
}
