using ERP.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccessoriesHeaderController : ControllerBase
    {
        private readonly AppDbContext _context;
        public AccessoriesHeaderController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AccessoriesHeader>>> GetAll()
        {
            return await _context.AccessoriesHeaders.Include(x => x.AccessoriesDetails).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AccessoriesHeader>> Get(int id)
        {
            var header = await _context.AccessoriesHeaders.Include(x => x.AccessoriesDetails).FirstOrDefaultAsync(x => x.Id == id);
            if (header == null) return NotFound();
            return header;
        }

        [HttpPost]
        public async Task<ActionResult<AccessoriesHeader>> Create([FromBody] AccessoriesHeaderRequest request)
        {
            // Auto-generate accesoryId in format acc-26/26-XXX
            var last = await _context.AccessoriesHeaders.OrderByDescending(x => x.Id).FirstOrDefaultAsync();
            int nextNum = (last?.Id ?? 0) + 1;
            string generatedId = $"acc-26/26-{nextNum:D3}";

            var header = new AccessoriesHeader
            {
                AccesoryId = generatedId,
                Date = request.AccessoriesHeader.Date?.ToUniversalTime(),
                ItemId = request.AccessoriesHeader.ItemId,
                ItemDescription = request.AccessoriesHeader.ItemDescription,
                CreatedAt = DateTime.UtcNow,
                AccessoriesDetails = request.AccessoriesHeader.AccessoriesDetails?.Select(d => new AccessoriesDetails
                {
                    AccessoriesName = d.Name,
                    ItemType = d.Type,
                    Qty = d.Qty
                }).ToList()
            };

            _context.AccessoriesHeaders.Add(header);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = header.Id }, header);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] AccessoriesHeaderRequest request)
        {
            var header = await _context.AccessoriesHeaders.Include(x => x.AccessoriesDetails).FirstOrDefaultAsync(x => x.Id == id);
            if (header == null) return NotFound();
            var dto = request.AccessoriesHeader;
            header.Date = dto.Date?.ToUniversalTime();
            header.ItemId = dto.ItemId;
            header.ItemDescription = dto.ItemDescription;
            // Update details: for simplicity, clear and re-add
            header.AccessoriesDetails.Clear();
            if (dto.AccessoriesDetails != null)
            {
                foreach (var d in dto.AccessoriesDetails)
                {
                    header.AccessoriesDetails.Add(new AccessoriesDetails
                    {
                        AccessoriesName = d.Name,
                        ItemType = d.Type,
                        Qty = d.Qty
                    });
                }
            }
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var header = await _context.AccessoriesHeaders.FindAsync(id);
            if (header == null) return NotFound();
            _context.AccessoriesHeaders.Remove(header);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
