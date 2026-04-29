namespace ERP.API.Models
{
    public class PurchaseOrderStatusUpdateRequest
    {
        public int PoIdInt { get; set; } // Internal PO id
        public string Status { get; set; }
    }
}
