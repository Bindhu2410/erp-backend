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
    public class CsBankAccountBranchController : ControllerBase
    {
        private readonly ICsBankAccountBranchService _bankAccountBranchService;

        public CsBankAccountBranchController(ICsBankAccountBranchService bankAccountBranchService)
        {
            _bankAccountBranchService = bankAccountBranchService;
        }

        [HttpPost("{bankAccountId}/branches/{branchId}")]
        [SwaggerOperation(Summary = "Creates a new bank account branch mapping")]
        [ProducesResponseType(typeof(CsBankAccountBranch), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateBankAccountBranch(int bankAccountId, int branchId)
        {
            try
            {
                var result = await _bankAccountBranchService.CreateBankAccountBranchAsync(bankAccountId, branchId);
                return CreatedAtAction(
                    nameof(GetBranchesByBankAccount),
                    new { bankAccountId = result.BankAccountId },
                    new { message = "Bank account branch mapping created successfully", data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating bank account branch mapping", error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpDelete("{bankAccountId}/branches/{branchId}")]
        [SwaggerOperation(Summary = "Deletes a bank account branch mapping")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteBankAccountBranch(int bankAccountId, int branchId)
        {
            try
            {
                var result = await _bankAccountBranchService.DeleteBankAccountBranchAsync(bankAccountId, branchId);
                if (!result)
                    return NotFound(new { message = $"Bank account branch mapping for bankAccountId {bankAccountId} and branchId {branchId} not found for deletion.", data = (object?)null });
                return Ok(new { message = "Bank account branch mapping deleted successfully", data = (object?)null });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting bank account branch mapping", error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpGet("{bankAccountId}/branches")]
        [SwaggerOperation(Summary = "Gets all branches for a bank account")]
        [ProducesResponseType(typeof(IEnumerable<CsBankAccountBranch>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetBranchesByBankAccount(int bankAccountId)
        {
            try
            {
                var result = await _bankAccountBranchService.GetBranchesByBankAccountAsync(bankAccountId);
                return Ok(new { message = "Branches retrieved successfully for bank account", data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving branches for bank account", error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpGet("branches/{branchId}/accounts")]
        [SwaggerOperation(Summary = "Gets all bank accounts for a branch with pagination")]
        [ProducesResponseType(typeof(IEnumerable<CsBankAccountBranchDetail>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetBankAccountsByBranch(
            int branchId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var (accounts, totalCount) = await _bankAccountBranchService.GetBankAccountsByBranchAsync(
                    branchId, pageNumber, pageSize);
                Response.Headers["X-Total-Count"] = totalCount.ToString();
                return Ok(new { message = "Bank accounts retrieved successfully for branch", data = accounts, totalCount });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving bank accounts for branch", error = ex.Message, stackTrace = ex.StackTrace });
            }
        }
    }
}
