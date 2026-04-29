using ERP.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VendorController : ControllerBase
    {
        private readonly AppDbContext _context;
        public VendorController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetVendorList()
        {
            var vendors = await _context.Suppliers
                .Where(s => s.IsActive)
                .Select(s => new { s.Id, s.VendorName })
                .ToListAsync();
            return Ok(vendors);
        }
    }
}
