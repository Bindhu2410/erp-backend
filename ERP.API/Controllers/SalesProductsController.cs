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
using ERP.API.Models;

namespace ERP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SalesProductsController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SalesProductsController> _logger;
        private readonly string _connectionString;

        public SalesProductsController(IConfiguration configuration, ILogger<SalesProductsController> logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _connectionString = _configuration.GetConnectionString("DefaultConnection") ??
                throw new InvalidOperationException("DefaultConnection string is not configured");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SalesProducts>>> GetSalesProducts()
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();
                var products = await connection.QueryAsync<dynamic>(@"SELECT * FROM sales_product WHERE is_active = true ORDER BY date_created DESC");
                var itemIds = products.Select(p => (int)p.item_id).Distinct().ToList();
                var items = await connection.QueryAsync<dynamic>(@"SELECT * FROM item_master WHERE id = ANY(@ItemIds)", new { ItemIds = itemIds });
                var categories = await connection.QueryAsync<dynamic>(@"SELECT * FROM categories");
                var rates = await connection.QueryAsync<dynamic>(@"SELECT * FROM rate_master WHERE item_id = ANY(@ItemIds)", new { ItemIds = itemIds });

                var resultList = products.Select(sp => {
                    var item = items.FirstOrDefault(i => i.id == sp.item_id);
                    var category = categories.FirstOrDefault(c => c.id == (item?.category_id));
                    var rate = rates.FirstOrDefault(r => r.item_id == sp.item_id);
                    // Simulate lookup for valuation, inventory, uom, etc. (add queries if needed)
                    string valuationMethodName = item?.valuation_method_id != null ? null : null; // Add lookup if needed
                    string inventoryMethodName = item?.inventory_method_id != null ? null : null; // Add lookup if needed
                    string inventoryTypeName = item?.group_id != null ? null : null; // Add lookup if needed
                    string uomName = item?.uom_id != null ? null : null; // Add lookup if needed
                    return new {
                        ChildItemId = sp.item_id,
                        Quantity = sp.qty,
                        Make = item?.make,
                        Model = item?.model,
                        Product = item?.product,
                        CategoryName = category?.name,
                        ValuationMethodName = valuationMethodName,
                        InventoryMethodName = inventoryMethodName,
                        InventoryTypeName = inventoryTypeName,
                        UnitPrice = item?.unit_price,
                        ItemName = item?.item_name,
                        ItemCode = item?.item_code,
                        CatNo = item?.cat_no,
                        UomName = uomName,
                        PurchaseRate = rate?.purchase_rate,
                        SaleRate = rate?.sale_rate,
                        QuoteRate = rate?.quote_rate,
                        HSN = rate?.hsn_code,
                        Tax = rate?.tax_percentage,
                        Stage = sp.stage,
                        StageItemId = sp.stage_item_id
                    };
                }).ToList();
                var response = new {
                    Id = products.FirstOrDefault()?.id,
                    BomId = products.FirstOrDefault()?.bom_id,
                    ChildItems = resultList
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving interest products: {Message}", ex.Message);
                return StatusCode(500, new { message = "An error occurred while retrieving interest products", error = ex.Message });
            }
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<SalesProducts>> GetSalesProduct(int id)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();
                var sp = await connection.QueryFirstOrDefaultAsync<dynamic>(@"SELECT * FROM sales_product WHERE id = @Id", new { Id = id });
                if (sp == null || !(sp.is_active ?? true))
                {
                    return NotFound($"Sales product with ID {id} not found or inactive");
                }
                // Fetch all child items for this BOM
                var childItems = await connection.QueryAsync<dynamic>(@"SELECT * FROM bill_of_material_child_items WHERE bill_of_material_id = (SELECT id FROM bill_of_materials WHERE bom_id = @BomId)", new { BomId = sp.bom_id });
                var childItemIds = childItems.Select(ci => (int)ci.child_item_id).ToList();
                var items = await connection.QueryAsync<dynamic>(@"SELECT * FROM item_master WHERE id = ANY(@ItemIds)", new { ItemIds = childItemIds });
                var categories = await connection.QueryAsync<dynamic>(@"SELECT * FROM categories");
                var uoms = await connection.QueryAsync<dynamic>(@"SELECT * FROM uom");
                var valuationMethods = await connection.QueryAsync<dynamic>(@"SELECT * FROM valuation_method");
                var inventoryMethods = await connection.QueryAsync<dynamic>(@"SELECT * FROM inventory_method");
                var inventoryTypes = await connection.QueryAsync<dynamic>(@"SELECT * FROM inventory_types");
                var rates = await connection.QueryAsync<dynamic>(@"SELECT * FROM rate_master WHERE item_id = ANY(@ItemIds)", new { ItemIds = childItemIds });

                var childItemList = childItems.Select(ci => {
                    var item = items.FirstOrDefault(i => i.id == ci.child_item_id);
                    var categoryName = item?.category_id != null ? categories.FirstOrDefault(c => c.id == item.category_id)?.name : null;
                    var uomName = item?.uom_id != null ? uoms.FirstOrDefault(u => u.id == item.uom_id)?.code : null;
                    var valuationMethodName = item?.valuation_method_id != null ? valuationMethods.FirstOrDefault(vm => vm.id == item.valuation_method_id)?.name : null;
                    var inventoryMethodName = item?.inventory_method_id != null ? inventoryMethods.FirstOrDefault(im => im.id == item.inventory_method_id)?.name : null;
                    var inventoryTypeName = item?.group_id != null ? inventoryTypes.FirstOrDefault(it => it.id == item.group_id)?.name : null;
                    var rate = rates.FirstOrDefault(r => r.item_id == ci.child_item_id);
                    // Use sp.stage as business stage, sp.stage_item_id as unique stage item id
                    return new {
                        ChildItemId = ci.child_item_id,
                        Quantity = ci.quantity,
                        Make = item?.make,
                        Model = item?.model,
                        Product = item?.product,
                        CategoryName = categoryName,
                        ValuationMethodName = valuationMethodName,
                        InventoryMethodName = inventoryMethodName,
                        InventoryTypeName = inventoryTypeName,
                        UnitPrice = item?.unit_price,
                        ItemName = item?.item_name,
                        ItemCode = item?.item_code,
                        CatNo = item?.cat_no,
                        UomName = uomName,
                        PurchaseRate = rate?.purchase_rate,
                        SaleRate = rate?.sale_rate,
                        QuoteRate = rate?.quote_rate,
                        HSN = rate?.hsn_code,
                        Tax = rate?.tax_percentage,
                        Stage = sp.stage, // Lead, Opportunity, Demo, Quotation, etc.
                        StageItemId = sp.stage_item_id // Unique ID for the stage
                    };
                }).ToList();
                // Fetch accessory items if any
                List<dynamic> accessoryItems = new List<dynamic>();
                List<int> ids = new List<int>();
                // Read bom_accessory_item_ids as JSON array
                if (sp.bom_accessory_item_ids != null)
                {
                    try
                    {
                        var json = sp.bom_accessory_item_ids.ToString();
                        ids = Newtonsoft.Json.JsonConvert.DeserializeObject<List<int>>(json);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Failed to parse bom_accessory_item_ids: {sp.bom_accessory_item_ids}");
                    }
                }
                _logger.LogInformation($"Parsed accessory item ids from bom_accessory_item_ids: {string.Join(",", ids)}");
                if (ids.Any())
                {
                    var accessoryQuery = @"SELECT * FROM item_master WHERE id = ANY(@AccessoryItemIds)";
                    var rawAccessoryItems = await connection.QueryAsync<dynamic>(accessoryQuery, new { AccessoryItemIds = ids });
                    accessoryItems = rawAccessoryItems.Select(item => (dynamic)new {
                        id = item.id,
                        user_created = item.user_created,
                        date_created = item.date_created,
                        user_updated = item.user_updated,
                        date_updated = item.date_updated,
                        group_id = item.group_id,
                        category_id = item.category_id,
                        make = item.make,
                        model = item.model,
                        product = item.product,
                        brand = item.brand,
                        item_name = item.item_name,
                        item_code = item.item_code,
                        is_active = item.is_active,
                        image_url = item.image_url,
                        unit_price = item.unit_price,
                        uom_id = item.uom_id,
                        cat_no = item.cat_no,
                        inventory_method_id = item.inventory_method_id,
                        hsn = item.hsn,
                        tax_percentage = item.tax_percentage,
                        valuation_method_id = item.valuation_method_id
                    }).ToList();
                }

                var response = new {
                    Id = sp.id,
                    BomId = sp.bom_id,
                    ChildItems = childItemList,
                    AccessoryItems = accessoryItems
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving sales product {Id}: {Message}", id, ex.Message);
                return StatusCode(500, new { message = "An error occurred while retrieving the sales product", error = ex.Message });
            }
        }
        [HttpPost]
        public async Task<ActionResult<SalesProducts>> CreateSalesProduct([FromBody] SalesProducts request)
    /// <summary>
    /// Creates a new sales product.
    /// </summary>
    /// <remarks>
    /// Example request:
    ///
    ///     POST /api/SalesProducts
    ///     Content-Type: application/json
    ///     {
    ///         "BomId": "BOM12345",
    ///         "AccessoryItemIds": [456, 789],
    ///         "Quantity": 10
    ///     }
    /// </remarks>
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (string.IsNullOrWhiteSpace(request.BomId))
                {
                    return BadRequest("bomId is required");
                }

                if (request.AccessoryItemIds == null)
                {
                    request.AccessoryItemIds = new List<int>();
                }

                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                // Fetch BOM details
                var bomQuery = @"SELECT b.id, b.bom_id, b.bom_name, b.bom_type, b.bom_name
                                FROM bill_of_materials b
                                WHERE b.bom_id = @BomId LIMIT 1";
                var bomDetails = await connection.QueryFirstOrDefaultAsync<dynamic>(bomQuery, new { BomId = request.BomId });
                if (bomDetails == null)
                {
                    return BadRequest("Invalid bomId or BOM not found");
                }

                var product = new SalesProducts
                {
                    BomId = request.BomId,
                    Quantity = request.Quantity,
                    UnitPrice = 0, // Set after fetching child items
                    IsActive = true,
                    DateCreated = DateTime.UtcNow,
                    Stage = !string.IsNullOrWhiteSpace(request.Stage) ? request.Stage : null,
                    StageItemId = !string.IsNullOrWhiteSpace(request.StageItemId) ? request.StageItemId : null
                };

                // Fetch BOM child items
                var childItemsQuery = @"SELECT ci.*, i.* FROM bill_of_material_child_items ci
                                        JOIN item_master i ON ci.child_item_id = i.id
                                        WHERE ci.bill_of_material_id = (SELECT id FROM bill_of_materials WHERE bom_id = @BomId)";
                var childItems = await connection.QueryAsync<dynamic>(childItemsQuery, new { BomId = request.BomId });

                // Calculate unit price from child items if needed
                decimal totalUnitPrice = 0;
                var childItemList = childItems.Select(ci => {
                    totalUnitPrice += (ci.unit_price ?? 0) * (ci.quantity ?? 1);
                    return new {
                        ChildItemId = ci.child_item_id,
                        ItemName = ci.item_name,
                        Quantity = ci.quantity,
                        UnitPrice = ci.unit_price
                    };
                }).ToList();
                product.UnitPrice = totalUnitPrice;

                // Prepare child item IDs and accessory item IDs as JSON arrays
                var bomChildItemIdsJson = Newtonsoft.Json.JsonConvert.SerializeObject(childItemList.Select(ci => ci.ChildItemId).ToList());
                var accessoryItemIdsJson = Newtonsoft.Json.JsonConvert.SerializeObject(request.AccessoryItemIds);

                var id = await connection.QuerySingleAsync<int>(
                    @"INSERT INTO sales_product 
                    (bom_id, qty, unit_price, is_active, date_created, stage, stage_item_id, bom_child_item_ids, bom_accessory_item_ids) 
                    VALUES 
                    (@BomId, @Quantity, @UnitPrice, @IsActive, @DateCreated, @Stage, @StageItemId, CAST(@BomChildItemIds AS JSONB), CAST(@AccessoryItemIds AS JSONB))
                    RETURNING id",
                    new {
                        BomId = product.BomId,
                        Quantity = product.Quantity,
                        UnitPrice = product.UnitPrice,
                        IsActive = product.IsActive,
                        DateCreated = product.DateCreated,
                        Stage = product.Stage,
                        StageItemId = product.StageItemId,
                        BomChildItemIds = bomChildItemIdsJson,
                        AccessoryItemIds = accessoryItemIdsJson
                    });

                // Fetch accessory items by IDs
                List<dynamic> accessoryItems = new List<dynamic>();
                if (request.AccessoryItemIds.Count > 0)
                {
                    var accessoryQuery = @"SELECT * FROM item_master WHERE id = ANY(@AccessoryItemIds)";
                    accessoryItems = (await connection.QueryAsync<dynamic>(accessoryQuery, new { AccessoryItemIds = request.AccessoryItemIds })).ToList();
                }

                // Build response object
                var response = new {
                    Id = id,
                    BomId = request.BomId,
                    Quantity = request.Quantity,
                    UnitPrice = product.UnitPrice,
                    Stage = product.Stage,
                    StageItemId = product.StageItemId,
                    ChildItems = childItemList,
                    AccessoryItems = accessoryItems
                };

                return CreatedAtAction(nameof(GetSalesProduct), new { id }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating sales product. Exception: {ex.Message}\nStackTrace: {ex.StackTrace}\nInnerException: {ex.InnerException?.Message}");
                return StatusCode(500, $"An unexpected error occurred while creating the sales product: {ex.Message}");
            }
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSalesProduct(int id, [FromBody] SalesProducts product)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (id != product.Id)
                {
                    return BadRequest("ID mismatch");
                }

                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                // Set update values
                product.DateUpdated = DateTime.UtcNow;

                var result = await connection.ExecuteAsync(
                    @"UPDATE sales_products SET
                    user_updated = @UserUpdated,
                    date_updated = @DateUpdated,
                    qty = @Quantity,
                    amount = @Amount,
                    inventory_items_id = @InventoryItemsId,
                    stage = @Stage,
                    stage_item_id = @StageItemId
                    WHERE id = @Id",
                    new
                    {
                        product.Id,
                        product.UserUpdated,
                        product.DateUpdated,
                        product.Quantity,
                        product.Amount,
                        product.InventoryItemsId,
                        product.Stage,
                        product.StageItemId
                    });

                if (result == 0)
                {
                    return NotFound($"Sales product with ID {id} not found");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating sales product {Id}: {Message}", id, ex.Message);
                return StatusCode(500, "An unexpected error occurred while updating the sales product");
            }
        }
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteSalesProduct(int id)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                await connection.ExecuteAsync(
                    "CALL sp_delete_sales_product(@Id)",
                    new { Id = id });

                return Ok(new { message = $"Interest product {id} deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting interest product {Id}: {Message}", id, ex.Message);
                return StatusCode(500, new { message = "An error occurred while deleting the interest product", error = ex.Message });
            }
        }

        [HttpGet("stage/{stage}/{stageItemId}")]
        public async Task<ActionResult<IEnumerable<SalesProducts>>> GetByStage(string stage, long stageItemId)
        {
            try
            {
                if (string.IsNullOrEmpty(stage))
                {
                    return BadRequest("Stage is required");
                }

                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = @"SELECT 
                        sp.id as Id,
                        sp.user_created as UserCreated,
                        sp.date_created as DateCreated,
                        sp.user_updated as UserUpdated,
                        sp.date_updated as DateUpdated,
                        sp.qty as Quantity,
                        sp.amount as Amount,
                        sp.item_id as InventoryItemsId,
                        sp.stage as Stage,
                        sp.stage_item_id as StageItemId,
                        sp.is_active as IsActive,
                        sp.unit_price as UnitPrice,
                        sp.bom_id as BomId,
                        sp.bom_child_item_ids as BomChildItemIds,
                        sp.bom_accessory_item_ids as BomAccessoryItemIds
                    FROM sales_product sp
                    WHERE sp.stage = @Stage 
                        AND sp.stage_item_id = @StageItemId 
                        AND sp.is_active = true 
                    ORDER BY sp.date_created DESC";

                var products = await connection.QueryAsync<SalesProducts>(query, new { Stage = stage, StageItemId = stageItemId });

                if (!products.Any())
                {
                    return NotFound($"No products found for stage {stage} and item ID {stageItemId}");
                }

                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving products for stage {Stage} and item ID {StageItemId}: {Message}",
                    stage, stageItemId, ex.Message);
                return StatusCode(500, new { message = "An error occurred while retrieving the products", error = ex.Message });
            }
        }

        [HttpPut("{id}/activate")]
        public async Task<IActionResult> ActivateProduct(int id, [FromQuery] int userId)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                // First check if the product exists
                var product = await connection.QueryFirstOrDefaultAsync<(int? id, int? inventoryItemId)>(
                    "SELECT id, item_id FROM sales_product WHERE id = @Id",
                    new { Id = id });

                if (!product.id.HasValue)
                {
                    return NotFound($"Sales product with ID {id} not found");
                }

                // Begin transaction since we're updating multiple tables
                using var transaction = connection.BeginTransaction();
                try
                {
                    // Update sales product
                    await connection.ExecuteAsync(
                        @"UPDATE sales_product 
                        SET is_active = true,
                            user_updated = @UserId,
                            date_updated = CURRENT_TIMESTAMP
                        WHERE id = @Id",
                        new { Id = id, UserId = userId });

                    // If there's a linked inventory item, activate it too
                    if (product.inventoryItemId.HasValue)
                    {
                        await connection.ExecuteAsync(
                            @"UPDATE inventory_items 
                            SET isactive = true,
                                updated_by = @UserId,
                                updated_date = CURRENT_TIMESTAMP
                            WHERE id = @Id",
                            new { Id = product.inventoryItemId, UserId = userId });
                    }

                    transaction.Commit();
                    return Ok(new { message = $"Successfully activated sales product {id} and its inventory item" });
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating sales product {Id}: {Message}", id, ex.Message);
                return StatusCode(500, new { message = "An error occurred while activating the sales product", error = ex.Message });
            }
        }
    }
}
