using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.API.Models.CompanySetup
{
    [Table("cs_bank_accounts")]
    public class CsBankAccount
    {
        [Key]
        [Column("bank_account_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BankAccountId { get; set; }

        [Required]
        [Column("company_id")]
        public int CompanyId { get; set; }

        [Required]
        [StringLength(255)]
        [Column("bank_name")]
        public string BankName { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        [Column("bank_branch_name")]
        public string BankBranchName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Column("account_number")]
        public string AccountNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        [Column("ifsc_code")]
        public string IFSCCode { get; set; } = string.Empty;

        [StringLength(20)]
        [Column("swift_code")]
        public string? SwiftCode { get; set; }

        [Required]
        [StringLength(50)]
        [Column("purpose")]
        public string Purpose { get; set; } = string.Empty;

        [Required]
        [StringLength(5)]
        [Column("currency")]
        public string Currency { get; set; } = string.Empty;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        // Navigation property for company
        [ForeignKey("CompanyId")]
        public virtual CsCompany? Company { get; set; }

        public long TotalCount { get; set; }
    }
}
