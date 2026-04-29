using ERP.API.Models;

namespace ERP.API.Services
{
    public class SalesStateService : BaseDataService<SalesState>
    {
        public SalesStateService(string connectionString) 
            : base(connectionString, "sales_states")
        {
        }

        protected override string GenerateInsertQuery()
        {
            var properties = typeof(SalesState).GetProperties()
                .Where(p => p.Name.ToLower() != "id" && !p.PropertyType.IsClass);
            
            var columns = string.Join(", ", properties.Select(p => GetColumnName(p)));
            var parameters = string.Join(", ", properties.Select(p => $"@{p.Name}"));
            
            return $"INSERT INTO {_tableName} ({columns}) VALUES ({parameters}) RETURNING id";
        }

        protected override string GenerateUpdateQuery()
        {
            var properties = typeof(SalesState).GetProperties()
                .Where(p => p.Name.ToLower() != "id" && !p.PropertyType.IsClass);
            
            var setClause = string.Join(", ", properties.Select(p => $"{GetColumnName(p)} = @{p.Name}"));
            
            return $"UPDATE {_tableName} SET {setClause} WHERE id = @Id";
        }        internal async Task<IEnumerable<SalesState>> GetAllAsync(Func<SalesState, bool> predicate)
        {
            var states = await base.GetAllAsync();
            return states.Where(predicate);
        }
    }
}