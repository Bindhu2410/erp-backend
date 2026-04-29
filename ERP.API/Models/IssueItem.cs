
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.API.Models
{
    [Table("issue_items")]
    public class IssueItem
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("issue_id")]
        public int IssueId { get; set; }

        [Column("s_no")]
        public int SNo { get; set; }

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

        [Column("equ_ins")]
        public string? EquIns { get; set; }

        [Column("batch_no")]
        public string? BatchNo { get; set; }

        [Column("receipt_no")]
        public string? ReceiptNo { get; set; }

        [Column("unit")]
        public string? Unit { get; set; }

        [Column("qty_avl")]
        public decimal? QtyAvl { get; set; }

        [Column("qty")]
        public decimal? Qty { get; set; }

        [Column("rate")]
        public decimal? Rate { get; set; }

        [Column("amount")]
        public decimal? Amount { get; set; }

        [Column("remarks")]
        public string? Remarks { get; set; }

        [Column("hsn_code")]
        public string? HsnCode { get; set; }

        [Column("cgst_rate")]
        public decimal? CgstRate { get; set; }

        [Column("sgst_rate")]
        public decimal? SgstRate { get; set; }

        [Column("igst_rate")]
        public decimal? IgstRate { get; set; }

        [Column("cgst_amount")]
        public decimal? CgstAmount { get; set; }

        [Column("sgst_amount")]
        public decimal? SgstAmount { get; set; }

        [Column("igst_amount")]
        public decimal? IgstAmount { get; set; }

        [ForeignKey("IssueId")]
        public Issue? Issue { get; set; }
    }
}
