using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.API.Models
{
    [Table("bill_of_material_child_items")]
    public class BillOfMaterialChildItem
    {
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("bill_of_material_id")]
    public int BillOfMaterialId { get; set; }

    [Column("child_item_id")]
    public int ChildItemId { get; set; }

    [Column("quantity")]
    public decimal Quantity { get; set; }

    [Column("amount")]
    public decimal? Amount { get; set; }

    public BillOfMaterial BillOfMaterial { get; set; }
    public ItemMaster ChildItem { get; set; }
    }
}
