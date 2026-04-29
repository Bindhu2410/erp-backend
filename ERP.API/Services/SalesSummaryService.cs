using ERP.API.Models;

namespace ERP.API.Services
{
    public class SalesSummaryService : BaseDataService<SalesSummary>
    {
        public SalesSummaryService(string connectionString) 
            : base(connectionString, "sales_summaries")
        {
        }        public override async Task<IEnumerable<SalesSummary>> GetAllAsync(string whereClause = "1=1", object? parameters = null)
        {
            return await base.GetAllAsync(whereClause, parameters ?? new { });
        }

        protected override string GenerateInsertQuery()
        {
            return @"INSERT INTO sales_summaries (
                user_created, date_created, user_updated, date_updated,
                icon_url, title, description, date_time, isactive, 
                stage_item_id, entities)
                VALUES (
                @UserCreated, @DateCreated, @UserUpdated, @DateUpdated,
                @IconUrl, @Title, @Description, @DateTime, @IsActive,
                @StageItemId, @Entities)
                RETURNING id";
        }

        protected override string GenerateUpdateQuery()
        {
            return @"UPDATE sales_summaries SET
                user_updated = @UserUpdated,
                date_updated = @DateUpdated,
                icon_url = @IconUrl,
                title = @Title,
                description = @Description,
                date_time = @DateTime,
                isactive = @IsActive,
                stage_item_id = @StageItemId,
                entities = @Entities
                WHERE id = @Id";
        }
    }
}