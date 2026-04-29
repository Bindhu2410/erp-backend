using Microsoft.AspNetCore.Mvc;
using ERP.API.Models.CompanySetup;
using ERP.API.Services.CompanySetup;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Swashbuckle.AspNetCore.Annotations;

namespace ERP.API.Controllers.CompanySetup
{
    [ApiController]
    [Route("api/[controller]")]
    public class CsBranchCostCentreController : ControllerBase
    {
        private readonly ICsBranchCostCentreService _branchCostCentreService;

        public CsBranchCostCentreController(ICsBranchCostCentreService branchCostCentreService)
        {
            _branchCostCentreService = branchCostCentreService;
        }

        [HttpPost("{branchId}/costcentres/{costCentreId}")]
        [SwaggerOperation(Summary = "Creates a new branch cost centre mapping")]
        [ProducesResponseType(typeof(CsBranchCostCentre), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateBranchCostCentre(int branchId, int costCentreId)
        {
            try
            {
                var result = await _branchCostCentreService.CreateBranchCostCentreAsync(branchId, costCentreId);
                return CreatedAtAction(
                    nameof(GetCostCentresByBranch),
                    new { branchId = result.BranchId },
                    new { message = "Branch cost centre mapping created successfully", data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating branch cost centre mapping", error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpDelete("{branchId}/costcentres/{costCentreId}")]
        [SwaggerOperation(Summary = "Deletes a branch cost centre mapping")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteBranchCostCentre(int branchId, int costCentreId)
        {
            try
            {
                var result = await _branchCostCentreService.DeleteBranchCostCentreAsync(branchId, costCentreId);
                if (!result)
                    return NotFound(new { message = $"Branch cost centre mapping for branch {branchId} and cost centre {costCentreId} not found for deletion.", data = (object?)null });
                return Ok(new { message = "Branch cost centre mapping deleted successfully", data = (object?)null });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting branch cost centre mapping", error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpGet("{branchId}/costcentres")]
        [SwaggerOperation(Summary = "Gets all cost centres for a branch")]
        [ProducesResponseType(typeof(IEnumerable<CsBranchCostCentreDetail>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCostCentresByBranch(int branchId)
        {
            try
            {
                var result = await _branchCostCentreService.GetCostCentresByBranchAsync(branchId);
                return Ok(new { message = "Cost centres retrieved successfully for branch", data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving cost centres for branch", error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpGet("costcentres/{costCentreId}/branches")]
        [SwaggerOperation(Summary = "Gets all branches for a cost centre with pagination")]
        [ProducesResponseType(typeof(CsBranchCostCentrePagedResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetBranchesByCostCentre(
            int costCentreId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _branchCostCentreService.GetBranchesByCostCentreAsync(costCentreId, pageNumber, pageSize);
                return Ok(new { message = "Branches retrieved successfully for cost centre", data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving branches for cost centre", error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpGet("{branchId}/costcentres/dropdown")]
        [SwaggerOperation(Summary = "Gets cost centres in a hierarchical dropdown format for a branch")]
        [ProducesResponseType(typeof(IEnumerable<CsBranchCostCentreDropdownItem>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetBranchCostCentresDropdown(int branchId)
        {
            try
            {
                var result = await _branchCostCentreService.GetBranchCostCentresDropdownAsync(branchId);
                return Ok(new { message = "Branch cost centres dropdown retrieved successfully", data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving branch cost centres dropdown", error = ex.Message, stackTrace = ex.StackTrace });
            }
        }
    }
}
