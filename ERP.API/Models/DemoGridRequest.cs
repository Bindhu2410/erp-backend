using System;
using System.ComponentModel.DataAnnotations;

namespace ERP.API.Models
{
    public class DemoGridRequest
    {
        public string? SearchText { get; set; }
        public string[]? CustomerNames { get; set; }
        public string[]? Statuses { get; set; }
        public string[]? DemoApproaches { get; set; }
        public string[]? DemoOutcomes { get; set; }
        public int[]? SelectedDemoIds { get; set; }
        public int[]? PresenterIds { get; set; }
        public string? LeadId { get; set; }
        public string? ContactMobileNum { get; set; }
        public string? Address { get; set; }
        public int UserCreated { get; set; } // Required for strict user filtering

        [Range(1, int.MaxValue, ErrorMessage = "Page number must be greater than 0")]
        public int PageNumber { get; set; } = 1;

        [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
        public int PageSize { get; set; } = 10;

        public string OrderBy { get; set; } = "date_created";

        [RegularExpression("^(ASC|DESC)$", ErrorMessage = "Order direction must be either 'ASC' or 'DESC'")]
        public string OrderDirection { get; set; } = "DESC";
    }
}
