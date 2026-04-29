using System;

namespace ERP.API.Models.DTOs
{
    public class OrderProcessingSummaryDto
    {
        public string PurchaseOrderId { get; set; }
        public DateTime PurchaseOrderCreatedDate { get; set; }
        public DateTime? InvoiceGeneratedDate { get; set; }
        public string InvoiceId { get; set; }
    }
}
