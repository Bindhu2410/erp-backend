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
    public class BomNameController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BomNameController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/BomName
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BomName>>> GetBomNames()
        {
            return await _context.BomNames.ToListAsync();
        }

        // GET: api/BomName/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BomName>> GetBomName(int id)
        {
            var bomName = await _context.BomNames.FindAsync(id);
            if (bomName == null)
            {
                return NotFound();
            }
            return bomName;
        }

        // POST: api/BomName
        [HttpPost]
        public async Task<ActionResult<BomName>> PostBomName([FromBody] BomName bomName)
        {
            if (string.IsNullOrWhiteSpace(bomName.Name))
            {
                return BadRequest("Name is required.");
            }
            // Optionally set date_created if not provided
            if (!bomName.DateCreated.HasValue)
            {
                bomName.DateCreated = DateTime.UtcNow;
            }
            _context.BomNames.Add(bomName);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetBomName), new { id = bomName.Id }, bomName);
        }

        // PUT: api/BomName/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBomName(int id, [FromBody] BomName bomName)
        {
            if (id != bomName.Id)
            {
                return BadRequest();
            }
            if (string.IsNullOrWhiteSpace(bomName.Name))
            {
                return BadRequest("Name is required.");
            }
            // Optionally set date_updated
            bomName.DateUpdated = DateTime.UtcNow;
            _context.Entry(bomName).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.BomNames.Any(e => e.Id == id))
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

        // DELETE: api/BomName/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBomName(int id)
        {
            var bomName = await _context.BomNames.FindAsync(id);
            if (bomName == null)
            {
                return NotFound();
            }
            _context.BomNames.Remove(bomName);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
