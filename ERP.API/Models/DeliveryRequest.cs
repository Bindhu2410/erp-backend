using System;
using System.Collections.Generic;

namespace ERP.API.Models
{
    public class DeliveryRequest
    {
        public int? UserCreated { get; set; }
        public DateTime? DateCreated { get; set; }
        public int? UserUpdated { get; set; }
        public DateTime? DateUpdated { get; set; }
        public string? SalesOrderId { get; set; }
        public string? PoId { get; set; }
        public string? DeliveryId { get; set; }
        public DateTime DeliveryDate { get; set; }
        public string? DeliveryStatus { get; set; }
        public string? DispatchAddress { get; set; }
        public string? Priority { get; set; }
        public string? TransporterName { get; set; }
        public string? VehicleNo { get; set; }
        public string? DriverName { get; set; }
        public long? DriverContact { get; set; }
        public string? ModeOfDelivery { get; set; }
        public string? InvoiceId { get; set; }
        public List<DeliveryItemRequest>? Items { get; set; }
    }

    public class DeliveryItemRequest
    {
        public int ItemId { get; set; }
        public int UserCreated { get; set; }
        public DateTime DateCreated { get; set; }
        public int UserUpdated { get; set; }
        public DateTime DateUpdated { get; set; }
        public int Qty { get; set; }
        public decimal Amount { get; set; }
        public bool IsActive { get; set; }
        public decimal UnitPrice { get; set; }
        public int[]? IncludedChildItemIds { get; set; }
        public int[]? AccessoriesIds { get; set; }
    }
}
