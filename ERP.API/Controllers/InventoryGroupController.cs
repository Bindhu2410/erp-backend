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
    public class InventoryGroupController : ControllerBase
    {
        private readonly AppDbContext _context;

        public InventoryGroupController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/InventoryGroup
        [HttpGet]
        public async Task<ActionResult<IEnumerable<InventoryGroup>>> GetInventoryGroups()
        {
            return await _context.InventoryGroups.ToListAsync();
        }

        // GET: api/InventoryGroup/5
        [HttpGet("{id}")]
        public async Task<ActionResult<InventoryGroup>> GetInventoryGroup(int id)
        {
            var inventoryGroup = await _context.InventoryGroups.FindAsync(id);
            if (inventoryGroup == null)
            {
                return NotFound();
            }
            return inventoryGroup;
        }

        // POST: api/InventoryGroup
        [HttpPost]
        [Consumes("application/json")]
        public async Task<ActionResult<InventoryGroup>> PostInventoryGroup([FromBody] InventoryGroup inventoryGroup)
        {
            _context.InventoryGroups.Add(inventoryGroup);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetInventoryGroup), new { id = inventoryGroup.Id }, inventoryGroup);
        }

        // PUT: api/InventoryGroup/5
        [HttpPut("{id}")]
        [Consumes("application/json")]
        public async Task<IActionResult> PutInventoryGroup(int id, [FromBody] InventoryGroup inventoryGroup)
        {
            if (id != inventoryGroup.Id)
            {
                return BadRequest();
            }
            _context.Entry(inventoryGroup).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.InventoryGroups.Any(e => e.Id == id))
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

        // DELETE: api/InventoryGroup/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInventoryGroup(int id)
        {
            var inventoryGroup = await _context.InventoryGroups.FindAsync(id);
            if (inventoryGroup == null)
            {
                return NotFound();
            }
            _context.InventoryGroups.Remove(inventoryGroup);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
