using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using ERP.API.Models;
using Dapper;

namespace ERP.API.Services
{
    public class SalesActivityEventService : BaseDataService<SalesActivityEvent>
    {
        public SalesActivityEventService(string connectionString) 
            : base(connectionString, "sales_activity_events")
        {
        }

        protected override string GenerateInsertQuery()
        {
            var properties = typeof(SalesActivityEvent).GetProperties()
                .Where(p => p.Name.ToLower() != "id" &&
                p.Name.ToLower() != "assigneduser" &&
                           p.Name.ToLower() != "dateupdated" &&
                           (p.PropertyType.IsGenericType 
                            ? p.PropertyType.GetGenericTypeDefinition().Name
                            : p.PropertyType.Name).ToLower() != "list`1");
            
            var columns = string.Join(", ", properties.Select(p => GetColumnName(p)));
            var parameters = string.Join(", ", properties.Select(p => $"@{p.Name}"));
            
            return $"INSERT INTO {_tableName} ({columns}) VALUES ({parameters}) RETURNING id";
        }

        protected override string GenerateUpdateQuery()
        {
            var properties = typeof(SalesActivityEvent).GetProperties()
                .Where(p => p.Name.ToLower() != "id" &&
                p.Name.ToLower() != "assigneduser" &&
                           p.Name.ToLower() != "dateupdated" &&
                           (p.PropertyType.IsGenericType 
                            ? p.PropertyType.GetGenericTypeDefinition().Name
                            : p.PropertyType.Name).ToLower() != "list`1");
            
            var setClauses = string.Join(", ", properties.Select(p => $"{GetColumnName(p)} = @{p.Name}"));
            
            return $"UPDATE {_tableName} SET {setClauses}, date_updated = CURRENT_TIMESTAMP WHERE id = @Id";
        }

        public async Task<IEnumerable<SalesActivityEvent>> GetByStageAsync(string stage, string stageItemId)
        {
            using var connection = CreateConnection();
            var query = @"
                SELECT *
                FROM sales_activity_events
                WHERE stage = @Stage
                AND stage_item_id = @StageItemId
                AND isactive = true
                ORDER BY start_date DESC, start_time DESC";

            var result = await connection.QueryAsync<SalesActivityEvent>(
                query,
                new { Stage = stage, StageItemId = stageItemId }
            );
            return result;
        }

        public async Task<IEnumerable<SalesActivityEvent>> GetUpcomingEventsAsync()
        {
            return await GetAllAsync("start_date >= @Today", new { Today = DateTime.Today });
        }
    }
}