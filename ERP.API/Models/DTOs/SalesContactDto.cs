using System;
using System.ComponentModel.DataAnnotations;

namespace ERP.API.Models.DTOs
{    public class SalesContactDto
    {
        public int? Id { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }
        public int? UserCreated { get; set; }
        public int? UserUpdated { get; set; }
        [Required]
        public string ContactName { get; set; } = string.Empty;
        public string? DepartmentName { get; set; }
        public string? Specialist { get; set; }
        public string? Degree { get; set; }
        [EmailAddress]
        public string? Email { get; set; }
        public string? MobileNo { get; set; }
        public string? Website { get; set; }
        public bool IsActive { get; set; } = true;
        public bool? OwnClinic { get; set; }
        public string? VisitingHours { get; set; }
        public string? ClinicVisitingHours { get; set; }
        public string? LandLineNo { get; set; }
        public string? Fax { get; set; }
        public string? Salutation { get; set; }
        public string? JobTitle { get; set; }
        public bool? IsDefault { get; set; } = false;
        public int? SalesLeadId { get; set; }
    }
}
