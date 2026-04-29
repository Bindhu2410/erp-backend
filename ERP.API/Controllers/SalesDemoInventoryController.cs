using ERP.API.Models;
using ERP.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SalesDemoInventoryController : ControllerBase
    {
        private readonly SalesDemoInventoryService _service;

        public SalesDemoInventoryController(SalesDemoInventoryService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SalesDemoInventory>>> GetAll()
        {
            var items = await _service.GetAllWithProductAsync();
            return Ok(items);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SalesDemoInventory?>> GetById(int id)
        {
            var item = await _service.GetByIdWithProductAsync(id);
            if (item == null)
                return NotFound();
            return Ok(item);
        }
        [HttpPost]
        public async Task<ActionResult<int>> Create([FromBody] SalesDemoInventory entity)
        {
            var id = await _service.CreateAsync(entity);
            return CreatedAtAction(nameof(GetById), new { id }, entity);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] SalesDemoInventory entity)
        {
            if (entity.Id != id)
                return BadRequest("ID mismatch");
            var updated = await _service.UpdateAsync(entity);
            if (!updated)
                return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _service.DeleteAsync(id);
            if (!deleted)
                return NotFound();
            return NoContent();
        }
    }
}
