namespace ERP.API.Models
{
    public class StorageConfiguration
    {
        public string BasePath { get; set; } = "Storage";
        public DocumentPaths Documents { get; set; } = new();
        public TemplatePaths Templates { get; set; } = new();
        public BackupPaths Backups { get; set; } = new();
        public LogPaths Logs { get; set; } = new();
        public TempPaths Temp { get; set; } = new();
    }

    public class DocumentPaths
    {
        public string Sales { get; set; } = "Documents/Sales";
        public string Quotations { get; set; } = "Documents/Quotations";
        public string Demos { get; set; } = "Documents/Demos";
        public string Invoices { get; set; } = "Documents/Invoices";
        public string PurchaseOrders { get; set; } = "Documents/PurchaseOrders";
        public string Deliveries { get; set; } = "Documents/Deliveries";
        public string Claims { get; set; } = "Documents/Claims";
        public string Attachments { get; set; } = "Documents/Attachments";
        public string Employees { get; set; } = "Documents/Employees";
        public string EmployeePhoto { get; set; } = "Documents/Employees/Photos";
        public string EmployeeEducation { get; set; } = "Documents/Employees/Education";
    }

    public class TemplatePaths
    {
        public string Reports { get; set; } = "Templates/Reports";
        public string Emails { get; set; } = "Templates/Emails";
        public string Documents { get; set; } = "Templates/Documents";
    }

    public class BackupPaths
    {
        public string Database { get; set; } = "Backups/Database";
        public string Files { get; set; } = "Backups/Files";
        public string Logs { get; set; } = "Backups/Logs";
    }

    public class LogPaths
    {
        public string Application { get; set; } = "Logs/Application";
        public string Audit { get; set; } = "Logs/Audit";
        public string Error { get; set; } = "Logs/Error";
        public string Performance { get; set; } = "Logs/Performance";
    }

    public class TempPaths
    {
        public string Uploads { get; set; } = "Temp/Uploads";
        public string Processing { get; set; } = "Temp/Processing";
        public string Cache { get; set; } = "Temp/Cache";
    }
}