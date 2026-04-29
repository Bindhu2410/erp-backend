using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.API.Models.CompanySetup
{
    [Table("cs_gst_rates")]
    public class CsGstRate
    {
        [Key]
        [Column("gst_rate_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int GstRateId { get; set; }

        [Required]
        [Column("company_id")]
        public int CompanyId { get; set; }

        [Required]
        [StringLength(20)]
        [Column("hsn_sac_code")]
        public string HsnSacCode { get; set; } = string.Empty;

        [Required]
        [Column("is_hsn")]
        public bool IsHsn { get; set; }

        [Required]
        [Column("gst_rate")]
        public decimal GstRate { get; set; }

        [Required]
        [Column("effective_date")]
        public DateTime EffectiveDate { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }
    }
}
