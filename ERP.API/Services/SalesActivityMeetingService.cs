using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using ERP.API.Models;
using Dapper;

namespace ERP.API.Services
{
    public class SalesActivityMeetingService : BaseDataService<SalesActivityMeeting>
    {
        public SalesActivityMeetingService(string connectionString) 
            : base(connectionString, "sales_activity_meetings")
        {
        }

        protected override string GenerateInsertQuery()
        {
            var properties = typeof(SalesActivityMeeting).GetProperties()
                .Where(p => p.Name.ToLower() != "id" &&
                           p.Name.ToLower() != "assigneduser" &&
                           (p.PropertyType.IsGenericType
                            ? p.PropertyType.GetGenericTypeDefinition().Name
                            : p.PropertyType.Name).ToLower() != "list`1");

            var columns = string.Join(", ", properties.Select(p => GetColumnName(p)));
            var parameters = string.Join(", ", properties.Select(p => $"@{p.Name}"));

            return $"INSERT INTO {_tableName} ({columns}) VALUES ({parameters}) RETURNING id";
        }

        protected override string GenerateUpdateQuery()
        {
            var properties = typeof(SalesActivityMeeting).GetProperties()
                .Where(p => p.Name.ToLower() != "id" &&
                           p.Name.ToLower() != "assigneduser" &&
                           (p.PropertyType.IsGenericType
                            ? p.PropertyType.GetGenericTypeDefinition().Name
                            : p.PropertyType.Name).ToLower() != "list`1");

            var setClauses = string.Join(", ", properties.Select(p => $"{GetColumnName(p)} = @{p.Name}"));

            return $"UPDATE {_tableName} SET {setClauses} WHERE id = @Id";
        }

        public async Task<IEnumerable<SalesActivityMeeting>> GetByCustomerIdAsync(string customerId)
        {
            return await GetAllAsync("customer_id = @CustomerId", new { CustomerId = customerId });
        }

        public async Task<IEnumerable<SalesActivityMeeting>> GetBySalesLeadIdAsync(int salesLeadId)
        {
            return await GetAllAsync("sales_leads_id = @SalesLeadId", new { SalesLeadId = salesLeadId });
        }

        public async Task<IEnumerable<SalesActivityMeeting>> GetByStageAsync(string stage, string stageItemId)
        {
            using var connection = CreateConnection();            var query = @"
                SELECT *
                FROM sales_activity_meetings
                WHERE stage = @Stage
                AND stage_item_id = @StageItemId
                ORDER BY meeting_date_time DESC";

            var result = await connection.QueryAsync<SalesActivityMeeting>(
                query,
                new { Stage = stage, StageItemId = stageItemId }
            );
            return result;
        }

        public async Task<IEnumerable<SalesActivityMeeting>> GetUpcomingMeetingsAsync()
        {
            return await GetAllAsync("meeting_date_time > @Now", new { Now = DateTime.UtcNow });
        }

        public async Task<IEnumerable<SalesActivityMeeting>> GetByParentMeetingIdAsync(int parentMeetingId)
        {
            return await GetAllAsync("activity_parent_meetings_id = @ParentMeetingId", new { ParentMeetingId = parentMeetingId });
        }
    }
}