using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ERP.API.Models;

namespace ERP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SalesCustomerController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SalesCustomerController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SalesCustomer>>> GetSalesCustomers()
        {
            return await _context.SalesCustomers.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SalesCustomer>> GetSalesCustomer(int id)
        {
            var customer = await _context.SalesCustomers.FindAsync(id);
            if (customer == null)
            {
                return NotFound();
            }
            return customer;
        }
    }
}
