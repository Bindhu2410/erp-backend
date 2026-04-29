namespace ERP.API.Models.DTOs.CompanySetup
{
    public class CsOpeningBalanceDto
    {
        public int BalanceId { get; set; }
        public int CompanyId { get; set; }
        public int AccountId { get; set; }
        public int PeriodId { get; set; }
        public decimal BalanceAmount { get; set; }
        public string BalanceType { get; set; }
        public string AccountCode { get; set; }
        public string AccountName { get; set; }
        public int TotalRecords { get; set; }
    }
}
