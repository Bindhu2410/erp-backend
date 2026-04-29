namespace ERP.API.Models
{
    public class SupplierResponse
    {
        public int SupplierId { get; set; }
        public string? VendorName { get; set; }
        public string? VendorCode { get; set; }
        public decimal? PurchaseRate { get; set; }
        public string? Description { get; set; }
    }
}
