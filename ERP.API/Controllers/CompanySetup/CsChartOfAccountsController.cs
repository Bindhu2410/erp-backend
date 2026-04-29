using Microsoft.AspNetCore.Mvc;
using ERP.API.Models.CompanySetup;
using ERP.API.Services.CompanySetup;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Swashbuckle.AspNetCore.Annotations;

namespace ERP.API.Controllers.CompanySetup
{
    [ApiController]
    [Route("api/[controller]")]
    public class CsChartOfAccountsController : ControllerBase
    {
        private readonly ICsChartOfAccountService _chartOfAccountService;

        public CsChartOfAccountsController(ICsChartOfAccountService chartOfAccountService)
        {
            _chartOfAccountService = chartOfAccountService;
        }

        [HttpGet]
        [SwaggerOperation(Summary = "Gets all chart of accounts")]
        [ProducesResponseType(typeof(IEnumerable<CsChartOfAccount>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<CsChartOfAccount>>> GetAll()
        {
            try
            {
                var results = await _chartOfAccountService.GetAllAsync();
                return Ok(new {
                    message = "Chart of accounts retrieved successfully.",
                    data = results
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new {
                    message = "An error occurred while retrieving chart of accounts.",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        [HttpPost]
        [SwaggerOperation(Summary = "Creates a new chart of account")]
        [ProducesResponseType(typeof(CsChartOfAccount), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CsChartOfAccount>> CreateChartOfAccount([FromBody] CsChartOfAccount chartOfAccount)
        {
            try
            {
                if (chartOfAccount == null)
                {
                    return BadRequest(new {
                        message = "Chart of account data is required"
                    });
                }
                var result = await _chartOfAccountService.CreateChartOfAccountAsync(chartOfAccount);
                return CreatedAtAction(
                    nameof(GetChartOfAccountById),
                    new { accountId = result.AccountId },
                    new {
                        message = "Chart of account created successfully.",
                        data = result
                    });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new {
                    message = "An error occurred while creating chart of account.",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        [HttpPut("{accountId}")]
        [SwaggerOperation(Summary = "Updates an existing chart of account")]
        [ProducesResponseType(typeof(CsChartOfAccount), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CsChartOfAccount>> UpdateChartOfAccount(int accountId, [FromBody] CsChartOfAccount chartOfAccount)
        {
            try
            {
                if (chartOfAccount == null)
                {
                    return BadRequest(new {
                        message = "Chart of account data is required"
                    });
                }
                if (accountId != chartOfAccount.AccountId)
                {
                    return BadRequest(new {
                        message = "Account ID mismatch"
                    });
                }
                var result = await _chartOfAccountService.UpdateChartOfAccountAsync(accountId, chartOfAccount);
                return Ok(new {
                    message = "Chart of account updated successfully.",
                    data = result
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new {
                    message = $"Chart of account with ID {accountId} not found.",
                    data = (object?)null
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new {
                    message = "An error occurred while updating chart of account.",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        [HttpDelete("{accountId}")]
        [SwaggerOperation(Summary = "Deletes a chart of account")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> DeleteChartOfAccount(int accountId)
        {
            try
            {
                var result = await _chartOfAccountService.DeleteChartOfAccountAsync(accountId);
                if (!result)
                {
                    return NotFound(new {
                        message = $"Chart of account with ID {accountId} not found.",
                        data = (object?)null
                    });
                }
                return Ok(new {
                    message = "Chart of account deleted successfully.",
                    data = (object?)null
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new {
                    message = "Business rule violation.",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new {
                    message = "An error occurred while deleting chart of account.",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        [HttpGet("{accountId}")]
        [SwaggerOperation(Summary = "Gets a chart of account by ID")]
        [ProducesResponseType(typeof(CsChartOfAccount), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CsChartOfAccount>> GetChartOfAccountById(int accountId)
        {
            try
            {
                var result = await _chartOfAccountService.GetChartOfAccountByIdAsync(accountId);
                if (result == null)
                {
                    return NotFound(new {
                        message = $"Chart of account with ID {accountId} not found.",
                        data = (object?)null
                    });
                }
                return Ok(new {
                    message = "Chart of account retrieved successfully.",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new {
                    message = "An error occurred while retrieving chart of account.",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        [HttpGet("company/{companyId}")]
        [SwaggerOperation(Summary = "Gets chart of accounts for a company with pagination and search")]
        [ProducesResponseType(typeof(CsChartOfAccountPagedResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<CsChartOfAccountPagedResponse>> GetChartOfAccountsByCompany(
            int companyId,
            [FromQuery] string? searchText = null,
            [FromQuery] string? accountType = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var searchRequest = new CsChartOfAccountSearchRequest
                {
                    CompanyId = companyId,
                    SearchText = searchText,
                    AccountType = accountType,
                    IsActive = isActive,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
                var result = await _chartOfAccountService.GetChartOfAccountsByCompanyAsync(searchRequest);
                return Ok(new {
                    message = "Chart of accounts retrieved successfully.",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new {
                    message = "An error occurred while retrieving chart of accounts by company.",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        [HttpGet("company/{companyId}/hierarchy")]
        [SwaggerOperation(Summary = "Gets the hierarchical structure of chart of accounts for a company")]
        [ProducesResponseType(typeof(IEnumerable<CsChartOfAccountDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<CsChartOfAccountDto>>> GetChartOfAccountsHierarchy(
            int companyId,
            [FromQuery] bool includeInactive = false)
        {
            try
            {
                var result = await _chartOfAccountService.GetChartOfAccountsHierarchyAsync(companyId, includeInactive);
                return Ok(new {
                    message = "Chart of accounts hierarchy retrieved successfully.",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new {
                    message = "An error occurred while retrieving chart of accounts hierarchy.",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        [HttpGet("company/{companyId}/dropdown")]
        [SwaggerOperation(Summary = "Gets chart of accounts in dropdown format for a company")]
        [ProducesResponseType(typeof(IEnumerable<CsChartOfAccountDropdownItem>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<CsChartOfAccountDropdownItem>>> GetChartOfAccountsDropdown(
            int companyId,
            [FromQuery] string? accountType = null)
        {
            try
            {
                var result = await _chartOfAccountService.GetChartOfAccountsDropdownAsync(companyId, accountType);
                return Ok(new {
                    message = "Chart of accounts dropdown retrieved successfully.",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new {
                    message = "An error occurred while retrieving chart of accounts dropdown.",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }
    }
}
