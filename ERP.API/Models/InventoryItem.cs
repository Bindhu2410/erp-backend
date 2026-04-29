using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.API.Models
{
    [Table("inventory_items")]
    public class InventoryItem : BaseEntity
    {
        [Column("quantity")]
        public int Quantity { get; set; }

        [MaxLength(255)]
        [Column("hsn")]
        public string? Hsn { get; set; }

        [MaxLength(255)]
        [Column("rack")]
        public string? Rack { get; set; }

        [MaxLength(255)]
        [Column("shelf")]
        public string? Shelf { get; set; }

        [MaxLength(255)]
        [Column("column")]
        public string? Column { get; set; }

        [MaxLength(255)]
        [Column("brand")]
        public string? Brand { get; set; }

        [Column("uom")]
        public int? Uom { get; set; }

        [MaxLength(255)]
        [Column("status")]
        public string? Status { get; set; }

        [Column("make_id")]
        public int? MakeId { get; set; }

        [Column("model_id")]
        public int? ModelId { get; set; }

        [Column("product_id")]
        public int? ProductId { get; set; }

        [Column("category_id")]
        public int? CategoryId { get; set; }

        [MaxLength(255)]
        [Column("item_code")]
        public string? ItemCode { get; set; }

        [MaxLength(255)]
        [Column("item_name")]
        public string? ItemName { get; set; }

        [MaxLength(255)]
        [Column("item_description")]
        public string? ItemDescription { get; set; }

        [MaxLength(255)]
        [Column("internal_serial_num")]
        public string? InternalSerialNumber { get; set; }

        [MaxLength(255)]
        [Column("external_serial_num")]
        public string? ExternalSerialNumber { get; set; }

        [MaxLength(255)]
        [Column("tax_percentage")]
        public string? TaxPercentage { get; set; }

        [MaxLength(255)]
        [Column("critical")]
        public string? Critical { get; set; }

        [MaxLength(255)]
        [Column("parent_items_code")]
        public string? ParentItemsCode { get; set; }

        [MaxLength(255)]
        [Column("valuation_method")]
        public string? ValuationMethod { get; set; }

        [MaxLength(255)]
        [Column("category_no")]
        public string? CategoryNo { get; set; }

        [MaxLength(255)]
        [Column("standard_selling_rate")]
        public string? StandardSellingRate { get; set; }

        [MaxLength(255)]
        [Column("minimum_selling_rate")]
        public string? MinimumSellingRate { get; set; }

        [MaxLength(255)]
        [Column("unit_of_measures")]
        public string? UnitOfMeasures { get; set; }

        [Column("group_of_item")]
        public bool? GroupOfItem { get; set; }

        [MaxLength(255)]
        [Column("supplier_id")]
        public string? SupplierId { get; set; }

        [MaxLength(255)]
        [Column("sales_account")]
        public string? SalesAccount { get; set; }

        [MaxLength(255)]
        [Column("safety_stock")]
        public string? SafetyStock { get; set; }

        [MaxLength(255)]
        [Column("buying_unit_of_measure")]
        public string? BuyingUnitOfMeasure { get; set; }

        [MaxLength(255)]
        [Column("item_full_name")]
        public string? ItemFullName { get; set; }

        [MaxLength(255)]
        [Column("consumption_uom")]
        public string? ConsumptionUom { get; set; }

        [MaxLength(255)]
        [Column("buom_to_uom")]
        public string? BuomToUom { get; set; }

        [MaxLength(255)]
        [Column("item_for")]
        public string? ItemFor { get; set; }

        [MaxLength(255)]
        [Column("cuom_to_uom")]
        public string? CuomToUom { get; set; }

        [MaxLength(255)]
        [Column("reorder_qty")]
        public string? ReorderQty { get; set; }

        [MaxLength(255)]
        [Column("purchase_account")]
        public string? PurchaseAccount { get; set; }

        [Column("isactive")]
        public bool IsActive { get; set; }

        [MaxLength(255)]
        [Column("category_name")]
        public string? CategoryNameFromDb { get; set; }

        [Column("inventory_item_categories_id")]
        public int? InventoryItemCategoriesId { get; set; }

        [Column("parent_inventory_items_id")]
        public int? ParentInventoryItemsId { get; set; }

        // Navigation properties for joins
        [NotMapped]
        public string? Make { get; set; }

        [NotMapped]
        public string? Model { get; set; }

        [NotMapped]
        public string? Product { get; set; }        [NotMapped]
        public string? Category { get; set; }

        [NotMapped]
        public string? MakeName { get; set; }

        [NotMapped]
        public string? ModelName { get; set; }

        [NotMapped]
        public string? ProductName { get; set; }

        [NotMapped]
        public string? CategoryName { get; set; }
    }
}