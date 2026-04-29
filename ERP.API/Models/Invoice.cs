namespace ERP.API.Models
{
    public class CreateInvoiceRequest
    {
        public int? QuotationId { get; set; }
        public string PoId { get; set; }
        public string SalesOrderId { get; set; }
        public string DeliveryId { get; set; } // Added for delivery-invoice link
        // Add other fields as needed
    }

    public class InvoiceResponse
    {
        public int? Id { get; set; }
        public string? InvoiceId { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? po_id { get; set; }
        public string? sales_order_id { get; set; }
        public int quotation_id { get; set; }
        public string delivery_id { get; set; } // Added delivery_id
        public object? PurchaseOrderInfo { get; set; }
        public object? QuotationInfo { get; set; }
        public object? Items { get; set; }
        public object? TermsAndConditions { get; set; }
        public ERP.API.Models.Delivery Delivery { get; set; } // Added delivery details
        // Add other fields as needed
    }

    public class InvoiceDto
    {
        public int? Id { get; set; }
        public string? InvoiceId { get; set; }
        public decimal? TotalAmount { get; set; }
        public string? Status { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? PoId { get; set; }
        public string? SalesOrderId { get; set; }
        public int? QuotationId { get; set; }
        public string? DeliveryId { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? Amount { get; set; }
        // Add more fields here if your table has more columns
        public object? QuotationInfo { get; set; }
        public object? Items { get; set; }
    }
}
