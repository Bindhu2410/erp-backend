using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.API.Models
{
    [Table("receipt_accessories")]
    public class ReceiptAccessory
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("receipt_id")]
        public int ReceiptId { get; set; }

        [Column("s_no")]
        public int? SNo { get; set; }

        [Column("accessories")]
        public string? Accessories { get; set; }

        [Column("iss_acc_qty")]
        public decimal? IssAccQty { get; set; }

        [Column("re_acc_qty")]
        public decimal? ReAccQty { get; set; }

        [ForeignKey("ReceiptId")]
        public virtual Receipt? Receipt { get; set; }
    }
}
