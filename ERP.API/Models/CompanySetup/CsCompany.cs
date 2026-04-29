using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.API.Models.CompanySetup
{
    [Table("cs_companies")]
    public class CsCompany
    {
        [Key]
        [Column("company_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CompanyId { get; set; }

        [Column("parent_company_id")]
        public int? ParentCompanyId { get; set; }

        [Required]
        [StringLength(255)]
        [Column("legal_company_name")]
        public string LegalCompanyName { get; set; } = string.Empty;

        [Required]
        [StringLength(225)]
        [Column("registered_address_line1")]
        public string RegisteredAddressLine1 { get; set; } = string.Empty;

        [StringLength(225)]
        [Column("registered_address_line2")]
        public string? RegisteredAddressLine2 { get; set; }

        [Required]
        [StringLength(100)]
        [Column("city")]
        public string City { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Column("state")]
        public string State { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        [Column("pincode")]
        public string Pincode { get; set; } = string.Empty;

        [StringLength(20)]
        [Column("phone_number")]
        public string? PhoneNumber { get; set; }

        [StringLength(255)]
        [Column("email_address")]
        public string? EmailAddress { get; set; }

        [StringLength(255)]
        [Column("website_url")]
        public string? WebsiteUrl { get; set; }

        [StringLength(255)]
        [Column("company_logo_path")]
        public string? CompanyLogoPath { get; set; }

        [Required]
        [StringLength(5)]
        [Column("base_currency")]
        public string BaseCurrency { get; set; } = "INR";

        [Required]
        [Column("financial_year_start_date")]
        public DateTime FinancialYearStartDate { get; set; }

        [Required]
        [Column("financial_year_end_date")]
        public DateTime FinancialYearEndDate { get; set; }

        [Required]
        [StringLength(10)]
        [Column("pan")]
        public string Pan { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        [Column("tan")]
        public string Tan { get; set; } = string.Empty;

        [Required]
        [StringLength(15)]
        [Column("gstin")]
        public string Gstin { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Column("legal_entity_type")]
        public string LegalEntityType { get; set; } = "Private Limited";

        [Required]
        [StringLength(255)]
        [Column("legal_name_as_per_pan_tan")]
        public string LegalNameAsPerPanTan { get; set; } = string.Empty;

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        // Navigation property for parent company
        [ForeignKey("ParentCompanyId")]
        public virtual CsCompany? ParentCompany { get; set; }

        // Navigation property for child companies
        public virtual ICollection<CsCompany> ChildCompanies { get; set; } = new List<CsCompany>();
    }
}
