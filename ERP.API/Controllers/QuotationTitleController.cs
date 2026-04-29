using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ERP.API.Models;

namespace ERP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuotationTitleController : ControllerBase
    {
        private readonly AppDbContext _context;

        public QuotationTitleController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/QuotationTitle
        [HttpGet]
        public async Task<ActionResult<IEnumerable<QuotationTitle>>> GetQuotationTitles()
        {
            return await _context.QuotationTitles.ToListAsync();
        }

        // GET: api/QuotationTitle/5
        [HttpGet("{id}")]
        public async Task<ActionResult<QuotationTitle>> GetQuotationTitle(int id)
        {
            var quotationTitle = await _context.QuotationTitles.FindAsync(id);

            if (quotationTitle == null)
            {
                return NotFound();
            }

            return quotationTitle;
        }

        // POST: api/QuotationTitle
        [HttpPost]
        public async Task<ActionResult<QuotationTitle>> PostQuotationTitle([FromBody] QuotationTitle quotationTitle)
        {
            _context.QuotationTitles.Add(quotationTitle);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetQuotationTitle), new { id = quotationTitle.Id }, quotationTitle);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutQuotationTitle([FromRoute] int id, [FromBody] QuotationTitle quotationTitle)
        {
            if (id != quotationTitle.Id)
            {
                return BadRequest();
            }

            _context.Entry(quotationTitle).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!QuotationTitleExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        // DELETE: api/QuotationTitle/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQuotationTitle(int id)
        {
            var quotationTitle = await _context.QuotationTitles.FindAsync(id);
            if (quotationTitle == null)
            {
                return NotFound();
            }

            _context.QuotationTitles.Remove(quotationTitle);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool QuotationTitleExists(int id)
        {
            return _context.QuotationTitles.Any(e => e.Id == id);
        }
    }
}
