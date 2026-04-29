using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using ERP.API.Models;
using ERP.API.Services;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SalesCityController : ControllerBase
    {
        private readonly SalesCityService _salesCityService;

        public SalesCityController(SalesCityService salesCityService)
        {
            _salesCityService = salesCityService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SalesCity>>> GetAll()
        {
            var cities = await _salesCityService.GetAllAsync();
            return Ok(cities);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<IEnumerable<SalesCity>>> GetById(int id)
        {
            var city = await _salesCityService.GetByIdAsync(id);
            if (city == null)
                return NotFound($"City with ID {id} not found");

            return Ok(new[] { city });
        }

        [HttpGet("district/{districtId}")]
        public async Task<ActionResult<IEnumerable<SalesCity>>> GetByDistrictId(int districtId)
        {
            var cities = await _salesCityService.GetAllAsync("sales_districts_id = @DistrictId", new { DistrictId = districtId });
            return Ok(cities);
        }

        [HttpPost]
        public async Task<ActionResult<int>> Create(SalesCity city)
        {
            var id = await _salesCityService.CreateAsync(city);
            return CreatedAtAction(nameof(GetById), new { id }, id);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, SalesCity city)
        {
            if (id != city.Id)
                return BadRequest();

            await _salesCityService.UpdateAsync(city);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _salesCityService.DeleteAsync(id);
            return NoContent();
        }
    }
}