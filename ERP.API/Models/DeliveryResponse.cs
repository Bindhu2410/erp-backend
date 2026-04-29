using System.Collections.Generic;

namespace ERP.API.Models
{
    public class DeliveryResponse : Delivery
    {
        public object? QuotationInfo { get; set; }
        public string? LeadAddress { get; set; }
    }
}
