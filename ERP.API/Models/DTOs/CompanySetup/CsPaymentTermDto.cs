using System;

namespace ERP.API.Models.DTOs.CompanySetup
{
    public class CsPaymentTermDto
    {
        public int TermId { get; set; }
        public int? CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public string? TermName { get; set; }
        public string? CalculationType { get; set; }
        public int? DueDays { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public int? DiscountDays { get; set; }
        public int TotalRecords { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CsPaymentTermSearchDto
    {
        public int? CompanyId { get; set; }
        public string? TermName { get; set; }
        public string? CalculationType { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
