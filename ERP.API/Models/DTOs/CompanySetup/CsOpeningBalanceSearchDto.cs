namespace ERP.API.Models.DTOs.CompanySetup
{
    public class CsOpeningBalanceSearchDto
    {
        public int? CompanyId { get; set; }
        public int? AccountId { get; set; }
        public int? PeriodId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
