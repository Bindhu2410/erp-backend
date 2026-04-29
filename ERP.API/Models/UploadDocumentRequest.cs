using Microsoft.AspNetCore.Http;

namespace ERP.API.Models
{
    public class UploadDocumentRequest
    {
        public IFormFile File { get; set; }
        public string? FolderId { get; set; }
        public bool MakePublic { get; set; }
    }
}
