using System;
using System.ComponentModel.DataAnnotations;

namespace ERP.API.Models.DTOs.CompanySetup
{
    public class CsCompanyDto
    {
        public int CompanyId { get; set; }
        public int? ParentCompanyId { get; set; }
        public string LegalCompanyName { get; set; } = string.Empty;
        public string RegisteredAddressLine1 { get; set; } = string.Empty;
        public string? RegisteredAddressLine2 { get; set; }
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Pincode { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? EmailAddress { get; set; }
        public string? WebsiteUrl { get; set; }
        public string? CompanyLogoPath { get; set; }
        public string BaseCurrency { get; set; } = "INR";
        public DateTime FinancialYearStartDate { get; set; }
        public DateTime FinancialYearEndDate { get; set; }
        public string Pan { get; set; } = string.Empty;
        public string Tan { get; set; } = string.Empty;
        public string Gstin { get; set; } = string.Empty;
        public string LegalEntityType { get; set; } = "Private Limited";
        public string LegalNameAsPerPanTan { get; set; } = string.Empty;
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
