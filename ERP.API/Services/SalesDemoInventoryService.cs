using ERP.API.Models;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using ERP.API.Services;
using Dapper;
using Npgsql;

namespace ERP.API.Services
{
    public class SalesDemoInventoryService : BaseDataService<SalesDemoInventory>, IDataService<SalesDemoInventory>
    {
        public SalesDemoInventoryService(IConfiguration configuration)
            : base(configuration.GetConnectionString("DefaultConnection")!, "demo_inventory")
        {
        }

        public SalesDemoInventoryService(string connectionString)
            : base(connectionString, "demo_inventory")
        {
        }

        public async Task<SalesDemoInventory?> GetByIdAsync(int id)
        {
            // Assuming you have a method in BaseDataService to get by id, otherwise implement the logic here
            return await GetByIdInternalAsync(id);
        }

        private async Task<SalesDemoInventory?> GetByIdInternalAsync(int id)
        {
            // Use the base class's GetByIdAsync method
            return await base.GetByIdAsync(id);
        }

        public async Task<SalesDemoInventory?> GetByIdWithProductAsync(int id)
        {
            using var connection = CreateConnection();
            var query = @"SELECT di.*, p.name AS Product
                          FROM demo_inventory di
                          LEFT JOIN inventory_items ii ON di.item_id = ii.id
                          LEFT JOIN products p ON ii.product_id = p.id
                          WHERE di.id = @Id";
            return await connection.QueryFirstOrDefaultAsync<SalesDemoInventory>(query, new { Id = id });
        }

        public async Task<IEnumerable<SalesDemoInventory>> GetAllWithProductAsync()
        {
            using var connection = CreateConnection();
            var query = @"SELECT di.*, p.name AS Product
                          FROM demo_inventory di
                          LEFT JOIN inventory_items ii ON di.item_id = ii.id
                          LEFT JOIN products p ON ii.product_id = p.id";
            var result = await connection.QueryAsync<SalesDemoInventory>(query);
            return result ?? Enumerable.Empty<SalesDemoInventory>();
        }

        protected override string GenerateInsertQuery()
        {
            return @"INSERT INTO demo_inventory (
                item_id, status, condition, demo_start_date, demo_expected_end_date, demo_actual_end_date, assigned_to_type, notes, original_cost, current_value, last_inspection_date, last_maintenance_date, user_created, date_created, user_updated, date_updated
            ) VALUES (
                @ItemId, @Status, @Condition, @DemoStartDate, @DemoExpectedEndDate, @DemoActualEndDate, @AssignedToType, @Notes, @OriginalCost, @CurrentValue, @LastInspectionDate, @LastMaintenanceDate, @UserCreated, @DateCreated, @UserUpdated, @DateUpdated
            ) RETURNING id;";
        }

        protected override string GenerateUpdateQuery()
        {
            return @"UPDATE demo_inventory SET
                item_id = @ItemId,
                status = @Status,
                condition = @Condition,
                demo_start_date = @DemoStartDate,
                demo_expected_end_date = @DemoExpectedEndDate,
                demo_actual_end_date = @DemoActualEndDate,
                assigned_to_type = @AssignedToType,
                notes = @Notes,
                original_cost = @OriginalCost,
                current_value = @CurrentValue,
                last_inspection_date = @LastInspectionDate,
                last_maintenance_date = @LastMaintenanceDate,
                user_created = @UserCreated,
                date_created = @DateCreated,
                user_updated = @UserUpdated,
                date_updated = @DateUpdated
            WHERE id = @Id;";
        }
    }
}
