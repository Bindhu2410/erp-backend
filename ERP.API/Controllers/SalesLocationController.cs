using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using ERP.API.Models;
using ERP.API.Services;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SalesLocationController : ControllerBase
    {
        private readonly SalesLocationService _salesLocationService;

        public SalesLocationController(SalesLocationService salesLocationService)
        {
            _salesLocationService = salesLocationService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SalesLocation>>> GetAll()
        {
            var locations = await _salesLocationService.GetAllLocationsAsync();
            return Ok(locations);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SalesLocation>> GetById(int id)
        {
            var location = await _salesLocationService.GetLocationByIdAsync(id);
            if (location == null)
                return NotFound($"Location with ID {id} not found");

            return Ok(location);
        }        [HttpPost("search")]
        public async Task<ActionResult<IEnumerable<SalesLocation>>> Search([FromBody] string? country = null, string? state = null, string? territory = null, string? district = null, string? city = null, string? area = null, string? pincode = null)
        {
            var locations = await _salesLocationService.SearchLocationsAsync(
                country ?? string.Empty,
                state ?? string.Empty,
                territory ?? string.Empty,
                district ?? string.Empty,
                city ?? string.Empty,
                area ?? string.Empty,
                pincode ?? string.Empty);
            return Ok(locations);
        }
       

        [HttpPost]
        public async Task<ActionResult<int>> Create(SalesLocation location)
        {
            var id = await _salesLocationService.CreateAsync(location);
            return CreatedAtAction(nameof(GetById), new { id }, id);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, SalesLocation location)
        {
            if (id != location.RowId)
                return BadRequest("ID mismatch");

            var success = await _salesLocationService.UpdateAsync(location);
            if (!success)
                return NotFound($"Location with ID {id} not found");

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _salesLocationService.DeleteAsync(id);
            if (!success)
                return NotFound($"Location with ID {id} not found");

            return NoContent();
        }
    }
}
