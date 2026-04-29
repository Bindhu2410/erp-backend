using ERP.API.Models;
using Microsoft.Extensions.Configuration;

namespace ERP.API.Services
{
    public class SalesDocumentService : BaseDataService<SalesDocument>
    {        public SalesDocumentService(IConfiguration configuration) 
            : base(configuration.GetConnectionString("DefaultConnection") ?? throw new ArgumentNullException("DefaultConnection string not found"), "sales_documents")
        {
        }

        protected override string GenerateInsertQuery()
        {
            return @"INSERT INTO sales_documents (file_url, title, file_type, file_name, icon_url, 
                    description, isactive, document_id, stage, stage_item_id, user_created, date_created)
                    VALUES (@FileUrl, @Title, @FileType, @FileName, @IconUrl, @Description, 
                    @IsActive, @DocumentId, @Stage, @StageItemId, @UserCreated, @DateCreated)
                    RETURNING id";
        }

        protected override string GenerateUpdateQuery()
        {
            return @"UPDATE sales_documents 
                    SET file_url = @FileUrl, 
                        title = @Title,
                        file_type = @FileType,
                        file_name = @FileName,
                        icon_url = @IconUrl,
                        description = @Description,
                        isactive = @IsActive,
                        document_id = @DocumentId,
                        stage = @Stage,
                        stage_item_id = @StageItemId,
                        user_updated = @UserUpdated,
                        date_updated = @DateUpdated
                    WHERE id = @Id";
        }
    }
}