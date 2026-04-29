using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using ERP.API.Models;
using ERP.API.Services;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SalesDistrictController : ControllerBase
    {
        private readonly SalesDistrictService _salesDistrictService;

        public SalesDistrictController(SalesDistrictService salesDistrictService)
        {
            _salesDistrictService = salesDistrictService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SalesDistrict>>> GetAll()
        {
            var districts = await _salesDistrictService.GetAllAsync();
            return Ok(districts);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<IEnumerable<SalesDistrict>>> GetById(int id)
        {
            var district = await _salesDistrictService.GetByIdAsync(id);
            if (district == null)
                return NotFound($"District with ID {id} not found");

            return Ok(new[] { district });
        }

        [HttpGet("territory/{territoryId}")]
        public async Task<ActionResult<IEnumerable<SalesDistrict>>> GetByTerritoryId(int territoryId)
        {
            var districts = await _salesDistrictService.GetAllAsync("sales_territories_id = @TerritoryId", new { TerritoryId = territoryId });
            return Ok(districts);
        }

        [HttpPost]
        public async Task<ActionResult<int>> Create(SalesDistrict district)
        {
            var id = await _salesDistrictService.CreateAsync(district);
            return CreatedAtAction(nameof(GetById), new { id }, id);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, SalesDistrict district)
        {
            if (id != district.Id)
                return BadRequest();

            await _salesDistrictService.UpdateAsync(district);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _salesDistrictService.DeleteAsync(id);
            return NoContent();
        }
    }
}