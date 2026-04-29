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
    public class ItemStockTransactionController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ItemStockTransactionController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/ItemStockTransaction
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ItemStockTransaction>>> GetItemStockTransactions()
        {
            var transactions = await _context.ItemStockTransactions.ToListAsync();
            var itemIds = transactions.Select(x => x.ItemId).Distinct().ToList();
            var itemNames = _context.ItemMasters.Where(im => itemIds.Contains(im.Id)).ToDictionary(im => im.Id, im => im.ItemName);
            foreach (var t in transactions)
            {
                if (itemNames.TryGetValue(t.ItemId, out var name))
                    t.ItemName = name;
            }
            return transactions;
        }

        // GET: api/ItemStockTransaction/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ItemStockTransaction>> GetItemStockTransaction(int id)
        {
            var transaction = await _context.ItemStockTransactions.FindAsync(id);
            if (transaction == null)
            {
                return NotFound();
            }
            var itemMaster = await _context.ItemMasters.FindAsync(transaction.ItemId);
            transaction.ItemName = itemMaster?.ItemName;
            return transaction;
        }

        // POST: api/ItemStockTransaction
        [HttpPost]
        public async Task<ActionResult<ItemStockTransaction>> PostItemStockTransaction([FromBody] ItemStockTransaction transaction)
        {
            _context.ItemStockTransactions.Add(transaction);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetItemStockTransaction), new { id = transaction.Id }, transaction);
        }

        // PUT: api/ItemStockTransaction/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutItemStockTransaction([FromRoute] int id, [FromBody] ItemStockTransaction transaction)
        {
            if (id != transaction.Id)
            {
                return BadRequest();
            }
            _context.Entry(transaction).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.ItemStockTransactions.Any(e => e.Id == id))
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

        // DELETE: api/ItemStockTransaction/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteItemStockTransaction(int id)
        {
            var transaction = await _context.ItemStockTransactions.FindAsync(id);
            if (transaction == null)
            {
                return NotFound();
            }
            _context.ItemStockTransactions.Remove(transaction);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
