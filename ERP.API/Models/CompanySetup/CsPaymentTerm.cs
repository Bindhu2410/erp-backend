using System;

namespace ERP.API.Models.CompanySetup
{
    public class CsPaymentTerm
    {
        public int TermId { get; set; }
        public int? CompanyId { get; set; }
        public string TermName { get; set; }
        public string CalculationType { get; set; }
        public int? DueDays { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public int? DiscountDays { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
