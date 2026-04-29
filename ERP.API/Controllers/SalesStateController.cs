using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using ERP.API.Models;
using ERP.API.Services;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SalesStateController : ControllerBase
    {
        private readonly SalesStateService _salesStateService;

        public SalesStateController(SalesStateService salesStateService)
        {
            _salesStateService = salesStateService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SalesState>>> GetAll()
        {
            var states = await _salesStateService.GetAllAsync();
            return Ok(states);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<IEnumerable<SalesState>>> GetById(int id)
        {
            var state = await _salesStateService.GetByIdAsync(id);
            if (state == null)
                return NotFound($"State with ID {id} not found");

            return Ok(new[] { state });
        }

        [HttpGet("country/{countryId}")]
        public async Task<ActionResult<IEnumerable<SalesState>>> GetByCountryId(int countryId)
        {
            var states = await _salesStateService.GetAllAsync("sales_countries_id = @CountryId", new { CountryId = countryId });
            return Ok(states);
        }

        [HttpPost]
        public async Task<ActionResult<int>> Create(SalesState state)
        {
            var id = await _salesStateService.CreateAsync(state);
            return CreatedAtAction(nameof(GetById), new { id }, id);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, SalesState state)
        {
            if (id != state.Id)
                return BadRequest();

            await _salesStateService.UpdateAsync(state);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _salesStateService.DeleteAsync(id);
            return NoContent();
        }
    }
}