using System;

namespace ERP.API.Models
{
    public class SalesLeadGridResult
    {
        public int TotalRecords { get; set; }
        public int Id { get; set; }
        public int? UserCreated { get; set; }
        public string? UserCreatedUsername { get; set; }
        public string? UserCreatedRolename { get; set; }
        public DateTime? DateCreated { get; set; }
        public int? UserUpdated { get; set; }
        public DateTime? DateUpdated { get; set; }
        public string? CustomerName { get; set; }
        public string? LeadSource { get; set; }
        public string? ReferralSourceName { get; set; }
        public string? HospitalOfReferral { get; set; }
        public string? DepartmentOfReferral { get; set; }
        public string? SocialMedia { get; set; }
        public DateTime? EventDate { get; set; }
        public string? EventName { get; set; }
        public string? LeadId { get; set; }
        public string? Status { get; set; }
        public string? Score { get; set; }
        public bool IsActive { get; set; }
        public string? Comments { get; set; }
        public string? LeadType { get; set; }
        public string? ContactName { get; set; }
        public string? Salutation { get; set; }
        public string? ContactMobileNo { get; set; }
        public string? LandLineNo { get; set; }
        public string? Email { get; set; }
        public string? Fax { get; set; }
        public string? DoorNo { get; set; }
        public string? Street { get; set; }
        public string? Landmark { get; set; }
        public string? Website { get; set; }
        public int? AreaId { get; set; }
        public int? PincodeId { get; set; }
        public string? Area { get; set; }
        public string? Territory { get; set; }
        public string? City { get; set; }
        public string? Pincode { get; set; }
        public string? District { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }
    }
}