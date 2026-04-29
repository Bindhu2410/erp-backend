using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ERP.API.Models;
using ERP.API.Models.DTOs;
using ERP.API.Services;

namespace ERP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GeographicalDivisionController : ControllerBase
    {
        private readonly IGeographicalDivisionService _divisionService;
        private readonly ILogger<GeographicalDivisionController> _logger;

        public GeographicalDivisionController(
            IGeographicalDivisionService divisionService,
            ILogger<GeographicalDivisionController> logger)
        {
            _divisionService = divisionService;
            _logger = logger;
        }

        // GET: api/GeographicalDivision
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GeographicalDivision>>> GetDivisions()
        {
            try
            {
                var divisions = await _divisionService.GetAllAsync();
                return Ok(divisions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving geographical divisions: {Message}", ex.Message);
                return StatusCode(500, $"Failed to retrieve geographical divisions: {ex.Message}");
            }
        }

        // GET: api/GeographicalDivision/5
        [HttpGet("{id}")]
        public async Task<ActionResult<GeographicalDivision>> GetDivision(long id)
        {
            try
            {
                var division = await _divisionService.GetByIdAsync(id);

                if (division == null)
                {
                    _logger.LogInformation("Geographical division not found: {Id}", id);
                    return NotFound($"Geographical division with ID {id} not found");
                }

                return Ok(division);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving geographical division {Id}: {Message}", id, ex.Message);
                return StatusCode(500, $"Failed to retrieve geographical division {id}: {ex.Message}");
            }
        }

        // GET: api/GeographicalDivision/hierarchy/pincode/560001
        [HttpGet("hierarchy/pincode/{pincode}")]
        public async Task<ActionResult<IEnumerable<GeographicalHierarchyDto>>> GetHierarchyByPincode(string pincode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(pincode))
                {
                    return BadRequest("Pincode cannot be empty");
                }

                var hierarchy = await _divisionService.GetHierarchyByPincodeAsync(pincode);
                
                if (!hierarchy.Any())
                {
                    return NotFound($"No hierarchy found for pincode {pincode}");
                }

                return Ok(hierarchy);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving geographical hierarchy for pincode {Pincode}: {Message}", pincode, ex.Message);
                return StatusCode(500, $"Failed to retrieve hierarchy for pincode {pincode}: {ex.Message}");
            }
        }

        // POST: api/GeographicalDivision
        [HttpPost]
        public async Task<ActionResult<GeographicalDivision>> CreateDivision(GeographicalDivision division)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var id = await _divisionService.CreateAsync(division);
                division.DivisionId = id;

                _logger.LogInformation("Created geographical division with ID {Id}", id);
                return CreatedAtAction(nameof(GetDivision), new { id }, division);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating geographical division: {Message}", ex.Message);
                return StatusCode(500, $"Failed to create geographical division: {ex.Message}");
            }
        }

        // PUT: api/GeographicalDivision/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDivision(long id, GeographicalDivision division)
        {
            if (id != division.DivisionId)
            {
                return BadRequest("ID mismatch");
            }

            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var success = await _divisionService.UpdateAsync(division);

                if (!success)
                {
                    _logger.LogWarning("Geographical division with ID {Id} not found for update", id);
                    return NotFound($"Geographical division with ID {id} not found");
                }

                _logger.LogInformation("Updated geographical division with ID {Id}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating geographical division {Id}: {Message}", id, ex.Message);
                return StatusCode(500, $"Failed to update geographical division {id}: {ex.Message}");
            }
        }

        // DELETE: api/GeographicalDivision/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDivision(long id)
        {
            try
            {
                var success = await _divisionService.DeleteAsync(id);

                if (!success)
                {
                    _logger.LogWarning("Geographical division with ID {Id} not found for deletion", id);
                    return NotFound($"Geographical division with ID {id} not found");
                }

                _logger.LogInformation("Deleted geographical division with ID {Id}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting geographical division {Id}: {Message}", id, ex.Message);
                return StatusCode(500, $"Failed to delete geographical division {id}: {ex.Message}");
            }
        }
    }
}
