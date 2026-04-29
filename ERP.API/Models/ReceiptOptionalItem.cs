using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.API.Models
{
    [Table("receipt_optional_items")]
    public class ReceiptOptionalItem
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("receipt_id")]
        public int ReceiptId { get; set; }

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

        [Column("description")]
        public string? Description { get; set; }

        [Column("quantity")]
        public decimal? Quantity { get; set; }

        [Column("rate")]
        public decimal? Rate { get; set; }

        [ForeignKey("ReceiptId")]
        public virtual Receipt? Receipt { get; set; }
    }
}
