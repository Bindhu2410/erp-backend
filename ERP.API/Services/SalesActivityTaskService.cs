using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using ERP.API.Models;
using Dapper;

namespace ERP.API.Services
{
    public class SalesActivityTaskService : BaseDataService<SalesActivityTask>
    {
        public SalesActivityTaskService(string connectionString) 
            : base(connectionString, "sales_activity_tasks")
        {
        }

        // In SalesActivityTaskService.cs
        protected override string GenerateInsertQuery()
        {
            var properties = typeof(SalesActivityTask).GetProperties()
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
            var properties = typeof(SalesActivityTask).GetProperties()
                .Where(p => p.Name.ToLower() != "id" &&
                            p.Name.ToLower() != "assigneduser" && 
                            p.Name.ToLower() != "dateupdated" &&
                            (p.PropertyType.IsGenericType
                             ? p.PropertyType.GetGenericTypeDefinition().Name
                             : p.PropertyType.Name).ToLower() != "list`1");

            var setClauses = string.Join(", ", properties.Select(p => $"{GetColumnName(p)} = @{p.Name}"));

            return $"UPDATE {_tableName} SET {setClauses}, date_updated = CURRENT_TIMESTAMP WHERE id = @Id";
        }

        public async Task<IEnumerable<SalesActivityTask>> GetByStageAsync(string stage, string stageItemId)
        {
            using var connection = CreateConnection();
            var query = @"
                SELECT *
                FROM sales_activity_tasks
                WHERE stage = @Stage
                AND stage_item_id = @StageItemId
                AND isactive = true
                ORDER BY due_date DESC";

            var result = await connection.QueryAsync<SalesActivityTask>(
                query,
                new { Stage = stage, StageItemId = stageItemId }
            );
            return result;
        }

        public async Task<IEnumerable<SalesActivityTask>> GetUpcomingTasksAsync()
        {
            return await GetAllAsync("due_date >= @Today", new { Today = DateTime.Today });
        }

        public async Task<IEnumerable<SalesActivityTask>> GetByParentTaskIdAsync(int parentTaskId)
        {
            return await GetAllAsync("parent_task_id = @ParentTaskId", new { ParentTaskId = parentTaskId });
        }
    }
}