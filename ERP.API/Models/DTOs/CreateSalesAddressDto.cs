using System;
using System.ComponentModel.DataAnnotations;

namespace ERP.API.Models.DTOs
{
    public class CreateSalesAddressDto
    {
        // Audit Fields
        public int? Id { get; set; }
        public int? UserCreated { get; set; }
        public DateTime? DateCreated { get; set; }
        public int? UserUpdated { get; set; }
        public DateTime? DateUpdated { get; set; }

        // Basic Information
        [Required(ErrorMessage = "Contact name is required")]
        public string ContactName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address type is required")]
        public string Type { get; set; } = string.Empty;

        // Status
        public bool? IsActive { get; set; } = true;

        // Location Details
        public string? Block { get; set; }
        public string? Department { get; set; }
        public string? Area { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public int? Pincode { get; set; }

        // Additional Details
        public string? OpportunityId { get; set; }
        [StringLength(5)]
        public string? DoorNo { get; set; }
        [StringLength(50)]
        public string? Street { get; set; }
        [StringLength(50)]
        public string? Landmark { get; set; }
        public bool? IsDefault { get; set; }
        [Required(ErrorMessage = "Sales Lead ID is required")]
        public int? SalesLeadId { get; set; }
    }
}
