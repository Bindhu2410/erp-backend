namespace ERP.API.Models.DTOs.CompanySetup
{
    public class CsCompanySearchDto
    {
        public string? SearchTerm { get; set; }
        public int? ParentCompanyId { get; set; }
        public string? LegalEntityType { get; set; }
    }
}
