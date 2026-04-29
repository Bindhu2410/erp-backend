using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.API.Models
{
    [Table("item_quality_control", Schema = "public")]
    public class ItemQualityControl
    {
        [Key]
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

        [Column("item_id")]
        public int ItemId { get; set; }

        [Column("qc_flag")]
        public bool? QcFlag { get; set; }

        [Column("qc_template_id")]
        public int? QcTemplateId { get; set; }

        [Column("opening_stock_qty")]
        public decimal? OpeningStockQty { get; set; }
    }
}
