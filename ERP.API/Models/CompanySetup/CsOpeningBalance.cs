using System;

namespace ERP.API.Models.CompanySetup
{
    public class CsOpeningBalance
    {
        public int BalanceId { get; set; }
        public int CompanyId { get; set; }
        public int AccountId { get; set; }
        public int PeriodId { get; set; }
        public decimal BalanceAmount { get; set; }
        public string BalanceType { get; set; }
        public string AccountCode { get; set; }
        public string AccountName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public long TotalCount { get; set; }
    }
}
