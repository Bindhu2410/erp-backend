using System;
using System.Collections.Generic;

namespace ERP.API.Models.DTOs
{
    public class OrderAcceptanceFullResponse
    {
        public int Id { get; set; }
        public string OrderAcceptanceId { get; set; }
        public int UserCreated { get; set; }
        public DateTime DateCreated { get; set; }
        public int? UserUpdated { get; set; }
        public DateTime? DateUpdated { get; set; }
        public string Subject { get; set; }
        public string PurchaseOrderId { get; set; }
        public string SalesOrderId { get; set; }
        public string Comments { get; set; }
        public string FileUrl { get; set; }
        public string FileName { get; set; }
        public int? QuotationId { get; set; }
        public object QuotationInfo { get; set; }
        public IEnumerable<object> Items { get; set; }
        public object TermsAndConditions { get; set; }
        public object Address { get; set; }
        public object LeadAddress { get; set; }
        
    }
}
