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
    public class SupplierItemsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SupplierItemsController(AppDbContext context)
        {
            _context = context;
        }

      
      
        /// <summary>
        /// Get items for a specific supplier
        /// </summary>
        /// <param name="supplierId">Supplier ID</param>
        /// <returns>List of items for the supplier</returns>
        [HttpGet("supplier/{supplierId}")]
        public async Task<ActionResult<SupplierItemsResponse>> GetSupplierById(int supplierId)
        {
            try
            {
                var supplier = await _context.Suppliers.FindAsync(supplierId);
                if (supplier == null)
                {
                    return NotFound(new { message = "Supplier not found" });
                }

                var items = await (from item in _context.ItemMasters
                                   join uom in _context.Uoms on item.UomId equals uom.Id into uomJoin
                                   from uom in uomJoin.DefaultIfEmpty()
                                   where item.SupplierId == supplierId
                                   select new SupplierItemDto
                                   {
                                       ItemId = item.Id,
                                       ItemCode = item.ItemCode,
                                       ItemName = item.ItemName,
                                       LongItemName = item.LongItemName,
                                       ItemDescription = item.ItemDescription,
                                       SupplierId = item.SupplierId,
                                       SupplierName = supplier.VendorName,
                                       SupplierCode = supplier.VendorCode,
                                       UnitPrice = item.UnitPrice,
                                       UomId = item.UomId,
                                       UomName = uom != null ? uom.Code : null,
                                       Brand = item.Brand,
                                       Specification = item.Specification,
                                       IsActive = item.IsActive,
                                       DateCreated = item.DateCreated,
                                       DateUpdated = item.DateUpdated,
                                       SupplierCity = supplier.City,
                                       SupplierState = supplier.State,
                                       SupplierCountry = supplier.Country,
                                       SupplierContact = string.Join(", ", supplier.Email ?? new List<string>())
                                   }).ToListAsync();

                var response = new SupplierItemsResponse
                {
                    SupplierId = supplier.Id,
                    SupplierName = supplier.VendorName,
                    SupplierCode = supplier.VendorCode,
                    SupplierCity = supplier.City,
                    SupplierState = supplier.State,
                    SupplierCountry = supplier.Country,
                    ContactEmail = supplier.Email != null && supplier.Email.Count > 0 ? supplier.Email.First() : null,
                    ContactPhone = supplier.Phone != null && supplier.Phone.Count > 0 ? supplier.Phone.First() : null,
                    IsActive = supplier.IsActive,
                    Items = items
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error fetching supplier items", error = ex.Message });
            }
        }

        /// <summary>
        /// Get items for a specific item
        /// </summary>
        /// <param name="itemId">Item ID</param>
        /// <returns>Supplier details with item information</returns>
        [HttpGet("item/{itemId}")]
        public async Task<ActionResult<SupplierItemDto>> GetItemWithSupplier(int itemId)
        {
            try
            {
                var itemData = await (from item in _context.ItemMasters
                                      join supplier in _context.Suppliers on item.SupplierId equals supplier.Id into supplierJoin
                                      from supplier in supplierJoin.DefaultIfEmpty()
                                      join uom in _context.Uoms on item.UomId equals uom.Id into uomJoin
                                      from uom in uomJoin.DefaultIfEmpty()
                                      where item.Id == itemId
                                      select new SupplierItemDto
                                      {
                                          ItemId = item.Id,
                                          ItemCode = item.ItemCode,
                                          ItemName = item.ItemName,
                                          LongItemName = item.LongItemName,
                                          ItemDescription = item.ItemDescription,
                                          SupplierId = item.SupplierId,
                                          SupplierName = supplier != null ? supplier.VendorName : null,
                                          SupplierCode = supplier != null ? supplier.VendorCode : null,
                                          UnitPrice = item.UnitPrice,
                                          UomId = item.UomId,
                                          UomName = uom != null ? uom.Code : null,
                                          Brand = item.Brand,
                                          Specification = item.Specification,
                                          IsActive = item.IsActive,
                                          DateCreated = item.DateCreated,
                                          DateUpdated = item.DateUpdated,
                                          SupplierCity = supplier != null ? supplier.City : null,
                                          SupplierState = supplier != null ? supplier.State : null,
                                          SupplierCountry = supplier != null ? supplier.Country : null,
                                          SupplierContact = supplier != null ? string.Join(", ", supplier.Email ?? new List<string>()) : null
                                      }).FirstOrDefaultAsync();

                if (itemData == null)
                {
                    return NotFound(new { message = "Item not found" });
                }

                return Ok(itemData);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error fetching item with supplier", error = ex.Message });
            }
        }

    
        
    }
}
