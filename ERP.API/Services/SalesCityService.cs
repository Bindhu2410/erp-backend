using ERP.API.Models;
using Dapper;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ERP.API.Services
{
    public class SalesCityService : BaseDataService<SalesCity>
    {
        public SalesCityService(string connectionString) 
            : base(connectionString, "sales_cities")
        {
        }

        protected override string GenerateInsertQuery()
        {
            var properties = typeof(SalesCity).GetProperties()
                .Where(p => p.Name.ToLower() != "id" && !p.PropertyType.IsClass);
            
            var columns = string.Join(", ", properties.Select(p => GetColumnName(p)));
            var parameters = string.Join(", ", properties.Select(p => $"@{p.Name}"));
            
            return $"INSERT INTO {_tableName} ({columns}) VALUES ({parameters}) RETURNING id";
        }

        protected override string GenerateUpdateQuery()
        {
            var properties = typeof(SalesCity).GetProperties()
                .Where(p => p.Name.ToLower() != "id" && !p.PropertyType.IsClass);
            
            var setClause = string.Join(", ", properties.Select(p => $"{GetColumnName(p)} = @{p.Name}"));
            
            return $"UPDATE {_tableName} SET {setClause} WHERE id = @Id";
        }        public async Task<IEnumerable<SalesCity>> GetByDistrictIdAsync(int districtId)
        {
            using var connection = CreateConnection();
            var query = @"SELECT * FROM sales_cities WHERE sales_districts_id = @DistrictId";
            var parameters = new { DistrictId = districtId };
            return await connection.QueryAsync<SalesCity>(query, parameters);
        }
    }
}