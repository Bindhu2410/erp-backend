using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ERP.API.Models;
using ERP.API.Services;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public class SalesTermsAndConditionsController : ControllerBase
    {
        private readonly ISalesTermsAndConditionsService _termsService;
        private readonly ILogger<SalesTermsAndConditionsController> _logger;

        public SalesTermsAndConditionsController(
            ISalesTermsAndConditionsService termsService,
            ILogger<SalesTermsAndConditionsController> logger)
        {
            _termsService = termsService;
            _logger = logger;
        }

        /// <summary>
        /// Get all terms and conditions templates
        /// </summary>
        /// <returns>List of all active templates</returns>
        /// <response code="200">Returns the list of templates</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<SalesTermsAndConditions>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<SalesTermsAndConditions>>> GetAll()
        {
            try
            {
                var terms = await _termsService.GetAllAsync();
                return Ok(terms);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all terms and conditions: {Message}", ex.Message);
                return StatusCode(500, new { error = "An error occurred while retrieving terms and conditions", details = ex.Message });
            }
        }

        /// <summary>
        /// Get terms and conditions by ID
        /// </summary>
        /// <param name="id">The ID of the terms and conditions template</param>
        /// <returns>The terms and conditions template</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(SalesTermsAndConditions), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<SalesTermsAndConditions>> GetById(int id)
        {
            try
            {
                var terms = await _termsService.GetByIdAsync(id);
                return Ok(terms);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting terms and conditions by id {Id}: {Message}", id, ex.Message);
                return StatusCode(500, new { error = "An error occurred while retrieving terms and conditions", details = ex.Message });
            }
        }

        /// <summary>
        /// Get terms and conditions by quotation ID
        /// </summary>
        /// <param name="quotationId">The ID of the quotation</param>
        /// <returns>The terms and conditions for the quotation</returns>
        [HttpGet("quotation/{quotationId}")]
        [ProducesResponseType(typeof(SalesTermsAndConditions), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<SalesTermsAndConditions>> GetByQuotationId(int quotationId)
        {
            try
            {
                var terms = await _termsService.GetByQuotationIdAsync(quotationId);
                return Ok(terms);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting terms and conditions for quotation {QuotationId}: {Message}", quotationId, ex.Message);
                return StatusCode(500, new { error = "An error occurred while retrieving terms and conditions", details = ex.Message });
            }
        }

        /// <summary>
        /// Create a new terms and conditions template
        /// </summary>
        /// <returns>The ID of the created template</returns>
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<int>> Create([FromBody] SalesTermsAndConditions terms)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Basic validation
                if (terms == null)
                {
                    return BadRequest(new { error = "Terms and conditions data is required" });
                }

                var id = await _termsService.CreateAsync(terms);
                return CreatedAtAction(nameof(GetById), new { id }, id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating terms and conditions: {Message}", ex.Message);
                return StatusCode(500, new { error = "An error occurred while creating terms and conditions", details = ex.Message });
            }
        }

        /// <summary>
        /// Update terms and conditions by ID
        /// </summary>
        /// <param name="id">The ID of the template to update</param>
        /// <param name="terms">The updated terms and conditions data</param>
        /// <returns>No content</returns>
        [HttpPut("{id}")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] SalesTermsAndConditions terms)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (id != terms.Id)
                {
                    return BadRequest(new { error = "ID mismatch between URL and body" });
                }

                await _termsService.UpdateAsync(terms);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating terms and conditions {Id}: {Message}", id, ex.Message);
                return StatusCode(500, new { error = "An error occurred while updating terms and conditions", details = ex.Message });
            }
        }

        /// <summary>
        /// Update terms and conditions by quotation ID
        /// </summary>
        /// <param name="quotationId">The ID of the quotation</param>
        /// <param name="terms">The updated terms and conditions data</param>
        /// <returns>No content</returns>
        [HttpPut("quotation/{quotationId}")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateByQuotationId(int quotationId, [FromBody] SalesTermsAndConditions terms)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // First get the existing terms for this quotation
                var existingTerms = await _termsService.GetByQuotationIdAsync(quotationId);
                
                // Update the terms
                terms.Id = existingTerms.Id;
                terms.QuotationId = quotationId;
                terms.UserUpdated = existingTerms.UserUpdated;
                terms.DateUpdated = DateTime.UtcNow;
                
                await _termsService.UpdateAsync(terms);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating terms and conditions for quotation {QuotationId}: {Message}", quotationId, ex.Message);
                return StatusCode(500, new { error = "An error occurred while updating terms and conditions", details = ex.Message });
            }
        }

        /// <summary>
        /// Delete a terms and conditions template
        /// </summary>
        /// <param name="id">The ID of the template to delete</param>
        /// <returns>No content</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _termsService.DeleteAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting terms and conditions {Id}: {Message}", id, ex.Message);
                return StatusCode(500, new { error = "An error occurred while deleting terms and conditions", details = ex.Message });
            }
        }
    }
}
