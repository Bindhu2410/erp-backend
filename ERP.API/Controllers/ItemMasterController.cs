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
    public class ItemMasterController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ItemMasterController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/ItemMaster
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ERP.API.Models.ItemMasterResponse>>> GetItemMasters()
        {
            var items = await _context.ItemMasters.ToListAsync();
            var inventoryGroups = await _context.InventoryGroups.ToListAsync();
            var categories = await _context.Categories.ToListAsync();
            var uoms = await _context.Uoms.ToListAsync();
            var inventoryMethods = await _context.InventoryMethods.ToListAsync();
            var valuationMethods = await _context.ValuationMethods.ToListAsync();
            var makes = await _context.Makes.ToListAsync();
            var models = await _context.Models.ToListAsync();
            var products = await _context.Products.ToListAsync();

            var response = items.Select(x => new ERP.API.Models.ItemMasterResponse
            {
                Id = x.Id,
                UserCreated = x.UserCreated,
                DateCreated = x.DateCreated,
                UserUpdated = x.UserUpdated,
                DateUpdated = x.DateUpdated,
                GroupId = x.GroupId,
                GroupName = (x.GroupId == -1 ? inventoryGroups.FirstOrDefault(g => g.Id == 1)?.Name : inventoryGroups.FirstOrDefault(g => g.Id == x.GroupId)?.Name),
                CategoryId = x.CategoryId,
                CategoryName = (x.CategoryId == -1 ? categories.FirstOrDefault(c => c.Id == 1)?.Name : categories.FirstOrDefault(c => c.Id == x.CategoryId)?.Name),
                MakeId = x.MakeId,
                Make = x.MakeId.HasValue ? makes.FirstOrDefault(m => m.Id == x.MakeId)?.Name : null,
                ModelId = x.ModelId,
                Model = x.ModelId.HasValue ? models.FirstOrDefault(m => m.Id == x.ModelId)?.Name : null,
                ProductId = x.ProductId,
                Product = x.ProductId.HasValue ? products.FirstOrDefault(p => p.Id == x.ProductId)?.Name : null,
                Brand = x.Brand,
                ItemName = x.ItemName,
                ItemCode = x.ItemCode,
                IsActive = x.IsActive,
                ImageUrl = x.ImageUrl,
                UnitPrice = x.UnitPrice,
                UomId = x.UomId,
                UomName = (x.UomId == -1 ? uoms.FirstOrDefault(u => u.Id == 1)?.Code : uoms.FirstOrDefault(u => u.Id == x.UomId)?.Code),
                CatNo = x.CatNo,
                InventoryMethodId = x.InventoryMethodId,
                InventoryMethodName = (x.InventoryMethodId == -1 ? inventoryMethods.FirstOrDefault(im => im.Id == 1)?.Name : inventoryMethods.FirstOrDefault(im => im.Id == x.InventoryMethodId)?.Name),
                Hsn = x.Hsn,
                TaxPercentage = x.TaxPercentage,
                ValuationMethodId = x.ValuationMethodId,
                ValuationMethodName = (x.ValuationMethodId == -1 ? valuationMethods.FirstOrDefault(vm => vm.Id == 1)?.Name : valuationMethods.FirstOrDefault(vm => vm.Id == x.ValuationMethodId)?.Name),
                // Include all missing fields
                LongItemName = x.LongItemName,
                ItemDescription = x.ItemDescription,
                SupplierId = x.SupplierId,
                InventoryType = x.InventoryType,
                Specification = x.Specification,
                Criticality = x.Criticality,
                StockToBank = x.StockToBank,
                LpRate = x.LpRate,
                ValuationMethodText = x.ValuationMethodText,
                RelatedStockAccount = x.RelatedStockAccount,
                Cf = x.Cf,
                BomApplicable = x.BomApplicable
            });
            return Ok(response);
        }

        // GET: api/ItemMaster/demo-items
        // Returns only active items whose inventory group name is "Demo"
        [HttpGet("demo-items")]
        public async Task<ActionResult<IEnumerable<ERP.API.Models.ItemMasterResponse>>> GetDemoItems()
        {
            var demoGroup = await _context.InventoryGroups
                .FirstOrDefaultAsync(g => g.Name.ToLower() == "demo");

            if (demoGroup == null)
                return Ok(new List<ERP.API.Models.ItemMasterResponse>());

            var items = await _context.ItemMasters
                .Where(x => x.GroupId == demoGroup.Id && x.IsActive)
                .ToListAsync();

            var inventoryGroups = await _context.InventoryGroups.ToListAsync();
            var categories = await _context.Categories.ToListAsync();
            var uoms = await _context.Uoms.ToListAsync();
            var inventoryMethods = await _context.InventoryMethods.ToListAsync();
            var valuationMethods = await _context.ValuationMethods.ToListAsync();
            var makes = await _context.Makes.ToListAsync();
            var models = await _context.Models.ToListAsync();
            var products = await _context.Products.ToListAsync();

            var response = items.Select(x => new ERP.API.Models.ItemMasterResponse
            {
                Id = x.Id,
                UserCreated = x.UserCreated,
                DateCreated = x.DateCreated,
                UserUpdated = x.UserUpdated,
                DateUpdated = x.DateUpdated,
                GroupId = x.GroupId,
                GroupName = inventoryGroups.FirstOrDefault(g => g.Id == x.GroupId)?.Name,
                CategoryId = x.CategoryId,
                CategoryName = (x.CategoryId == -1 ? categories.FirstOrDefault(c => c.Id == 1)?.Name : categories.FirstOrDefault(c => c.Id == x.CategoryId)?.Name),
                MakeId = x.MakeId,
                Make = x.MakeId.HasValue ? makes.FirstOrDefault(m => m.Id == x.MakeId)?.Name : null,
                ModelId = x.ModelId,
                Model = x.ModelId.HasValue ? models.FirstOrDefault(m => m.Id == x.ModelId)?.Name : null,
                ProductId = x.ProductId,
                Product = x.ProductId.HasValue ? products.FirstOrDefault(p => p.Id == x.ProductId)?.Name : null,
                Brand = x.Brand,
                ItemName = x.ItemName,
                ItemCode = x.ItemCode,
                IsActive = x.IsActive,
                ImageUrl = x.ImageUrl,
                UnitPrice = x.UnitPrice,
                UomId = x.UomId,
                UomName = (x.UomId == -1 ? uoms.FirstOrDefault(u => u.Id == 1)?.Code : uoms.FirstOrDefault(u => u.Id == x.UomId)?.Code),
                CatNo = x.CatNo,
                InventoryMethodId = x.InventoryMethodId,
                InventoryMethodName = (x.InventoryMethodId == -1 ? inventoryMethods.FirstOrDefault(im => im.Id == 1)?.Name : inventoryMethods.FirstOrDefault(im => im.Id == x.InventoryMethodId)?.Name),
                Hsn = x.Hsn,
                TaxPercentage = x.TaxPercentage,
                ValuationMethodId = x.ValuationMethodId,
                ValuationMethodName = (x.ValuationMethodId == -1 ? valuationMethods.FirstOrDefault(vm => vm.Id == 1)?.Name : valuationMethods.FirstOrDefault(vm => vm.Id == x.ValuationMethodId)?.Name),
                LongItemName = x.LongItemName,
                ItemDescription = x.ItemDescription,
                SupplierId = x.SupplierId,
                InventoryType = x.InventoryType,
                Specification = x.Specification,
                Criticality = x.Criticality,
                StockToBank = x.StockToBank,
                LpRate = x.LpRate,
                ValuationMethodText = x.ValuationMethodText,
                RelatedStockAccount = x.RelatedStockAccount,
                Cf = x.Cf,
                BomApplicable = x.BomApplicable
            });
            return Ok(response);
        }

        // GET: api/ItemMaster/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ERP.API.Models.ItemMasterResponse>> GetItemMaster(int id)
        {
            var x = await _context.ItemMasters.FindAsync(id);
            if (x == null)
            {
                return NotFound();
            }
            var group = await _context.InventoryGroups.FirstOrDefaultAsync(g => g.Id == (x.GroupId == -1 ? 1 : x.GroupId));
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == (x.CategoryId == -1 ? 1 : x.CategoryId));
            var uom = await _context.Uoms.FirstOrDefaultAsync(u => u.Id == (x.UomId == -1 ? 1 : x.UomId));
            var inventoryMethod = await _context.InventoryMethods.FirstOrDefaultAsync(im => im.Id == (x.InventoryMethodId == -1 ? 1 : x.InventoryMethodId));
            var valuationMethod = await _context.ValuationMethods.FirstOrDefaultAsync(vm => vm.Id == (x.ValuationMethodId == -1 ? 1 : x.ValuationMethodId));
            var make = x.MakeId.HasValue ? await _context.Makes.FirstOrDefaultAsync(m => m.Id == x.MakeId) : null;
            var model = x.ModelId.HasValue ? await _context.Models.FirstOrDefaultAsync(m => m.Id == x.ModelId) : null;
            var product = x.ProductId.HasValue ? await _context.Products.FirstOrDefaultAsync(p => p.Id == x.ProductId) : null;

            // Fetch rates from RateMasterItems
            var rateItem = await (from rmi in _context.RateMasterItems
                                 join rm in _context.RateMasters on rmi.RateMasterId equals rm.Id
                                 where rmi.ItemId == id
                                 select new { rmi, rm }
                                ).FirstOrDefaultAsync();

            var response = new ERP.API.Models.ItemMasterResponse
            {
                Id = x.Id,
                UserCreated = x.UserCreated,
                DateCreated = x.DateCreated,
                UserUpdated = x.UserUpdated,
                DateUpdated = x.DateUpdated,
                GroupId = x.GroupId,
                GroupName = group?.Name,
                CategoryId = x.CategoryId,
                CategoryName = category?.Name,
                Make = make?.Name,
                Model = model?.Name,
                Product = product?.Name,
                Brand = x.Brand,
                ItemName = x.ItemName,
                ItemCode = x.ItemCode,
                IsActive = x.IsActive,
                ImageUrl = x.ImageUrl,
                UnitPrice = x.UnitPrice,
                UomId = x.UomId,
                UomName = uom?.Code,
                CatNo = x.CatNo,
                InventoryMethodId = x.InventoryMethodId,
                InventoryMethodName = inventoryMethod?.Name,
                Hsn = x.Hsn,
                TaxPercentage = x.TaxPercentage,
                ValuationMethodId = x.ValuationMethodId,
                ValuationMethodName = valuationMethod?.Name,
                PurchaseRate = rateItem?.rmi.PurchaseRate,
                SaleRate = rateItem?.rmi.SalesRate,
                QuoteRate = rateItem?.rmi.QuotationRate,
                HsnCode = rateItem?.rmi.HsnCode != null ? int.Parse(rateItem.rmi.HsnCode) : (int?)null,
                TaxPercent = rateItem?.rmi.Tax != null ? (int?)rateItem.rmi.Tax : null,
                // Additional missing fields
                LongItemName = x.LongItemName,
                ItemDescription = x.ItemDescription,
                SupplierId = x.SupplierId,
                InventoryType = x.InventoryType,
                Specification = x.Specification,
                Criticality = x.Criticality,
                StockToBank = x.StockToBank,
                LpRate = x.LpRate,
                ValuationMethodText = x.ValuationMethodText,
                RelatedStockAccount = x.RelatedStockAccount,
                Cf = x.Cf,
                BomApplicable = x.BomApplicable,
                MakeId = x.MakeId,
                ModelId = x.ModelId,
                ProductId = x.ProductId
            };
            return Ok(response);
        }

        // POST: api/ItemMaster
        [HttpPost]
        [Consumes("application/json")]
        public async Task<ActionResult<ItemMasterResponse>> PostItemMaster([FromBody] ItemMaster itemMaster)
        {
            // Debug logging to check if ItemDescription is being received
            Console.WriteLine($"Received ItemDescription: '{itemMaster.ItemDescription}'");
            Console.WriteLine($"Received ItemName: '{itemMaster.ItemName}'");
            
            _context.ItemMasters.Add(itemMaster);
            await _context.SaveChangesAsync();
            
            // Check if it was saved correctly
            var savedItem = await _context.ItemMasters.FindAsync(itemMaster.Id);
            Console.WriteLine($"Saved ItemDescription: '{savedItem?.ItemDescription}'");
            
            var response = new ItemMasterResponse
            {
                Id = itemMaster.Id,
                UserCreated = itemMaster.UserCreated,
                DateCreated = itemMaster.DateCreated,
                UserUpdated = itemMaster.UserUpdated,
                DateUpdated = itemMaster.DateUpdated,
                GroupId = itemMaster.GroupId,
                CategoryId = itemMaster.CategoryId,
                MakeId = itemMaster.MakeId,
                ModelId = itemMaster.ModelId,
                ProductId = itemMaster.ProductId,
                Make = itemMaster.Make,
                Model = itemMaster.Model,
                Product = itemMaster.Product,
                Brand = itemMaster.Brand,
                ItemName = itemMaster.ItemName,
                ItemCode = itemMaster.ItemCode,
                IsActive = itemMaster.IsActive,
                ImageUrl = itemMaster.ImageUrl,
                UnitPrice = itemMaster.UnitPrice,
                UomId = itemMaster.UomId,
                CatNo = itemMaster.CatNo,
                InventoryMethodId = itemMaster.InventoryMethodId,
                Hsn = itemMaster.Hsn,
                TaxPercentage = itemMaster.TaxPercentage,
                ValuationMethodId = itemMaster.ValuationMethodId,
                // Include all missing fields
                LongItemName = itemMaster.LongItemName,
                ItemDescription = itemMaster.ItemDescription,
                SupplierId = itemMaster.SupplierId,
                InventoryType = itemMaster.InventoryType,
                Specification = itemMaster.Specification,
                Criticality = itemMaster.Criticality,
                StockToBank = itemMaster.StockToBank,
                LpRate = itemMaster.LpRate,
                ValuationMethodText = itemMaster.ValuationMethodText,
                RelatedStockAccount = itemMaster.RelatedStockAccount,
                Cf = itemMaster.Cf,
                BomApplicable = itemMaster.BomApplicable
            };
            return CreatedAtAction(nameof(GetItemMaster), new { id = itemMaster.Id }, response);
        }

        // POST: api/ItemMaster/array
        [HttpPost("array")]
        [Consumes("application/json")]
        public async Task<ActionResult<IEnumerable<ItemMasterResponse>>> PostItemMasterArray([FromBody] List<ItemMaster> itemMasters)
        {
            if (itemMasters == null || !itemMasters.Any())
            {
                return BadRequest("No items provided.");
            }
            _context.ItemMasters.AddRange(itemMasters);
            await _context.SaveChangesAsync();
            var responses = itemMasters.Select(itemMaster => new ItemMasterResponse
            {
                Id = itemMaster.Id,
                UserCreated = itemMaster.UserCreated,
                DateCreated = itemMaster.DateCreated,
                UserUpdated = itemMaster.UserUpdated,
                DateUpdated = itemMaster.DateUpdated,
                GroupId = itemMaster.GroupId,
                CategoryId = itemMaster.CategoryId,
                MakeId = itemMaster.MakeId,
                ModelId = itemMaster.ModelId,
                ProductId = itemMaster.ProductId,
                Make = itemMaster.Make,
                Model = itemMaster.Model,
                Product = itemMaster.Product,
                Brand = itemMaster.Brand,
                ItemName = itemMaster.ItemName,
                ItemCode = itemMaster.ItemCode,
                IsActive = itemMaster.IsActive,
                ImageUrl = itemMaster.ImageUrl,
                UnitPrice = itemMaster.UnitPrice,
                UomId = itemMaster.UomId,
                CatNo = itemMaster.CatNo,
                InventoryMethodId = itemMaster.InventoryMethodId,
                Hsn = itemMaster.Hsn,
                TaxPercentage = itemMaster.TaxPercentage,
                ValuationMethodId = itemMaster.ValuationMethodId,
                // Include all missing fields
                LongItemName = itemMaster.LongItemName,
                ItemDescription = itemMaster.ItemDescription,
                SupplierId = itemMaster.SupplierId,
                InventoryType = itemMaster.InventoryType,
                Specification = itemMaster.Specification,
                Criticality = itemMaster.Criticality,
                StockToBank = itemMaster.StockToBank,
                LpRate = itemMaster.LpRate,
                ValuationMethodText = itemMaster.ValuationMethodText,
                RelatedStockAccount = itemMaster.RelatedStockAccount,
                Cf = itemMaster.Cf,
                BomApplicable = itemMaster.BomApplicable
            });
            return Ok(responses);
        }

        // PUT: api/ItemMaster/5
        [HttpPut("{id}")]
        [Consumes("application/json")]
        public async Task<IActionResult> PutItemMaster(int id, [FromBody] ItemMaster itemMaster)
        {
            if (id != itemMaster.Id)
            {
                return BadRequest();
            }
            _context.Entry(itemMaster).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.ItemMasters.Any(e => e.Id == id))
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

        // DELETE: api/ItemMaster/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteItemMaster(int id)
        {
            var itemMaster = await _context.ItemMasters.FindAsync(id);
            if (itemMaster == null)
            {
                return NotFound();
            }
            _context.ItemMasters.Remove(itemMaster);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
