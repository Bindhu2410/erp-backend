using System;
using System.ComponentModel.DataAnnotations;

namespace ERP.API.Models.DTOs.CompanySetup
{
    public class CsGstRateDto
    {
        public int GstRateId { get; set; }

        [Required]
        public int CompanyId { get; set; }

        [Required]
        [StringLength(20)]
        public string HsnSacCode { get; set; } = string.Empty;

        [Required]
        public bool IsHsn { get; set; }

        [Required]
        public decimal GstRate { get; set; }

        [Required]
        public DateTime EffectiveDate { get; set; }

        public DateTime CreatedAt { get; set; }
        
        public DateTime? UpdatedAt { get; set; }
    }
}
