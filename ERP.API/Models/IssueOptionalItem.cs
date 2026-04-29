
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.API.Models
{
    [Table("issue_optional_items")]
    public class IssueOptionalItem
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("issue_id")]
        public int IssueId { get; set; }

        [Column("s_no")]
        public int SNo { get; set; }

        [Column("opt_make")]
        public string? OptMake { get; set; }

        [Column("opt_category")]
        public string? OptCategory { get; set; }

        [Column("opt_product")]
        public string? OptProduct { get; set; }

        [Column("opt_model")]
        public string? OptModel { get; set; }

        [Column("opt_item")]
        public string? OptItem { get; set; }

        [Column("opt_item_desc")]
        public string? OptItemDesc { get; set; }

        [Column("opt_qty")]
        public decimal? OptQty { get; set; }

        [Column("opt_rate")]
        public decimal? OptRate { get; set; }

        [Column("opt_amount")]
        public decimal? OptAmount { get; set; }

        [ForeignKey("IssueId")]
        public Issue? Issue { get; set; }
    }
}
