using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using ERP.API.Models;
using ERP.API.Services;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SalesAreaController : ControllerBase
    {
        private readonly SalesAreaService _salesAreaService;

        public SalesAreaController(SalesAreaService salesAreaService)
        {
            _salesAreaService = salesAreaService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SalesArea>>> GetAll()
        {
            var areas = await _salesAreaService.GetAllAsync();
            return Ok(areas);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<IEnumerable<SalesArea>>> GetById(int id)
        {
            var area = await _salesAreaService.GetByIdAsync(id);
            if (area == null)
                return NotFound($"Area with ID {id} not found");

            return Ok(new[] { area });
        }

        [HttpGet("city/{cityId}")]
        public async Task<ActionResult<IEnumerable<SalesArea>>> GetByCityId(int cityId)
        {
            var areas = await _salesAreaService.GetAllAsync("sales_cities_id = @CityId", new { CityId = cityId });
            return Ok(areas);
        }

        [HttpPost]
        public async Task<ActionResult<int>> Create(SalesArea area)
        {
            var id = await _salesAreaService.CreateAsync(area);
            return CreatedAtAction(nameof(GetById), new { id }, id);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, SalesArea area)
        {
            if (id != area.Id)
                return BadRequest();

            await _salesAreaService.UpdateAsync(area);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _salesAreaService.DeleteAsync(id);
            return NoContent();
        }
    }
}