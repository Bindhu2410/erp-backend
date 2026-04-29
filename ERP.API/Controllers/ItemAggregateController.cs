using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Npgsql;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ERP.API.Models;
using System.Collections.Generic;

namespace ERP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemAggregateController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ItemAggregateController(AppDbContext context)
        {
            _context = context;
        }

        // Map ItemMasterRequest DTO -> ItemMaster entity (copy matching properties)
        private ItemMaster MapDtoToEntity(ItemMasterRequest dto)
        {
            var dest = new ItemMaster();
            if (dto == null) return dest;
            var srcProps = typeof(ItemMasterRequest).GetProperties();
            var destType = typeof(ItemMaster);
            foreach (var p in srcProps)
            {
                var target = destType.GetProperty(p.Name);
                if (target == null || !target.CanWrite) continue;
                try
                {
                    var val = p.GetValue(dto);
                    target.SetValue(dest, val);
                }
                catch
                {
                    // ignore mismatched types
                }
            }
            return dest;
        }

        // Helper method to convert all DateTime fields to UTC
        private void EnsureUtcDateTimes(object entity)
        {
            if (entity == null) return;
            
            var properties = entity.GetType().GetProperties()
                .Where(p => p.PropertyType == typeof(DateTime?) || p.PropertyType == typeof(DateTime));
            
            foreach (var prop in properties)
            {
                if (prop.PropertyType == typeof(DateTime?))
                {
                    var value = (DateTime?)prop.GetValue(entity);
                    if (value.HasValue && value.Value.Kind == DateTimeKind.Unspecified)
                    {
                        prop.SetValue(entity, DateTime.SpecifyKind(value.Value, DateTimeKind.Utc));
                    }
                }
                else if (prop.PropertyType == typeof(DateTime))
                {
                    var value = (DateTime)prop.GetValue(entity);
                    if (value.Kind == DateTimeKind.Unspecified)
                    {
                        prop.SetValue(entity, DateTime.SpecifyKind(value, DateTimeKind.Utc));
                    }
                }
            }
        }
        private async Task<string> GenerateNextItemCodeAsync()
        {
            // Get all item codes that match expected pattern
            var codes = await _context.ItemMasters
                .Where(x => x.ItemCode != null && x.ItemCode.StartsWith("ITM-"))
                .Select(x => x.ItemCode)
                .ToListAsync();

            var max = 0;
            var re = new Regex(@"^ITM-(\d+)$");
            foreach (var c in codes)
            {
                if (c == null) continue;
                var m = re.Match(c);
                if (m.Success && int.TryParse(m.Groups[1].Value, out var n))
                {
                    if (n > max) max = n;
                }
            }
            var next = max + 1;
            return $"ITM-{next.ToString().PadLeft(4, '0')}";
        }

        private bool IsUniqueConstraintViolationOnItemCode(DbUpdateException ex)
        {
            if (ex?.InnerException is PostgresException pgEx)
            {
                // Postgres unique violation code is 23505
                if (pgEx.SqlState == "23505")
                {
                    // check constraint name or message
                    if (!string.IsNullOrEmpty(pgEx.ConstraintName) && pgEx.ConstraintName.Contains("item_code"))
                        return true;
                    if (!string.IsNullOrEmpty(pgEx.MessageText) && pgEx.MessageText.Contains("item_code"))
                        return true;
                }
            }
            return false;
        }

        // GET: api/ItemAggregate
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ItemMaster>>> Get()
        {
            var items = await (from item in _context.ItemMasters
                              join make in _context.Makes on item.MakeId equals make.Id into makeGroup
                              from make in makeGroup.DefaultIfEmpty()
                              join model in _context.Models on item.ModelId equals model.Id into modelGroup
                              from model in modelGroup.DefaultIfEmpty()
                              join product in _context.Products on item.ProductId equals product.Id into productGroup
                              from product in productGroup.DefaultIfEmpty()
                              select new ItemMaster
                              {
                                  Id = item.Id,
                                  UserCreated = item.UserCreated,
                                  DateCreated = item.DateCreated,
                                  UserUpdated = item.UserUpdated,
                                  DateUpdated = item.DateUpdated,
                                  GroupId = item.GroupId,
                                  CategoryId = item.CategoryId,
                                  LongItemName = item.LongItemName,
                                  ItemDescription = item.ItemDescription,
                                  MakeId = item.MakeId,
                                  ModelId = item.ModelId,
                                  ProductId = item.ProductId,
                                  Make = make != null ? make.Name : null,
                                  Model = model != null ? model.Name : null,
                                  Product = product != null ? product.Name : null,
                                  Brand = item.Brand,
                                  ItemName = item.ItemName,
                                  ItemCode = item.ItemCode,
                                  InventoryType = item.InventoryType,
                                  Specification = item.Specification,
                                  Criticality = item.Criticality,
                                  StockToBank = item.StockToBank,
                                  LpRate = item.LpRate,
                                  IsActive = item.IsActive,
                                  ImageUrl = item.ImageUrl,
                                  UnitPrice = item.UnitPrice,
                                  UomId = item.UomId,
                                  CatNo = item.CatNo,
                                  InventoryMethodId = item.InventoryMethodId,
                                  Hsn = item.Hsn,
                                  TaxPercentage = item.TaxPercentage,
                                  ValuationMethodId = item.ValuationMethodId,
                                  ValuationMethodText = item.ValuationMethodText,
                                  RelatedStockAccount = item.RelatedStockAccount,
                                  Cf = item.Cf,
                                  BomApplicable = item.BomApplicable
                              }).ToListAsync();
            return Ok(items);
        }

        // GET: api/ItemAggregate/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ItemAggregateResponse>> GetById(int id)
        {
            var item = await (from i in _context.ItemMasters
                             where i.Id == id
                             join make in _context.Makes on i.MakeId equals make.Id into makeGroup
                             from make in makeGroup.DefaultIfEmpty()
                             join model in _context.Models on i.ModelId equals model.Id into modelGroup
                             from model in modelGroup.DefaultIfEmpty()
                             join product in _context.Products on i.ProductId equals product.Id into productGroup
                             from product in productGroup.DefaultIfEmpty()
                             select new ItemMaster
                             {
                                 Id = i.Id,
                                 UserCreated = i.UserCreated,
                                 DateCreated = i.DateCreated,
                                 UserUpdated = i.UserUpdated,
                                 DateUpdated = i.DateUpdated,
                                 GroupId = i.GroupId,
                                 CategoryId = i.CategoryId,
                                 LongItemName = i.LongItemName,
                                 ItemDescription = i.ItemDescription,
                                 MakeId = i.MakeId,
                                 ModelId = i.ModelId,
                                 ProductId = i.ProductId,
                                 Make = make != null ? make.Name : null,
                                 Model = model != null ? model.Name : null,
                                 Product = product != null ? product.Name : null,
                                 Brand = i.Brand,
                                 ItemName = i.ItemName,
                                 ItemCode = i.ItemCode,
                                 InventoryType = i.InventoryType,
                                 Specification = i.Specification,
                                 Criticality = i.Criticality,
                                 StockToBank = i.StockToBank,
                                 LpRate = i.LpRate,
                                 IsActive = i.IsActive,
                                 ImageUrl = i.ImageUrl,
                                 UnitPrice = i.UnitPrice,
                                 UomId = i.UomId,
                                 CatNo = i.CatNo,
                                 InventoryMethodId = i.InventoryMethodId,
                                 Hsn = i.Hsn,
                                 TaxPercentage = i.TaxPercentage,
                                 ValuationMethodId = i.ValuationMethodId,
                                 ValuationMethodText = i.ValuationMethodText,
                                 RelatedStockAccount = i.RelatedStockAccount,
                                 Cf = i.Cf,
                                 BomApplicable = i.BomApplicable
                             }).FirstOrDefaultAsync();
            if (item == null) return NotFound();

            var planning = await _context.ItemPlannings.FirstOrDefaultAsync(p => p.ItemId == id);
            var packing = await _context.ItemUomPackingDetails.FirstOrDefaultAsync(p => p.ItemId == id);
            var accounting = await _context.ItemAccountingInfos.FirstOrDefaultAsync(a => a.ItemId == id);
            var stocks = await _context.ItemLocationStocks.Where(s => s.ItemId == id).ToListAsync();
            var qc = await _context.ItemQualityControls.FirstOrDefaultAsync(q => q.ItemId == id);
            var supplierRates = await (from rmi in _context.RateMasterItems
                                       join rm in _context.RateMasters on rmi.RateMasterId equals rm.Id
                                       where rmi.ItemId == id
                                       join s in _context.Suppliers on rmi.SupplierId equals s.Id into gj
                                       from s in gj.DefaultIfEmpty()
                                       select new SupplierResponse
                                       {
                                           SupplierId = s != null ? s.Id : 0,
                                           VendorName = s != null ? s.VendorName : null,
                                           VendorCode = s != null ? s.VendorCode : null,
                                           PurchaseRate = rmi.PurchaseRate,
                                           Description = rm.Remarks
                                       }).ToListAsync();

            var response = new ItemAggregateResponse
            {
                ItemMaster = item,
                ItemPlanning = planning,
                ItemUomPackingDetails = packing,
                ItemAccountingInfo = accounting,
                LocationStocks = stocks,
                ItemQualityControl = qc,
                Suppliers = supplierRates
            };

            return Ok(response);
        }

        // POST: api/ItemAggregate
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ItemAggregateRequest request)
        {
            if (request?.ItemMaster == null)
                return BadRequest("ItemMaster is required");

            const int maxAttempts = 5;
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                using (var tx = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        // Map ItemMasterRequest DTO -> ItemMaster entity
                        var itemEntity = MapDtoToEntity(request.ItemMaster);

                        // Ensure all DateTime fields are UTC
                        EnsureUtcDateTimes(itemEntity);

                        // Treat placeholder or empty values (e.g. the sample payload uses "string") as missing
                        if (!string.IsNullOrWhiteSpace(itemEntity.ItemCode))
                        {
                            var codeTrim = itemEntity.ItemCode.Trim();
                            if (string.Equals(codeTrim, "string", System.StringComparison.OrdinalIgnoreCase))
                                itemEntity.ItemCode = null;
                        }

                        // Auto-generate item code if missing
                        if (string.IsNullOrWhiteSpace(itemEntity.ItemCode))
                        {
                            itemEntity.ItemCode = await GenerateNextItemCodeAsync();
                        }

                        _context.ItemMasters.Add(itemEntity);
                        await _context.SaveChangesAsync();
                        var itemId = itemEntity.Id;

                        // Insert related entities (if provided)
                        if (request.ItemPlanning != null)
                        {
                            EnsureUtcDateTimes(request.ItemPlanning);
                            request.ItemPlanning.Id = 0; // Reset ID for new entity
                            request.ItemPlanning.ItemId = itemId;
                            _context.ItemPlannings.Add(request.ItemPlanning);
                        }
                        if (request.ItemUomPackingDetails != null)
                        {
                            EnsureUtcDateTimes(request.ItemUomPackingDetails);
                            request.ItemUomPackingDetails.Id = 0; // Reset ID for new entity
                            request.ItemUomPackingDetails.ItemId = itemId;
                            _context.ItemUomPackingDetails.Add(request.ItemUomPackingDetails);
                        }
                        if (request.ItemAccountingInfo != null)
                        {
                            EnsureUtcDateTimes(request.ItemAccountingInfo);
                            request.ItemAccountingInfo.Id = 0; // Reset ID for new entity
                            request.ItemAccountingInfo.ItemId = itemId;
                            _context.ItemAccountingInfos.Add(request.ItemAccountingInfo);
                        }
                        if (request.LocationStocks != null && request.LocationStocks.Any())
                        {
                            foreach (var s in request.LocationStocks)
                            {
                                EnsureUtcDateTimes(s);
                                s.Id = 0; // Reset ID for new entity
                                s.ItemId = itemId;
                                var existingLoc = await _context.ItemLocationStocks.FirstOrDefaultAsync(x => x.ItemId == itemId
                                                                                                         && x.Rack == s.Rack
                                                                                                         && x.Shelf == s.Shelf
                                                                                                         && x.ColumnNo == s.ColumnNo);
                                if (existingLoc == null)
                                {
                                    _context.ItemLocationStocks.Add(s);
                                }
                                else
                                {
                                    s.Id = existingLoc.Id;
                                    _context.Entry(existingLoc).CurrentValues.SetValues(s);
                                }
                            }
                        }
                        if (request.ItemQualityControl != null)
                        {
                            EnsureUtcDateTimes(request.ItemQualityControl);
                            request.ItemQualityControl.Id = 0; // Reset ID for new entity
                            request.ItemQualityControl.ItemId = itemId;
                            _context.ItemQualityControls.Add(request.ItemQualityControl);
                        }

                        // Suppliers + RateMaster
                        if (request.Suppliers != null && request.Suppliers.Any())
                        {
                            foreach (var sp in request.Suppliers)
                            {
                                if (string.IsNullOrWhiteSpace(sp.VendorCode) && string.IsNullOrWhiteSpace(sp.VendorName))
                                    continue;

                                Supplier supplier = null;
                                if (!string.IsNullOrWhiteSpace(sp.VendorCode))
                                {
                                    supplier = await _context.Suppliers.FirstOrDefaultAsync(s => s.VendorCode == sp.VendorCode);
                                }
                                if (supplier == null && !string.IsNullOrWhiteSpace(sp.VendorName))
                                {
                                    supplier = await _context.Suppliers.FirstOrDefaultAsync(s => s.VendorName == sp.VendorName);
                                }

                                if (supplier == null)
                                {
                                    supplier = new Supplier
                                    {
                                        VendorCode = sp.VendorCode,
                                        VendorName = sp.VendorName,
                                        Address = sp.Description,
                                        CreatedAt = DateTime.UtcNow,
                                        UpdatedAt = DateTime.UtcNow,
                                        IsActive = true
                                    };
                                    _context.Suppliers.Add(supplier);
                                    await _context.SaveChangesAsync();
                                }

                                    // If ItemMaster doesn't have a supplier set yet, set it to this supplier
                                    if (itemEntity.SupplierId == null || itemEntity.SupplierId == 0)
                                    {
                                        itemEntity.SupplierId = supplier.Id;
                                        // itemEntity is tracked; change will be saved by the pending SaveChangesAsync
                                    }

                                // create rate master and rate master item for this supplier and item
                                var rateMaster = new RateMaster
                                {
                                    RateMasterId = await GenerateRateMasterId(),
                                    DocDate = DateTime.UtcNow,
                                    EffectiveDate = DateTime.UtcNow,
                                    Type = "Supplier Rate",
                                    Remarks = sp.Description,
                                    UserCreated = 1, // You may want to get this from current user context
                                    DateCreated = DateTime.UtcNow
                                };
                                _context.RateMasters.Add(rateMaster);
                                await _context.SaveChangesAsync();

                                var rateMasterItem = new RateMasterItem
                                {
                                    RateMasterId = rateMaster.Id,
                                    ItemId = itemId,
                                    SupplierId = supplier.Id,
                                    PurchaseRate = sp.Rate,
                                    CurrencyType = "INR" // Default currency, you may want to make this configurable
                                };
                                _context.RateMasterItems.Add(rateMasterItem);
                            }
                        }

                        await _context.SaveChangesAsync();
                        await tx.CommitAsync();

                        // Load inserted aggregate to return
                        var createdItem = await _context.ItemMasters.FindAsync(itemId);
                        var createdPlanning = await _context.ItemPlannings.FirstOrDefaultAsync(p => p.ItemId == itemId);
                        var createdPacking = await _context.ItemUomPackingDetails.FirstOrDefaultAsync(p => p.ItemId == itemId);
                        var createdAccounting = await _context.ItemAccountingInfos.FirstOrDefaultAsync(a => a.ItemId == itemId);
                        var createdStocks = await _context.ItemLocationStocks.Where(s => s.ItemId == itemId).ToListAsync();
                        var createdQc = await _context.ItemQualityControls.FirstOrDefaultAsync(q => q.ItemId == itemId);
                        var createdSupplierRates = await (from rmi in _context.RateMasterItems
                                                         join rm in _context.RateMasters on rmi.RateMasterId equals rm.Id
                                                         where rmi.ItemId == itemId
                                                         join s in _context.Suppliers on rmi.SupplierId equals s.Id into gj
                                                         from s in gj.DefaultIfEmpty()
                                                         select new SupplierResponse
                                                         {
                                                            SupplierId = s != null ? s.Id : 0,
                                                            VendorName = s != null ? s.VendorName : null,
                                                            VendorCode = s != null ? s.VendorCode : null,
                                                            PurchaseRate = rmi.PurchaseRate,
                                                            Description = rm.Remarks
                                                         }).ToListAsync();

                        var createdResponse = new ItemAggregateResponse
                        {
                            ItemMaster = createdItem,
                            ItemPlanning = createdPlanning,
                            ItemUomPackingDetails = createdPacking,
                            ItemAccountingInfo = createdAccounting,
                            LocationStocks = createdStocks,
                            ItemQualityControl = createdQc,
                            Suppliers = createdSupplierRates
                        };

                        return CreatedAtAction(nameof(GetById), new { id = itemId }, createdResponse);
                    }
                    catch (DbUpdateException dbEx)
                    {
                        await tx.RollbackAsync();
                        // If unique constraint on item_code, retry generating a new code
                        if (IsUniqueConstraintViolationOnItemCode(dbEx) && attempt < maxAttempts - 1)
                        {
                            // small delay to reduce tight loop (helps with concurrency)
                            await Task.Delay(50);
                            continue;
                        }
                        throw;
                    }
                    catch
                    {
                        await tx.RollbackAsync();
                        throw;
                    }
                }
            }

            return StatusCode(500, "Failed to create item after multiple attempts due to duplicate item codes.");
        }

        // PUT: api/ItemAggregate/5
        [HttpPut("{id}")]
        public async Task<ActionResult<ItemAggregateResponse>> Put(int id, [FromBody] ItemAggregateRequest request)
        {
            if (request?.ItemMaster == null)
                return BadRequest();

            const int maxAttempts = 5;
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                // Reload existing item to ensure fresh state on retry
                var existingItem = await _context.ItemMasters.FindAsync(id);
                if (existingItem == null) return NotFound();

                using (var tx = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        // Update ItemMaster: map DTO to entity and set values on existing
                        var updatedEntity = MapDtoToEntity(request.ItemMaster);
                        updatedEntity.Id = id;

                        // Cleanup ItemCode if provided as placeholder
                        if (!string.IsNullOrWhiteSpace(updatedEntity.ItemCode))
                        {
                            var codeTrim = updatedEntity.ItemCode.Trim();
                            if (string.Equals(codeTrim, "string", System.StringComparison.OrdinalIgnoreCase))
                                updatedEntity.ItemCode = null;
                        }

                        // Auto-generate item code if missing
                        if (string.IsNullOrWhiteSpace(updatedEntity.ItemCode))
                        {
                            updatedEntity.ItemCode = await GenerateNextItemCodeAsync();
                        }

                        // Ensure all DateTime fields are UTC
                        EnsureUtcDateTimes(updatedEntity);
                        _context.Entry(existingItem).CurrentValues.SetValues(updatedEntity);

                        // Planning
                        if (request.ItemPlanning != null)
                        {
                            EnsureUtcDateTimes(request.ItemPlanning);
                            var existing = await _context.ItemPlannings.FirstOrDefaultAsync(p => p.ItemId == id);
                            if (existing == null)
                            {
                                request.ItemPlanning.ItemId = id;
                                _context.ItemPlannings.Add(request.ItemPlanning);
                            }
                            else
                            {
                                request.ItemPlanning.Id = existing.Id;
                                request.ItemPlanning.ItemId = id;
                                _context.Entry(existing).CurrentValues.SetValues(request.ItemPlanning);
                            }
                        }

                        // Packing
                        if (request.ItemUomPackingDetails != null)
                        {
                            EnsureUtcDateTimes(request.ItemUomPackingDetails);
                            var existing = await _context.ItemUomPackingDetails.FirstOrDefaultAsync(p => p.ItemId == id);
                            if (existing == null)
                            {
                                request.ItemUomPackingDetails.ItemId = id;
                                _context.ItemUomPackingDetails.Add(request.ItemUomPackingDetails);
                            }
                            else
                            {
                                request.ItemUomPackingDetails.Id = existing.Id;
                                request.ItemUomPackingDetails.ItemId = id;
                                _context.Entry(existing).CurrentValues.SetValues(request.ItemUomPackingDetails);
                            }
                        }

                        // Accounting
                        if (request.ItemAccountingInfo != null)
                        {
                            EnsureUtcDateTimes(request.ItemAccountingInfo);
                            var existing = await _context.ItemAccountingInfos.FirstOrDefaultAsync(a => a.ItemId == id);
                            if (existing == null)
                            {
                                request.ItemAccountingInfo.ItemId = id;
                                _context.ItemAccountingInfos.Add(request.ItemAccountingInfo);
                            }
                            else
                            {
                                request.ItemAccountingInfo.Id = existing.Id;
                                request.ItemAccountingInfo.ItemId = id;
                                _context.Entry(existing).CurrentValues.SetValues(request.ItemAccountingInfo);
                            }
                        }

                        // Location stocks: upsert provided stocks and remove obsolete ones
                        if (request.LocationStocks != null)
                        {
                            var existingStocksList = await _context.ItemLocationStocks.Where(s => s.ItemId == id).ToListAsync();

                            // Upsert incoming
                            foreach (var s in request.LocationStocks)
                            {
                                EnsureUtcDateTimes(s);
                                s.ItemId = id;
                                var match = existingStocksList.FirstOrDefault(x => x.Rack == s.Rack
                                                                                    && x.Shelf == s.Shelf
                                                                                    && x.ColumnNo == s.ColumnNo);
                                if (match == null)
                                {
                                    _context.ItemLocationStocks.Add(s);
                                }
                                else
                                {
                                    s.Id = match.Id;
                                    _context.Entry(match).CurrentValues.SetValues(s);
                                }
                            }

                            // Remove any existing stocks not present in incoming list
                            var toRemove = existingStocksList.Where(e => !request.LocationStocks.Any(r => r.Rack == e.Rack
                                                                                                            && r.Shelf == e.Shelf
                                                                                                            && r.ColumnNo == e.ColumnNo)).ToList();
                            if (toRemove.Any()) _context.ItemLocationStocks.RemoveRange(toRemove);
                        }

                        // QC
                        if (request.ItemQualityControl != null)
                        {
                            EnsureUtcDateTimes(request.ItemQualityControl);
                            var existing = await _context.ItemQualityControls.FirstOrDefaultAsync(q => q.ItemId == id);
                            if (existing == null)
                            {
                                request.ItemQualityControl.ItemId = id;
                                _context.ItemQualityControls.Add(request.ItemQualityControl);
                            }
                            else
                            {
                                request.ItemQualityControl.Id = existing.Id;
                                request.ItemQualityControl.ItemId = id;
                                _context.Entry(existing).CurrentValues.SetValues(request.ItemQualityControl);
                            }
                        }

                        // Suppliers + RateMaster (update or create)
                        if (request.Suppliers != null && request.Suppliers.Any())
                        {
                            foreach (var sp in request.Suppliers)
                            {
                                if (string.IsNullOrWhiteSpace(sp.VendorCode) && string.IsNullOrWhiteSpace(sp.VendorName))
                                    continue;

                                Supplier supplier = null;
                                if (!string.IsNullOrWhiteSpace(sp.VendorCode))
                                {
                                    supplier = await _context.Suppliers.FirstOrDefaultAsync(s => s.VendorCode == sp.VendorCode);
                                }
                                if (supplier == null && !string.IsNullOrWhiteSpace(sp.VendorName))
                                {
                                    supplier = await _context.Suppliers.FirstOrDefaultAsync(s => s.VendorName == sp.VendorName);
                                }

                                if (supplier == null)
                                {
                                    supplier = new Supplier
                                    {
                                        VendorCode = sp.VendorCode,
                                        VendorName = sp.VendorName,
                                        Address = sp.Description,
                                        CreatedAt = DateTime.UtcNow,
                                        UpdatedAt = DateTime.UtcNow,
                                        IsActive = true
                                    };
                                    _context.Suppliers.Add(supplier);
                                    await _context.SaveChangesAsync();
                                }
                                else
                                {
                                    // update basic fields
                                    supplier.VendorName = sp.VendorName ?? supplier.VendorName;
                                    supplier.Address = sp.Description ?? supplier.Address;
                                    supplier.UpdatedAt = DateTime.UtcNow;
                                    _context.Entry(supplier).State = EntityState.Modified;
                                    await _context.SaveChangesAsync();
                                }

                                    // If ItemMaster doesn't have a supplier set yet, set it to this supplier
                                    if (existingItem.SupplierId == null || existingItem.SupplierId == 0)
                                    {
                                        existingItem.SupplierId = supplier.Id;
                                        // Entry already tracked; will be saved by the pending SaveChangesAsync
                                    }

                                // find existing rate for this item+supplier
                                var existingRateItem = await _context.RateMasterItems
                                    .FirstOrDefaultAsync(rmi => rmi.ItemId == id && rmi.SupplierId == supplier.Id);
                                
                                if (existingRateItem == null)
                                {
                                    // Create new rate master and item
                                    var rateMaster = new RateMaster
                                    {
                                        RateMasterId = await GenerateRateMasterId(),
                                        DocDate = DateTime.UtcNow,
                                        EffectiveDate = DateTime.UtcNow,
                                        Type = "Supplier Rate",
                                        Remarks = sp.Description,
                                        UserUpdated = 1, // You may want to get this from current user context
                                        DateUpdated = DateTime.UtcNow
                                    };
                                    _context.RateMasters.Add(rateMaster);
                                    await _context.SaveChangesAsync();

                                    var rateMasterItem = new RateMasterItem
                                    {
                                        RateMasterId = rateMaster.Id,
                                        ItemId = id,
                                        SupplierId = supplier.Id,
                                        PurchaseRate = sp.Rate,
                                        CurrencyType = "INR"
                                    };
                                    _context.RateMasterItems.Add(rateMasterItem);
                                }
                                else
                                {
                                    // Update existing rate item
                                    existingRateItem.PurchaseRate = sp.Rate ?? existingRateItem.PurchaseRate;
                                    _context.Entry(existingRateItem).State = EntityState.Modified;
                                    
                                    // Update the related rate master
                                    var rateMaster = await _context.RateMasters.FindAsync(existingRateItem.RateMasterId);
                                    if (rateMaster != null)
                                    {
                                        rateMaster.Remarks = sp.Description ?? rateMaster.Remarks;
                                        rateMaster.DateUpdated = DateTime.UtcNow;
                                        _context.Entry(rateMaster).State = EntityState.Modified;
                                    }
                                }
                            }
                        }

                        await _context.SaveChangesAsync();
                        await tx.CommitAsync();

                        // Load updated aggregate and return it
                        var updatedItem = await _context.ItemMasters.FindAsync(id);
                        if (updatedItem == null) return NotFound();

                        var updatedPlanning = await _context.ItemPlannings.FirstOrDefaultAsync(p => p.ItemId == id);
                        var updatedPacking = await _context.ItemUomPackingDetails.FirstOrDefaultAsync(p => p.ItemId == id);
                        var updatedAccounting = await _context.ItemAccountingInfos.FirstOrDefaultAsync(a => a.ItemId == id);
                        var updatedStocks = await _context.ItemLocationStocks.Where(s => s.ItemId == id).ToListAsync();
                        var updatedQc = await _context.ItemQualityControls.FirstOrDefaultAsync(q => q.ItemId == id);
                        var updatedSupplierRates = await (from rmi in _context.RateMasterItems
                                                          join rm in _context.RateMasters on rmi.RateMasterId equals rm.Id
                                                          where rmi.ItemId == id
                                                          join s in _context.Suppliers on rmi.SupplierId equals s.Id into gj
                                                          from s in gj.DefaultIfEmpty()
                                                          select new SupplierResponse
                                                          {
                                                              // SupplierId removed
                                                              VendorName = s != null ? s.VendorName : null,
                                                              VendorCode = s != null ? s.VendorCode : null,
                                                              PurchaseRate = rmi.PurchaseRate,
                                                              Description = rm.Remarks
                                                          }).ToListAsync();

                        var updatedResponse = new ItemAggregateResponse
                        {
                            ItemMaster = updatedItem,
                            ItemPlanning = updatedPlanning,
                            ItemUomPackingDetails = updatedPacking,
                            ItemAccountingInfo = updatedAccounting,
                            LocationStocks = updatedStocks,
                            ItemQualityControl = updatedQc,
                            Suppliers = updatedSupplierRates
                        };

                        return Ok(updatedResponse);
                    }
                    catch (DbUpdateException dbEx)
                    {
                        await tx.RollbackAsync();
                        // If unique constraint on item_code, retry generating a new code
                        if (IsUniqueConstraintViolationOnItemCode(dbEx) && attempt < maxAttempts - 1)
                        {
                            await Task.Delay(50);
                            // Detach existingItem to avoid conflict on next FindAsync or SaveChanges
                            _context.Entry(existingItem).State = EntityState.Detached;
                            continue;
                        }
                        throw;
                    }
                    catch
                    {
                        await tx.RollbackAsync();
                        throw;
                    }
                }
            }
            return StatusCode(500, "Failed to update item after multiple attempts due to duplicate item codes.");
        }

        // DELETE: api/ItemAggregate/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.ItemMasters.FindAsync(id);
            if (item == null) return NotFound();

            using (var tx = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Remove related entities
                    var pl = _context.ItemPlannings.Where(p => p.ItemId == id);
                    var pack = _context.ItemUomPackingDetails.Where(p => p.ItemId == id);
                    var acc = _context.ItemAccountingInfos.Where(a => a.ItemId == id);
                    var stocks = _context.ItemLocationStocks.Where(s => s.ItemId == id);
                    var qc = _context.ItemQualityControls.Where(q => q.ItemId == id);

                    _context.ItemPlannings.RemoveRange(pl);
                    _context.ItemUomPackingDetails.RemoveRange(pack);
                    _context.ItemAccountingInfos.RemoveRange(acc);
                    _context.ItemLocationStocks.RemoveRange(stocks);
                    _context.ItemQualityControls.RemoveRange(qc);

                    _context.ItemMasters.Remove(item);

                    await _context.SaveChangesAsync();
                    await tx.CommitAsync();
                    return NoContent();
                }
                catch
                {
                    await tx.RollbackAsync();
                    throw;
                }
            }
        }

        private async Task<string> GenerateRateMasterId()
        {
            var currentDate = DateTime.Now;
            var currentYear = currentDate.Year;
            var nextYear = currentYear + 1;
            var yearSuffix = $"{currentYear.ToString().Substring(2)}-{nextYear.ToString().Substring(2)}";
            
            // Get the count of rate masters for current financial year
            var yearStart = new DateTime(currentYear, 4, 1); // Assuming financial year starts from April
            var yearEnd = yearStart.AddYears(1);
            
            var count = await _context.RateMasters
                .Where(rm => rm.DateCreated >= yearStart && rm.DateCreated < yearEnd)
                .CountAsync();
            
            var sequenceNumber = (count + 1).ToString("D3");
            
            return $"RM/{yearSuffix}/{sequenceNumber}";
        }
    }
}
