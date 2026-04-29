using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using System.Data;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using Microsoft.Extensions.Logging;
using System.Linq;
using ERP.API.Models.DTOs;
using Microsoft.AspNetCore.Http;

namespace ERP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemDropdownController : ControllerBase
    {
    // ...existing code...

        /// <summary>
        /// Gets a list of issue doc_id for dropdown
        /// </summary>
        /// <returns>List of issue doc_id</returns>
        /// <response code="200">Returns the list of issue doc_id</response>
        /// <response code="500">If there was an error retrieving the issue doc_id</response>
        [HttpGet("issue-id-list")]
        [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<string>>> GetIssueIdList()
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();
                var issues = await connection.QueryAsync<dynamic>(
                    "SELECT * FROM issues WHERE doc_id IS NOT NULL ORDER BY doc_id");

                var resultList = new List<object>();
                foreach (var issue in issues)
                {
                    // Fetch BOM details similar to IssuesController
                    List<object> bomDetails = new List<object>();
                    if (issue.bom_id != null)
                    {
                        var bomIds = issue.bom_id is string[] ? (string[])issue.bom_id : new string[] { issue.bom_id.ToString() };
                        foreach (var bomId in bomIds)
                        {
                            var bom = await connection.QueryFirstOrDefaultAsync<dynamic>(
                                "SELECT id, bom_id, bom_name, bom_type FROM bill_of_materials WHERE bom_id = @BomId",
                                new { BomId = bomId });
                            if (bom != null)
                            {
                                var childItems = await connection.QueryAsync<dynamic>(
                                    "SELECT child_item_id, quantity FROM bill_of_material_child_items WHERE bill_of_material_id = @BillOfMaterialId",
                                    new { BillOfMaterialId = bom.id });
                                var childItemDetails = new List<object>();
                                foreach (var child in childItems)
                                {
                                    var item = await connection.QueryFirstOrDefaultAsync<dynamic>(
                                        "SELECT * FROM item_master WHERE id = @Id",
                                        new { Id = child.child_item_id });
                                    if (item != null)
                                    {
                                        childItemDetails.Add(new {
                                            childItemId = child.child_item_id,
                                            quantity = child.quantity,
                                            make = item.make,
                                            model = item.model,
                                            product = item.product,
                                            categoryName = item.category_id, // You may want to join with category table for name
                                            valuationMethodName = item.valuation_method_id, // You may want to join for name
                                            inventoryMethodName = item.inventory_method_id, // You may want to join for name
                                            unitPrice = item.unit_price,
                                            itemName = item.item_name,
                                            itemCode = item.item_code,
                                            catNo = item.cat_no,
                                            uomName = item.uom_id, // You may want to join for name
                                            hsn = item.hsn,
                                            tax = item.tax_percentage
                                        });
                                    }
                                }
                                bomDetails.Add(new {
                                    id = bom.id,
                                    bomId = bom.bom_id,
                                    bomName = bom.bom_name,
                                    bomType = bom.bom_type,
                                    childItems = childItemDetails
                                });
                            }
                        }
                    }
                    resultList.Add(new {
                        issue = issue,
                        bomDetails = bomDetails
                    });
                }

                if (!resultList.Any())
                {
                    _logger.LogWarning("No issue doc_id found in issues table");
                }

                return Ok(resultList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving issue doc_id list: {Message}", ex.Message);
                return StatusCode(500, new { message = "An error occurred while retrieving issue doc_id list", error = ex.Message });
            }
        }
        private readonly IConfiguration _configuration;
        private readonly ILogger<ItemDropdownController> _logger;
        private readonly string _connectionString;

        public ItemDropdownController(IConfiguration configuration, ILogger<ItemDropdownController> logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _connectionString = _configuration.GetConnectionString("DefaultConnection") ?? 
                throw new InvalidOperationException("DefaultConnection string is not configured");
        }

        /// <summary>
        /// Gets a list of item names for dropdown
        /// </summary>
        /// <returns>List of item names</returns>
        /// <response code="200">Returns the list of item names</response>
        /// <response code="500">If there was an error retrieving the item names</response>
        [HttpGet("item-names")]
        [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<string>>> GetItemNames()
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();
                var result = await connection.QueryAsync<string>(
                    @"SELECT item_name FROM item_master WHERE item_name IS NOT NULL ORDER BY item_name");

                var resultList = result.ToList();

                if (!resultList.Any())
                {
                    _logger.LogWarning("No item names found in item_master");
                }

                return Ok(resultList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving item names: {Message}", ex.Message);
                return StatusCode(500, new { message = "An error occurred while retrieving item names", error = ex.Message });
            }
        }

        /// <summary>
        /// Gets a list of items for dropdown (id, name, code)
        /// </summary>
        /// <returns>List of items</returns>
        /// <response code="200">Returns the list of items</response>
        /// <response code="500">If there was an error retrieving the items</response>
        [HttpGet("item-list")]
        [ProducesResponseType(typeof(IEnumerable<ItemDropdownDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<ItemDropdownDto>>> GetItemList()
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();
                var result = await connection.QueryAsync<dynamic>(
                    @"SELECT 
                        im.id AS ItemId,
                        cat.name AS CategoryName,
                        ig.name AS GroupName,
                        vm.name AS ValuationMethodName,
                        invm.name AS InventoryMethodName,
                        im.inventory_type AS InventoryTypeName,
                        m.name AS Make,
                        mo.name AS Model,
                        p.name AS Product,
                        im.item_name AS ItemName,
                        im.item_code AS ItemCode,
                        im.cat_no AS CatNo,
                        uom.code AS UomName,
                        im.unit_price AS UnitPrice,
                        im.hsn AS HSN,
                        im.tax_percentage AS TaxPercentage
                    FROM item_master im
                    LEFT JOIN categories cat ON im.category_id = cat.id
                    LEFT JOIN inventory_group ig ON im.group_id = ig.id
                    LEFT JOIN valuation_method vm ON im.valuation_method_id = vm.id
                    LEFT JOIN inventory_method invm ON im.inventory_method_id = invm.id
                    LEFT JOIN make m ON im.make_id = m.id
                    LEFT JOIN model mo ON im.model_id = mo.id
                    LEFT JOIN product p ON im.product_id = p.id
                    LEFT JOIN uom ON im.uom_id = uom.id
                    WHERE im.item_code IS NOT NULL AND im.item_name IS NOT NULL
                    ORDER BY im.item_name"
                );

                var itemIds = result.Where(r => r.itemid != null).Select(r => (int)r.itemid).ToList();
                var rates = new List<dynamic>();
                if (itemIds.Any())
                {
                    rates = (await connection.QueryAsync<dynamic>(
                        "SELECT item_id, purchase_rate, sales_rate, quotation_rate FROM rate_master_items WHERE item_id = ANY(@ItemIds)",
                        new { ItemIds = itemIds }
                    )).ToList();
                }

                var resultList = result.Select(r => new ItemDropdownDto
                {
                    ItemId = r.itemid ?? 0,
                    CategoryName = r.categoryname,
                    GroupName = r.groupname,
                    ValuationMethodName = r.valuationmethodname,
                    InventoryMethodName = r.inventorymethodname,
                    InventoryTypeName = r.inventorytypename,
                    Make = r.make,
                    Model = r.model,
                    Product = r.product,
                    ItemName = r.itemname,
                    ItemCode = r.itemcode,
                    CatNo = r.catno,
                    UomName = r.uomname,
                    UnitPrice = r.unitprice ?? 0,
                    HSN = r.hsn,
                    TaxPercentage = r.taxpercentage,
                    PurchaseRate = rates.FirstOrDefault(rt => rt.item_id == (r.itemid ?? 0))?.purchase_rate,
                    SaleRate = rates.FirstOrDefault(rt => rt.item_id == (r.itemid ?? 0))?.sales_rate,
                    QuoteRate = rates.FirstOrDefault(rt => rt.item_id == (r.itemid ?? 0))?.quotation_rate
                }).ToList();

                if (!resultList.Any())
                {
                    _logger.LogWarning("No items found in item_master");
                }

                return Ok(resultList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving item list: {Message}", ex.Message);
                return StatusCode(500, new { message = "An error occurred while retrieving item list", error = ex.Message });
            }
        }
    }
}
