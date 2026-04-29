using System;
using System.ComponentModel.DataAnnotations;

namespace ERP.API.Models
{
    public class SalesLeadEntity
    {
        public int Id { get; set; }
        public int? UserCreated { get; set; }
        public DateTime? DateCreated { get; set; }
        public int? UserUpdated { get; set; }
        public DateTime? DateUpdated { get; set; }
        
        [Required]
        [StringLength(100)]
        public string CustomerName { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string? LeadSource { get; set; }
        
        [StringLength(100)]
        public string? ReferralSourceName { get; set; }
        
        [StringLength(100)]
        public string? HospitalOfReferral { get; set; }
        
        [StringLength(100)]
        public string? DepartmentOfReferral { get; set; }
        
        [StringLength(50)]
        public string? SocialMedia { get; set; }
        
        public DateTime? EventDate { get; set; }
        
        
        [StringLength(100)]
        public string? EventName { get; set; }
        
        [StringLength(50)]
        public string? LeadId { get; set; }
        
        [StringLength(50)]
        public string? Status { get; set; }
        
        [StringLength(20)]
        public string? Score { get; set; }
        
        public bool IsActive { get; set; }
        
        [StringLength(500)]
        public string? Comments { get; set; }
        
        [StringLength(50)]
        public string? LeadType { get; set; }
        
        [StringLength(100)]
        public string? ContactName { get; set; }
        
        [StringLength(20)]
        public string? Salutation { get; set; }
        
        public long? ContactMobileNo { get; set; }
        
        [StringLength(20)]
        public string? LandLineNo { get; set; }
        
        [StringLength(100)]
        [EmailAddress]
        public string? Email { get; set; }
        
        [StringLength(20)]
        public string? Fax { get; set; }
        
        [StringLength(50)]
        public string? DoorNo { get; set; }
        
        [StringLength(100)]
        public string? Street { get; set; }
        
        [StringLength(100)]
        public string? Landmark { get; set; }
        
        [StringLength(100)]
        [Url]
        public string? Website { get; set; }
        
        public int? GeographicalDivisionsId { get; set; }
        
        [StringLength(100)]
        public string? Territory { get; set; }
        
        public int? AreaId { get; set; }
        
        [StringLength(100)]
        public string? Area { get; set; }
        
        [StringLength(100)]
        public string? City { get; set; }
        
        public int? PincodeId { get; set; }
        
        [StringLength(10)]
        public string? Pincode { get; set; }
        
        [StringLength(100)]
        public string? District { get; set; }
        
        [StringLength(100)]
        public string? State { get; set; }
        
        [StringLength(100)]
        public string? Country { get; set; }
        
        [StringLength(50)]
        public string? ConvertedCustomerId { get; set; }
        
        public int? UserId { get; set; }
        /// <summary>
        /// The user id to whom the lead is assigned.
        /// </summary>
        public int? AssignedTo { get; set; }
    }
}
