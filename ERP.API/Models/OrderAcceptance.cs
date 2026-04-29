using System;

namespace ERP.API.Models
{
    public class OrderAcceptance
    {
        public int Id { get; set; }
        public string OrderAcceptanceId { get; set; }
        [System.ComponentModel.DataAnnotations.Required]
        public int UserCreated { get; set; }
        [System.ComponentModel.DataAnnotations.Required]
        public DateTime DateCreated { get; set; }
        public int? UserUpdated { get; set; }
        public DateTime? DateUpdated { get; set; }
        [System.ComponentModel.DataAnnotations.Required]
        public string Subject { get; set; }
        [System.ComponentModel.DataAnnotations.Required]
        public string PurchaseOrderId { get; set; }
         public string SalesOrderId { get; set; }
        public string Comments { get; set; }
        public string FileUrl { get; set; }
        public string FileName { get; set; }
        // QuotationId is optional
        public int? QuotationId { get; set; }
        // SalesOrderId - automatically populated based on po_id
       
    }
}
