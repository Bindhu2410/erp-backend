using System;

namespace ERP.API.Models.DTOs
{
    public class QuotationWithOrderResponse
    {
        public QuotationDetails Quotation { get; set; }
        public SalesOrderDetails? SalesOrder { get; set; }
    }

    public class QuotationDetails
    {
        // Base audit fields
        public int? UserCreated { get; set; }
        public DateTime? DateCreated { get; set; }
        public int? UserUpdated { get; set; }
        public DateTime? DateUpdated { get; set; }

        // Quotation fields
        public int Id { get; set; }
        public string? QuotationType { get; set; }
        public string? OrderType { get; set; }
        public DateTime? QuotationDate { get; set; }
        public string? Status { get; set; }
        public string? Version { get; set; }
        public string? Terms { get; set; }
        public DateTime? ValidTill { get; set; }
        public string? QuotationFor { get; set; }
        public string? LostReason { get; set; }
        public int? CustomerId { get; set; }
        public string? Comments { get; set; }
        public string? DeliveryWithin { get; set; }
        public string? DeliveryAfter { get; set; }
        public bool IsActive { get; set; }
        public string? QuotationId { get; set; }
        public string? OpportunityId { get; set; }
        public string? LeadId { get; set; }
        public int Taxes { get; set; }
        public string? Delivery { get; set; }
        public string? Payment { get; set; }
        public string? Warranty { get; set; }
        public int FreightCharge { get; set; }
        public bool? IsCurrent { get; set; }
        public int? ParentSalesQuotationsId { get; set; }
        public int Discount { get; set; }

        // Customer Information
        public string? CustomerName { get; set; }
        public string? CustomerAddress { get; set; }
        public string? CustomerMobile { get; set; }
        public string? CustomerEmail { get; set; }

        // Amounts for compatibility with SalesOrder creation
        public decimal? TotalAmount { get; set; }
        public decimal? TaxAmount { get; set; }
        public decimal? GrandTotal { get; set; }
    }

    public class SalesOrderDetails
    {
        public int Id { get; set; }
        public string? OrderId { get; set; }
        public DateTime? OrderDate { get; set; }
        public DateTime? ExpectedDeliveryDate { get; set; }
        public string? Status { get; set; }
        public string? PoId { get; set; }
        public DateTime? AcceptanceDate { get; set; }
        public decimal? TotalAmount { get; set; }
        public decimal? TaxAmount { get; set; }
        public decimal? GrandTotal { get; set; }
        public string? Notes { get; set; }
    }
}
