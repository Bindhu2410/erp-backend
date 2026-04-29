  
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.API.Models
{
    [Table("item_master")]
    public class ItemMaster
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
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

        [Column("group_id")]
        public int? GroupId { get; set; }

        [Column("category_id")]
        public int? CategoryId { get; set; }

        [Column("long_item_name")]
        [MaxLength(500)]
        public string? LongItemName { get; set; }

        [Column("item_description")]
        [MaxLength(255)]
        public string? ItemDescription { get; set; }

        // DB stores foreign keys for make/model/product
        [Column("make_id")]
        public int? MakeId { get; set; }

        [Column("model_id")]
        public int? ModelId { get; set; }

        [Column("product_id")]
        public int? ProductId { get; set; }

        [Column("supplier_id")]
        public int? SupplierId { get; set; }

        // These are convenience fields populated by joins on reads; not persisted to this table
        [NotMapped]
        public string? Make { get; set; }

        [NotMapped]
        public string? Model { get; set; }

        [NotMapped]
        public string? Product { get; set; }

        [Column("brand")]
        [MaxLength(255)]
        public string? Brand { get; set; }

        [Column("item_name")]
        [MaxLength(255)]
        public string? ItemName { get; set; }

        [Column("item_code")]
        [MaxLength(255)]
        public string? ItemCode { get; set; }

        [Column("inventory_type")]
        [MaxLength(100)]
        public string? InventoryType { get; set; }

        // Category name is derived elsewhere; table stores category_id
        [NotMapped]
        [MaxLength(100)]
        public string? Category { get; set; }

    

        [Column("specification")]
        [MaxLength(255)]
        public string? Specification { get; set; }

      

    

        [Column("criticality")]
        [MaxLength(10)]
        public string? Criticality { get; set; }

        [Column("stock_to_bank")]
        [MaxLength(100)]
        public string? StockToBank { get; set; }

        [Column("lp_rate")]
        public decimal? LpRate { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("image_url")]
        [MaxLength(2048)]
        public string? ImageUrl { get; set; }

        [Column("unit_price")]
        public decimal? UnitPrice { get; set; }

        [Column("uom_id")]
        public int? UomId { get; set; }

        [Column("cat_no")]
        [MaxLength(255)]
        public string? CatNo { get; set; }

        [Column("inventory_method_id")]
        public int? InventoryMethodId { get; set; }

        [Column("hsn")]
        [MaxLength(20)]
        public string? Hsn { get; set; }

        [Column("tax_percentage")]
        public decimal? TaxPercentage { get; set; }

        [Column("valuation_method_id")]
        public int? ValuationMethodId { get; set; }
        
        [Column("valuation_method")]
        [MaxLength(100)]
        public string? ValuationMethodText { get; set; }

        [Column("related_stock_account")]
        [MaxLength(100)]
        public string? RelatedStockAccount { get; set; }

       
        [Column("cf")]
        public int? Cf { get; set; }

   

        [Column("bom_applicable")]
        public bool? BomApplicable { get; set; }

    

        // Backwards-compatibility: string UOM label used elsewhere
        [NotMapped]
        public string? Uom { get; set; }
    }
}
