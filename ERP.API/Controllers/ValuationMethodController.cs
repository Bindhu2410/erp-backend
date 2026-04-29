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
    public class ValuationMethodController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ValuationMethodController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/ValuationMethod
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ValuationMethod>>> GetValuationMethods()
        {
            return await _context.ValuationMethods.ToListAsync();
        }

        // GET: api/ValuationMethod/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ValuationMethod>> GetValuationMethod(int id)
        {
            var valuationMethod = await _context.ValuationMethods.FindAsync(id);
            if (valuationMethod == null)
            {
                return NotFound();
            }
            return valuationMethod;
        }

        // POST: api/ValuationMethod
        [HttpPost]
        [Consumes("application/json")]
        public async Task<ActionResult<ValuationMethod>> PostValuationMethod([FromBody] ValuationMethod valuationMethod)
        {
            _context.ValuationMethods.Add(valuationMethod);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetValuationMethod), new { id = valuationMethod.Id }, valuationMethod);
        }

        // PUT: api/ValuationMethod/5
        [HttpPut("{id}")]
        [Consumes("application/json")]
        public async Task<IActionResult> PutValuationMethod(int id, [FromBody] ValuationMethod valuationMethod)
        {
            if (id != valuationMethod.Id)
            {
                return BadRequest();
            }
            _context.Entry(valuationMethod).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.ValuationMethods.Any(e => e.Id == id))
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

        // DELETE: api/ValuationMethod/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteValuationMethod(int id)
        {
            var valuationMethod = await _context.ValuationMethods.FindAsync(id);
            if (valuationMethod == null)
            {
                return NotFound();
            }
            _context.ValuationMethods.Remove(valuationMethod);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
