namespace ERP.API.Models.DTOs.CompanySetup
{
    public class CsTdsRateSearchDto
    {
        public int? CompanyId { get; set; }
        public string SectionType { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
