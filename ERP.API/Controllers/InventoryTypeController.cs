using ERP.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InventoryTypeController : ControllerBase
    {
        private readonly AppDbContext _context;

        public InventoryTypeController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/InventoryType
        [HttpGet]
        public async Task<ActionResult<IEnumerable<InventoryType>>> GetInventoryTypes()
        {
            return await _context.InventoryTypes.ToListAsync();
        }

        // GET: api/InventoryType/5
        [HttpGet("{id}")]
        public async Task<ActionResult<InventoryType>> GetInventoryType(int id)
        {
            var inventoryType = await _context.InventoryTypes.FindAsync(id);
            if (inventoryType == null)
            {
                return NotFound();
            }
            return inventoryType;
        }

        // POST: api/InventoryType
        [HttpPost]
        public async Task<ActionResult<InventoryType>> PostInventoryType([FromBody]InventoryType inventoryType)
        {
            _context.InventoryTypes.Add(inventoryType);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetInventoryType), new { id = inventoryType.Id }, inventoryType);
        }

        // PUT: api/InventoryType/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutInventoryType([FromRoute] int id, [FromBody] InventoryType inventoryType)
        {
            if (id != inventoryType.Id)
            {
                return BadRequest();
            }
            _context.Entry(inventoryType).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.InventoryTypes.Any(e => e.Id == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return NoContent();
        }

        // DELETE: api/InventoryType/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInventoryType(int id)
        {
            var inventoryType = await _context.InventoryTypes.FindAsync(id);
            if (inventoryType == null)
            {
                return NotFound();
            }
            _context.InventoryTypes.Remove(inventoryType);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
