using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.API.Models
{
    [Table("item_location_stock", Schema = "public")]
    public class ItemLocationStock
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

        // warehouse_id removed from schema; matching now uses item_id + rack + shelf + column_no

        [Column("rack")]
        [MaxLength(50)]
        public string Rack { get; set; }

        [Column("shelf")]
        [MaxLength(50)]
        public string Shelf { get; set; }

        [Column("column_no")]
        [MaxLength(50)]
        public string ColumnNo { get; set; }

        [Column("in_place")]
        [MaxLength(100)]
        public string InPlace { get; set; }

        [Column("opening_stock")]
        public decimal? OpeningStock { get; set; }

        [Column("opening_stock_value")]
        public decimal? OpeningStockValue { get; set; }

        [Column("opening_cost_rate")]
        public decimal? OpeningCostRate { get; set; }

        [Column("reorder_level")]
        public decimal? ReorderLevel { get; set; }

        [Column("min_level")]
        public decimal? MinLevel { get; set; }

        [Column("max_level")]
        public decimal? MaxLevel { get; set; }

        [Column("selling_rate")]
        public decimal? SellingRate { get; set; }

        [Column("received_qty_primary")]
        public decimal? ReceivedQtyPrimary { get; set; }

        [Column("issued_qty_primary")]
        public decimal? IssuedQtyPrimary { get; set; }

        [Column("purchase_unit")]
        [MaxLength(50)]
        public string PurchaseUnit { get; set; }

        [Column("euro_purchase_rate")]
        public decimal? EuroPurchaseRate { get; set; }

        [Column("inclusive_tax_price")]
        public decimal? InclusiveTaxPrice { get; set; }

        // SupplierId removed as per new schema
    }
}
