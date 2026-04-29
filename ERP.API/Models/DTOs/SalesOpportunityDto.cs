using System;

namespace ERP.API.Models.DTOs
{
    public class SalesOpportunityDto
    {
        public int Id { get; set; }
        public int? UserCreated { get; set; }
        public DateTime? DateCreated { get; set; }
        public int? UserUpdated { get; set; }
        public DateTime? DateUpdated { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? ExpectedCompletion { get; set; }
        public string OpportunityType { get; set; } = string.Empty;
        public string OpportunityFor { get; set; } = string.Empty;
        public string? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerType { get; set; }
        public string OpportunityName { get; set; } = string.Empty;
        public string? OpportunityId { get; set; }
        public string? Comments { get; set; }
        public bool IsActive { get; set; }
        public string? LeadId { get; set; }
        public int? SalesLeadsId { get; set; }
        public int? SalesRepresentativeId { get; set; }
        public string? ContactName { get; set; }
        public string? ContactMobileNo { get; set; }
    }
}
