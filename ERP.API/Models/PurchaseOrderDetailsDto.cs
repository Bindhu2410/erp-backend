namespace ERP.API.Models
{
    public class PurchaseOrderDetailsDto
    {
    public PurchaseOrderDto? PurchaseOrder { get; set; }
    [Newtonsoft.Json.JsonProperty("items")]
    public List<SalesItemResponseDto>? Items { get; set; }
    public string VendorName { get; set; }
    }

    public class PurchaseOrderDto
    /// <summary>
    /// Example:
    /// {
    ///   "Items": [
    ///     { "ItemId": 0, "Quantity": 0 }
    ///   ]
    /// }
    /// </summary>
    {
    public int Id { get; set; }
    public int? UserCreated { get; set; }
    public DateTime DateCreated { get; set; }
    public int? UserUpdated { get; set; }
    public DateTime? DateUpdated { get; set; }
    [System.ComponentModel.DataAnnotations.Schema.Column("po_id")]
    public string PoId { get; set; } // external PO identifier
    [System.ComponentModel.DataAnnotations.Schema.Column("purchase_requisition_id")]
    public string PurchaseRequisitionId { get; set; }
    public string Status { get; set; } // Open, Approved, Short Closed, Closed
    public int? SupplierId { get; set; }
    public int? QuotationId { get; set; }
    public int? SalesOrderId { get; set; }
    public DateTime? DeliveryDate { get; set; }
    public string Description { get; set; }
    public List<PurchaseOrderItemDto> Items { get; set; } // Only itemId and quantity for POST
    }

    public class QuotationDto
    {
        public int Id { get; set; }
        public string? QuotationNumber { get; set; }
        public string? Status { get; set; }
        public int? CustomerId { get; set; }
        public string? Version { get; set; }
        public string? Terms { get; set; }
        public DateTime? ValidTill { get; set; }
        public string? QuotationFor { get; set; }
        public string? LostReason { get; set; }
        public string? CustomerName { get; set; }
        public string? QuotationType { get; set; }
        public DateTime? QuotationDate { get; set; }
        public string? OpportunityId { get; set; }
        public string? OrderType { get; set; }
        public string? Comments { get; set; }
        public string? DeliveryWithin { get; set; }
        public string? DeliveryAfter { get; set; }
        public bool IsActive { get; set; }
        public string? QuotationId { get; set; }
        public string? LeadId { get; set; }
        public string? Taxes { get; set; }
        public string? Delivery { get; set; }
        public string? Payment { get; set; }
        public string? Warranty { get; set; }
        public string? FreightCharge { get; set; }
        public bool? IsCurrent { get; set; }
        public DateTime? ExpireDate { get; set; }
        public string? Territory { get; set; }
        public DateTime? ExpectedCompletion { get; set; }
        public string? DeliveryPrepareAfter { get; set; }
        public string? Address { get; set; }
        public string? Contact { get; set; }
        public string? SelectedTaxes { get; set; }
        public string? SelectedFreightCharges { get; set; }
        public string? SelectedDelivery { get; set; }
        public string? SelectedPayment { get; set; }
        public string? SelectedWarranty { get; set; }
        public bool? LatestQuotation { get; set; }
        public bool? Published { get; set; }
        public string? CurrentModifierName { get; set; }
        // Add all other columns from sales_quotations table
    }
}
