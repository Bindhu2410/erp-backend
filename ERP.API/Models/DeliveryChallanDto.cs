using System;
using System.Collections.Generic;

namespace ERP.API.Models
{
    public class DeliveryChallanRequest
    {
        public int? Id { get; set; }
        public DateTime DeliveryDate { get; set; }
        public string? DeliveryChallanId { get; set; } // Added if user wants to provide custom ID
        public int? SalesOrderId { get; set; }
        public int? SalesmanId { get; set; }
        public int? PartyId { get; set; }
        public string? DeliveryStatus { get; set; }
        public string? DispatchAddress { get; set; }
        public string? Priority { get; set; }
        public string? TransporterName { get; set; }
        public string? VehicleNo { get; set; }
        public string? DriverName { get; set; }
        public long? DriverContact { get; set; }
        public string? ModeOfDelivery { get; set; }
        public string? Notes { get; set; }

        // New Fields from UI
        public string? Location { get; set; }
        public string? Form20SNo { get; set; }
        public string? Form20No { get; set; }
        public string? RefNo { get; set; }
        public DateTime? RefDate { get; set; }
        public string? DispatchedBy { get; set; }
        public string? DeliveredBy { get; set; }
        public string? GoodsConsignFrom { get; set; }
        public string? GoodsConsignTo { get; set; }
        public string? BookingAddress { get; set; }
        public decimal? BookingQty { get; set; }
        public decimal? AppValue { get; set; }
        public string? DeliveryAt { get; set; }
        public string? DeliveryAdd1 { get; set; }
        public string? DeliveryAdd2 { get; set; }
        public string? DocumentThrough { get; set; }
        public string? InvoiceNo { get; set; }
        public DateTime? InvoiceDate { get; set; }

        // Footer Fields
        public decimal? GrossAmount { get; set; }
        public decimal? NetAmount { get; set; }
        public decimal? TotalQty { get; set; }
        public string? AmountInWords { get; set; }
        public string? DeliveryTo { get; set; }
        public string? Remarks { get; set; }
        public string? PreparedBy { get; set; }
        public string? AuthorizedBy { get; set; }
        public string? ReceivedBy { get; set; }

        public int? UserCreated { get; set; }
        public int? UserUpdated { get; set; }
        public List<DeliveryChallanItemRequest>? Items { get; set; }
    }

    public class DeliveryChallanItemRequest
    {
        public int ItemId { get; set; }
        public decimal Qty { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? Amount { get; set; }

        // New Fields from UI
        public string? SoNo { get; set; }
        public string? Make { get; set; }
        public string? Category { get; set; }
        public string? Product { get; set; }
        public string? Model { get; set; }
        public string? VisualItemId { get; set; }
        public string? EqulIns { get; set; }
        public string? MatchNo { get; set; }
        public decimal? OrdQty { get; set; }
        public decimal? CurrentStock { get; set; }
        public string? Unit { get; set; }
    }

    public class DeliveryChallanResponse : DeliveryChallan
    {
        public string? SalesOrderNo { get; set; }
        public string? SalesmanName { get; set; }
        public string? PartyName { get; set; }
        public List<DeliveryChallanItemResponse>? ItemDetails { get; set; }
    }

    public class DeliveryChallanItemResponse : DeliveryChallanItem
    {
        public string? ItemName { get; set; }
        public string? ItemCode { get; set; }
        // Uom, Make, Model are already in DeliveryChallanItem but can be enriched if needed
    }

    public class DeliveryChallanGridRequest
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchText { get; set; }
        public string? Status { get; set; }
    }

    public class DeliveryChallanGridResponse
    {
        public IEnumerable<DeliveryChallanResponse> Data { get; set; } = new List<DeliveryChallanResponse>();
        public int TotalRecords { get; set; }
    }
}
