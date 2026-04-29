using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ERP.API.Models
{
    [Table("claim_voucher_items")]
    public class ClaimVoucherItem
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("claim_voucher_id")]
        public int ClaimVoucherId { get; set; }

        [Column("sales_man")]
        [StringLength(255)]
        public string? SalesMan { get; set; }

        [Column("debit_account")]
        [StringLength(255)]
        public string? DebitAccount { get; set; }

        [Column("credit_account")]
        [StringLength(255)]
        public string? CreditAccount { get; set; }

        [Column("amount")]
        public decimal? Amount { get; set; }

        [Column("notes")]
        [StringLength(255)]
        public string? Notes { get; set; }

        // Navigation back to ClaimVoucher (ignored for JSON binding)
        [JsonIgnore]
        public ClaimVoucher? ClaimVoucher { get; set; }
    }
}
