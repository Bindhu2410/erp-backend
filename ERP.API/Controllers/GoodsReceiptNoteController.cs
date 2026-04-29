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
    public class GoodsReceiptNoteController : ControllerBase
    {
        private readonly AppDbContext _context;

        public GoodsReceiptNoteController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/GoodsReceiptNote
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GoodsReceiptNote>>> GetGoodsReceiptNotes()
        {
            var grns = await _context.GoodsReceiptNotes
                .Include(g => g.Items)
                .ToListAsync();
            var enriched = new List<object>();
            foreach (var grn in grns)
            {
                var itemDetails = new List<ItemMasterResponse?>();
                foreach (var item in grn.Items)
                {
                    itemDetails.Add(await GetItemDetails(item.ItemId));
                }
                enriched.Add(new { grn, itemDetails });
            }
            return Ok(enriched);
        }

        // GET: api/GoodsReceiptNote/5
        [HttpGet("{id}")]
        public async Task<ActionResult<GoodsReceiptNote>> GetGoodsReceiptNote(int id)
        {
            var grn = await _context.GoodsReceiptNotes
                .Include(g => g.Items)
                .FirstOrDefaultAsync(g => g.Id == id);
            if (grn == null)
            {
                return NotFound();
            }
            var itemDetails = new List<ItemMasterResponse?>();
            foreach (var item in grn.Items)
            {
                itemDetails.Add(await GetItemDetails(item.ItemId));
            }
            return Ok(new { grn, itemDetails });
        }

        // POST: api/GoodsReceiptNote
        [HttpPost]
        public async Task<ActionResult<GoodsReceiptNote>> PostGoodsReceiptNote([FromBody] GoodsReceiptNote grn)
        {
            // Auto-generate GRN number
            var fyStart = grn.GrnDate.Month >= 4 ? grn.GrnDate.Year : grn.GrnDate.Year - 1;
            var fyEnd = (fyStart + 1) % 100;
            var fyStr = $"{fyStart % 100:D2}-{fyEnd:D2}";
            var count = await _context.GoodsReceiptNotes.CountAsync(g =>
                (g.GrnDate.Month >= 4 ? g.GrnDate.Year : g.GrnDate.Year - 1) == fyStart);
            var sequence = count + 1;
            grn.GrnNo = $"GRN/{fyStr}/{sequence:D2}";

            // Add GRN and its items
            if (grn.Items != null)
            {
                foreach (var item in grn.Items)
                {
                    item.QcPassed = item.QcPassed; // ensure field is set
                }
            }
            _context.GoodsReceiptNotes.Add(grn);
            await _context.SaveChangesAsync();
            var itemDetails = new List<ItemMasterResponse?>();
            foreach (var item in grn.Items)
            {
                itemDetails.Add(await GetItemDetails(item.ItemId));
            }
            return CreatedAtAction(nameof(GetGoodsReceiptNote), new { id = grn.Id }, new { grn, itemDetails });
        }

        // PUT: api/GoodsReceiptNote/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutGoodsReceiptNote([FromRoute] int id, [FromBody] GoodsReceiptNote grn)
        {
            if (id != grn.Id)
            {
                return BadRequest();
            }

            // Update GRN and its items
            var existingGrn = await _context.GoodsReceiptNotes
                .Include(g => g.Items)
                .FirstOrDefaultAsync(g => g.Id == id);
            if (existingGrn == null)
            {
                return NotFound();
            }

            // Update GRN fields
            _context.Entry(existingGrn).CurrentValues.SetValues(grn);

            // Update items
            if (grn.Items != null)
            {
                // Remove deleted items
                foreach (var existingItem in existingGrn.Items.ToList())
                {
                    if (!grn.Items.Any(i => i.Id == existingItem.Id))
                        _context.GoodsReceiptNoteItems.Remove(existingItem);
                }
                // Add or update items
                foreach (var item in grn.Items)
                {
                    var existingItem = existingGrn.Items.FirstOrDefault(i => i.Id == item.Id);
                    if (existingItem != null)
                    {
                        _context.Entry(existingItem).CurrentValues.SetValues(item);
                    }
                    else
                    {
                        existingGrn.Items.Add(item);
                    }
                }
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.GoodsReceiptNotes.Any(e => e.Id == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            var itemDetails = new List<ItemMasterResponse?>();
            foreach (var item in existingGrn.Items)
            {
                itemDetails.Add(await GetItemDetails(item.ItemId));
            }
            return Ok(new { grn = existingGrn, itemDetails });
        }

        // Helper to get item details for a given itemId
        private async Task<ItemMasterResponse?> GetItemDetails(int itemId)
        {
            var item = await _context.ItemMasters.FindAsync(itemId);
            if (item == null) return null;
            var group = await _context.InventoryGroups.FirstOrDefaultAsync(g => g.Id == (item.GroupId == -1 ? 1 : item.GroupId));
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == (item.CategoryId == -1 ? 1 : item.CategoryId));
            var uom = await _context.Uoms.FirstOrDefaultAsync(u => u.Id == (item.UomId == -1 ? 1 : item.UomId));
            var inventoryMethod = await _context.InventoryMethods.FirstOrDefaultAsync(im => im.Id == (item.InventoryMethodId == -1 ? 1 : item.InventoryMethodId));
            var valuationMethod = await _context.ValuationMethods.FirstOrDefaultAsync(vm => vm.Id == (item.ValuationMethodId == -1 ? 1 : item.ValuationMethodId));
            var rateItem = await (from rmi in _context.RateMasterItems
                                 join rm in _context.RateMasters on rmi.RateMasterId equals rm.Id
                                 where rmi.ItemId == itemId
                                 select new { rmi, rm }
                                ).FirstOrDefaultAsync();
            return new ItemMasterResponse
            {
                Id = item.Id,
                UserCreated = item.UserCreated,
                DateCreated = item.DateCreated,
                UserUpdated = item.UserUpdated,
                DateUpdated = item.DateUpdated,
                GroupId = item.GroupId,
                GroupName = group?.Name,
                CategoryId = item.CategoryId,
                CategoryName = category?.Name,
                Make = item.Make,
                Model = item.Model,
                Product = item.Product,
                Brand = item.Brand,
                ItemName = item.ItemName,
                ItemCode = item.ItemCode,
                IsActive = item.IsActive,
                ImageUrl = item.ImageUrl,
                UnitPrice = item.UnitPrice,
                UomId = item.UomId,
                UomName = uom?.Code,
                CatNo = item.CatNo,
                InventoryMethodId = item.InventoryMethodId,
                InventoryMethodName = inventoryMethod?.Name,
                Hsn = item.Hsn,
                TaxPercentage = item.TaxPercentage,
                ValuationMethodId = item.ValuationMethodId,
                ValuationMethodName = valuationMethod?.Name,
                PurchaseRate = rateItem?.rmi.PurchaseRate,
                SaleRate = rateItem?.rmi.SalesRate,
                QuoteRate = rateItem?.rmi.QuotationRate,
                HsnCode = rateItem?.rmi.HsnCode != null ? int.Parse(rateItem.rmi.HsnCode) : (int?)null,
                TaxPercent = rateItem?.rmi.Tax != null ? (int?)rateItem.rmi.Tax : null
            };
        }

        // DELETE: api/GoodsReceiptNote/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGoodsReceiptNote(int id)
        {
            var grn = await _context.GoodsReceiptNotes
                .Include(g => g.Items)
                .FirstOrDefaultAsync(g => g.Id == id);
            if (grn == null)
            {
                return NotFound();
            }
            _context.GoodsReceiptNotes.Remove(grn);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
