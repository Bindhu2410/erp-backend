using System;
using System.ComponentModel.DataAnnotations;

namespace ERP.API.Models.DTOs
{
    public class CreateSalesAddressRequest
    {
        // Basic Information
        [Required(ErrorMessage = "Contact name is required")]
        public string ContactName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Address type is required")]
        public string Type { get; set; } = string.Empty;
        
        // Location Details
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Pincode { get; set; }
        
        // Additional Details
        public string? Block { get; set; }
        public string? Department { get; set; }
        public string? Area { get; set; }
        public string? OpportunityId { get; set; }
        public string? DoorNo { get; set; }
        public string? Street { get; set; }
        public string? Landmark { get; set; }
          // Configuration
        public bool IsActive { get; set; } = false;
        public bool IsDefault { get; set; } = false;
        
        // Relations
        public string? SalesLeadId { get; set; }
        public bool? ToCommunication { get; set; }
    }
}
