using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ERP.API.Models;

namespace ERP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InventoryMethodController : ControllerBase
    {
        private readonly AppDbContext _context;

        public InventoryMethodController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/InventoryMethod
        [HttpGet]
        public async Task<ActionResult<IEnumerable<InventoryMethod>>> GetInventoryMethods()
        {
            return await _context.InventoryMethods.ToListAsync();
        }

        // GET: api/InventoryMethod/5
        [HttpGet("{id}")]
        public async Task<ActionResult<InventoryMethod>> GetInventoryMethod(int id)
        {
            var inventoryMethod = await _context.InventoryMethods.FindAsync(id);
            if (inventoryMethod == null)
            {
                return NotFound();
            }
            return inventoryMethod;
        }

        // POST: api/InventoryMethod
        [HttpPost]
        public async Task<ActionResult<InventoryMethod>> PostInventoryMethod([FromBody] InventoryMethod inventoryMethod)
        {
            _context.InventoryMethods.Add(inventoryMethod);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetInventoryMethod), new { id = inventoryMethod.Id }, inventoryMethod);
        }

        // PUT: api/InventoryMethod/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutInventoryMethod([FromRoute] int id, [FromBody] InventoryMethod inventoryMethod)
        {
            if (id != inventoryMethod.Id)
            {
                return BadRequest();
            }
            _context.Entry(inventoryMethod).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.InventoryMethods.Any(e => e.Id == id))
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

        // DELETE: api/InventoryMethod/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInventoryMethod(int id)
        {
            var inventoryMethod = await _context.InventoryMethods.FindAsync(id);
            if (inventoryMethod == null)
            {
                return NotFound();
            }
            _context.InventoryMethods.Remove(inventoryMethod);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
