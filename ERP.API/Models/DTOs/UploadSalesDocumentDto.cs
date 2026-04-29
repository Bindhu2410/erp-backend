using Microsoft.AspNetCore.Http;

namespace ERP.API.Models.DTOs
{
    public class UploadSalesDocumentDto
    {
        public IFormFile File { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Stage { get; set; }
        public string StageItemId { get; set; }
        public string DocumentId { get; set; }
    }
}
