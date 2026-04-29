using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.API.Models
{
    [Table("bill_of_material_optional_items")]
    public class BillOfMaterialOptionalItem
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("bill_of_material_id")]
        public int BillOfMaterialId { get; set; }

        [Column("optional_item_id")]
        public int OptionalItemId { get; set; }

        [Column("quantity")]
        public decimal Quantity { get; set; } = 1;

        [Column("amount")]
        public decimal? Amount { get; set; }

        [Column("remarks")]
        public string? Remarks { get; set; }

        public BillOfMaterial BillOfMaterial { get; set; }
        public ItemMaster OptionalItem { get; set; }
    }

    public class BillOfMaterialOptionalItemRequestDto
    {
        public int BillOfMaterialId { get; set; }
        public int OptionalItemId { get; set; }
        public decimal Quantity { get; set; } = 1;
        public decimal? Amount { get; set; }
        public string? Remarks { get; set; }
    }
}
