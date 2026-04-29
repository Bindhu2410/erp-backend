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
    public class UomController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UomController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Uom
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Uom>>> GetUoms()
        {
            return await _context.Uoms.ToListAsync();
        }

        // GET: api/Uom/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Uom>> GetUom(int id)
        {
            var uom = await _context.Uoms.FindAsync(id);
            if (uom == null)
            {
                return NotFound();
            }
            return uom;
        }

        // POST: api/Uom
        [HttpPost]
        [Consumes("application/json")]
        public async Task<ActionResult<Uom>> PostUom([FromBody] Uom uom)
        {
            _context.Uoms.Add(uom);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetUom), new { id = uom.Id }, uom);
        }

        // PUT: api/Uom/5
        [HttpPut("{id}")]
        [Consumes("application/json")]
        public async Task<IActionResult> PutUom(int id, [FromBody] Uom uom)
        {
            if (id != uom.Id)
            {
                return BadRequest();
            }
            _context.Entry(uom).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Uoms.Any(e => e.Id == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            // Return the updated Uom object
            return Ok(uom);
        }

        // DELETE: api/Uom/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUom(int id)
        {
            var uom = await _context.Uoms.FindAsync(id);
            if (uom == null)
            {
                return NotFound();
            }
            _context.Uoms.Remove(uom);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
