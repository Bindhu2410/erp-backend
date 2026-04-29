using System;
using System.ComponentModel.DataAnnotations;

namespace ERP.API.Models.DTOs
{
    public class SalesAddressDto
    {
        public int? Id { get; set; }
        public int? UserCreated { get; set; }
        public DateTime? DateCreated { get; set; }
        public int? UserUpdated { get; set; }
        public DateTime? DateUpdated { get; set; }
        public string? ContactName { get; set; }
        public string? SalesLeadId { get; set; }
        public bool? IsActive { get; set; } = true;
        public string? ContactMobileNum { get; set; }
                public string? Type { get; set; }
        public string? Block { get; set; }
        public string? Department { get; set; }
        public string? Area { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public int? Pincode { get; set; }
        public string? OpportunityId { get; set; }
        public string? DoorNo { get; set; }
        public string? Street { get; set; }
        public string? Landmark { get; set; }
        public bool? IsDefault { get; set; }
    }
}