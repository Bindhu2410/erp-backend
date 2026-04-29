using System;
using System.Collections.Generic;

namespace ERP.API.Models.DTOs
{
    public class SalesDemoDetailsDto
    {
        public int? Id { get; set; }
        public int? UserCreated { get; set; }
        public DateTime? DateCreated { get; set; }
        public int? UserUpdated { get; set; }
        public DateTime? DateUpdated { get; set; }
        public int? UserId { get; set; }
        public DateTime DemoDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? OpportunityId { get; set; }
        public string DemoContact { get; set; } = string.Empty;
        public string DemoName { get; set; } = string.Empty;
        public string DemoApproach { get; set; } = string.Empty;
        public string DemoOutcome { get; set; } = string.Empty;
        public string DemoFeedback { get; set; } = string.Empty;
        public string Comments { get; set; } = string.Empty;
        public int? PresenterId { get; set; }
        public string? PresenterName { get; set; }
        public List<int> PresenterIds { get; set; } = new List<int>();
        public List<string> PresenterNames { get; set; } = new List<string>();
        public string? LeadId { get; set; }
        public string? ContactMobileNum { get; set; }
        public string? CustomerName { get; set; }
    }
}
