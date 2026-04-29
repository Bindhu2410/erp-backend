using System;
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
    public class BomItemMappingController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BomItemMappingController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/BomItemMapping
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BomItemMapping>>> GetMappings()
        {
            return await _context.BomItemMappings
                .Include(m => m.BomName)
                .Include(m => m.Item)
                .ToListAsync();
        }

        // GET: api/BomItemMapping/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BomItemMapping>> GetMapping(int id)
        {
            var mapping = await _context.BomItemMappings
                .Include(m => m.BomName)
                .Include(m => m.Item)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (mapping == null)
            {
                return NotFound();
            }
            return mapping;
        }

        // GET: api/BomItemMapping/bybom/123
        [HttpGet("bybom/{bomNameId}")]
        public async Task<ActionResult<IEnumerable<BomItemMapping>>> GetByBom(int bomNameId)
        {
            var list = await _context.BomItemMappings
                .Where(m => m.BomNameId == bomNameId)
                .Include(m => m.Item)
                .ToListAsync();
            return list;
        }

        // POST: api/BomItemMapping
        [HttpPost]
        public async Task<ActionResult<BomItemMapping>> PostMapping([FromBody] BomItemMapping mapping)
        {
            if (mapping == null || mapping.BomNameId == 0 || mapping.ItemId == 0)
            {
                return BadRequest("bom_name_id and item_id are required.");
            }

            // prevent duplicates
            var exists = await _context.BomItemMappings.AnyAsync(m => m.BomNameId == mapping.BomNameId && m.ItemId == mapping.ItemId);
            if (exists)
            {
                return Conflict("Mapping already exists.");
            }

            _context.BomItemMappings.Add(mapping);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return BadRequest(ex.Message);
            }

            return CreatedAtAction(nameof(GetMapping), new { id = mapping.Id }, mapping);
        }

        // PUT: api/BomItemMapping/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMapping(int id, [FromBody] BomItemMapping mapping)
        {
            if (id != mapping.Id)
            {
                return BadRequest();
            }
            _context.Entry(mapping).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.BomItemMappings.AnyAsync(e => e.Id == id))
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

        // DELETE: api/BomItemMapping/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMapping(int id)
        {
            var mapping = await _context.BomItemMappings.FindAsync(id);
            if (mapping == null)
            {
                return NotFound();
            }
            _context.BomItemMappings.Remove(mapping);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
