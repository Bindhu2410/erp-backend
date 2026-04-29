using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.API.Models
{
    [Table("item_stock_transaction", Schema = "public")]
    public class ItemStockTransaction
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

        [Required]
        [Column("transaction_type")]
        [MaxLength(50)]
        public string TransactionType { get; set; }

        [Column("reference_no")]
        [MaxLength(100)]
        public string ReferenceNo { get; set; }

        [Required]
        [Column("quantity")]
        public decimal Quantity { get; set; }

        [Column("unit_price")]
        public decimal? UnitPrice { get; set; }

        [Column("total_value")]
        public decimal? TotalValue { get; set; }

        [Column("transaction_date")]
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

    [Column("created_by")]
    public int? CreatedBy { get; set; }

    [Column("user_created")]
    public int? UserCreated { get; set; }

    [Column("date_created")]
    public DateTime? DateCreated { get; set; }

    [Column("user_updated")]
    public int? UserUpdated { get; set; }

    [Column("date_updated")]
    public DateTime? DateUpdated { get; set; }

    [Column("remarks")]
    public string Remarks { get; set; }
    }
}
