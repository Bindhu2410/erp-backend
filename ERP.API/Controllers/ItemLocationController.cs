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
    public class ItemLocationController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ItemLocationController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/ItemLocation
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ItemLocation>>> GetItemLocations()
        {
            var itemLocations = await _context.ItemLocations.ToListAsync();
            var itemIds = itemLocations.Select(x => x.ItemId).Distinct().ToList();
            var itemMasters = await _context.ItemMasters.Where(im => itemIds.Contains(im.Id)).ToListAsync();
            foreach (var loc in itemLocations)
            {
                loc.ItemName = itemMasters.FirstOrDefault(im => im.Id == loc.ItemId)?.ItemName;
            }
            return itemLocations;
        }

        // GET: api/ItemLocation/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ItemLocation>> GetItemLocation(int id)
        {
            var itemLocation = await _context.ItemLocations.FindAsync(id);
            if (itemLocation == null)
            {
                return NotFound();
            }
            var itemMaster = await _context.ItemMasters.FirstOrDefaultAsync(im => im.Id == itemLocation.ItemId);
            itemLocation.ItemName = itemMaster?.ItemName;
            return itemLocation;
        }

        // POST: api/ItemLocation
        [HttpPost]
        public async Task<ActionResult<ItemLocation>> PostItemLocation([FromBody] ItemLocation itemLocation)
        {
            _context.ItemLocations.Add(itemLocation);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetItemLocation), new { id = itemLocation.Id }, itemLocation);
        }

        // PUT: api/ItemLocation/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutItemLocation([FromRoute] int id, [FromBody] ItemLocation itemLocation)
        {
            if (id != itemLocation.Id)
            {
                return BadRequest();
            }
            _context.Entry(itemLocation).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.ItemLocations.Any(e => e.Id == id))
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

        // DELETE: api/ItemLocation/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteItemLocation(int id)
        {
            var itemLocation = await _context.ItemLocations.FindAsync(id);
            if (itemLocation == null)
            {
                return NotFound();
            }
            _context.ItemLocations.Remove(itemLocation);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
