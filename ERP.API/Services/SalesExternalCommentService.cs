using ERP.API.Models;

namespace ERP.API.Services
{
    public class SalesExternalCommentService : BaseDataService<SalesExternalComment>
    {
        public SalesExternalCommentService(string connectionString) 
            : base(connectionString, "sales_external_comments")
        {
        }

        protected override string GenerateInsertQuery()
        {
            return @"
                INSERT INTO sales_external_comments 
                (user_created, date_created, user_updated, date_updated, 
                title, description, date_time, stage, stage_item_id, 
                isactive, activity_id)
                VALUES 
                (@UserCreated, @DateCreated, @UserUpdated, @DateUpdated,
                @Title, @Description, @DateTime, @Stage, @StageItemId,
                @IsActive, @ActivityId)
                RETURNING id";
        }

        protected override string GenerateUpdateQuery()
        {
            return @"
                UPDATE sales_external_comments 
                SET user_created = @UserCreated,
                    date_created = @DateCreated,
                    user_updated = @UserUpdated,
                    date_updated = @DateUpdated,
                    title = @Title,
                    description = @Description,
                    date_time = @DateTime,
                    stage = @Stage,
                    stage_item_id = @StageItemId,
                    isactive = @IsActive,
                    activity_id = @ActivityId
                WHERE id = @Id";
        }
    }
}