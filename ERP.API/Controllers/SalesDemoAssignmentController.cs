using ERP.API.Models;
using ERP.API.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SalesDemoAssignmentController : ControllerBase
    {
        private readonly SalesDemoAssignmentService _service;

        public SalesDemoAssignmentController(SalesDemoAssignmentService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SalesDemoAssignment>>> GetAll()
        {
            var items = await _service.GetAllAsync();
            return Ok(items);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SalesDemoAssignment?>> GetById(int id)
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null)
                return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public async Task<ActionResult<int>> Create([FromBody] SalesDemoAssignment entity)
        {
            var id = await _service.CreateAsync(entity);
            return Ok(id);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] SalesDemoAssignment entity)
        {
            if (id != entity.Id)
                return BadRequest();
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
