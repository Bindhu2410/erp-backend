namespace ERP.API.Models
{
    public class PurchaseRequisitionDropdownDto
    {
        public int Id { get; set; }
        public string PurchaseRequisitionId { get; set; }
        public string RequesterName { get; set; }
        public string Description { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public decimal? BudgetAmount { get; set; }
        public string Status { get; set; }
        public int? UserCreated { get; set; }
        public int? UserUpdated { get; set; }
        public string VendorName { get; set; }
        public int? SupplierId { get; set; }
        public List<SalesItemResponse> Items { get; set; } = new List<SalesItemResponse>();
    }
}