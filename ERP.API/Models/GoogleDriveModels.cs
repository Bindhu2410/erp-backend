// Models/GoogleDriveModels.cs
using System.ComponentModel.DataAnnotations;

namespace ERP.API.Models
{
    public class DocumentUploadResponse
    {
        public string? FileId { get; set; }
        public string? FileName { get; set; }
        public string? FolderId { get; set; }
        public string? WebViewLink { get; set; }
        public string? WebContentLink { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool IsSuccess { get; set; }
        public long FileSizeBytes { get; set; }
        public string? MimeType { get; set; }
    }
    public class DocumentUploadRequest
    {
        /// <summary>
        /// The file to upload to Google Drive
        /// </summary>
        [Required]
        public IFormFile File { get; set; }
        
        /// <summary>
        /// Optional project/folder name to organize files
        /// </summary>
        public string? ProjectName { get; set; }
        
        /// <summary>
        /// Make the file publicly accessible (for open source projects)
        /// </summary>
        public bool MakePublic { get; set; } = false;
    }

    public class MultipleDocumentUploadRequest
    {
        /// <summary>
        /// List of files to upload
        /// </summary>
        [Required]
        [MinLength(1, ErrorMessage = "At least one file is required")]
        public List<IFormFile> Files { get; set; } = new();
        
        /// <summary>
        /// Optional project/folder name to organize files
        /// </summary>
        public string? ProjectName { get; set; }
        public string? WebContentLink { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool IsSuccess { get; set; }
        public long FileSizeBytes { get; set; }
        public string? MimeType { get; set; }
    }

    public class ShareDocumentRequest
    {
        /// <summary>
        /// Email address to share the document with
        /// </summary>
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        /// <summary>
        /// Permission level: reader, writer, commenter
        /// </summary>
        [RegularExpression("^(reader|writer|commenter)$", ErrorMessage = "Role must be reader, writer, or commenter")]
        public string Role { get; set; } = "reader";
    }

    public class FileInfoResponse
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? MimeType { get; set; }
        public long? Size { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? ModifiedTime { get; set; }
        public string? WebViewLink { get; set; }
        public string? WebContentLink { get; set; }
        public List<string> Parents { get; set; } = new();
    }
}
