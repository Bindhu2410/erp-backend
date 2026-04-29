
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ERP.API.Models;
using ERP.API.Models.DTOs;
// DTOs for request and response
public class PurchaseRequisitionItemDto
{
    public int ItemId { get; set; }
    public int Quantity { get; set; }
}

public class PurchaseRequisitionRequest
{
    public string PurchaseRequisitionId { get; set; }
    public string RequesterName { get; set; }
    public string Description { get; set; }
    public int? SupplierId { get; set; }
    public List<PurchaseRequisitionItemDto> Items { get; set; }
    public DateTime? DeliveryDate { get; set; }
    public decimal? BudgetAmount { get; set; }
    public string Status { get; set; }
    public int? UserCreated { get; set; }
    public int? UserUpdated { get; set; }
}

public class BomDetailsDto
{
    public string BomName { get; set; }
    public string BomType { get; set; }
    public List<BomChildItemDto> ChildItems { get; set; }
}


public class PurchaseRequisitionItemResponse
{
    public int ItemId { get; set; }
    public int Quantity { get; set; }
    public string? Make { get; set; }
    public string? Model { get; set; }
    public string? CategoryName { get; set; }
    public string? Product { get; set; }
    public string? Brand { get; set; }
    public string? ItemName { get; set; }
    public string? ItemCode { get; set; }
    public decimal? UnitPrice { get; set; }
    public string? Hsn { get; set; }
    public decimal? TaxPercentage { get; set; }
    public string? UomName { get; set; }
    public string? CatNo { get; set; }
    public string? ValuationMethodName { get; set; }
}

public class PurchaseRequisitionResponse
{
    public int Id { get; set; }
    public string PurchaseRequisitionId { get; set; }
    public string RequesterName { get; set; }
    public string Description { get; set; }
    public int? SupplierId { get; set; }
    public string VendorName { get; set; }
    public DateTime? DeliveryDate { get; set; }
    public decimal? BudgetAmount { get; set; }
    public string Status { get; set; }
    public int? UserCreated { get; set; }
    public int? UserUpdated { get; set; }
    public List<PurchaseRequisitionItemResponse> Items { get; set; }
}

