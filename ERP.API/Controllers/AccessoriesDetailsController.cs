using ERP.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccessoriesDetailsController : ControllerBase
    {
        private readonly AppDbContext _context;
        public AccessoriesDetailsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AccessoriesDetails>>> GetAll()
        {
            return await _context.AccessoriesDetails.Include(x => x.AccessoriesHeader).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AccessoriesDetails>> Get(int id)
        {
            var detail = await _context.AccessoriesDetails.Include(x => x.AccessoriesHeader).FirstOrDefaultAsync(x => x.Id == id);
            if (detail == null) return NotFound();
            return detail;
        }


        [HttpPost]
        public async Task<ActionResult> Create([FromBody] AccessoriesHeaderRequest request)
        {
            var dto = request.AccessoriesHeader;
            if (dto?.AccessoriesDetails == null || dto.AccessoriesDetails.Count == 0)
                return BadRequest("No accessories details provided.");

            // Auto-generate accesoryId in format acc-26/26-XXX
            var last = await _context.AccessoriesHeaders.OrderByDescending(x => x.Id).FirstOrDefaultAsync();
            int nextNum = (last?.Id ?? 0) + 1;
            string generatedId = $"acc-26/26-{nextNum:D3}";

            var header = new AccessoriesHeader
            {
                AccesoryId = generatedId,
                Date = dto.Date?.ToUniversalTime(),
                ItemId = dto.ItemId,
                ItemDescription = dto.ItemDescription,
                CreatedAt = DateTime.UtcNow,
                AccessoriesDetails = dto.AccessoriesDetails.Select(d => new AccessoriesDetails
                {
                    AccessoriesName = d.Name,
                    ItemType = d.Type,
                    Qty = d.Qty
                }).ToList()
            };
            _context.AccessoriesHeaders.Add(header);
            await _context.SaveChangesAsync();
            return CreatedAtAction("Get", new { id = header.Id }, header);
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
            var detail = await _context.AccessoriesDetails.FindAsync(id);
            if (detail == null) return NotFound();
            _context.AccessoriesDetails.Remove(detail);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
