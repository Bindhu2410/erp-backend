namespace ERP.API.Models.DTOs.CompanySetup
{
    public class CsCompanyHierarchyDto
    {
        public int CompanyId { get; set; }
        public int? ParentCompanyId { get; set; }
        public string LegalCompanyName { get; set; } = string.Empty;
        public int Level { get; set; }
        public string Path { get; set; } = string.Empty;
    }
}
