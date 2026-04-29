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
    public class ClaimVouchersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ClaimVouchersController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/ClaimVouchers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ClaimVoucher>>> GetClaimVouchers()
        {
            var list = await _context.ClaimVouchers
                .Include(cv => cv.Items)
                .ToListAsync();
            return Ok(list);
        }

        // GET: api/ClaimVouchers/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetClaimVoucher(int id)
        {
            var cv = await _context.ClaimVouchers
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (cv == null) return NotFound();
            return Ok(cv);
        }

        // POST: api/ClaimVouchers
        [HttpPost]
        public async Task<ActionResult<ClaimVoucher>> PostClaimVoucher([FromBody] ClaimVoucher claimVoucher)
        {
            // Generate doc_id in the format ECV-001 (3 digit sequential)
            int count = await _context.ClaimVouchers.CountAsync(c => c.DocId != null && c.DocId.StartsWith("ECV-"));
            int nextSeq = count + 1;
            string seqPart = nextSeq.ToString("D3");
            claimVoucher.DocId = $"ECV-{seqPart}";

            // Ensure DateTime fields are UTC
            if (claimVoucher.DateCreated.HasValue)
            {
                if (claimVoucher.DateCreated.Value.Kind == DateTimeKind.Unspecified)
                    claimVoucher.DateCreated = DateTime.SpecifyKind(claimVoucher.DateCreated.Value, DateTimeKind.Utc);
                else if (claimVoucher.DateCreated.Value.Kind == DateTimeKind.Local)
                    claimVoucher.DateCreated = claimVoucher.DateCreated.Value.ToUniversalTime();
            }
            if (claimVoucher.DateUpdated.HasValue)
            {
                if (claimVoucher.DateUpdated.Value.Kind == DateTimeKind.Unspecified)
                    claimVoucher.DateUpdated = DateTime.SpecifyKind(claimVoucher.DateUpdated.Value, DateTimeKind.Utc);
                else if (claimVoucher.DateUpdated.Value.Kind == DateTimeKind.Local)
                    claimVoucher.DateUpdated = claimVoucher.DateUpdated.Value.ToUniversalTime();
            }
            if (claimVoucher.Date.HasValue)
            {
                if (claimVoucher.Date.Value.Kind == DateTimeKind.Unspecified)
                    claimVoucher.Date = DateTime.SpecifyKind(claimVoucher.Date.Value, DateTimeKind.Utc);
                else if (claimVoucher.Date.Value.Kind == DateTimeKind.Local)
                    claimVoucher.Date = claimVoucher.Date.Value.ToUniversalTime();
            }
            if (claimVoucher.FromDate.HasValue)
            {
                if (claimVoucher.FromDate.Value.Kind == DateTimeKind.Unspecified)
                    claimVoucher.FromDate = DateTime.SpecifyKind(claimVoucher.FromDate.Value, DateTimeKind.Utc);
                else if (claimVoucher.FromDate.Value.Kind == DateTimeKind.Local)
                    claimVoucher.FromDate = claimVoucher.FromDate.Value.ToUniversalTime();
            }
            if (claimVoucher.ToDate.HasValue)
            {
                if (claimVoucher.ToDate.Value.Kind == DateTimeKind.Unspecified)
                    claimVoucher.ToDate = DateTime.SpecifyKind(claimVoucher.ToDate.Value, DateTimeKind.Utc);
                else if (claimVoucher.ToDate.Value.Kind == DateTimeKind.Local)
                    claimVoucher.ToDate = claimVoucher.ToDate.Value.ToUniversalTime();
            }

            // If items are provided, compute total amount
            if (claimVoucher.Items != null && claimVoucher.Items.Count > 0)
            {
                claimVoucher.TotalAmount = claimVoucher.Items.Where(i => i.Amount.HasValue).Sum(i => i.Amount.Value);
            }

            _context.ClaimVouchers.Add(claimVoucher);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetClaimVoucher), new { id = claimVoucher.Id }, claimVoucher);
        }

        // PUT: api/ClaimVouchers/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutClaimVoucher([FromRoute] int id, [FromBody] ClaimVoucher claimVoucher)
        {
            if (id != claimVoucher.Id) return BadRequest();
            // Load existing voucher and update fields + items
            var existing = await _context.ClaimVouchers.Include(c => c.Items).FirstOrDefaultAsync(c => c.Id == id);
            if (existing == null) return NotFound();

            // update scalar properties (excluding removed per-item fields)
            existing.Status = claimVoucher.Status;
            existing.Date = claimVoucher.Date;
            existing.FromDate = claimVoucher.FromDate;
            existing.ToDate = claimVoucher.ToDate;
            existing.UserUpdated = claimVoucher.UserUpdated;
            existing.DateUpdated = claimVoucher.DateUpdated;

            // Replace items if provided
            if (claimVoucher.Items != null)
            {
                // Remove existing items
                if (existing.Items != null && existing.Items.Count > 0)
                {
                    _context.ClaimVoucherItems.RemoveRange(existing.Items);
                }

                // Attach new items
                existing.Items = claimVoucher.Items.Select(i =>
                {
                    i.ClaimVoucherId = existing.Id; // ensure FK is set
                    return i;
                }).ToList();

                // Recompute total
                existing.TotalAmount = existing.Items.Where(i => i.Amount.HasValue).Sum(i => i.Amount.Value);
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.ClaimVouchers.Any(e => e.Id == id)) return NotFound();
                else throw;
            }
            return Ok(existing);
        }

        // DELETE: api/ClaimVouchers/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClaimVoucher(int id)
        {
            var cv = await _context.ClaimVouchers.FindAsync(id);
            if (cv == null) return NotFound();
            _context.ClaimVouchers.Remove(cv);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
