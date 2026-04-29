using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.API.Models
{
    [Table("claim_voucher")]
    public class ClaimVoucher
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("user_created")]
        public int? UserCreated { get; set; }

        [Column("date_created")]
        public DateTime? DateCreated { get; set; }

        [Column("user_updated")]
        public int? UserUpdated { get; set; }

        [Column("date_updated")]
        public DateTime? DateUpdated { get; set; }

        [Column("doc_id")]
        public string? DocId { get; set; }

        [Column("date")]
        public DateTime? Date { get; set; }

        [Column("from_date")]
        public DateTime? FromDate { get; set; }

        [Column("to_date")]
        public DateTime? ToDate { get; set; }

        [Column("status")]
        public string? Status { get; set; }

        // Per-item fields (moved to ClaimVoucherItem). Use the Items collection instead.

        [Column("total_amount")]
        public decimal? TotalAmount { get; set; }

        // Multiple items for this claim voucher
        public List<ClaimVoucherItem>? Items { get; set; }
    }
}
