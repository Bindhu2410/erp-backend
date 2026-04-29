using System;
using System.Collections.Generic;
using ERP.API.Models.DTOs;
using ERP.API.Models; // NEW: for SalesTermsAndConditions

namespace ERP.API.Models.DTOs
{
    public class SalesQuotationWithItemsRequest
    {
        public CreateQuotationRequestDto Quotation { get; set; }
        public List<SalesItemRequest> Items { get; set; }
        public SalesTermsAndConditions TermsAndConditions { get; set; } // NEW: for consolidated update
        // Expose CustomerName at the top level for convenience
        public string? CustomerName { get; set; }
        // Contact fields for quotation
        public string? ContactName { get; set; }
        public string? ContactMobileNo { get; set; }
        /// <summary>
        /// Optional: List of child item IDs to include in the response for each parent item
        /// </summary>
        public List<int> ChildItemsId { get; set; }
    }

    public class SalesQuotationWithItemsResponse
    {
        public QuotationResponseDto Quotation { get; set; }
        public string? CustomerName { get; set; }
    public List<object> Items { get; set; }
        public SalesTermsAndConditions TermsAndConditions { get; set; } // Added for API response
        public SalesAddressDto? CustomerAddress { get; set; } // Changed to object for address in response
        public object? LeadAddress { get; set; } // Lead address fields from sales_lead
        // Contact fields for quotation
        public string? ContactName { get; set; }
        public string? ContactMobileNo { get; set; }
    }
}
