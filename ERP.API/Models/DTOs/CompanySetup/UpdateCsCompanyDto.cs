using System;
using System.ComponentModel.DataAnnotations;

namespace ERP.API.Models.DTOs.CompanySetup
{
    public class UpdateCsCompanyDto
    {
        [Required(ErrorMessage = "Company ID is required")]
        public int CompanyId { get; set; }

        public int? ParentCompanyId { get; set; }

        [Required(ErrorMessage = "Legal company name is required")]
        [StringLength(255)]
        public string LegalCompanyName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Registered address line 1 is required")]
        [StringLength(225)]
        public string RegisteredAddressLine1 { get; set; } = string.Empty;

        [StringLength(225)]
        public string? RegisteredAddressLine2 { get; set; }

        [Required(ErrorMessage = "City is required")]
        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "State is required")]
        [StringLength(100)]
        public string State { get; set; } = string.Empty;

        [Required(ErrorMessage = "Pincode is required")]
        [StringLength(10)]
        public string Pincode { get; set; } = string.Empty;

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [StringLength(255)]
        [EmailAddress]
        public string? EmailAddress { get; set; }

        [StringLength(255)]
        [Url]
        public string? WebsiteUrl { get; set; }

        [StringLength(255)]
        public string? CompanyLogoPath { get; set; }

        [StringLength(5)]
        public string BaseCurrency { get; set; } = "INR";

        [Required(ErrorMessage = "Financial year start date is required")]
        public DateTime FinancialYearStartDate { get; set; }

        [Required(ErrorMessage = "Financial year end date is required")]
        public DateTime FinancialYearEndDate { get; set; }

        [Required(ErrorMessage = "PAN is required")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "PAN must be exactly 10 characters")]
        [RegularExpression(@"^[A-Z]{5}[0-9]{4}[A-Z]{1}$", ErrorMessage = "Invalid PAN format")]
        public string Pan { get; set; } = string.Empty;

        [Required(ErrorMessage = "TAN is required")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "TAN must be exactly 10 characters")]
        public string Tan { get; set; } = string.Empty;

        [Required(ErrorMessage = "GSTIN is required")]
        [StringLength(15, MinimumLength = 15, ErrorMessage = "GSTIN must be exactly 15 characters")]
        [RegularExpression(@"^[0-9]{2}[A-Z]{5}[0-9]{4}[A-Z]{1}[1-9A-Z]{1}[Z]{1}[0-9A-Z]{1}$", ErrorMessage = "Invalid GSTIN format")]
        public string Gstin { get; set; } = string.Empty;

        [Required(ErrorMessage = "Legal entity type is required")]
        [StringLength(50)]
        public string LegalEntityType { get; set; } = "Private Limited";

        [Required(ErrorMessage = "Legal name as per PAN/TAN is required")]
        [StringLength(255)]
        public string LegalNameAsPerPanTan { get; set; } = string.Empty;
    }
}
