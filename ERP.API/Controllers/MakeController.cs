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
    public class MakeController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MakeController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Make>>> GetMakes()
        {
            return await _context.Makes.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Make>> GetMake(int id)
        {
            var make = await _context.Makes.FindAsync(id);
            if (make == null) return NotFound();
            return make;
        }

        [HttpPost]
        public async Task<ActionResult<Make>> PostMake([FromBody] Make make)
        {
            _context.Makes.Add(make);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetMake), new { id = make.Id }, make);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutMake([FromRoute] int id, [FromBody] Make make)
        {
            if (id != make.Id) return BadRequest();
            _context.Entry(make).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Makes.Any(e => e.Id == id)) return NotFound();
                else throw;
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMake(int id)
        {
            var make = await _context.Makes.FindAsync(id);
            if (make == null) return NotFound();
            _context.Makes.Remove(make);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
