using System;

namespace ERP.API.Models.DTOs.CompanySetup
{
    public class CsGstRateSearchDto
    {
        public int? CompanyId { get; set; }
        public string? SearchText { get; set; }
        public bool? IsHsn { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
