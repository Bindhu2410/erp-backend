using System;

namespace ERP.API.Models.CompanySetup
{
    public class CsGstRateWithCompany
    {
        public int GstRateId { get; set; }
        public int CompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string HsnSacCode { get; set; } = string.Empty;
        public bool IsHsn { get; set; }
        public decimal GstRate { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
