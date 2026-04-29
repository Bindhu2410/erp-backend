namespace ERP.API.Models
{
    public class SupplierPayload
    {
        public string VendorName { get; set; }
        public string VendorCode { get; set; }
        public string Description { get; set; }
        public decimal? Rate { get; set; }
    }
}
