namespace ERP.API.Models.DTOs.CompanySetup
{
    public class CsHsnCodeDto
    {
        public int HsnCodeId { get; set; }
        public int CompanyId { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public decimal DefaultGstRate { get; set; }
    }

    public class CsHsnCodeSearchDto
    {
        public int CompanyId { get; set; }
        public string? SearchText { get; set; }
    }
}
