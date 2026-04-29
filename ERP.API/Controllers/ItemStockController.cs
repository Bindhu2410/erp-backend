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
    public class ItemStockController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ItemStockController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/ItemStock
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ItemStock>>> GetItemStocks()
        {
            var stocks = await _context.ItemStocks.ToListAsync();
            var itemIds = stocks.Select(x => x.ItemId).Distinct().ToList();
            var itemNames = _context.ItemMasters.Where(im => itemIds.Contains(im.Id)).ToDictionary(im => im.Id, im => im.ItemName);
            foreach (var stock in stocks)
            {
                if (itemNames.TryGetValue(stock.ItemId, out var name))
                    stock.ItemName = name;
            }
            return stocks;
        }

        // GET: api/ItemStock/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ItemStock>> GetItemStock(int id)
        {
            var itemStock = await _context.ItemStocks.FindAsync(id);
            if (itemStock == null)
            {
                return NotFound();
            }
            var itemMaster = await _context.ItemMasters.FindAsync(itemStock.ItemId);
            itemStock.ItemName = itemMaster?.ItemName;
            return itemStock;
        }

        // POST: api/ItemStock
        [HttpPost]
        public async Task<ActionResult<ItemStock>> PostItemStock([FromBody] ItemStock itemStock)
        {
            _context.ItemStocks.Add(itemStock);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetItemStock), new { id = itemStock.Id }, itemStock);
        }

        // PUT: api/ItemStock/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutItemStock([FromRoute] int id, [FromBody] ItemStock itemStock)
        {
            if (id != itemStock.Id)
            {
                return BadRequest();
            }
            _context.Entry(itemStock).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.ItemStocks.Any(e => e.Id == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return NoContent();
        }

        // DELETE: api/ItemStock/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteItemStock(int id)
        {
            var itemStock = await _context.ItemStocks.FindAsync(id);
            if (itemStock == null)
            {
                return NotFound();
            }
            _context.ItemStocks.Remove(itemStock);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
