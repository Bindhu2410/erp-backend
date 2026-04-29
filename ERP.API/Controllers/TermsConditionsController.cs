using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ERP.API.Models;
using ERP.API.Services;
using Microsoft.Extensions.Logging;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class TermsConditionsController : ControllerBase
    {
        private readonly ITermsConditionsService _service;
        private readonly ILogger<TermsConditionsController> _logger;

        public TermsConditionsController(
            ITermsConditionsService service,
            ILogger<TermsConditionsController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TermsConditions>>> GetAll()
        {
            try
            {
                var result = await _service.GetAllAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all terms and conditions");
                return StatusCode(500, new { error = "An error occurred", details = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TermsConditions>> GetById(int id)
        {
            try
            {
                var result = await _service.GetByIdAsync(id);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting terms and conditions by id {Id}", id);
                return StatusCode(500, new { error = "An error occurred", details = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<int>> Create([FromBody] TermsConditions termsConditions)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var id = await _service.CreateAsync(termsConditions);
                return CreatedAtAction(nameof(GetById), new { id }, id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating terms and conditions");
                return StatusCode(500, new { error = "An error occurred", details = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] TermsConditions termsConditions)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (id != termsConditions.Id)
                    return BadRequest(new { error = "ID mismatch" });

                await _service.UpdateAsync(termsConditions);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating terms and conditions {Id}", id);
                return StatusCode(500, new { error = "An error occurred", details = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _service.DeleteAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting terms and conditions {Id}", id);
                return StatusCode(500, new { error = "An error occurred", details = ex.Message });
            }
        }
    }
}
