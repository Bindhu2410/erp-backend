using System.Linq;
using System.Threading.Tasks;
using ERP.API.Models;
using Dapper;
using System;

namespace ERP.API.Services
{
    public class SalesAddressService : BaseDataService<SalesAddress>
    {
        public SalesAddressService(string connectionString)
            : base(connectionString, "sales_addresses")
        {
        }

        protected override string GenerateInsertQuery()
        {
            var properties = typeof(SalesAddress).GetProperties()
                .Where(p => p.Name.ToLower() != "id");
            
            var columns = string.Join(", ", properties.Select(p => GetColumnName(p)));
            var parameters = string.Join(", ", properties.Select(p => $"@{p.Name}"));
            
            return $"INSERT INTO {_tableName} ({columns}) VALUES ({parameters}) RETURNING id";
        }

        protected override string GenerateUpdateQuery()
        {
            var properties = typeof(SalesAddress).GetProperties()
                .Where(p => p.Name.ToLower() != "id" && 
                           p.Name.ToLower() != "dateupdated"); // Exclude dateUpdated from properties
            
            var setClauses = string.Join(", ", properties.Select(p => $"{GetColumnName(p)} = @{p.Name}"));
            
            // Add the date_updated with CURRENT_TIMESTAMP
            return $"UPDATE {_tableName} SET {setClauses}, date_updated = CURRENT_TIMESTAMP WHERE id = @Id";
        }
        public async Task<IEnumerable<SalesAddress>> GetBySalesLeadIdAsync(int? salesLeadId)
        {
            if (!salesLeadId.HasValue)
                return Array.Empty<SalesAddress>();
            return await GetAllAsync("sales_lead_id = @SalesLeadId", new { SalesLeadId = salesLeadId.Value });
        }
        public async Task<bool> SetDefaultAddressAsync(int addressId, int? salesLeadId)
        {
            if (!salesLeadId.HasValue)
                return false;

            using var connection = CreateConnection();

            // First, set all addresses for this lead to non-default
            await connection.ExecuteAsync(
                $"UPDATE {_tableName} SET default_address = false WHERE sales_lead_id = @SalesLeadId",
                new { SalesLeadId = salesLeadId.Value }
            );

            // Then set the specified address as default
            var rowsAffected = await connection.ExecuteAsync(
                $"UPDATE {_tableName} SET default_address = true WHERE id = @Id AND sales_lead_id = @SalesLeadId",
                new { Id = addressId, SalesLeadId = salesLeadId.Value }
            );

            return rowsAffected > 0;
        }
    }
}