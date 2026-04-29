using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using ERP.API.Models;
using Dapper;

namespace ERP.API.Services
{
    public class InternalDiscussionService : BaseDataService<InternalDiscussion>
    {
        public InternalDiscussionService(string connectionString) 
            : base(connectionString, "internal_discussion")
        {
        }

        protected override string GenerateInsertQuery()
        {
            var properties = typeof(InternalDiscussion).GetProperties()
                .Where(p => p.Name.ToLower() != "id");
            
            var columns = string.Join(", ", properties.Select(p => GetColumnName(p)));
            var parameters = string.Join(", ", properties.Select(p => $"@{p.Name}"));
            
            return $"INSERT INTO {_tableName} ({columns}) VALUES ({parameters}) RETURNING id";
        }

        protected override string GenerateUpdateQuery()
        {
            var properties = typeof(InternalDiscussion).GetProperties()
                .Where(p => p.Name.ToLower() != "id" && 
                           p.Name.ToLower() != "dateupdated");
            
            var setClauses = string.Join(", ", properties.Select(p => $"{GetColumnName(p)} = @{p.Name}"));
            
            return $"UPDATE {_tableName} SET {setClauses}, date_updated = CURRENT_TIMESTAMP WHERE id = @Id";
        }        public async Task<IEnumerable<InternalDiscussion>> GetByStageAsync(string stage, string stageItemId)
        {
            return await GetAllAsync("stage = @Stage AND stage_item_id = @StageItemId", 
                new { Stage = stage, StageItemId = stageItemId });
        }

        public async Task<IEnumerable<InternalDiscussion>> GetRepliesAsync(int parentId)
        {
            return await GetAllAsync("parent = @ParentId", 
                new { ParentId = parentId });
        }

        public async Task<bool> MarkAsSeenAsync(int id, string userId)
        {
            using var connection = CreateConnection();
            var discussion = await GetByIdAsync(id);
            
            if (discussion == null)
                return false;

            var seenBy = string.IsNullOrEmpty(discussion.SeenBy) 
                ? userId 
                : $"{discussion.SeenBy},{userId}";

            if (!discussion.SeenBy?.Contains(userId) ?? true)
            {
                var rowsAffected = await connection.ExecuteAsync(
                    $"UPDATE {_tableName} SET seen_by = @SeenBy WHERE id = @Id",
                    new { Id = id, SeenBy = seenBy }
                );
                
                return rowsAffected > 0;
            }

            return true;
        }
    }
}
