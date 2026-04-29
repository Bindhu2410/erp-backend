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
    public class BillOfMaterialOptionalItemsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BillOfMaterialOptionalItemsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetAll([FromQuery] int? billOfMaterialId)
        {
            var query = _context.BillOfMaterialOptionalItems
                .Include(x => x.OptionalItem)
                .Include(x => x.BillOfMaterial)
                .AsQueryable();

            if (billOfMaterialId.HasValue)
                query = query.Where(x => x.BillOfMaterialId == billOfMaterialId.Value);

            var items = await query
                .Select(x => new
                {
                    x.Id,
                    x.BillOfMaterialId,
                    x.OptionalItemId,
                    OptionalItemName = x.OptionalItem != null ? x.OptionalItem.ItemName : null,
                    x.Quantity,
                    x.Amount,
                    x.Remarks
                })
                .ToListAsync();

            return Ok(items);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<object>> Get(int id)
        {
            var item = await _context.BillOfMaterialOptionalItems
                .Include(x => x.OptionalItem)
                .Include(x => x.BillOfMaterial)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (item == null) return NotFound();

            return Ok(new
            {
                item.Id,
                item.BillOfMaterialId,
                item.OptionalItemId,
                OptionalItemName = item.OptionalItem?.ItemName,
                item.Quantity,
                item.Amount,
                item.Remarks
            });
        }

        [HttpPost]
        public async Task<ActionResult<object>> Create([FromBody] BillOfMaterialOptionalItemRequestDto dto)
        {
            if (dto == null)
                return BadRequest("Request body is required.");

            if (dto.BillOfMaterialId <= 0)
                return BadRequest("BillOfMaterialId is required and must be greater than 0.");

            if (dto.OptionalItemId <= 0)
                return BadRequest("OptionalItemId is required and must be greater than 0.");

            var bomExists = await _context.BillOfMaterials.AnyAsync(b => b.Id == dto.BillOfMaterialId);
            if (!bomExists)
                return BadRequest($"Bill of material id {dto.BillOfMaterialId} does not exist.");

            var itemExists = await _context.ItemMasters.AnyAsync(i => i.Id == dto.OptionalItemId);
            if (!itemExists)
                return BadRequest($"Optional item id {dto.OptionalItemId} does not exist.");

            var model = new BillOfMaterialOptionalItem
            {
                BillOfMaterialId = dto.BillOfMaterialId,
                OptionalItemId = dto.OptionalItemId,
                Quantity = dto.Quantity <= 0 ? 1 : dto.Quantity,
                Amount = dto.Amount,
                Remarks = dto.Remarks
            };

            _context.BillOfMaterialOptionalItems.Add(model);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = model.Id }, new
            {
                model.Id,
                model.BillOfMaterialId,
                model.OptionalItemId,
                model.Quantity,
                model.Amount,
                model.Remarks
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] BillOfMaterialOptionalItemRequestDto dto)
        {
            if (dto == null)
                return BadRequest("Request body is required.");

            var existing = await _context.BillOfMaterialOptionalItems.FindAsync(id);
            if (existing == null)
                return NotFound();

            if (dto.BillOfMaterialId > 0)
            {
                var bomExists = await _context.BillOfMaterials.AnyAsync(b => b.Id == dto.BillOfMaterialId);
                if (!bomExists)
                    return BadRequest($"Bill of material id {dto.BillOfMaterialId} does not exist.");
                existing.BillOfMaterialId = dto.BillOfMaterialId;
            }

            if (dto.OptionalItemId > 0)
            {
                var itemExists = await _context.ItemMasters.AnyAsync(i => i.Id == dto.OptionalItemId);
                if (!itemExists)
                    return BadRequest($"Optional item id {dto.OptionalItemId} does not exist.");
                existing.OptionalItemId = dto.OptionalItemId;
            }

            existing.Quantity = dto.Quantity <= 0 ? existing.Quantity : dto.Quantity;
            existing.Amount = dto.Amount;
            existing.Remarks = dto.Remarks;

            _context.BillOfMaterialOptionalItems.Update(existing);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                existing.Id,
                existing.BillOfMaterialId,
                existing.OptionalItemId,
                existing.Quantity,
                existing.Amount,
                existing.Remarks
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _context.BillOfMaterialOptionalItems.FindAsync(id);
            if (existing == null)
                return NotFound();

            _context.BillOfMaterialOptionalItems.Remove(existing);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
