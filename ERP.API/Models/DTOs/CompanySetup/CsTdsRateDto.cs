using System;

namespace ERP.API.Models.DTOs.CompanySetup
{
    public class CsTdsRateDto
    {
        public int TdsRateId { get; set; }
        public int CompanyId { get; set; }
        public string SectionType { get; set; }
        public decimal ThresholdAmount { get; set; }
        public decimal Rate { get; set; }
        public DateTime EffectiveDate { get; set; }
    }
}
