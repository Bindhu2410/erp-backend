using Microsoft.AspNetCore.Mvc;
using ERP.API.Services;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StorageController : ControllerBase
    {
        private readonly IStorageService _storageService;
        private readonly ILogger<StorageController> _logger;

        public StorageController(IStorageService storageService, ILogger<StorageController> logger)
        {
            _storageService = storageService;
            _logger = logger;
        }

        [HttpPost("upload/{category}")]
        public async Task<IActionResult> UploadFile(string category, IFormFile file, [FromQuery] string? employeeId = null)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest("No file uploaded");

                var originalFileName = Path.GetFileName(file.FileName);
                var storedFileName = $"{Guid.NewGuid()}_{originalFileName}";
                var filePath = _storageService.GetDocumentPath(category, storedFileName, employeeId);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return Ok(new
                {
                    FileName = originalFileName,
                    StoredFileName = storedFileName,
                    Category = category,
                    FilePath = filePath
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("paths")]
        public IActionResult GetStoragePaths()
        {
            try
            {
                var paths = new
                {
                    Documents = new
                    {
                        Sales = _storageService.GetFullPath("Documents/Sales"),
                        Demos = _storageService.GetFullPath("Documents/Demos"),
                        Quotations = _storageService.GetFullPath("Documents/Quotations"),
                        Invoices = _storageService.GetFullPath("Documents/Invoices"),
                        PurchaseOrders = _storageService.GetFullPath("Documents/PurchaseOrders"),
                        Deliveries = _storageService.GetFullPath("Documents/Deliveries"),
                        Claims = _storageService.GetFullPath("Documents/Claims"),
                        Attachments = _storageService.GetFullPath("Documents/Attachments")
                    },
                    Templates = new
                    {
                        Reports = _storageService.GetFullPath("Templates/Reports"),
                        Emails = _storageService.GetFullPath("Templates/Emails"),
                        Documents = _storageService.GetFullPath("Templates/Documents")
                    },
                    Logs = new
                    {
                        Application = _storageService.GetFullPath("Logs/Application"),
                        Audit = _storageService.GetFullPath("Logs/Audit"),
                        Error = _storageService.GetFullPath("Logs/Error"),
                        Performance = _storageService.GetFullPath("Logs/Performance")
                    }
                };

                return Ok(paths);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting storage paths");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("download/{category}/{fileName}")]
        public IActionResult DownloadFile(string category, string fileName, [FromQuery] string? employeeId = null)
        {
            try
            {
                var filePath = _storageService.GetDocumentPath(category, fileName, employeeId);
                
                if (!System.IO.File.Exists(filePath))
                    return NotFound("File not found");

                var fileBytes = System.IO.File.ReadAllBytes(filePath);
                var contentType = GetContentType(fileName);
                
                return File(fileBytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading file");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("files/{category}")]
        public IActionResult GetFiles(string category)
        {
            try
            {
                var categoryPath = category.ToLower() switch
                {
                    "demo" => "Documents/Demos",
                    "sales" => "Documents/Sales",
                    "quotations" => "Documents/Quotations",
                    "invoices" => "Documents/Invoices",
                    "purchaseorders" => "Documents/PurchaseOrders",
                    "deliveries" => "Documents/Deliveries",
                    "claims" => "Documents/Claims",
                    "attachments" => "Documents/Attachments",
                    _ => "Documents/Attachments"
                };

                var fullPath = _storageService.GetFullPath(categoryPath);
                
                if (!Directory.Exists(fullPath))
                    return Ok(new { Files = new string[0] });

                var files = Directory.GetFiles(fullPath)
                    .Select(f => new {
                        FileName = Path.GetFileName(f),
                        Size = new FileInfo(f).Length,
                        CreatedDate = System.IO.File.GetCreationTime(f),
                        ModifiedDate = System.IO.File.GetLastWriteTime(f)
                    }).ToArray();

                return Ok(new { Files = files });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting files");
                return StatusCode(500, "Internal server error");
            }
        }

        private string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".pdf" => "application/pdf",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".txt" => "text/plain",
                ".csv" => "text/csv",
                ".json" => "application/json",
                ".zip" => "application/zip",
                _ => "application/octet-stream"
            };
        }
    }
}