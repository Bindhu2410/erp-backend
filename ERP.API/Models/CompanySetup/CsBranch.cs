using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.API.Models.CompanySetup
{
    [Table("cs_branches")]
    public class CsBranch
    {
        [Key]
        [Column("branch_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BranchId { get; set; }

        [Required]
        [Column("company_id")]
        public int CompanyId { get; set; }

        [Required]
        [StringLength(50)]
        [Column("branch_code")]
        public string BranchCode { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        [Column("branch_name")]
        public string BranchName { get; set; } = string.Empty;

        [Required]
        [StringLength(225)]
        [Column("branch_address_line1")]
        public string BranchAddressLine1 { get; set; } = string.Empty;

        [StringLength(225)]
        [Column("branch_address_line2")]
        public string? BranchAddressLine2 { get; set; }

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
        [Column("branch_phone_number")]
        public string? BranchPhoneNumber { get; set; }

        [StringLength(255)]
        [Column("branch_email_address")]
        public string? BranchEmailAddress { get; set; }

        [StringLength(15)]
        [Column("branch_gstin")]
        public string? BranchGstin { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("is_head_office")]
        public bool IsHeadOffice { get; set; } = false;

        // Navigation property for company
        [ForeignKey("CompanyId")]
        public virtual CsCompany? Company { get; set; }
    }
}
