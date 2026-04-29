namespace ERP.API.Models.DTOs.CompanySetup
{
    public class CsBranchDropdownDto
    {
        public int BranchId { get; set; }
        public string BranchCode { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;
        public bool IsHeadOffice { get; set; }
        public string FullAddress { get; set; } = string.Empty;
    }
}
