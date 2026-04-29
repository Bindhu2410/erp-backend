using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using ERP.API.Models;
using ERP.API.Services;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SalesCountryController : ControllerBase
    {
        private readonly SalesCountryService _salesCountryService;

        public SalesCountryController(SalesCountryService salesCountryService)
        {
            _salesCountryService = salesCountryService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SalesCountry>>> GetAll()
        {
            var countries = await _salesCountryService.GetAllAsync();
            return Ok(countries);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<IEnumerable<SalesCountry>>> GetById(int id)
        {
            var country = await _salesCountryService.GetByIdAsync(id);
            if (country == null)
                return NotFound($"Country with ID {id} not found");

            return Ok(new[] { country });
        }

        [HttpPost]
        public async Task<ActionResult<int>> Create(SalesCountry country)
        {
            var id = await _salesCountryService.CreateAsync(country);
            return CreatedAtAction(nameof(GetById), new { id }, id);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, SalesCountry country)
        {
            if (id != country.Id)
                return BadRequest();

            await _salesCountryService.UpdateAsync(country);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _salesCountryService.DeleteAsync(id);
            return NoContent();
        }
    }
}