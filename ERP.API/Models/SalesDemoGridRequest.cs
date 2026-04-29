using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace ERP.API.Models
{
    public class SalesDemoGridRequest
    {
        /// <summary>Text to search in demo grid columns</summary>
        public string? SearchText { get; set; }

        /// <summary>Filter by customer names</summary>
        public string[]? CustomerNames { get; set; }

        /// <summary>Filter by statuses</summary>
        public string[]? Statuses { get; set; }

        /// <summary>Filter by demo approaches</summary>
        public string[]? DemoApproaches { get; set; }

        /// <summary>Filter by demo outcomes</summary>
        public string[]? DemoOutcomes { get; set; }

        /// <summary>Filter by selected demo IDs</summary>
        public int[]? SelectedDemoIds { get; set; }

        /// <summary>Filter by presenter IDs</summary>
        public int[]? PresenterIds { get; set; }

        /// <summary>Filter by opportunity ID</summary>
        public string? OpportunityId { get; set; }

        /// <summary>Filter by lead ID</summary>
        public string? LeadId { get; set; }

        /// <summary>Filter by contact mobile number</summary>
        public string? ContactMobileNum { get; set; }

        /// <summary>Filter by address</summary>
        public string? Address { get; set; }

        /// <summary>Filter by user who created the demo</summary>
        public int? UserCreated { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Page number must be greater than 0")]
        public int PageNumber { get; set; } = 1;

        [Range(1, int.MaxValue, ErrorMessage = "Page size must be greater than 0")]
        public int PageSize { get; set; } = 10;

        [System.Text.Json.Serialization.JsonPropertyName("OrderBy")]
        public string OrderBy { get; set; } = "date_created";

        [RegularExpression("^(ASC|DESC)$", ErrorMessage = "Order direction must be either 'ASC' or 'DESC'")]
        [System.Text.Json.Serialization.JsonPropertyName("OrderDirection")]
        public string OrderDirection { get; set; } = "DESC";
    }
}
