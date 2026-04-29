using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using ERP.API.Models;
using ERP.API.Services;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SalesTerritoryController : ControllerBase
    {
        private readonly SalesTerritoryService _salesTerritoryService;

        public SalesTerritoryController(SalesTerritoryService salesTerritoryService)
        {
            _salesTerritoryService = salesTerritoryService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SalesTerritory>>> GetAll()
        {
            var territories = await _salesTerritoryService.GetAllAsync();
            return Ok(territories);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<IEnumerable<SalesTerritory>>> GetById(int id)
        {
            var territory = await _salesTerritoryService.GetByIdAsync(id);
            if (territory == null)
                return NotFound($"Territory with ID {id} not found");

            return Ok(new[] { territory });
        }

        [HttpPost]
        public async Task<ActionResult<int>> Create(SalesTerritory territory)
        {
            var id = await _salesTerritoryService.CreateAsync(territory);
            return CreatedAtAction(nameof(GetById), new { id }, id);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, SalesTerritory territory)
        {
            if (id != territory.Id)
                return BadRequest();

            await _salesTerritoryService.UpdateAsync(territory);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _salesTerritoryService.DeleteAsync(id);
            return NoContent();
        }
    }
}