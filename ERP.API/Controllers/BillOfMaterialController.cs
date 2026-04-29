using ERP.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BillOfMaterialController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BillOfMaterialController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetAll()
        {
            var boms = await _context.Set<BillOfMaterial>()
                .Include(b => b.ChildItems)
                .ToListAsync();

            var quoteTitleIds = boms.Where(b => b.QuoteTitleId.HasValue).Select(b => b.QuoteTitleId.Value).Distinct().ToList();
            var tcTemplateIds = boms.Where(b => b.TcTemplateId.HasValue).Select(b => b.TcTemplateId.Value).Distinct().ToList();
            var quoteTitles = await _context.QuotationTitles.Where(q => quoteTitleIds.Contains(q.Id)).ToListAsync();
            var tcTemplates = await _context.Set<SalesTermsAndConditions>().Where(t => tcTemplateIds.Contains(t.Id)).ToListAsync();

            var childItemIds = boms.SelectMany(b => b.ChildItems.Select(ci => ci.ChildItemId)).Distinct().ToList();
            var childItems = await _context.Set<ItemMaster>().Where(i => childItemIds.Contains(i.Id)).ToListAsync();

            var categoryIds = childItems.Select(i => i.CategoryId).Where(x => x.HasValue).Select(x => x.Value).Distinct().ToList();
            var uomIds = childItems.Select(i => i.UomId).Where(x => x.HasValue).Select(x => x.Value).Distinct().ToList();
            var valuationMethodIds = childItems.Select(i => i.ValuationMethodId).Where(x => x.HasValue).Select(x => x.Value).Distinct().ToList();
            var inventoryMethodIds = childItems.Select(i => i.InventoryMethodId).Where(x => x.HasValue).Select(x => x.Value).Distinct().ToList();
            var inventoryTypeIds = childItems.Select(i => i.GroupId).Where(x => x.HasValue).Select(x => x.Value).Distinct().ToList();
            var inventoryGroupIds = childItems.Select(i => i.GroupId).Where(x => x.HasValue).Select(x => x.Value).Distinct().ToList();
            var makeIds = childItems.Select(i => i.MakeId).Where(x => x.HasValue).Select(x => x.Value).Distinct().ToList();
            var modelIds = childItems.Select(i => i.ModelId).Where(x => x.HasValue).Select(x => x.Value).Distinct().ToList();
            var productIds = childItems.Select(i => i.ProductId).Where(x => x.HasValue).Select(x => x.Value).Distinct().ToList();

            var categories = await _context.Set<Category>().Where(c => categoryIds.Contains(c.Id)).ToListAsync();
            var uoms = await _context.Set<Uom>().Where(u => uomIds.Contains(u.Id)).ToListAsync();
            var valuationMethods = await _context.Set<ValuationMethod>().Where(vm => valuationMethodIds.Contains(vm.Id)).ToListAsync();
            var inventoryMethods = await _context.Set<InventoryMethod>().Where(im => inventoryMethodIds.Contains(im.Id)).ToListAsync();
            var inventoryTypes = await _context.Set<InventoryType>().Where(it => inventoryTypeIds.Contains(it.Id)).ToListAsync();
            var makes = await _context.Set<Make>().Where(m => makeIds.Contains(m.Id)).ToListAsync();
            var models = await _context.Set<Model>().Where(m => modelIds.Contains(m.Id)).ToListAsync();
            var products = await _context.Set<Product>().Where(p => productIds.Contains(p.Id)).ToListAsync();
            var inventoryGroups = await _context.Set<InventoryGroup>().Where(g => inventoryGroupIds.Contains(g.Id)).ToListAsync();
            var rateMasterItems = await (from rmi in _context.RateMasterItems
                                        join rm in _context.RateMasters on rmi.RateMasterId equals rm.Id
                                        where childItemIds.Contains(rmi.ItemId)
                                        select new { rmi, rm }
                                       ).ToListAsync();

            var result = boms.Select(bom => new
            {
                bom.Id,
                bom.BomId,
                bom.BomName,
                bom.BomType,
                bom.EffectiveFrom,
                bom.EffectiveTo,
                bom.QuoteTitleId,
                QuoteTitleName = bom.QuoteTitleId.HasValue ? quoteTitles.FirstOrDefault(q => q.Id == bom.QuoteTitleId)?.Title : null,
                bom.TcTemplateId,
                TcTemplateName = bom.TcTemplateId.HasValue ? tcTemplates.FirstOrDefault(t => t.Id == bom.TcTemplateId)?.TemplateName : null,
                bom.Make,
                ChildItems = bom.ChildItems.Select(ci => {
                    var item = childItems.FirstOrDefault(i => i.Id == ci.ChildItemId);
                    var categoryName = item?.CategoryId != null ? categories.FirstOrDefault(c => c.Id == item.CategoryId)?.Name : null;
                    var uomName = item?.UomId != null ? uoms.FirstOrDefault(u => u.Id == item.UomId)?.Code : null;
                    var valuationMethodName = item?.ValuationMethodId != null ? valuationMethods.FirstOrDefault(vm => vm.Id == item.ValuationMethodId)?.Name : null;
                    var inventoryMethodName = item?.InventoryMethodId != null ? inventoryMethods.FirstOrDefault(im => im.Id == item.InventoryMethodId)?.Name : null;
                    var inventoryTypeName = item?.GroupId != null ? inventoryTypes.FirstOrDefault(it => it.Id == item.GroupId)?.Name : null;
                    var inventoryGroupName = item?.GroupId != null ? inventoryGroups.FirstOrDefault(g => g.Id == item.GroupId)?.Name : null;
                    var makeName = item?.MakeId != null ? makes.FirstOrDefault(m => m.Id == item.MakeId)?.Name : null;
                    var modelName = item?.ModelId != null ? models.FirstOrDefault(m => m.Id == item.ModelId)?.Name : null;
                    var productName = item?.ProductId != null ? products.FirstOrDefault(p => p.Id == item.ProductId)?.Name : null;
                    var rateItem = rateMasterItems.FirstOrDefault(rm => rm.rmi.ItemId == ci.ChildItemId);
                    return new
                    {
                        ci.ChildItemId,
                        ci.Quantity,
                        ci.Amount,
                        Make = makeName,
                        Model = modelName,
                        Product = productName,
                        CategoryName = categoryName,
                        ValuationMethodName = valuationMethodName,
                        InventoryMethodName = inventoryMethodName,
                        InventoryTypeName = inventoryTypeName,
                        InventoryGroupName = inventoryGroupName,
                        item?.UnitPrice,
                        item?.ItemName,
                        item?.ItemCode,
                        item?.CatNo,
                        UomName = uomName,
                        PurchaseRate = rateItem?.rmi.PurchaseRate,
                        SaleRate = rateItem?.rmi.SalesRate,
                        QuoteRate = rateItem?.rmi.QuotationRate,
                        HSN = rateItem?.rmi.HsnCode,
                        Tax = rateItem?.rmi.Tax
                    };
                }).ToList()
            });
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<object>> Get(int id)
        {
            var bom = await _context.Set<BillOfMaterial>()
                .Include(b => b.ChildItems)
                .FirstOrDefaultAsync(b => b.Id == id);
            if (bom == null) return NotFound();

            var quoteTitle = bom.QuoteTitleId.HasValue ? await _context.QuotationTitles.FindAsync(bom.QuoteTitleId.Value) : null;
            var tcTemplate = bom.TcTemplateId.HasValue ? await _context.Set<SalesTermsAndConditions>().FindAsync(bom.TcTemplateId.Value) : null;

            var childItemIds = bom.ChildItems.Select(ci => ci.ChildItemId).ToList();
            var childItems = await _context.Set<ItemMaster>().Where(i => childItemIds.Contains(i.Id)).ToListAsync();
            var categoryIds = childItems.Select(i => i.CategoryId).Where(x => x.HasValue).Select(x => x.Value).Distinct().ToList();
            var uomIds = childItems.Select(i => i.UomId).Where(x => x.HasValue).Select(x => x.Value).Distinct().ToList();
            var valuationMethodIds = childItems.Select(i => i.ValuationMethodId).Where(x => x.HasValue).Select(x => x.Value).Distinct().ToList();
            var inventoryMethodIds = childItems.Select(i => i.InventoryMethodId).Where(x => x.HasValue).Select(x => x.Value).Distinct().ToList();
            var inventoryTypeIds = childItems.Select(i => i.GroupId).Where(x => x.HasValue).Select(x => x.Value).Distinct().ToList();
            var inventoryGroupIds = childItems.Select(i => i.GroupId).Where(x => x.HasValue).Select(x => x.Value).Distinct().ToList();
            var makeIds = childItems.Select(i => i.MakeId).Where(x => x.HasValue).Select(x => x.Value).Distinct().ToList();
            var modelIds = childItems.Select(i => i.ModelId).Where(x => x.HasValue).Select(x => x.Value).Distinct().ToList();
            var productIds = childItems.Select(i => i.ProductId).Where(x => x.HasValue).Select(x => x.Value).Distinct().ToList();

            var categories = await _context.Set<Category>().Where(c => categoryIds.Contains(c.Id)).ToListAsync();
            var uoms = await _context.Set<Uom>().Where(u => uomIds.Contains(u.Id)).ToListAsync();
            var valuationMethods = await _context.Set<ValuationMethod>().Where(vm => valuationMethodIds.Contains(vm.Id)).ToListAsync();
            var inventoryMethods = await _context.Set<InventoryMethod>().Where(im => inventoryMethodIds.Contains(im.Id)).ToListAsync();
            var inventoryTypes = await _context.Set<InventoryType>().Where(it => inventoryTypeIds.Contains(it.Id)).ToListAsync();
            var inventoryGroups = await _context.Set<InventoryGroup>().Where(g => inventoryGroupIds.Contains(g.Id)).ToListAsync();
            var makes = await _context.Set<Make>().Where(m => makeIds.Contains(m.Id)).ToListAsync();
            var models = await _context.Set<Model>().Where(m => modelIds.Contains(m.Id)).ToListAsync();
            var products = await _context.Set<Product>().Where(p => productIds.Contains(p.Id)).ToListAsync();
            var rateMasterItems = await (from rmi in _context.RateMasterItems
                                        join rm in _context.RateMasters on rmi.RateMasterId equals rm.Id
                                        where childItemIds.Contains(rmi.ItemId)
                                        select new { rmi, rm }
                                       ).ToListAsync();

            var response = new
            {
                bom.Id,
                bom.BomId,
                bom.BomName,
                bom.BomType,
                bom.EffectiveFrom,
                bom.EffectiveTo,
                bom.QuoteTitleId,
                QuoteTitleName = quoteTitle?.Title,
                bom.TcTemplateId,
                TcTemplateName = tcTemplate?.TemplateName,
                bom.Make,
                ChildItems = bom.ChildItems.Select(ci => {
                    var item = childItems.FirstOrDefault(i => i.Id == ci.ChildItemId);
                    var categoryName = item?.CategoryId != null ? categories.FirstOrDefault(c => c.Id == item.CategoryId)?.Name : null;
                    var uomName = item?.UomId != null ? uoms.FirstOrDefault(u => u.Id == item.UomId)?.Code : null;
                    var valuationMethodName = item?.ValuationMethodId != null ? valuationMethods.FirstOrDefault(vm => vm.Id == item.ValuationMethodId)?.Name : null;
                    var inventoryMethodName = item?.InventoryMethodId != null ? inventoryMethods.FirstOrDefault(im => im.Id == item.InventoryMethodId)?.Name : null;
                    var inventoryTypeName = item?.GroupId != null ? inventoryTypes.FirstOrDefault(it => it.Id == item.GroupId)?.Name : null;
                    var inventoryGroupName = item?.GroupId != null ? inventoryGroups.FirstOrDefault(g => g.Id == item.GroupId)?.Name : null;
                    var makeName = item?.MakeId != null ? makes.FirstOrDefault(m => m.Id == item.MakeId)?.Name : null;
                    var modelName = item?.ModelId != null ? models.FirstOrDefault(m => m.Id == item.ModelId)?.Name : null;
                    var productName = item?.ProductId != null ? products.FirstOrDefault(p => p.Id == item.ProductId)?.Name : null;
                    var rateItem = rateMasterItems.FirstOrDefault(rm => rm.rmi.ItemId == ci.ChildItemId);
                    return new
                    {
                        ci.ChildItemId,
                        ci.Quantity,
                        ci.Amount,
                        Make = makeName,
                        Model = modelName,
                        Product = productName,
                        CategoryName = categoryName,
                        ValuationMethodName = valuationMethodName,
                        InventoryMethodName = inventoryMethodName,
                        InventoryTypeName = inventoryTypeName,
                        InventoryGroupName = inventoryGroupName,
                        item?.UnitPrice,
                        item?.ItemName,
                        item?.ItemCode,
                        item?.CatNo,
                        UomName = uomName,
                        PurchaseRate = rateItem?.rmi.PurchaseRate,
                        SaleRate = rateItem?.rmi.SalesRate,
                        QuoteRate = rateItem?.rmi.QuotationRate,
                        HSN = rateItem?.rmi.HsnCode,
                        Tax = rateItem?.rmi.Tax
                    };
                }).ToList()
            };
            return Ok(response);
        }

        [HttpPost]
        public async Task<ActionResult<object>> Create([FromBody] BillOfMaterialRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto?.BomName))
                return BadRequest("BomName is required");



            // Auto-generate BOM ID in format BOM-25-26-01
            var currentYear = System.DateTime.Now.Year;
            var nextYear = currentYear + 1;
            var yearPart = $"{currentYear % 100:D2}-{nextYear % 100:D2}";
            var prefix = $"BOM-{yearPart}-";
            var lastBom = await _context.Set<BillOfMaterial>()
                .Where(x => x.BomId.StartsWith(prefix))
                .OrderByDescending(x => x.BomId)
                .FirstOrDefaultAsync();
            int serial = 1;
            if (lastBom != null && lastBom.BomId.Length >= prefix.Length + 2)
            {
                var serialStr = lastBom.BomId.Substring(prefix.Length, 2);
                if (int.TryParse(serialStr, out int lastSerial))
                {
                    serial = lastSerial + 1;
                }
            }
            var bom = new BillOfMaterial
            {
                BomId = $"BOM-{yearPart}-{serial:D2}",
                BomName = dto.BomName,
                BomType = dto.BomType,
                EffectiveFrom = dto.EffectiveFrom,
                EffectiveTo = dto.EffectiveTo,
                QuoteTitleId = dto.QuoteTitleId,
                TcTemplateId = dto.TcTemplateId,
                Make = dto.Make
            };
            _context.Set<BillOfMaterial>().Add(bom);
            await _context.SaveChangesAsync();

            // Add child items
            if (dto.ChildItems != null)
            {
                foreach (var ci in dto.ChildItems)
                {
                    var child = new BillOfMaterialChildItem
                    {
                        BillOfMaterialId = bom.Id,
                        ChildItemId = ci.ChildItemId,
                        Quantity = ci.Quantity,
                        Amount = ci.Amount
                    };
                    _context.Set<BillOfMaterialChildItem>().Add(child);
                }
                await _context.SaveChangesAsync();
            }

            // Fetch rate details for each child item
            var childItemIds = dto.ChildItems?.Select(ci => ci.ChildItemId).ToList() ?? new List<int>();
            var rateMasterItems = await (from rmi in _context.RateMasterItems
                                        join rm in _context.RateMasters on rmi.RateMasterId equals rm.Id
                                        where childItemIds.Contains(rmi.ItemId)
                                        select new { rmi, rm }
                                       ).ToListAsync();

            var response = new
            {
                bom.Id,
                bom.BomId,
                bom.BomName,
                bom.BomType,
                bom.EffectiveFrom,
                bom.EffectiveTo,
                bom.QuoteTitleId,
                bom.TcTemplateId,
                bom.Make,
                ChildItems = dto.ChildItems?.Select(ci => {
                    var rateItem = rateMasterItems.FirstOrDefault(rm => rm.rmi.ItemId == ci.ChildItemId);
                    return new
                    {
                        ci.ChildItemId,
                        ci.Quantity,
                        PurchaseRate = rateItem?.rmi.PurchaseRate,
                        SaleRate = rateItem?.rmi.SalesRate,
                        QuoteRate = rateItem?.rmi.QuotationRate,
                        HSN = rateItem?.rmi.HsnCode,
                        Tax = rateItem?.rmi.Tax
                    };
                }).ToList()
            };
            return CreatedAtAction(nameof(Get), new { id = bom.Id }, response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] BillOfMaterialRequestDto dto)
        {
            var existingBom = await _context.Set<BillOfMaterial>()
                .Include(b => b.ChildItems)
                .FirstOrDefaultAsync(b => b.Id == id);
            if (existingBom == null) return NotFound();

            existingBom.BomName = dto.BomName;
            existingBom.BomType = dto.BomType;
            existingBom.EffectiveFrom = dto.EffectiveFrom;
            existingBom.EffectiveTo = dto.EffectiveTo;
            existingBom.QuoteTitleId = dto.QuoteTitleId;
            existingBom.TcTemplateId = dto.TcTemplateId;
            existingBom.Make = dto.Make;
            _context.Entry(existingBom).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            // Update child items: remove old, add new
            var existingChildItems = _context.Set<BillOfMaterialChildItem>().Where(ci => ci.BillOfMaterialId == id);
            _context.Set<BillOfMaterialChildItem>().RemoveRange(existingChildItems);
            await _context.SaveChangesAsync();

            if (dto.ChildItems != null)
            {
                foreach (var ci in dto.ChildItems)
                {
                    var child = new BillOfMaterialChildItem
                    {
                        BillOfMaterialId = id,
                        ChildItemId = ci.ChildItemId,
                        Quantity = ci.Quantity,
                        Amount = ci.Amount
                    };
                    _context.Set<BillOfMaterialChildItem>().Add(child);
                }
                await _context.SaveChangesAsync();
            }

            var response = new
            {
                existingBom.Id,
                existingBom.BomId,
                existingBom.BomName,
                existingBom.BomType,
                existingBom.EffectiveFrom,
                existingBom.EffectiveTo,
                existingBom.QuoteTitleId,
                existingBom.TcTemplateId,
                existingBom.Make,
                ChildItems = dto.ChildItems?.Select(ci => new
                {
                    ci.ChildItemId,
                    ci.Quantity
                }).ToList()
            };
            return Ok(response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var bom = await _context.Set<BillOfMaterial>().FindAsync(id);
            if (bom == null) return NotFound();
            _context.Set<BillOfMaterial>().Remove(bom);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}