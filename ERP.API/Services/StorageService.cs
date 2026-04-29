using ERP.API.Models;

namespace ERP.API.Services
{
    public interface IStorageService
    {
        string GetFullPath(string relativePath);
        void EnsureDirectoryExists(string path);
        string GetDocumentPath(string category, string fileName, string? subFolder = null);
        string GetTemplatePath(string category, string fileName);
        string GetBackupPath(string category, string fileName);
        string GetLogPath(string category, string fileName);
        string GetTempPath(string category, string fileName);
    }

    public class StorageService : IStorageService
    {
        private readonly StorageConfiguration _config;
        private readonly string _rootPath;

        public StorageService(IConfiguration configuration)
        {
            _config = configuration.GetSection("Storage").Get<StorageConfiguration>() ?? new StorageConfiguration();
            _rootPath = _config.BasePath;
            InitializeDirectories();
        }

        public string GetFullPath(string relativePath)
        {
            return Path.Combine(_rootPath, relativePath);
        }

        public void EnsureDirectoryExists(string path)
        {
            var fullPath = Path.IsPathRooted(path) ? path : GetFullPath(path);
            var directory = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        public string GetDocumentPath(string category, string fileName, string? subFolder = null)
        {
            var basePath = category.ToLower() switch
            {
                "sales" => _config.Documents.Sales,
                "quotations" => _config.Documents.Quotations,
                "demos" or "demo" => _config.Documents.Demos,
                "invoices" => _config.Documents.Invoices,
                "purchaseorders" => _config.Documents.PurchaseOrders,
                "deliveries" => _config.Documents.Deliveries,
                "claims" => _config.Documents.Claims,
                "attachments" => _config.Documents.Attachments,
                "employees" => _config.Documents.Employees,
                "employeephoto" => !string.IsNullOrWhiteSpace(subFolder)
                    ? Path.Combine(_config.Documents.EmployeePhoto, subFolder)
                    : _config.Documents.EmployeePhoto,
                "employeeeducation" => !string.IsNullOrWhiteSpace(subFolder)
                    ? Path.Combine(_config.Documents.EmployeeEducation, subFolder)
                    : _config.Documents.EmployeeEducation,
                _ => _config.Documents.Attachments
            };
            
            var fullPath = GetFullPath(Path.Combine(basePath, fileName));
            EnsureDirectoryExists(fullPath);
            return fullPath;
        }

        public string GetTemplatePath(string category, string fileName)
        {
            var basePath = category.ToLower() switch
            {
                "reports" => _config.Templates.Reports,
                "emails" => _config.Templates.Emails,
                "documents" => _config.Templates.Documents,
                _ => _config.Templates.Documents
            };
            
            var fullPath = GetFullPath(Path.Combine(basePath, fileName));
            EnsureDirectoryExists(fullPath);
            return fullPath;
        }

        public string GetBackupPath(string category, string fileName)
        {
            var basePath = category.ToLower() switch
            {
                "database" => _config.Backups.Database,
                "files" => _config.Backups.Files,
                "logs" => _config.Backups.Logs,
                _ => _config.Backups.Files
            };
            
            var fullPath = GetFullPath(Path.Combine(basePath, fileName));
            EnsureDirectoryExists(fullPath);
            return fullPath;
        }

        public string GetLogPath(string category, string fileName)
        {
            var basePath = category.ToLower() switch
            {
                "application" => _config.Logs.Application,
                "audit" => _config.Logs.Audit,
                "error" => _config.Logs.Error,
                "performance" => _config.Logs.Performance,
                _ => _config.Logs.Application
            };
            
            var fullPath = GetFullPath(Path.Combine(basePath, fileName));
            EnsureDirectoryExists(fullPath);
            return fullPath;
        }

        public string GetTempPath(string category, string fileName)
        {
            var basePath = category.ToLower() switch
            {
                "uploads" => _config.Temp.Uploads,
                "processing" => _config.Temp.Processing,
                "cache" => _config.Temp.Cache,
                _ => _config.Temp.Uploads
            };
            
            var fullPath = GetFullPath(Path.Combine(basePath, fileName));
            EnsureDirectoryExists(fullPath);
            return fullPath;
        }

        private void InitializeDirectories()
        {
            var paths = new[]
            {
                _config.Documents.Sales,
                _config.Documents.Quotations,
                _config.Documents.Demos,
                _config.Documents.Invoices,
                _config.Documents.PurchaseOrders,
                _config.Documents.Deliveries,
                _config.Documents.Claims,
                _config.Documents.Attachments,
                _config.Templates.Reports,
                _config.Templates.Emails,
                _config.Templates.Documents,
                _config.Backups.Database,
                _config.Backups.Files,
                _config.Backups.Logs,
                _config.Logs.Application,
                _config.Logs.Audit,
                _config.Logs.Error,
                _config.Logs.Performance,
                _config.Temp.Uploads,
                _config.Temp.Processing,
                _config.Temp.Cache
            };

            foreach (var path in paths)
            {
                EnsureDirectoryExists(path);
            }
        }
    }
}