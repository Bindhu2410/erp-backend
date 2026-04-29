using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Dapper;
using Npgsql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ERP.API.Models.DTOs;
using ERP.API.Models;
using System.Collections.Generic;

namespace ERP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SalesItemsController : ControllerBase
    {
        private async Task<SalesItemResponse?> GetParentItemRecursive(int? parentId, NpgsqlConnection connection)
        {
            if (parentId == null)
                return null;
            var sql = @"SELECT 
                im.id as Id, null as UserCreated, null as DateCreated, null as UserUpdated, null as DateUpdated, 
                null as Qty, null as Amount, null as IsActive, im.id as ItemId, null as Stage, null as StageItemId,
                im.make as Make, im.model as Model, im.product as Product, im.category as Category, im.item_name as ItemName, im.item_code as ItemCode,
                im.unit_price as UnitPrice, im.hsn as Hsn, im.tax_percentage as TaxPercentage, im.uom as Uom,
                im.parent_id as ParentId
            FROM item_master im WHERE im.id = @Id;";
            var parent = await connection.QueryFirstOrDefaultAsync<SalesItemResponse>(sql, new { Id = parentId });
            if (parent != null && parent.ParentId != null)
            {
                parent.ParentItem = await GetParentItemRecursive(parent.ParentId, connection);
            }
            return parent;
        }
        private async Task<List<SalesItemResponse>> GetReferencedByItems(int itemId, NpgsqlConnection connection)
        {
            var sql = @"SELECT 
                im.id as Id, null as UserCreated, null as DateCreated, null as UserUpdated, null as DateUpdated, 
                null as Qty, null as Amount, null as IsActive, im.id as ItemId, null as Stage, null as StageItemId,
                im.make as Make, im.model as Model, im.product as Product, im.category as Category, im.item_name as ItemName, im.item_code as ItemCode,
                im.unit_price as UnitPrice, im.hsn as Hsn, im.tax_percentage as TaxPercentage, im.uom as Uom,
                im.parent_id as ParentId
            FROM item_master im WHERE im.parent_id = @ParentId;";
            var children = (await connection.QueryAsync<SalesItemResponse>(sql, new { ParentId = itemId })).AsList();
            foreach (var child in children)
            {
                child.ReferencedBy = await GetReferencedByItems(child.Id, connection);
            }
            return children;
        }
        private readonly IConfiguration _configuration;
        private readonly ILogger<SalesItemsController> _logger;
        private readonly string _connectionString;

        public SalesItemsController(IConfiguration configuration, ILogger<SalesItemsController> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
        }

        // GET: api/SalesItems
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SalesItemResponse>>> GetAll()
        {
            using var connection = new NpgsqlConnection(_connectionString);
            var sql = @"SELECT 
                si.id, si.user_created, si.date_created, si.user_updated, si.date_updated, 
                si.qty, si.amount, si.is_active, si.item_id, si.stage, si.unit_price, si.stage_item_id,
                im.make, im.model, im.product, im.category, im.item_name as ItemName, im.item_code as ItemCode,
                im.unit_price as UnitPrice, im.hsn as Hsn, im.tax_percentage as TaxPercentage, im.uom as Uom,
                im.parent_id as ParentId
            FROM public.sales_product si
            LEFT JOIN item_master im ON si.item_id = im.id;";
            var items = (await connection.QueryAsync<SalesItemResponse>(sql)).AsList();
            foreach (var item in items)
            {
                if (item.ItemId != null)
                {
                    // Use ItemId for parent/child lookups
                    item.ParentItem = await GetParentItemRecursive(item.ParentId, connection);
                    item.ReferencedBy = await GetReferencedByItems(item.ItemId.Value, connection);
                }
            }
            return Ok(items);
        }

        // GET: api/SalesItems/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<SalesItemResponse>> GetById(int id)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            var sql = @"SELECT 
                si.id, si.user_created, si.date_created, si.user_updated, si.date_updated, 
                si.qty, si.amount, si.is_active, si.item_id, si.stage, si.unit_price, si.stage_item_id,
                im.make, im.model, im.product, im.category, im.item_name as ItemName, im.item_code as ItemCode,
                im.unit_price as UnitPrice, im.hsn as Hsn, im.tax_percentage as TaxPercentage, im.uom as Uom,
                im.parent_id as ParentId
            FROM public.sales_product si
            LEFT JOIN item_master im ON si.item_id = im.id
            WHERE si.id = @Id;";
            var item = await connection.QueryFirstOrDefaultAsync<SalesItemResponse>(sql, new { Id = id });
            if (item == null)
                return NotFound();
            if (item.ItemId != null)
            {
                item.ParentItem = await GetParentItemRecursive(item.ParentId, connection);
                item.ReferencedBy = await GetReferencedByItems(item.ItemId.Value, connection);
            }
            return Ok(item);
        }

        // POST: api/SalesItems
        [HttpPost]
        public async Task<ActionResult<SalesItemResponse>> Create([FromBody] ERP.API.Models.DTOs.SalesItemRequest item)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            // Insert only BomId, Quantity, AccessoryItemIds
            var sql = @"INSERT INTO public.sales_product (bom_id, quantity, accessory_item_ids) VALUES (@BomId, @Quantity, @AccessoryItemIds) RETURNING id;";
            var id = await connection.ExecuteScalarAsync<int>(sql, new {
                BomId = item.BomId,
                Quantity = item.Quantity,
                AccessoryItemIds = item.AccessoryItemIds != null ? string.Join(",", item.AccessoryItemIds) : null
            });
            // Return minimal response
            return CreatedAtAction(nameof(GetById), new { id }, new { id, item.BomId, item.Quantity, item.AccessoryItemIds });
        }
        // PUT: api/SalesItems/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ERP.API.Models.DTOs.SalesItemRequest item)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            var sql = @"UPDATE public.sales_product SET bom_id = @BomId, quantity = @Quantity, accessory_item_ids = @AccessoryItemIds WHERE id = @Id;";
            await connection.ExecuteAsync(sql, new {
                BomId = item.BomId,
                Quantity = item.Quantity,
                AccessoryItemIds = item.AccessoryItemIds != null ? string.Join(",", item.AccessoryItemIds) : null,
                Id = id
            });
            return NoContent();
        }

        // DELETE: api/SalesItems/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            var sql = "DELETE FROM public.sales_product WHERE id = @Id;";
            await connection.ExecuteAsync(sql, new { Id = id });
            return NoContent();
        }

        // GET: api/SalesItems/with-details
        [HttpGet("with-details")]
        public async Task<ActionResult<IEnumerable<SalesItemResponse>>> GetAllWithDetails()
        {
            using var connection = new NpgsqlConnection(_connectionString);
            var sql = @"SELECT 
                si.id, si.user_created, si.date_created, si.user_updated, si.date_updated, 
                si.qty, si.amount, si.is_active, si.item_id, si.stage, si.unit_price, si.stage_item_id,
                im.make, im.model, im.category, im.item_name, im.brand, im.sku, im.description, im.item_code, 
                im.default_purchase_price, im.default_sale_price, im.unit_of_measure, im.product,
                im.parent_id as ParentId
            FROM public.sales_product si
            JOIN item_master im ON si.item_id = im.id;";
            var items = (await connection.QueryAsync<SalesItemResponse>(sql)).AsList();
            foreach (var item in items)
            {
                if (item.ItemId != null)
                {
                    item.ParentItem = await GetParentItemRecursive(item.ParentId, connection);
                    item.ReferencedBy = await GetReferencedByItems(item.ItemId.Value, connection);
                }
            }
            return Ok(items);
        }

        // GET: api/SalesItems/by-stage/{stage}/{stageItemId}
        [HttpGet("by-stage/{stage}/{stageItemId}")]
        public async Task<ActionResult<IEnumerable<SalesItemResponse>>> GetByStageAndStageItemId(string stage, string stageItemId)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            var sql = @"SELECT 
                si.id, si.user_created, si.date_created, si.user_updated, si.date_updated, 
                si.qty, si.amount, si.is_active, si.item_id, si.stage, si.unit_price, si.stage_item_id,
                im.make, im.model, im.product, im.category, im.item_name as ItemName, im.item_code as ItemCode,
                im.parent_id as ParentId
            FROM public.sales_product si
            JOIN item_master im ON si.item_id = im.id
            WHERE si.stage = @Stage AND si.stage_item_id = @StageItemId;";
            var items = (await connection.QueryAsync<SalesItemResponse>(sql, new { Stage = stage, StageItemId = stageItemId })).AsList();
            if (items == null)
                return NotFound();
            foreach (var item in items)
            {
                if (item.ItemId != null)
                {
                    item.ParentItem = await GetParentItemRecursive(item.ParentId, connection);
                    item.ReferencedBy = await GetReferencedByItems(item.ItemId.Value, connection);
                }
            }
            return Ok(items);
        }
    }
}
