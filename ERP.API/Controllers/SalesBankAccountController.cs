using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ERP.API.Models;
using ERP.API.Services;
using Microsoft.Extensions.Logging;
using System;
using Microsoft.AspNetCore.Http;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public class SalesBankAccountController : ControllerBase
    {
        private readonly ISalesBankAccountService _bankAccountService;
        private readonly ILogger<SalesBankAccountController> _logger;

        public SalesBankAccountController(
            ISalesBankAccountService bankAccountService,
            ILogger<SalesBankAccountController> logger)
        {
            _bankAccountService = bankAccountService;
            _logger = logger;
        }

        /// <summary>
        /// Get all bank accounts
        /// </summary>
        /// <returns>List of all active bank accounts</returns>
        /// <response code="200">Returns the list of bank accounts</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<SalesBankAccount>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<SalesBankAccount>>> GetAll()
        {
            try
            {
                var accounts = await _bankAccountService.GetAllAsync();
                return Ok(accounts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all bank accounts: {Message}", ex.Message);
                return StatusCode(500, "An error occurred while retrieving bank accounts");
            }
        }

        /// <summary>
        /// Get bank account by ID
        /// </summary>
        /// <param name="id">The ID of the bank account</param>
        /// <returns>The bank account</returns>
        /// <response code="200">Returns the bank account</response>
        /// <response code="404">If the bank account was not found</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(SalesBankAccount), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<SalesBankAccount>> GetById(int id)
        {
            try
            {
                var account = await _bankAccountService.GetByIdAsync(id);
                if (account == null)
                    return NotFound($"Bank account with ID {id} not found");

                return Ok(account);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Bank account with ID {id} not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting bank account by id {Id}: {Message}", id, ex.Message);
                return StatusCode(500, "An error occurred while retrieving bank account");
            }
        }

        /// <summary>
        /// Create a new bank account
        /// </summary>
        /// <param name="bankAccount">The bank account details</param>
        /// <returns>The ID of the created bank account</returns>
        /// <response code="201">Returns the ID of the created bank account</response>
        /// <response code="400">If the request data is invalid</response>
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<int>> Create([FromBody] SalesBankAccount bankAccount)
        {
            try
            {
                var id = await _bankAccountService.CreateAsync(bankAccount);
                return CreatedAtAction(nameof(GetById), new { id }, id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating bank account: {Message}", ex.Message);
                return StatusCode(500, "An error occurred while creating bank account");
            }
        }

        /// <summary>
        /// Update an existing bank account
        /// </summary>
        /// <param name="id">The ID of the bank account to update</param>
        /// <param name="bankAccount">The updated bank account data</param>
        /// <returns>No content</returns>
        /// <response code="204">If the update was successful</response>
        /// <response code="400">If the request data is invalid</response>
        [HttpPut("{id}")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(int id, [FromBody] SalesBankAccount bankAccount)
        {
            try
            {
                if (id != bankAccount.Id)
                    return BadRequest("ID mismatch");

                await _bankAccountService.UpdateAsync(bankAccount);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating bank account {Id}: {Message}", id, ex.Message);
                return StatusCode(500, "An error occurred while updating bank account");
            }
        }

        /// <summary>
        /// Delete a bank account
        /// </summary>
        /// <param name="id">The ID of the bank account to delete</param>
        /// <returns>No content</returns>
        /// <response code="204">If the deletion was successful</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _bankAccountService.DeleteAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting bank account {Id}: {Message}", id, ex.Message);
                return StatusCode(500, "An error occurred while deleting bank account");
            }
        }
    }
}
