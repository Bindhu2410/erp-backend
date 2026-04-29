namespace ERP.API.Models.DTOs.CompanySetup
{
    public class CsSacCodeDto
    {
        public int SacCodeId { get; set; }
        public int? CompanyId { get; set; }
        public string SacCode { get; set; }
        public string Description { get; set; }
        public decimal? DefaultGstRate { get; set; }
        public int TotalRecords { get; set; }
    }

    public class CsSacCodeSearchDto
    {
        public int? CompanyId { get; set; }
        public string SacCode { get; set; }
        public string Description { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
