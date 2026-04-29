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
using ERP.API.Models.DTOs;
using Microsoft.AspNetCore.Http;

namespace ERP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductDropdownController : ControllerBase
    {
        private async Task<SalesItemResponse?> GetParentItemRecursive(int? parentId, NpgsqlConnection connection)
        {
            if (parentId == null)
                return null;
            var sql = @"SELECT 
                im.id as Id, null as UserCreated, null as DateCreated, null as UserUpdated, null as DateUpdated, 
                null as Qty, null as Amount, null as IsActive, im.id as ItemId, null as Stage, null as StageItemId,
                    im.make as Make, im.model as Model, im.product as Product, im.category_id as CategoryId, c.name as Category, im.item_name as ItemName, im.item_code as ItemCode,
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
        private readonly ILogger<ProductDropdownController> _logger;
        private readonly string _connectionString;

        public ProductDropdownController(IConfiguration configuration, ILogger<ProductDropdownController> logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _connectionString = _configuration.GetConnectionString("DefaultConnection") ?? 
                throw new InvalidOperationException("DefaultConnection string is not configured");
        }

        [HttpGet("options")]
        public async Task<ActionResult<IEnumerable<ProductDropdownOptions>>> GetDropdownOptions()
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var result = await connection.QueryAsync<ProductDropdownOptions>(
                    @"SELECT * FROM get_product_dropdown_options()");

                var resultList = result.ToList();

                if (!resultList.Any())
                {
                    _logger.LogWarning("No product dropdown options found in database");
                }

                return Ok(resultList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving dropdown options: {Message}", ex.Message);
                return StatusCode(500, new { message = "An error occurred while retrieving dropdown options", error = ex.Message });
            }
        }

        /// <summary>
        /// Gets a simplified list of products with only item code and name
        /// </summary>
        /// <returns>List of products with item code and name</returns>
        /// <response code="200">Returns the list of products</response>
        /// <response code="500">If there was an error retrieving the products</response>
    [HttpPost("product-list")]
    [ProducesResponseType(typeof(IEnumerable<ProductListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ProductListDto>>> GetProductList([FromBody] ProductListRequestDto request)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();
                    var search = request?.Search ?? string.Empty;
                    int page = request?.Page ?? 1;
                    int pageSize = request?.PageSize ?? 50;
                    if (page < 1) page = 1;
                    if (pageSize < 1) pageSize = 50;
                    int offset = (page - 1) * pageSize;

                    var sql = @"SELECT 
                        im.id AS ItemId,
                        im.make AS Make,
                        im.model AS Model,
                        c.name AS CategoryName,
                        im.product AS Product,
                        im.item_name AS ItemName,
                        im.item_code AS ItemCode,
                        im.unit_price AS UnitPrice
                    FROM item_master im
                    LEFT JOIN public.categories c ON im.category_id = c.id
                    WHERE im.item_code IS NOT NULL AND im.item_name IS NOT NULL
                    " + (string.IsNullOrEmpty(search) ? "" : "AND (im.item_name ILIKE @search OR im.item_code ILIKE @search)\n") +
                    "ORDER BY im.item_name\nLIMIT @pageSize OFFSET @offset;";

                    var items = (await connection.QueryAsync(sql, new {
                        search = string.IsNullOrEmpty(search) ? null : $"%{search}%",
                        pageSize,
                        offset
                    })).ToList();
                    return Ok(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product list: {Message}", ex.Message);
                return StatusCode(500, new { message = "An error occurred while retrieving product list", error = ex.Message });
            }
        }
    }
}