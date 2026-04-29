using Dapper;
using ERP.API.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BomDropdownController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<BomDropdownController> _logger;

        public BomDropdownController(IConfiguration configuration, ILogger<BomDropdownController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        private class BomTempDto {
            public int Id { get; set; }
            public string BomId { get; set; }
            public string BomName { get; set; }
            public string BomType { get; set; }
            public List<ItemDropdownDto> ChildItems { get; set; }
        }

        [HttpPost("bom-list")]
        public async Task<ActionResult<IEnumerable<BomDropdownListDto>>> GetBomDropdownList([FromBody] BomDropdownListRequestDto request)
        {
            int page = request?.Page ?? 1;
            int pageSize = request?.PageSize ?? 50;
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 50;
            int offset = (page - 1) * pageSize;

            using (var connection = new Npgsql.NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                var sql = @"SELECT id, bom_id, bom_name, bom_type
                            FROM bill_of_materials
                            WHERE (@search IS NULL OR bom_name ILIKE '%' || @search || '%')
                            ORDER BY bom_name
                            LIMIT @pageSize OFFSET @offset;";
                var bomListRaw = (await connection.QueryAsync<dynamic>(sql, new { search = request?.Search, pageSize, offset })).ToList();
                var bomList = bomListRaw.Select(b => new BomTempDto {
                    Id = b.id,
                    BomId = b.bom_id,
                    BomName = b.bom_name,
                    BomType = b.bom_type,
                    ChildItems = new List<ItemDropdownDto>()
                }).ToList();

                foreach (var bom in bomList)
                {
                    var childSql = @"SELECT ci.child_item_id, ci.quantity, m.name as make, mo.name as model, p.name as product, im.category_id, im.uom_id, im.valuation_method_id, im.inventory_method_id, im.group_id, im.unit_price, im.item_name, im.item_code, im.cat_no
                                     FROM bill_of_material_child_items ci
                                     JOIN item_master im ON ci.child_item_id = im.id
                                     LEFT JOIN make m ON im.make_id = m.id
                                     LEFT JOIN model mo ON im.model_id = mo.id
                                     LEFT JOIN product p ON im.product_id = p.id
                                     WHERE ci.bill_of_material_id = @BillOfMaterialId;";
                    var childItems = (await connection.QueryAsync<dynamic>(childSql, new { BillOfMaterialId = bom.Id })).ToList();

                    // Fetch rates for child items from rate_master_items
                    var childItemIds = childItems.Select(ci => (int)ci.child_item_id).ToList();
                    var rateSql = @"SELECT * FROM rate_master_items WHERE item_id = ANY(@ItemIds);";
                    var rateMasters = (await connection.QueryAsync<dynamic>(rateSql, new { ItemIds = childItemIds })).ToList();

                    // Fetch lookup tables
                    var categoryIds = childItems.Select(ci => (int?)ci.category_id).Where(x => x.HasValue).Distinct().ToList();
                    var uomIds = childItems.Select(ci => (int?)ci.uom_id).Where(x => x.HasValue).Distinct().ToList();
                    var valuationMethodIds = childItems.Select(ci => (int?)ci.valuation_method_id).Where(x => x.HasValue).Distinct().ToList();
                    var inventoryMethodIds = childItems.Select(ci => (int?)ci.inventory_method_id).Where(x => x.HasValue).Distinct().ToList();
                    var inventoryTypeIds = childItems.Select(ci => (int?)ci.group_id).Where(x => x.HasValue).Distinct().ToList();

                    var categories = (await connection.QueryAsync<dynamic>("SELECT id, name FROM categories WHERE id = ANY(@Ids);", new { Ids = categoryIds })).ToList();
                    var uoms = (await connection.QueryAsync<dynamic>("SELECT id, code FROM uom WHERE id = ANY(@Ids);", new { Ids = uomIds })).ToList();
                    var valuationMethods = (await connection.QueryAsync<dynamic>("SELECT id, name FROM valuation_method WHERE id = ANY(@Ids);", new { Ids = valuationMethodIds })).ToList();
                    var inventoryMethods = (await connection.QueryAsync<dynamic>("SELECT id, name FROM inventory_method WHERE id = ANY(@Ids);", new { Ids = inventoryMethodIds })).ToList();
                    var inventoryTypes = (await connection.QueryAsync<dynamic>("SELECT id, name FROM inventory_types WHERE id = ANY(@Ids);", new { Ids = inventoryTypeIds })).ToList();

                    var childItemDtos = childItems.Select(ci => {
                        var categoryName = ci.category_id != null ? categories.FirstOrDefault(c => c.id == ci.category_id)?.name : null;
                        var uomName = ci.uom_id != null ? uoms.FirstOrDefault(u => u.id == ci.uom_id)?.code : null;
                        var valuationMethodName = ci.valuation_method_id != null ? valuationMethods.FirstOrDefault(vm => vm.id == ci.valuation_method_id)?.name : null;
                        var inventoryMethodName = ci.inventory_method_id != null ? inventoryMethods.FirstOrDefault(im => im.id == ci.inventory_method_id)?.name : null;
                        var inventoryTypeName = ci.group_id != null ? inventoryTypes.FirstOrDefault(it => it.id == ci.group_id)?.name : null;
                        var rate = rateMasters.FirstOrDefault(rm => rm.item_id == ci.child_item_id);
                        return new ItemDropdownDto {
                            ItemId = ci.child_item_id,
                            Quantity = ci.quantity,
                            Make = ci.make,
                            Model = ci.model,
                            Product = ci.product,
                            CategoryName = categoryName,
                            ValuationMethodName = valuationMethodName,
                            InventoryMethodName = inventoryMethodName,
                            InventoryTypeName = inventoryTypeName,
                            UnitPrice = ci.unit_price,
                            ItemName = ci.item_name,
                            ItemCode = ci.item_code,
                            CatNo = ci.cat_no,
                            UomName = uomName,
                            PurchaseRate = rate != null ? (rate.purchase_rate is decimal ? rate.purchase_rate : null) : null,
                            SaleRate = rate != null ? (rate.sales_rate is decimal ? rate.sales_rate : null) : null,
                            QuoteRate = rate != null ? (rate.quotation_rate is decimal ? rate.quotation_rate : null) : null,
                            HSN = ci.hsn,
                            TaxPercentage = ci.tax_percentage
                        };
                    }).ToList();
                    bom.ChildItems = childItemDtos;
                }
                // Map to BomDropdownListDto
                var result = bomList.Select(bom => {
                    foreach (var item in bom.ChildItems) {
                        if (item.HSN == null || item.TaxPercentage == null) {
                            var itemMasterSql = "SELECT hsn, tax_percentage FROM item_master WHERE id = @ItemId;";
                            var itemMaster = connection.QueryFirstOrDefault<dynamic>(itemMasterSql, new { ItemId = item.ItemId });
                            if (itemMaster != null) {
                                if (item.HSN == null && itemMaster.hsn != null) item.HSN = itemMaster.hsn.ToString();
                                if (item.TaxPercentage == null && itemMaster.tax_percentage != null) item.TaxPercentage = (itemMaster.tax_percentage is decimal ? itemMaster.tax_percentage : Convert.ToDecimal(itemMaster.tax_percentage));
                            }
                        }
                    }
                    return new BomDropdownListDto {
                        BomId = bom.BomId,
                        BomName = bom.BomName,
                        BomType = bom.BomType,
                        ChildItems = bom.ChildItems
                    };
                }).ToList();
                return Ok(result);
            }
        }
    }
}
