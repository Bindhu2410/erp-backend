using System;

namespace ERP.API.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public int? UserCreated { get; set; }
        public DateTime DateCreated { get; set; }
        public int? UserUpdated { get; set; }
        public DateTime? DateUpdated { get; set; }
        [System.ComponentModel.DataAnnotations.Required]
        public string InvoiceId { get; set; }
        public DateTime PaymentDate { get; set; }
        public DateTime? DueDate { get; set; }
        public string PaymentMethod { get; set; }
        public decimal AmountPaid { get; set; }
        public string PaymentStatus { get; set; }
        public decimal? OutstandingAmount { get; set; }
        public decimal? TotalAmount { get; set; }
        // CustomerName removed from request model
    }

    public class PaymentResponse : Payment
    {
        public string CustomerName { get; set; }
    }
}
