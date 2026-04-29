using System.Linq;
using ERP.API.Models;

namespace ERP.API.Services
{
    public class SalesLeadsBusinessChallengeService : BaseDataService<SalesLeadsBusinessChallenge>
    {
        public SalesLeadsBusinessChallengeService(string connectionString) 
            : base(connectionString, "sales_leads_business_challenges")
        {
        }

        protected override string GenerateInsertQuery()
        {
            var properties = typeof(SalesLeadsBusinessChallenge).GetProperties()
                .Where(p => p.Name.ToLower() != "id");
            
            var columns = string.Join(", ", properties.Select(p => GetColumnName(p)));
            var parameters = string.Join(", ", properties.Select(p => $"@{p.Name}"));
            
            return $"INSERT INTO {_tableName} ({columns}) VALUES ({parameters}) RETURNING id";
        }

        protected override string GenerateUpdateQuery()
        {
            var properties = typeof(SalesLeadsBusinessChallenge).GetProperties()
                .Where(p => p.Name.ToLower() != "id" && 
                           p.Name.ToLower() != "dateupdated"); // Exclude dateUpdated from properties
            
            var setClauses = string.Join(", ", properties.Select(p => $"{GetColumnName(p)} = @{p.Name}"));
            
            // Add the date_updated with CURRENT_TIMESTAMP
            return $"UPDATE {_tableName} SET {setClauses}, date_updated = CURRENT_TIMESTAMP WHERE id = @Id";
        }
    }
}