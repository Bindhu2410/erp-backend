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
    public class IssuesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public IssuesController(AppDbContext context)
        {
            _context = context;
        }


        // PUT: api/Issues/{issueId}/receipt
        [HttpPut("{issueId}/receipt")]
        public async Task<IActionResult> UpdateIssueReceiptIdAsync([FromRoute] int issueId, [FromBody] string docId)
        {
            var issue = await _context.Issues.FindAsync(issueId);
            if (issue == null)
                return NotFound();
            issue.ReceiptId = docId;
            _context.Entry(issue).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok(true);
        }


        // GET: api/Issues
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetIssues()
        {
            var issues = await _context.Issues
                .OrderByDescending(i => i.Id)
                .Include(i => i.OptionalItems)
                .Include(i => i.IssueItems)
                .ToListAsync();
            var result = new List<object>();
            foreach (var issue in issues)
            {
                var bomDetails = await GetBomDetailsForIssue(issue.BomIds);
                string partyName = string.Empty;
                if (!string.IsNullOrEmpty(issue.CustomerName) && int.TryParse(issue.CustomerName, out int leadId))
                {
                    partyName = await _context.SalesLeads
                        .Where(l => l.Id == leadId)
                        .Select(l => l.CustomerName)
                        .FirstOrDefaultAsync() ?? issue.CustomerName;
                }
                else
                {
                    partyName = issue.CustomerName ?? string.Empty;
                }
                result.Add(new { Issue = issue, PartyName = partyName, BomDetails = bomDetails });
            }
            return Ok(result);
        }

        // GET: api/Issues/5
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetIssue(int id)
        {
            var issue = await _context.Issues
                .Include(i => i.OptionalItems)
                .Include(i => i.IssueItems)
                .FirstOrDefaultAsync(i => i.Id == id);
            if (issue == null)
            {
                return NotFound();
            }

            var bomDetails = await GetBomDetailsForIssue(issue.BomIds);
            string partyName = string.Empty;
            if (!string.IsNullOrEmpty(issue.CustomerName) && int.TryParse(issue.CustomerName, out int leadId))
            {
                partyName = await _context.SalesLeads
                    .Where(l => l.Id == leadId)
                    .Select(l => l.CustomerName)
                    .FirstOrDefaultAsync() ?? issue.CustomerName;
            }
            else
            {
                partyName = issue.CustomerName ?? string.Empty;
            }
            return Ok(new { Issue = issue, PartyName = partyName, BomDetails = bomDetails });
        }

        // POST: api/Issues
        [HttpPost]
        public async Task<ActionResult<Issue>> PostIssue([FromBody] Issue issue)
        {
            // Generate doc_id in the format IS/25-26/01
            var currentYear = System.DateTime.Now.Year;
            var nextYear = currentYear + 1;
            string yearPart = $"{currentYear % 100:D2}-{nextYear % 100:D2}";

            // Get the count of issues for this year to generate the sequence
            int count = await _context.Issues.CountAsync(i => i.DocId != null && i.DocId.Contains($"IS/{yearPart}/"));
            int nextSeq = count + 1;
            string seqPart = nextSeq.ToString("D2");
            issue.DocId = $"IS/{yearPart}/{seqPart}";

            issue.DateCreated = System.DateTime.Now;
            _context.Issues.Add(issue);
            await _context.SaveChangesAsync();

            var bomDetails = await GetBomDetailsForIssue(issue.BomIds);
            return CreatedAtAction(nameof(GetIssue), new { id = issue.Id }, new { Issue = issue, BomDetails = bomDetails });
        }

        // PUT: api/Issues/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutIssue([FromRoute] int id, [FromBody] Issue issue)
        {
            if (id != issue.Id)
            {
                return BadRequest();
            }

            var existingIssue = await _context.Issues
                .Include(i => i.OptionalItems)
                .Include(i => i.IssueItems)
                .FirstOrDefaultAsync(i => i.Id == id);
            
            if (existingIssue == null)
            {
                return NotFound();
            }

            // Update header and footer fields
            _context.Entry(existingIssue).CurrentValues.SetValues(issue);
            existingIssue.DateUpdated = System.DateTime.Now;

            // Update OptionalItems
            var existingOptionalItems = await _context.IssueOptionalItems.Where(oi => oi.IssueId == id).ToListAsync();
            _context.IssueOptionalItems.RemoveRange(existingOptionalItems);
            
            if (issue.OptionalItems != null)
            {
                foreach (var item in issue.OptionalItems)
                {
                    item.Id = 0;
                    item.IssueId = id;
                    _context.IssueOptionalItems.Add(item);
                }
            }

            // Update IssueItems (The detailed grid items)
            var existingItems = await _context.IssueItems.Where(ii => ii.IssueId == id).ToListAsync();
            _context.IssueItems.RemoveRange(existingItems);

            if (issue.IssueItems != null)
            {
                foreach (var item in issue.IssueItems)
                {
                    item.Id = 0;
                    item.IssueId = id;
                    _context.IssueItems.Add(item);
                }
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Issues.Any(e => e.Id == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            var bomDetails = await GetBomDetailsForIssue(issue.BomIds);
            return Ok(new { Issue = existingIssue, BomDetails = bomDetails });
        }

        // Helper method to fetch BOM details for an issue
    private async Task<List<object>> GetBomDetailsForIssue(string[] bomIds)
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

        // DELETE: api/Issues/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteIssue(int id)
        {
            var issue = await _context.Issues
                .Include(i => i.OptionalItems)
                .Include(i => i.IssueItems)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (issue == null)
            {
                return NotFound();
            }

            // Remove children first to avoid FK constraint violations
            if (issue.OptionalItems != null)
                _context.IssueOptionalItems.RemoveRange(issue.OptionalItems);
            if (issue.IssueItems != null)
                _context.IssueItems.RemoveRange(issue.IssueItems);

            _context.Issues.Remove(issue);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}

