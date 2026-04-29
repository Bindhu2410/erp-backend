using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.API.Models
{
    [Table("goods_receipt_note", Schema = "public")]
    public class GoodsReceiptNote
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
           [Column("user_created")]
        public int UserCreated { get; set; }
        [Column("date_created")]
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
        [Column("user_updated")]
        public int? UserUpdated { get; set; }
        [Column("date_updated")]
        public DateTime? DateUpdated { get; set; }

    [MaxLength(50)]
    [Column("grn_no")]
    public string? GrnNo { get; set; }

        [Required]
        [Column("grn_date")]
        public DateTime GrnDate { get; set; }

        [MaxLength(50)]
        [Column("po_id")]
        public string? PoId { get; set; }

        [Column("supplier_id")]
        public int SupplierId { get; set; }
        [Column("narration")]
        public string? Narration { get; set; }
        [MaxLength(30)]
        [Column("status")]
        public string? Status { get; set; }
    // Navigation property for child items
    public List<GoodsReceiptNoteItem> Items { get; set; } = new List<GoodsReceiptNoteItem>();
     
    }
}
