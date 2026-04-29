using System;

namespace ERP.API.Models
{
    public class SalesDemoGrid
    {
        public int Id { get; set; }
        public int UserCreated { get; set; }
        public DateTime DateCreated { get; set; }
        public int? UserUpdated { get; set; }
        public DateTime? DateUpdated { get; set; }
        public int UserId { get; set; }
        public DateTime? DemoDate { get; set; }
        public string? Status { get; set; }
        public string? OpportunityId { get; set; }
        public int? CustomerId { get; set; } // int? to match SQL/table
        public string? DemoContact { get; set; }
        public string? CustomerName { get; set; }
        public string? DemoName { get; set; } // varchar(100) in table
        public string? DemoApproach { get; set; }
        public string? DemoOutcome { get; set; }
        public string? DemoFeedback { get; set; }
        public string? Comments { get; set; }
        public string? LeadId { get; set; }
        public string? ContactMobileNum { get; set; }
        public string? Address { get; set; }
        public int[]? PresenterIds { get; set; }
        public int TotalRecords { get; set; }
    }
}
