using System.ComponentModel;

namespace ERP.API.Models
{
    public class SalesOpportunityGridRequest
    {
        [DefaultValue(null)]
        public string? SearchText { get; set; }

        [DefaultValue(null)]
        public string[]? CustomerNames { get; set; }

        [DefaultValue(null)]
        public string[]? Territories { get; set; }

        [DefaultValue(null)]
        public string[]? Statuses { get; set; }

        [DefaultValue(null)]
        public string[]? Stages { get; set; }

        [DefaultValue(null)]
        public string[]? OpportunityTypes { get; set; }

        [DefaultValue(1)]
        public int? PageNumber { get; set; } = 1;

        [DefaultValue(10)]
        public int? PageSize { get; set; } = 10;

        [DefaultValue("sales_opportunities.date_created")]
        public string? OrderBy { get; set; } = "sales_opportunities.date_created";

        [DefaultValue("DESC")]
        public string? OrderDirection { get; set; } = "DESC";

        [DefaultValue(null)]
        public string[]? SelectedOpportunityIds { get; set; }

        [DefaultValue(null)]
        public int? UserCreated { get; set; }

        [DefaultValue(null)]
        public string[]? LeadIds { get; set; }
    }
}
