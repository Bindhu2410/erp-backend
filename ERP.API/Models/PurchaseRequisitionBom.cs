using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.API.Models
{
    public class PurchaseRequisitionBom
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("PurchaseRequisition")]
        public int PurchaseRequisitionId { get; set; }
    public int ItemId { get; set; }
    public int Quantity { get; set; } // Quantity for this item

        // Navigation properties (optional)
        // public PurchaseRequisition PurchaseRequisition { get; set; }
        // public Supplier Supplier { get; set; }
    }
}
