using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.API.Models
{
    [Table("receipt_items")]
    public class ReceiptItem
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("receipt_id")]
        public int ReceiptId { get; set; }

        [Column("issue_no")]
        public string? IssueNo { get; set; }

        [Column("batch_no")]
        public string? BatchNo { get; set; }

        [Column("acc_yn")]
        public string? AccYN { get; set; }

        [Column("quantity")]
        public decimal? Quantity { get; set; }

        [Column("unit")]
        public string? Unit { get; set; }

        [Column("rate")]
        public decimal? Rate { get; set; }

        [Column("amount")]
        public decimal? Amount { get; set; }

        [Column("remarks")]
        public string? Remarks { get; set; }

        [Column("make")]
        public string? Make { get; set; }

        [Column("category")]
        public string? Category { get; set; }

        [Column("product")]
        public string? Product { get; set; }

        [Column("model")]
        public string? Model { get; set; }

        [Column("item")]
        public string? Item { get; set; }

        [ForeignKey("ReceiptId")]
        public virtual Receipt? Receipt { get; set; }
    }
}
