
using System.Collections.Generic;
using System.Threading.Tasks;
using ERP.API.Models;
using Dapper;
using System.Data;

namespace ERP.API.Services
{
    public class DemoChecklistService : IDemoChecklistService
    {
        private readonly IDbConnection _db;
        public DemoChecklistService(IDbConnection db)
        {
            _db = db;
        }

        public async Task<IEnumerable<DemoChecklist>> GetAllChecklistsAsync()
        {
            var sql = @"
                SELECT c.*
                FROM demo_checklists c
                WHERE c.is_active = TRUE
            ";
            return await _db.QueryAsync<DemoChecklist>(sql);
        }

        public async Task<DemoChecklist> GetChecklistByIdAsync(int id)
        {
            var sql = "SELECT * FROM demo_checklists WHERE id = @id";
            return await _db.QueryFirstOrDefaultAsync<DemoChecklist>(sql, new { id });
        }

        public async Task<DemoChecklist> CreateChecklistAsync(DemoChecklist checklist)
        {
            var sql = @"INSERT INTO demo_checklists (checklist_name, demo_id, is_active) VALUES (@ChecklistName, @DemoId, @IsActive) RETURNING *";
            return await _db.QueryFirstOrDefaultAsync<DemoChecklist>(sql, checklist);
        }

        public async Task<DemoChecklist> UpdateChecklistAsync(DemoChecklist checklist)
        {
            var sql = @"UPDATE demo_checklists SET checklist_name = @ChecklistName, demo_id = @DemoId, is_active = @IsActive, updated_at = CURRENT_TIMESTAMP WHERE id = @Id RETURNING *";
            return await _db.QueryFirstOrDefaultAsync<DemoChecklist>(sql, checklist);
        }

        public async Task<bool> DeleteChecklistAsync(int id)
        {
            var sql = "UPDATE demo_checklists SET is_active = FALSE, updated_at = CURRENT_TIMESTAMP WHERE id = @id";
            return (await _db.ExecuteAsync(sql, new { id })) > 0;
        }

        public async Task<IEnumerable<DemoChecklist>> GetChecklistsByItemIdAsync(int itemId)
        {
            var sql = @"
                SELECT c.*
                FROM demo_checklists c
                INNER JOIN demo_checklist_items dci ON dci.id = c.checklist_id
                WHERE dci.id = @itemId AND c.is_active = TRUE
            ";
            return await _db.QueryAsync<DemoChecklist>(sql, new { itemId });
        }

        public async Task<IEnumerable<DemoChecklistItem>> GetChecklistItemsAsync(int checklistId)
        {
            var sql = "SELECT * FROM demo_checklist_items WHERE id = @checklistId";
            return await _db.QueryAsync<DemoChecklistItem>(sql, new { checklistId });
        }

        public async Task<DemoChecklistItem> AddChecklistItemAsync(DemoChecklistItem item)
        {
            var sql = @"INSERT INTO demo_checklist_items (id, checklist_name) VALUES (@ChecklistId, @ChecklistName) RETURNING *";
            return await _db.QueryFirstOrDefaultAsync<DemoChecklistItem>(sql, item);
        }

        public async Task<bool> DeleteChecklistItemAsync(int itemId)
        {
            var sql = "DELETE FROM demo_checklist_items WHERE id = @itemId";
            return (await _db.ExecuteAsync(sql, new { itemId })) > 0;
        }
    }
}
