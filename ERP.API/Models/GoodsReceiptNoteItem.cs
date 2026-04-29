using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.API.Models
{
    [Table("goods_receipt_note_items", Schema = "public")]
    public class GoodsReceiptNoteItem
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [ForeignKey("GoodsReceiptNote")]
        [Column("grn_id")]
        public int GrnId { get; set; }
    public GoodsReceiptNote? GoodsReceiptNote { get; set; }

        [Column("qc_passed")]
        public bool QcPassed { get; set; } = false;

        [Column("item_id")]
        public int ItemId { get; set; }

        [Column("grn_qty")]
        public decimal GrnQty { get; set; } = 0;

        [Column("pending_qty")]
        public decimal PendingQty { get; set; } = 0;

        [Column("billed_qty")]
        public decimal BilledQty { get; set; } = 0;

        [Column("amount")]
        public decimal Amount { get; set; } = 0;
            [Column("ordered_qty")]
            public decimal OrderedQty { get; set; } = 0;
    }
}
