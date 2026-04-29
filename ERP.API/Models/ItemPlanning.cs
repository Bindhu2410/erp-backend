using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.API.Models
{
    [Table("item_planning", Schema = "public")]
    public class ItemPlanning
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

        [Column("safety_stock_primary_uom")]
        public decimal? SafetyStockPrimaryUom { get; set; }

        [Column("minimum_order_qty_primary")]
        public decimal? MinimumOrderQtyPrimary { get; set; }

        [Column("maximum_order_qty_primary_uom")]
        public decimal? MaximumOrderQtyPrimaryUom { get; set; }

        [Column("standard_cost_price")]
        public decimal? StandardCostPrice { get; set; }

        [Column("order_days")]
        public decimal? OrderDays { get; set; }

        [Column("average_lead_time")]
        public decimal? AverageLeadTime { get; set; }

        [Column("reorder_qty_primary")]
        public decimal? ReorderQtyPrimary { get; set; }

        [Column("minimum_stock_primary")]
        public decimal? MinimumStockPrimary { get; set; }

        [Column("purchase_received_qty")]
        public decimal? PurchaseReceivedQty { get; set; }

        [Column("purchase_issued_qty")]
        public decimal? PurchaseIssuedQty { get; set; }
    }
}
