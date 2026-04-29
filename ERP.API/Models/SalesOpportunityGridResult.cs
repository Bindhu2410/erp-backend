using System;

namespace ERP.API.Models
{
    public class SalesOpportunityGridResult
    {
        public int TotalRecords { get; set; }
        public string? Id { get; set; }
        public int? UserCreated { get; set; }
        public DateTime? DateCreated { get; set; }
        public int? UserUpdated { get; set; }
        public DateTime? DateUpdated { get; set; }
        public string? OpportunityId { get; set; }
        public string? CustomerName { get; set; }
        public string? ContactName { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
        public string? Status { get; set; }
        public string? Stage { get; set; }
        public decimal? EstimatedValue { get; set; }
        public decimal? ActualValue { get; set; }
        public DateTime? ExpectedClosingDate { get; set; }
        public string? OpportunityType { get; set; }
        public string? BusinessChallenge { get; set; }
        public bool IsActive { get; set; }
        public string? Comments { get; set; }
        public int? TerritoryId { get; set; }
        public string? TerritoryName { get; set; }
        public string? CityName { get; set; }
        public string? StateName { get; set; }
        public string? ProbabilityOfWinning { get; set; }
        public string? CompetitorName { get; set; }
        public string? SalesRepresentative { get; set; }
        public string? OpportunityFor { get; set; } // Added for grid API

        // Newly added fields for grid API
        public DateTime? ExpectedCompletion { get; set; }
        public string? CustomerId { get; set; }
        public string? CustomerType { get; set; }
        public string? OpportunityName { get; set; }
        public string? LeadId { get; set; }
        public int? SalesRepresentativeId { get; set; }
        public string? ContactMobileNo { get; set; }
    }
}
