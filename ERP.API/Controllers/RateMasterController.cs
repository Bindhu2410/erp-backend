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
    public class RateMasterController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RateMasterController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/RateMaster
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RateMaster>>> GetRateMasters()
        {
            var rateMasters = await _context.RateMasters.ToListAsync();
            
            foreach (var rateMaster in rateMasters)
            {
                rateMaster.Items = await _context.RateMasterItems
                    .Where(item => item.RateMasterId == rateMaster.Id)
                    .ToListAsync();
            }
            
            return rateMasters;
        }

        // GET: api/RateMaster/5
        [HttpGet("{id}")]
        public async Task<ActionResult<RateMaster>> GetRateMaster(int id)
        {
            var rateMaster = await _context.RateMasters.FindAsync(id);
            if (rateMaster == null)
            {
                return NotFound();
            }

            rateMaster.Items = await _context.RateMasterItems
                .Where(item => item.RateMasterId == id)
                .ToListAsync();

            return rateMaster;
        }

        // POST: api/RateMaster
        [HttpPost]
        public async Task<ActionResult<RateMaster>> PostRateMaster([FromBody] RateMaster rateMaster)
        {
            // Auto-generate rate_master_id
            rateMaster.RateMasterId = await GenerateRateMasterId();
            rateMaster.DateCreated = DateTime.UtcNow;
            
            _context.RateMasters.Add(rateMaster);
            await _context.SaveChangesAsync();

            if (rateMaster.Items != null && rateMaster.Items.Any())
            {
                foreach (var item in rateMaster.Items)
                {
                    item.RateMasterId = rateMaster.Id;
                    _context.RateMasterItems.Add(item);
                }
                await _context.SaveChangesAsync();
            }

            return CreatedAtAction(nameof(GetRateMaster), new { id = rateMaster.Id }, rateMaster);
        }

        // PUT: api/RateMaster/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRateMaster([FromRoute] int id, [FromBody] RateMaster rateMaster)
        {
            if (id != rateMaster.Id)
            {
                return BadRequest();
            }

            rateMaster.DateUpdated = DateTime.UtcNow;
            _context.Entry(rateMaster).State = EntityState.Modified;

            // Remove existing items
            var existingItems = await _context.RateMasterItems
                .Where(item => item.RateMasterId == id)
                .ToListAsync();
            _context.RateMasterItems.RemoveRange(existingItems);

            // Add new items
            if (rateMaster.Items != null && rateMaster.Items.Any())
            {
                foreach (var item in rateMaster.Items)
                {
                    item.RateMasterId = id;
                    _context.RateMasterItems.Add(item);
                }
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.RateMasters.Any(e => e.Id == id))
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

        // DELETE: api/RateMaster/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRateMaster(int id)
        {
            var rateMaster = await _context.RateMasters.FindAsync(id);
            if (rateMaster == null)
            {
                return NotFound();
            }

            // Items will be deleted automatically due to CASCADE constraint
            _context.RateMasters.Remove(rateMaster);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private async Task<string> GenerateRateMasterId()
        {
            var currentDate = DateTime.UtcNow;
            var currentYear = currentDate.Year;
            var nextYear = currentYear + 1;
            var yearSuffix = $"{currentYear.ToString().Substring(2)}-{nextYear.ToString().Substring(2)}";
            var prefix = $"RM/{yearSuffix}/";
            
            // Get all rate master IDs for current financial year that match the pattern
            var existingIds = await _context.RateMasters
                .Where(rm => rm.RateMasterId != null && rm.RateMasterId.StartsWith(prefix))
                .Select(rm => rm.RateMasterId)
                .ToListAsync();
            
            // Extract sequence numbers and find the maximum
            var maxSequence = 0;
            foreach (var id in existingIds)
            {
                if (id.Length >= prefix.Length + 3)
                {
                    var sequencePart = id.Substring(prefix.Length);
                    if (int.TryParse(sequencePart, out var sequence))
                    {
                        if (sequence > maxSequence)
                            maxSequence = sequence;
                    }
                }
            }
            
            var nextSequence = maxSequence + 1;
            var sequenceNumber = nextSequence.ToString("D3"); // This will format as 001, 002, 003, etc.
            
            return $"RM/{yearSuffix}/{sequenceNumber}";
        }
    }
}