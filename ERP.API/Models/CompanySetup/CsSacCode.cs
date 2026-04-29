using System;

namespace ERP.API.Models.CompanySetup
{
    public class CsSacCode
    {
        public int SacCodeId { get; set; }
        public int? CompanyId { get; set; }
        public string SacCode { get; set; }
        public string Description { get; set; }
        public decimal? DefaultGstRate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
