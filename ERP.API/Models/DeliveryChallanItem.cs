using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.API.Models
{
    [Table("delivery_challan_items")]
    public class DeliveryChallanItem
    {
        [Key]
        public int Id { get; set; }

        [Column("delivery_challan_id")]
        public int DeliveryChallanId { get; set; }

        [Column("item_id")]
        public int ItemId { get; set; }

        [Column("qty")]
        public decimal Qty { get; set; }

        [Column("unit_price")]
        public decimal? UnitPrice { get; set; }

        [Column("amount")]
        public decimal? Amount { get; set; }

        // New Fields from UI
        [Column("so_no")]
        public string? SoNo { get; set; }

        [Column("make")]
        public string? Make { get; set; }

        [Column("category")]
        public string? Category { get; set; }

        [Column("product")]
        public string? Product { get; set; }

        [Column("model")]
        public string? Model { get; set; }

        [Column("visual_item_id")]
        public string? VisualItemId { get; set; }

        [Column("equl_ins")]
        public string? EqulIns { get; set; }

        [Column("match_no")]
        public string? MatchNo { get; set; }

        [Column("ord_qty")]
        public decimal? OrdQty { get; set; }

        [Column("current_stock")]
        public decimal? CurrentStock { get; set; }

        [Column("unit")]
        public string? Unit { get; set; }

        [Column("user_created")]
        public int? UserCreated { get; set; }

        [Column("date_created")]
        public DateTime? DateCreated { get; set; }

        [Column("user_updated")]
        public int? UserUpdated { get; set; }

        [Column("date_updated")]
        public DateTime? DateUpdated { get; set; }
    }
}
