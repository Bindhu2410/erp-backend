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
    public class ModelController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ModelController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Model>>> GetModels()
        {
            return await _context.Models.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Model>> GetModel(int id)
        {
            var model = await _context.Models.FindAsync(id);
            if (model == null) return NotFound();
            return model;
        }

        [HttpPost]
        public async Task<ActionResult<Model>> PostModel([FromBody] Model model)
        {
            _context.Models.Add(model);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetModel), new { id = model.Id }, model);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutModel([FromRoute] int id, [FromBody] Model model)
        {
            if (id != model.Id) return BadRequest();
            _context.Entry(model).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Models.Any(e => e.Id == id)) return NotFound();
                else throw;
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteModel(int id)
        {
            var model = await _context.Models.FindAsync(id);
            if (model == null) return NotFound();
            _context.Models.Remove(model);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
