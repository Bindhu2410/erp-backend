using System;

namespace ERP.API.Models.DTOs.CompanySetup
{
    public class CsBranchDto
    {
        public int BranchId { get; set; }
        public int CompanyId { get; set; }
        public string BranchCode { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;
        public string BranchAddressLine1 { get; set; } = string.Empty;
        public string? BranchAddressLine2 { get; set; }
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Pincode { get; set; } = string.Empty;
        public string? BranchPhoneNumber { get; set; }
        public string? BranchEmailAddress { get; set; }
        public string? BranchGstin { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsHeadOffice { get; set; } = false;
        public string? CompanyName { get; set; }
        public string? CompanyCode { get; set; }
        public long? TotalCount { get; set; }
    }
}