namespace ERP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PurchaseRequisitionsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PurchaseRequisitionsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/PurchaseRequisitions
        [HttpGet("Dropdown-po")]
        [ProducesResponseType(typeof(IEnumerable<PurchaseRequisitionResponse>), 200)]
        public async Task<ActionResult<IEnumerable<PurchaseRequisitionResponse>>> GetPurchaseRequisitions()
        {
            var list = await _context.PurchaseRequisitions
                .Where(pr => pr.Status == "Approved")
                .ToListAsync();
            var result = new List<PurchaseRequisitionResponse>();
            foreach (var pr in list)
            {
                var itemLinks = await _context.Set<PurchaseRequisitionBom>()
                    .Where(x => x.PurchaseRequisitionId == pr.Id)
                    .ToListAsync();
                var itemResponses = new List<PurchaseRequisitionItemResponse>();
                foreach (var x in itemLinks)
                {
                    var item = await _context.ItemMasters.FirstOrDefaultAsync(im => im.Id == x.ItemId);
                    string? categoryName = null;
                    string? uomName = null;
                    string? valuationMethodName = null;
                    if (item?.CategoryId != null)
                    {
                        var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == item.CategoryId);
                        categoryName = category?.Name;
                    }
                    if (item?.UomId != null)
                    {
                        var uom = await _context.Uoms.FirstOrDefaultAsync(u => u.Id == item.UomId);
                        uomName = uom?.Code;
                    }
                    if (item?.ValuationMethodId != null)
                    {
                        var valuationMethod = await _context.ValuationMethods.FirstOrDefaultAsync(v => v.Id == item.ValuationMethodId);
                        valuationMethodName = valuationMethod?.Name;
                    }
                    itemResponses.Add(new PurchaseRequisitionItemResponse {
                        ItemId = x.ItemId,
                        Quantity = x.Quantity,
                        Make = item?.Make,
                        Model = item?.Model,
                        CategoryName = categoryName,
                        Product = item?.Product,
                        Brand = item?.Brand,
                        ItemName = item?.ItemName,
                        ItemCode = item?.ItemCode,
                        UnitPrice = item?.UnitPrice,
                        Hsn = item?.Hsn,
                        TaxPercentage = item?.TaxPercentage,
                        UomName = uomName,
                        CatNo = item?.CatNo,
                        ValuationMethodName = valuationMethodName
                    });
                }
                Supplier supplier = null;
                if (pr.SupplierId != null)
                {
                    supplier = await _context.Suppliers.FirstOrDefaultAsync(s => s.Id == pr.SupplierId);
                }
                result.Add(new PurchaseRequisitionResponse {
                    Id = pr.Id,
                    PurchaseRequisitionId = pr.PurchaseRequisitionId,
                    RequesterName = pr.RequesterName,
                    Description = pr.Description,
                    SupplierId = pr.SupplierId,
                    VendorName = supplier?.VendorName,
                    DeliveryDate = pr.DeliveryDate,
                    BudgetAmount = pr.BudgetAmount,
                    Status = pr.Status,
                    UserCreated = pr.UserCreated,
                    UserUpdated = pr.UserUpdated,
                    Items = itemResponses
                });
            }
            return Ok(result);
        }
         [HttpGet("all")]
        [ProducesResponseType(typeof(IEnumerable<PurchaseRequisitionResponse>), 200)]
        public async Task<ActionResult<IEnumerable<PurchaseRequisitionResponse>>> GetAllPurchaseRequisitions()
        {
            var list = await _context.PurchaseRequisitions.ToListAsync();
              
            var result = new List<PurchaseRequisitionResponse>();
            foreach (var pr in list)
            {
                var itemLinks = await _context.Set<PurchaseRequisitionBom>()
                    .Where(x => x.PurchaseRequisitionId == pr.Id)
                    .ToListAsync();
                var itemResponses = new List<PurchaseRequisitionItemResponse>();
                foreach (var x in itemLinks)
                {
                    var item = await _context.ItemMasters.FirstOrDefaultAsync(im => im.Id == x.ItemId);
                    string? categoryName = null;
                    string? uomName = null;
                    string? valuationMethodName = null;
                    if (item?.CategoryId != null)
                    {
                        var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == item.CategoryId);
                        categoryName = category?.Name;
                    }
                    if (item?.UomId != null)
                    {
                        var uom = await _context.Uoms.FirstOrDefaultAsync(u => u.Id == item.UomId);
                        uomName = uom?.Code;
                    }
                    if (item?.ValuationMethodId != null)
                    {
                        var valuationMethod = await _context.ValuationMethods.FirstOrDefaultAsync(v => v.Id == item.ValuationMethodId);
                        valuationMethodName = valuationMethod?.Name;
                    }
                    itemResponses.Add(new PurchaseRequisitionItemResponse {
                        ItemId = x.ItemId,
                        Quantity = x.Quantity,
                        Make = item?.Make,
                        Model = item?.Model,
                        CategoryName = categoryName,
                        Product = item?.Product,
                        Brand = item?.Brand,
                        ItemName = item?.ItemName,
                        ItemCode = item?.ItemCode,
                        UnitPrice = item?.UnitPrice,
                        Hsn = item?.Hsn,
                        TaxPercentage = item?.TaxPercentage,
                        UomName = uomName,
                        CatNo = item?.CatNo,
                        ValuationMethodName = valuationMethodName
                    });
                }
                Supplier supplier = null;
                if (pr.SupplierId != null)
                {
                    supplier = await _context.Suppliers.FirstOrDefaultAsync(s => s.Id == pr.SupplierId);
                }
                result.Add(new PurchaseRequisitionResponse {
                    Id = pr.Id,
                    PurchaseRequisitionId = pr.PurchaseRequisitionId,
                    RequesterName = pr.RequesterName,
                    Description = pr.Description,
                    SupplierId = pr.SupplierId,
                    VendorName = supplier?.VendorName,
                    DeliveryDate = pr.DeliveryDate,
                    BudgetAmount = pr.BudgetAmount,
                    Status = pr.Status,
                    UserCreated = pr.UserCreated,
                    UserUpdated = pr.UserUpdated,
                    Items = itemResponses
                });
            }
            return Ok(result);
        }

        // GET: api/PurchaseRequisitions/5
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(PurchaseRequisitionResponse), 200)]
        public async Task<ActionResult<PurchaseRequisitionResponse>> GetPurchaseRequisition(int id)
        {
            var pr = await _context.PurchaseRequisitions.FirstOrDefaultAsync(x => x.Id == id);
            if (pr == null)
                return NotFound();
            var itemLinks = await _context.Set<PurchaseRequisitionBom>()
                .Where(x => x.PurchaseRequisitionId == pr.Id)
                .ToListAsync();
            var itemResponses = new List<PurchaseRequisitionItemResponse>();
            foreach (var x in itemLinks)
            {
                var item = await _context.ItemMasters.FirstOrDefaultAsync(im => im.Id == x.ItemId);
                string? categoryName = null;
                string? uomName = null;
                string? valuationMethodName = null;
                if (item?.CategoryId != null)
                {
                    var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == item.CategoryId);
                    categoryName = category?.Name;
                }
                if (item?.UomId != null)
                {
                    var uom = await _context.Uoms.FirstOrDefaultAsync(u => u.Id == item.UomId);
                    uomName = uom?.Code;
                }
                if (item?.ValuationMethodId != null)
                {
                    var valuationMethod = await _context.ValuationMethods.FirstOrDefaultAsync(v => v.Id == item.ValuationMethodId);
                    valuationMethodName = valuationMethod?.Name;
                }
                itemResponses.Add(new PurchaseRequisitionItemResponse {
                    ItemId = x.ItemId,
                    Quantity = x.Quantity,
                    Make = item?.Make,
                    Model = item?.Model,
                    CategoryName = categoryName,
                    Product = item?.Product,
                    Brand = item?.Brand,
                    ItemName = item?.ItemName,
                    ItemCode = item?.ItemCode,
                    UnitPrice = item?.UnitPrice,
                    Hsn = item?.Hsn,
                    TaxPercentage = item?.TaxPercentage,
                    UomName = uomName,
                    CatNo = item?.CatNo,
                    ValuationMethodName = valuationMethodName
                });
            }
            Supplier supplier = null;
            if (pr.SupplierId != null)
            {
                supplier = await _context.Suppliers.FirstOrDefaultAsync(s => s.Id == pr.SupplierId);
            }
            var result = new PurchaseRequisitionResponse {
                Id = pr.Id,
                PurchaseRequisitionId = pr.PurchaseRequisitionId,
                RequesterName = pr.RequesterName,
                Description = pr.Description,
                SupplierId = pr.SupplierId,
                VendorName = supplier?.VendorName,
                DeliveryDate = pr.DeliveryDate,
                BudgetAmount = pr.BudgetAmount,
                Status = pr.Status,
                UserCreated = pr.UserCreated,
                UserUpdated = pr.UserUpdated,
                Items = itemResponses
            };
            return Ok(result);
        }

        // POST: api/PurchaseRequisitions
        [HttpPost]
        [ProducesResponseType(typeof(PurchaseRequisitionResponse), 201)]
        public async Task<ActionResult<PurchaseRequisitionResponse>> CreatePurchaseRequisition([FromBody] PurchaseRequisitionRequest req)
        {
            if (req == null)
                return BadRequest();

            // Generate PurchaseRequisitionId: PR/YY-YY/NN
            var currentYear = System.DateTime.Now.Year;
            var nextYear = currentYear + 1;
            string yearPart = $"{currentYear % 100:D2}-{nextYear % 100:D2}";
            int count = await _context.PurchaseRequisitions.CountAsync(pr => pr.PurchaseRequisitionId != null && pr.PurchaseRequisitionId.Contains($"PR/{yearPart}/"));
            int nextSeq = count + 1;
            string seqPart = nextSeq.ToString("D2");
            string generatedId = $"PR/{yearPart}/{seqPart}";


            var entity = new PurchaseRequisition {
                PurchaseRequisitionId = generatedId,
                RequesterName = req.RequesterName,
                Description = req.Description,
                DeliveryDate = req.DeliveryDate,
                BudgetAmount = req.BudgetAmount,
                Status = req.Status,
                UserCreated = req.UserCreated,
                UserUpdated = req.UserUpdated,
                SupplierId = req.SupplierId
            };
            _context.PurchaseRequisitions.Add(entity);
            await _context.SaveChangesAsync();

            // Save item links
            if (req.Items != null)
            {
                foreach (var item in req.Items)
                {
                    var itemLink = new PurchaseRequisitionBom {
                        PurchaseRequisitionId = entity.Id,
                        ItemId = item.ItemId,
                        Quantity = item.Quantity
                    };
                    _context.Set<PurchaseRequisitionBom>().Add(itemLink);
                }
                await _context.SaveChangesAsync();
            }
            // Build response with full item details
            var itemLinks = await _context.Set<PurchaseRequisitionBom>()
                .Where(x => x.PurchaseRequisitionId == entity.Id)
                .ToListAsync();
            var itemResponses = new List<PurchaseRequisitionItemResponse>();
            foreach (var x in itemLinks)
            {
                var item = await _context.ItemMasters.FirstOrDefaultAsync(im => im.Id == x.ItemId);
                string? categoryName = null;
                string? uomName = null;
                string? valuationMethodName = null;
                if (item?.CategoryId != null)
                {
                    var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == item.CategoryId);
                    categoryName = category?.Name;
                }
                if (item?.UomId != null)
                {
                    var uom = await _context.Uoms.FirstOrDefaultAsync(u => u.Id == item.UomId);
                    uomName = uom?.Code;
                }
                if (item?.ValuationMethodId != null)
                {
                    var valuationMethod = await _context.ValuationMethods.FirstOrDefaultAsync(v => v.Id == item.ValuationMethodId);
                    valuationMethodName = valuationMethod?.Name;
                }
                itemResponses.Add(new PurchaseRequisitionItemResponse {
                    ItemId = x.ItemId,
                    Quantity = x.Quantity,
                    Make = item?.Make,
                    Model = item?.Model,
                    CategoryName = categoryName,
                    Product = item?.Product,
                    Brand = item?.Brand,
                    ItemName = item?.ItemName,
                    ItemCode = item?.ItemCode,
                    UnitPrice = item?.UnitPrice,
                    Hsn = item?.Hsn,
                    TaxPercentage = item?.TaxPercentage,
                    UomName = uomName,
                    CatNo = item?.CatNo,
                    ValuationMethodName = valuationMethodName
                });
            }
            Supplier supplier = null;
            if (entity.SupplierId != null)
            {
                supplier = await _context.Suppliers.FirstOrDefaultAsync(s => s.Id == entity.SupplierId);
            }
            var result = new PurchaseRequisitionResponse {
                Id = entity.Id,
                PurchaseRequisitionId = entity.PurchaseRequisitionId,
                RequesterName = entity.RequesterName,
                Description = entity.Description,
                SupplierId = entity.SupplierId,
                DeliveryDate = entity.DeliveryDate,
                BudgetAmount = entity.BudgetAmount,
                Status = entity.Status,
                UserCreated = entity.UserCreated,
                UserUpdated = entity.UserUpdated,
                Items = itemResponses,
                VendorName = supplier?.VendorName
            };
            // ...existing code...
            return CreatedAtAction(nameof(GetPurchaseRequisition), new { id = entity.Id }, result);
        }

        // PUT: api/PurchaseRequisitions/5
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(PurchaseRequisitionResponse), 200)]
        public async Task<ActionResult<PurchaseRequisitionResponse>> UpdatePurchaseRequisition([FromRoute] int id, [FromBody] PurchaseRequisitionRequest req)
        {
            if (req == null)
                return BadRequest();
            var entity = await _context.PurchaseRequisitions.FindAsync(id);
            if (entity == null)
                return NotFound();
            // Do not update PurchaseRequisitionId to avoid foreign key violation
            // entity.PurchaseRequisitionId = req.PurchaseRequisitionId;
            entity.RequesterName = req.RequesterName;
            entity.Description = req.Description;
            // entity.Quantity removed
            if (req.DeliveryDate.HasValue)
                entity.DeliveryDate = DateTime.SpecifyKind(req.DeliveryDate.Value, DateTimeKind.Utc);
            else
                entity.DeliveryDate = null;
            entity.BudgetAmount = req.BudgetAmount;
            entity.Status = req.Status;
            entity.UserCreated = req.UserCreated;
            entity.UserUpdated = req.UserUpdated;
            // Ensure all DateTime fields are UTC
            if (entity.DateCreated.Kind != DateTimeKind.Utc)
                entity.DateCreated = DateTime.SpecifyKind(entity.DateCreated, DateTimeKind.Utc);
            entity.SetDateUpdated(DateTime.UtcNow);
            _context.Entry(entity).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.PurchaseRequisitions.Any(e => e.Id == id))
                    return NotFound();
                else
                    throw;
            }

            // Update item links
            var existingItemLinks = _context.Set<PurchaseRequisitionBom>().Where(x => x.PurchaseRequisitionId == entity.Id);
            _context.Set<PurchaseRequisitionBom>().RemoveRange(existingItemLinks);
            if (req.Items != null)
            {
                foreach (var item in req.Items)
                {
                    var itemLink = new PurchaseRequisitionBom {
                        PurchaseRequisitionId = entity.Id,
                        ItemId = item.ItemId,
                        Quantity = item.Quantity
                    };
                    _context.Set<PurchaseRequisitionBom>().Add(itemLink);
                }
                await _context.SaveChangesAsync();
            }

            // Build response
            var itemLinks = await _context.Set<PurchaseRequisitionBom>()
                .Where(x => x.PurchaseRequisitionId == entity.Id)
                .ToListAsync();
            var itemResponses = itemLinks.Select(x => new PurchaseRequisitionItemResponse {
                ItemId = x.ItemId,
                Quantity = x.Quantity
            }).ToList();
            var result = new PurchaseRequisitionResponse {
                Id = entity.Id,
                PurchaseRequisitionId = entity.PurchaseRequisitionId,
                RequesterName = entity.RequesterName,
                Description = entity.Description,
                SupplierId = entity.SupplierId,
                DeliveryDate = entity.DeliveryDate,
                BudgetAmount = entity.BudgetAmount,
                Status = entity.Status,
                UserCreated = entity.UserCreated,
                UserUpdated = entity.UserUpdated,
                Items = itemResponses
            };
            return Ok(result);
        }

        // DELETE: api/PurchaseRequisitions/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePurchaseRequisition(int id)
        {
            var pr = await _context.PurchaseRequisitions.FindAsync(id);
            if (pr == null)
                return NotFound();
            _context.PurchaseRequisitions.Remove(pr);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // PATCH: api/PurchaseRequisitions/{id}
        public class PurchaseRequisitionPatchRequest
        {
            public string Status { get; set; }
            public string Description { get; set; }
        }

        [HttpPatch("{id}")]
        [ProducesResponseType(typeof(PurchaseRequisitionResponse), 200)]
        public async Task<ActionResult<PurchaseRequisitionResponse>> PatchPurchaseRequisition([FromRoute] int id, [FromBody] PurchaseRequisitionPatchRequest req)
        {
            if (req == null)
                return BadRequest();
            var entity = await _context.PurchaseRequisitions.FindAsync(id);
            if (entity == null)
                return NotFound();
            if (req.Status != null)
                entity.Status = req.Status;
            if (req.Description != null)
                entity.Description = req.Description;
            // Ensure all DateTime fields are UTC
            if (entity.DeliveryDate.HasValue)
                entity.DeliveryDate = DateTime.SpecifyKind(entity.DeliveryDate.Value, DateTimeKind.Utc);
            if (entity.DateCreated.Kind != DateTimeKind.Utc)
                entity.DateCreated = DateTime.SpecifyKind(entity.DateCreated, DateTimeKind.Utc);
            entity.SetDateUpdated(DateTime.UtcNow);
            _context.Entry(entity).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.PurchaseRequisitions.Any(e => e.Id == id))
                    return NotFound();
                else
                    throw;
            }
            Supplier supplier = null;
            if (entity.SupplierId != null)
            {
                supplier = await _context.Suppliers.FirstOrDefaultAsync(s => s.Id == entity.SupplierId);
            }
            var result = new PurchaseRequisitionResponse {
                Id = entity.Id,
                PurchaseRequisitionId = entity.PurchaseRequisitionId,
                RequesterName = entity.RequesterName,
                Description = entity.Description,
                SupplierId = entity.SupplierId,
                DeliveryDate = entity.DeliveryDate,
                BudgetAmount = entity.BudgetAmount,
                Status = entity.Status,
                UserCreated = entity.UserCreated,
                UserUpdated = entity.UserUpdated,
                Items = new List<PurchaseRequisitionItemResponse>(),
                VendorName = supplier?.VendorName
            };
            return Ok(result);
        }
    }
}
