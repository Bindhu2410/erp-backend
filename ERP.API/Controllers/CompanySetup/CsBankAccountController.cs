using Microsoft.AspNetCore.Mvc;
using ERP.API.Models.CompanySetup;
using ERP.API.Models.DTOs.CompanySetup;
using ERP.API.Services.CompanySetup;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Swashbuckle.AspNetCore.Annotations;

namespace ERP.API.Controllers.CompanySetup
{
    [ApiController]
    [Route("api/[controller]")]
    public class CsBankAccountController : ControllerBase
    {
        private readonly ICsBankAccountService _bankAccountService;

        public CsBankAccountController(ICsBankAccountService bankAccountService)
        {
            _bankAccountService = bankAccountService;
        }

        [HttpPost]
        [SwaggerOperation(Summary = "Creates a new bank account")]
        [ProducesResponseType(typeof(CsBankAccount), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CsBankAccount>> CreateBankAccount([FromBody] CsBankAccountDto bankAccountDto)
        {
            var result = await _bankAccountService.CreateBankAccountAsync(bankAccountDto);
            return CreatedAtAction(nameof(GetBankAccountById), new { id = result.BankAccountId }, result);
        }

        [HttpPut("{id}")]
        [SwaggerOperation(Summary = "Updates an existing bank account")]
        [ProducesResponseType(typeof(CsBankAccount), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CsBankAccount>> UpdateBankAccount(int id, [FromBody] CsBankAccountDto bankAccountDto)
        {
            var result = await _bankAccountService.UpdateBankAccountAsync(id, bankAccountDto);
            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        [HttpDelete("{id}")]
        [SwaggerOperation(Summary = "Deletes a bank account by ID")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteBankAccount(int id)
        {
            try
            {
                var result = await _bankAccountService.DeleteBankAccountAsync(id);
                if (!result)
                {
                    return NotFound(new {
                        message = "Bank account not found.",
                        data = (object?)null
                    });
                }
                return Ok(new {
                    message = "Bank account deleted successfully.",
                    data = (object?)null
                });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new {
                    message = "An error occurred while deleting the bank account.",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        [HttpGet("{id}")]
        [SwaggerOperation(Summary = "Gets a bank account by ID")]
        [ProducesResponseType(typeof(CsBankAccount), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CsBankAccount>> GetBankAccountById(int id)
        {
            var result = await _bankAccountService.GetBankAccountByIdAsync(id);
            if (result == null)
            {
                return NotFound(new { message = "Bank account not found", statusCode = 404 });
            }

            return Ok(new {
                message = "Bank account retrieved successfully",
                data = result,
                statusCode = 200
            });
        }

        [HttpGet("company/{companyId}")]
        [SwaggerOperation(Summary = "Gets bank accounts for a company with pagination")]
        [ProducesResponseType(typeof(IEnumerable<CsBankAccount>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<CsBankAccount>>> GetBankAccountsByCompany(
            int companyId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var (bankAccounts, totalCount) = await _bankAccountService.GetBankAccountsByCompanyAsync(companyId, pageNumber, pageSize);
            Response.Headers["X-Total-Count"] = totalCount.ToString();
            var dto = new {
                message = "Bank accounts retrieved successfully by company",
                data = bankAccounts,
                totalCount = totalCount,
                pageSize = pageSize,
                pageNumber = pageNumber
            };
            return Ok(dto);
        }

        [HttpGet("search")]
        [SwaggerOperation(Summary = "Searches bank accounts by filters")]
        [ProducesResponseType(typeof(IEnumerable<CsBankAccount>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<CsBankAccount>>> SearchBankAccounts(
            [FromQuery] int? companyId,
            [FromQuery] string? searchText = null,
            [FromQuery] string? purpose = null,
            [FromQuery] string? currency = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (companyId.HasValue && companyId.Value <= 0)
            {
                return BadRequest(new { message = "companyId, if provided, must be greater than zero." });
            }

            // Normalize empty strings to null so the stored procedure treats them as no-filter
            string? normalizedSearch = string.IsNullOrWhiteSpace(searchText) ? null : searchText;
            string? normalizedPurpose = string.IsNullOrWhiteSpace(purpose) ? null : purpose;
            string? normalizedCurrency = string.IsNullOrWhiteSpace(currency) ? null : currency;

            var (bankAccounts, totalCount) = await _bankAccountService.SearchBankAccountsAsync(
                companyId,
                normalizedSearch,
                normalizedPurpose,
                normalizedCurrency,
                pageNumber,
                pageSize);
            Response.Headers["X-Total-Count"] = totalCount.ToString();
            var dto = new {
                message = "Bank accounts retrieved successfully",
                data = bankAccounts,
                totalCount = totalCount,
                pageSize = pageSize,
                pageNumber = pageNumber
            };
            return Ok(dto);
        }
    }
}
