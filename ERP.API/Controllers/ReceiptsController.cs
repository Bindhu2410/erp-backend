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
    public class ReceiptsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReceiptsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Receipts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetReceipts()
        {
            var receipts = await _context.Receipts
                .Include(r => r.Items)
                .Include(r => r.OptionalItems)
                .Include(r => r.Accessories)
                .OrderByDescending(r => r.Id)
                .ToListAsync();
            
            var result = new List<object>();
            foreach (var receipt in receipts)
            {
                var bomDetails = await GetBomDetailsForReceipt(receipt.BomIds);

                // Fetch names for IDs
                string locationName = "";
                if (!string.IsNullOrEmpty(receipt.LocationId) && int.TryParse(receipt.LocationId, out int locId))
                {
                    var loc = await _context.Warehouses.FindAsync(locId);
                    locationName = loc?.WarehouseName ?? "";
                }

                string customerMappingName = "";
                if (!string.IsNullOrEmpty(receipt.ReceivedFrom) && int.TryParse(receipt.ReceivedFrom, out int custId))
                {
                    var cust = await _context.SalesCustomers.FindAsync(custId);
                    customerMappingName = cust?.Name ?? "";
                }

                string salesRepName = "";
                if (!string.IsNullOrEmpty(receipt.SalesRepresentative) && int.TryParse(receipt.SalesRepresentative, out int repId))
                {
                    var rep = await _context.SalesEmployees.FindAsync(repId);
                    if (rep != null) salesRepName = $"{rep.FirstName} {rep.LastName}".Trim();
                }

                result.Add(new { 
                    Receipt = receipt, 
                    BomDetails = bomDetails,
                    LocationName = locationName,
                    CustomerName = customerMappingName,
                    SalesRepresentativeName = salesRepName
                });
            }
            return Ok(result);
        }

        // GET: api/Receipts/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetReceipt(int id)
        {
            var receipt = await _context.Receipts
                .Include(r => r.Items)
                .Include(r => r.OptionalItems)
                .Include(r => r.Accessories)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (receipt == null)
            {
                return NotFound();
            }
            var bomDetails = await GetBomDetailsForReceipt(receipt.BomIds);
            return Ok(new { Receipt = receipt, BomDetails = bomDetails });
        }

        // POST: api/Receipts
        [HttpPost]
        public async Task<ActionResult<Receipt>> PostReceipt([FromBody]Receipt receipt)
        {
            // Generate doc_id in the format RC/25-26/01
            var currentYear = System.DateTime.Now.Year;
            var nextYear = currentYear + 1;
            string yearPart = $"{currentYear % 100:D2}-{nextYear % 100:D2}";
            int count = await _context.Receipts.CountAsync(r => r.DocId != null && r.DocId.Contains($"RC/{yearPart}/"));
            int nextSeq = count + 1;
            string seqPart = nextSeq.ToString("D2");
            receipt.DocId = $"RC/{yearPart}/{seqPart}";

            receipt.DateCreated = DateTime.UtcNow;

            // Ensure all DateTime fields are UTC
            if (receipt.ReceiptDate.HasValue && receipt.ReceiptDate.Value.Kind != DateTimeKind.Utc)
                receipt.ReceiptDate = DateTime.SpecifyKind(receipt.ReceiptDate.Value, DateTimeKind.Utc);
            
            if (receipt.DocDate.HasValue && receipt.DocDate.Value.Kind != DateTimeKind.Utc)
                receipt.DocDate = DateTime.SpecifyKind(receipt.DocDate.Value, DateTimeKind.Utc);

            if (receipt.RefDate.HasValue && receipt.RefDate.Value.Kind != DateTimeKind.Utc)
                receipt.RefDate = DateTime.SpecifyKind(receipt.RefDate.Value, DateTimeKind.Utc);

            _context.Receipts.Add(receipt);
            await _context.SaveChangesAsync();

            // If IssueId is provided, update the related Issue's ReceiptId field
            if (!string.IsNullOrEmpty(receipt.IssueId) && int.TryParse(receipt.IssueId, out int issueId))
            {
                var issue = await _context.Issues.FirstOrDefaultAsync(i => i.Id == issueId);
                if (issue != null)
                {
                    issue.ReceiptId = receipt.DocId;
                    _context.Entry(issue).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                }
            }

            var bomDetails = await GetBomDetailsForReceipt(receipt.BomIds);
            return CreatedAtAction(nameof(GetReceipt), new { id = receipt.Id }, new { Receipt = receipt, BomDetails = bomDetails });
        }

        // PUT: api/Receipts/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutReceipt([FromRoute]int id, [FromBody]Receipt receipt)
        {
            if (id != receipt.Id)
            {
                return BadRequest();
            }

            var existingReceipt = await _context.Receipts
                .Include(r => r.Items)
                .Include(r => r.OptionalItems)
                .Include(r => r.Accessories)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (existingReceipt == null)
            {
                return NotFound();
            }

            // Ensure all DateTime fields are UTC before setting values
            if (receipt.DocDate.HasValue) receipt.DocDate = DateTime.SpecifyKind(receipt.DocDate.Value, DateTimeKind.Utc);
            if (receipt.RefDate.HasValue) receipt.RefDate = DateTime.SpecifyKind(receipt.RefDate.Value, DateTimeKind.Utc);
            if (receipt.ReceiptDate.HasValue) receipt.ReceiptDate = DateTime.SpecifyKind(receipt.ReceiptDate.Value, DateTimeKind.Utc);

            // Update header fields
            _context.Entry(existingReceipt).CurrentValues.SetValues(receipt);
            existingReceipt.DateUpdated = DateTime.UtcNow;

            // Sync Items
            var existingItems = _context.ReceiptItems.Where(i => i.ReceiptId == id);
            _context.ReceiptItems.RemoveRange(existingItems);
            if (receipt.Items != null)
            {
                foreach (var item in receipt.Items)
                {
                    item.Id = 0;
                    item.ReceiptId = id;
                    _context.ReceiptItems.Add(item);
                }
            }

            // Sync OptionalItems
            var existingOptItems = _context.ReceiptOptionalItems.Where(i => i.ReceiptId == id);
            _context.ReceiptOptionalItems.RemoveRange(existingOptItems);
            if (receipt.OptionalItems != null)
            {
                foreach (var item in receipt.OptionalItems)
                {
                    item.Id = 0;
                    item.ReceiptId = id;
                    _context.ReceiptOptionalItems.Add(item);
                }
            }

            // Sync Accessories
            var existingAccessories = _context.ReceiptAccessories.Where(i => i.ReceiptId == id);
            _context.ReceiptAccessories.RemoveRange(existingAccessories);
            if (receipt.Accessories != null)
            {
                foreach (var item in receipt.Accessories)
                {
                    item.Id = 0;
                    item.ReceiptId = id;
                    _context.ReceiptAccessories.Add(item);
                }
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Receipts.Any(e => e.Id == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            var bomDetails = await GetBomDetailsForReceipt(receipt.BomIds);
            return Ok(new { Receipt = existingReceipt, BomDetails = bomDetails });
        }

        // DELETE: api/Receipts/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReceipt(int id)
        {
            var receipt = await _context.Receipts
                .Include(r => r.Items)
                .Include(r => r.OptionalItems)
                .Include(r => r.Accessories)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (receipt == null)
            {
                return NotFound();
            }

            if (receipt.Items != null) _context.ReceiptItems.RemoveRange(receipt.Items);
            if (receipt.OptionalItems != null) _context.ReceiptOptionalItems.RemoveRange(receipt.OptionalItems);
            if (receipt.Accessories != null) _context.ReceiptAccessories.RemoveRange(receipt.Accessories);

            _context.Receipts.Remove(receipt);
            await _context.SaveChangesAsync();
            return NoContent();
        }


        // Helper method to fetch BOM details for a receipt (same as Issue API)
        private async Task<List<object>> GetBomDetailsForReceipt(string[] bomIds)
        {
            var result = new List<object>();
            if (bomIds == null || bomIds.Length == 0) return result;
            foreach (var bomId in bomIds)
            {
                if (string.IsNullOrEmpty(bomId)) continue;
                var bom = await _context.BillOfMaterials
                    .Include(b => b.ChildItems)
                    .FirstOrDefaultAsync(b => b.BomId == bomId);
                if (bom == null) continue;
                var childItems = new List<object>();
                foreach (var child in bom.ChildItems)
                {
                    var item = await _context.ItemMasters.FirstOrDefaultAsync(i => i.Id == child.ChildItemId);
                    if (item == null) continue;
                    var category = item.CategoryId.HasValue
                        ? await _context.Categories.FirstOrDefaultAsync(c => c.Id == item.CategoryId.Value)
                        : null;
                    var valuationMethod = item.ValuationMethodId.HasValue
                        ? await _context.ValuationMethods.FirstOrDefaultAsync(v => v.Id == item.ValuationMethodId.Value)
                        : null;
                    var inventoryMethod = item.InventoryMethodId.HasValue
                        ? await _context.InventoryMethods.FirstOrDefaultAsync(im => im.Id == item.InventoryMethodId.Value)
                        : null;
                    var uom = item.UomId.HasValue
                        ? await _context.Uoms.FirstOrDefaultAsync(u => u.Id == item.UomId.Value)
                        : null;
                    childItems.Add(new {
                        child.ChildItemId,
                        child.Quantity,
                        Make = item.Make,
                        Model = item.Model,
                        Product = item.Product,
                        CategoryName = category?.Name,
                        ValuationMethodName = valuationMethod?.Name,
                        InventoryMethodName = inventoryMethod?.Name,
                        UnitPrice = item.UnitPrice,
                        ItemName = item.ItemName,
                        ItemCode = item.ItemCode,
                        CatNo = item.CatNo,
                        UomName = uom?.Code,
                        Hsn = item.Hsn,
                        Tax = item.TaxPercentage
                    });
                }
                result.Add(new
                {
                    bom.Id,
                    bom.BomId,
                    bom.BomName,
                    bom.BomType,
                    ChildItems = childItems
                });
            }
            return result;
        }
    }
}
