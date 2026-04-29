using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ERP.API.Models;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SupplierController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SupplierController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Supplier
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Supplier>>> GetSuppliers()
        {
            return await _context.Suppliers.ToListAsync();
        }

        // GET: api/Supplier/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Supplier>> GetSupplier(int id)
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null)
                return NotFound();
            return supplier;
        }

        // POST: api/Supplier
        [HttpPost]
        public async Task<ActionResult<Supplier>> CreateSupplier([FromBody] Supplier supplier)
        {
            // Auto-generate vendor_code
            var lastCode = await _context.Suppliers
                .OrderByDescending(s => s.Id)
                .Select(s => s.VendorCode)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (!string.IsNullOrEmpty(lastCode) && lastCode.StartsWith("ven-"))
            {
                var numPart = lastCode.Substring(4);
                if (int.TryParse(numPart, out int lastNum))
                {
                    nextNumber = lastNum + 1;
                }
            }
            supplier.VendorCode = $"ven-{nextNumber:D3}";

            supplier.CreatedAt = System.DateTime.UtcNow;
            supplier.UpdatedAt = System.DateTime.UtcNow;

            // Ensure IsRegistered is set (default false)
            // Ensure Phone is not null
            if (supplier.Phone == null)
                supplier.Phone = new List<string>();

            _context.Suppliers.Add(supplier);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetSupplier), new { id = supplier.Id }, supplier);
        }

        // PUT: api/Supplier/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSupplier([FromRoute]int id, [FromBody] Supplier supplier)
        {
            if (id != supplier.Id)
                return BadRequest();
            supplier.UpdatedAt = System.DateTime.UtcNow;
            _context.Entry(supplier).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Suppliers.Any(e => e.Id == id))
                    return NotFound();
                else
                    throw;
            }
            return NoContent();
        }

        // DELETE: api/Supplier/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSupplier(int id)
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null)
                return NotFound();
            _context.Suppliers.Remove(supplier);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
