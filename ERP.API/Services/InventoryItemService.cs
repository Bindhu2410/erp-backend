using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.ComponentModel.DataAnnotations.Schema;
using ERP.API.Models;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace ERP.API.Services
{
    public class InventoryItemService : BaseDataService<InventoryItem>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public InventoryItemService(string connectionString, IHttpContextAccessor httpContextAccessor) 
            : base(connectionString, "inventory_items")
        {
            _httpContextAccessor = httpContextAccessor;
            // Ensure required data setup
            InitializeData().Wait();
        }

        private async Task InitializeData()
        {
            using var connection = CreateConnection();
            
            // Update existing items to be active
            await connection.ExecuteAsync("UPDATE inventory_items SET isactive = true WHERE isactive = false OR isactive IS NULL");

            // Insert a test item if no items exist
            var count = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM inventory_items");
            if (count == 0)
            {
                var testItem = new InventoryItem
                {
                    ItemCode = "TEST001",
                    ItemName = "Test Item",
                    Quantity = 10,
                    IsActive = true,                    DateCreated = System.DateTime.UtcNow,
                    UserCreated = 1, // Default admin user
                    DateUpdated = System.DateTime.UtcNow,
                    UserUpdated = 1 // Default admin user
                };
                await CreateAsync(testItem);
            }
        }public override async Task<IEnumerable<InventoryItem>> GetAllAsync(string? whereClause = "", object? parameters = null)
        {
            using var connection = CreateConnection();
            var query = @"
                SELECT 
                    i.*,
                    m.name as Make,
                    md.name as Model,
                    p.name as Product,
                    c.name as Category,
                    COALESCE(i.item_code, '') as ItemCode,
                    COALESCE(i.item_name, '') as ItemName,
                    COALESCE(m.name, '') as MakeName,
                    COALESCE(md.name, '') as ModelName,
                    COALESCE(p.name, '') as ProductName,
                    COALESCE(c.name, '') as CategoryName
                FROM inventory_items i
                LEFT JOIN makes m ON i.make_id = m.id
                LEFT JOIN models md ON i.model_id = md.id
                LEFT JOIN products p ON i.product_id = p.id
                LEFT JOIN categories c ON i.category_id = c.id
                WHERE (i.isactive IS NULL OR i.isactive = true)";

            if (!string.IsNullOrWhiteSpace(whereClause))
            {
                query += " AND " + whereClause;
            }

            query += " ORDER BY i.id";

            return await connection.QueryAsync<InventoryItem>(query, parameters);
        }        public override async Task<InventoryItem?> GetByIdAsync(int? id)
        {
            if (!id.HasValue)
                return null;

            using var connection = CreateConnection();
            var query = @"
                SELECT 
                    i.*,
                    m.name as Make,
                    md.name as Model,
                    p.name as Product,
                    c.name as Category
                FROM inventory_items i
                LEFT JOIN makes m ON i.make_id = m.id
                LEFT JOIN models md ON i.model_id = md.id
                LEFT JOIN products p ON i.product_id = p.id
                LEFT JOIN categories c ON i.category_id = c.id
                WHERE i.id = @Id";
            return await connection.QueryFirstOrDefaultAsync<InventoryItem>(query, new { Id = id });
        }

        protected override string GenerateInsertQuery()
        {
            var properties = typeof(InventoryItem).GetProperties()
                .Where(p => p.Name.ToLower() != "id" &&
                           p.GetCustomAttributes(typeof(NotMappedAttribute), true).Length == 0 &&
                           (p.PropertyType.IsGenericType 
                            ? p.PropertyType.GetGenericTypeDefinition().Name
                            : p.PropertyType.Name).ToLower() != "list`1");
            
            var columns = string.Join(", ", properties.Select(p => GetColumnName(p)));
            var parameters = string.Join(", ", properties.Select(p => $"@{p.Name}"));
            
            return $"INSERT INTO {_tableName} ({columns}) VALUES ({parameters}) RETURNING id";
        }

        protected override string GenerateUpdateQuery()
        {
            var properties = typeof(InventoryItem).GetProperties()
                .Where(p => p.Name.ToLower() != "id" &&
                           p.Name.ToLower() != "dateupdated" &&
                           p.GetCustomAttributes(typeof(NotMappedAttribute), true).Length == 0 &&
                           (p.PropertyType.IsGenericType 
                            ? p.PropertyType.GetGenericTypeDefinition().Name
                            : p.PropertyType.Name).ToLower() != "list`1");
            
            var setClauses = string.Join(", ", properties.Select(p => $"{GetColumnName(p)} = @{p.Name}"));
            
            return $"UPDATE {_tableName} SET {setClauses}, date_updated = CURRENT_TIMESTAMP WHERE id = @Id";
        }

        public async Task<InventoryItem> Create(InventoryItem entity)
        {
            using var connection = CreateConnection();            var userId = _httpContextAccessor.HttpContext?.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId) && int.TryParse(userId, out int parsedId))
            {
                entity.UserCreated = parsedId;
                entity.UserUpdated = parsedId;
            }
            
            var query = GenerateInsertQuery();
            var id = await connection.ExecuteScalarAsync<int>(query, entity);
            entity.Id = id;
            return entity;
        }
    }
}