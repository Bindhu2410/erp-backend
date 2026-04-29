using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.API.Models
{
    [Table("sales_product")]
    public class SalesProduct : BaseEntity
    {
        // Compatibility properties for SalesOpportunityController
        [NotMapped]
        public int? Qty { get => Quantity; set => Quantity = value; }
        [NotMapped]
        public int? ItemId { get => InventoryItemsId; set => InventoryItemsId = value; }
        [NotMapped]
        public double? UnitPrice { get; set; }
        [NotMapped]
        public List<int>? IncludedChildItemIds { get; set; }
        [NotMapped]
        public List<int>? AccessoriesIds { get; set; }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override int? Id { get; set; }

        [Column("qty")]
        public int? Quantity { get; set; }

        [Column("amount")]        public double? Amount { get; set; }

    [Column("inventory_items_id")]
    public int? InventoryItemsId { get; set; }

        [Column("stage")]
        [Required]
        [MaxLength(255)]
        public string? Stage { get; set; } = string.Empty;

        [Column("stage_item_id")]
        [Required]
        public string? StageItemId { get; set; } = string.Empty;

        [Column("isactive")]
        public bool? IsActive { get; set; }

        public int? MakeId { get; set; }
        public string? MakeName { get; set; } = string.Empty;
        
        public int? ModelId { get; set; }
        public string? ModelName { get; set; } = string.Empty;
        
        public int? ProductId { get; set; }
        public string? ProductName { get; set; } = string.Empty;
        
        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; } = string.Empty;

        public string? ItemCode { get; set; } = string.Empty;
        public string? ItemName { get; set; } = string.Empty;
        // For nested/recursive item structures (child and accessory items)
        public List<SalesProduct>? IncludedChildItems { get; set; } = new List<SalesProduct>();
        public List<SalesProduct>? AccessoriesItems { get; set; } = new List<SalesProduct>();
        
    [Column("bom_id")]
    public string? BomId { get; set; }

    // Accept array for accessory item ids in request
    [NotMapped]
    public List<int>? BomAccessoryItemIds { get; set; }
    }
    public class SalesProductsDetails : SalesProduct {
        
    }
}
