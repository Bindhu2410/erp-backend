using System;
using System.Collections.Generic;

namespace ERP.API.Models
{    public class SalesLeadFilterResult
    {
        public int Id { get; set; }
        public string? CustomerName { get; set; }
        public string? TerritoryName { get; set; }
        public string? Status { get; set; }
        public int? Score { get; set; }
        public string? LeadType { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? ContactName { get; set; }
        public string? ContactEmail { get; set; }
        public string? Priority { get; set; }
        public long TotalCount { get; set; }
    }

    public class SalesLeadFilterResponse
    {
        public IEnumerable<SalesLeadFilterResult> Leads { get; set; } = new List<SalesLeadFilterResult>();
        public long TotalCount { get; set; }
    }

    public class SalesLeadDropdownOptions
    {
        public string[]? Territories { get; set; }
        public string[]? Customers { get; set; }
        public string[]? Statuses { get; set; }
        public string[]? Scores { get; set; }
        public string[]? LeadTypes { get; set; }
    }
}
