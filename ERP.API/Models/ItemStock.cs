using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.API.Models
{
    [Table("item_stock", Schema = "public")]
    public class ItemStock
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("item_id")]
        public int ItemId { get; set; }
        [NotMapped]
        public string? ItemName { get; set; }

        [Required]
        [Column("warehouse_id")]
        public int WarehouseId { get; set; }

        [Column("location_id")]
        public int? LocationId { get; set; }

    [Column("quantity_on_hand")]
    public decimal QuantityOnHand { get; set; }

    [Column("allocated_qty")]
    public decimal AllocatedQty { get; set; }

    [Column("stock_value")]
    public decimal StockValue { get; set; }

    [Column("reorder_qty")]
    public decimal ReorderQty { get; set; }

    [Column("last_updated")]
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

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
