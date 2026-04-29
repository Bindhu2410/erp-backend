using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using ERP.API.Models;

namespace ERP.API.Models
{
    [Table("sales_lead")]
    public class SalesLead : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override int? Id { get; set; }

        [Column("customer_name")]
        [MaxLength(255)]        public string? CustomerName { get; set; }

        [Column("lead_source")]
        [MaxLength(255)]
        public string? LeadSource { get; set; }

        [Column("referral_source_name")]
        [MaxLength(255)]
        public string? ReferralSourceName { get; set; }

        [Column("hospital_of_referral")]
        [MaxLength(255)]
        public string? HospitalOfReferral { get; set; }

        [Column("department_of_referral")]
        [MaxLength(255)]
        public string? DepartmentOfReferral { get; set; }

        [Column("social_media")]
        [MaxLength(255)]
        public string? SocialMedia { get; set; }

        [Column("event_date")]
        public DateTime? EventDate { get; set; }


        [Column("event_name")]
        [MaxLength(255)]
        public string? EventName { get; set; }

        [Column("lead_id")]
        [MaxLength(255)]
        public string? LeadId { get; set; }

        [Column("status")]        [MaxLength(255)]
        public string? Status { get; set; }

        [Column("score")]
        [MaxLength(255)]
        public string? Score { get; set; }

        [Column("isactive")]
        public bool? IsActive { get; set; }

        [Column("comments")]
        public string? Comments { get; set; }

        [Column("lead_type")]
        [MaxLength(255)]        public string? LeadType { get; set; }

        [Column("contact_name")]
        [MaxLength(100)]
        public string? ContactName { get; set; }

        [Column("salutation")]
        [MaxLength(10)]
        public string? Salutation { get; set; }

        [Column("contact_mobile_no")]
        public string? ContactMobileNo { get; set; }

        [Column("land_line_no")]
        [MaxLength(15)]
        public string? LandLineNo { get; set; }

        [Column("email")]
        [MaxLength(100)]
        public string? Email { get; set; }

        [Column("fax")]
        [MaxLength(15)]
        public string? Fax { get; set; }

        [Column("door_no")]
        [MaxLength(5)]
        [StringLength(5, ErrorMessage = "Door number cannot exceed 5 characters")]
        public string? DoorNo { get; set; }

        [Column("street")]
        [MaxLength(250)]
        public string? Street { get; set; }

        [Column("landmark")]
        [MaxLength(50)]
        public string? Landmark { get; set; }

        [Column("website")]
        [MaxLength(100)]
        public string? Website { get; set; }        [Column("territory")]
        [MaxLength(255)]
        public string? Territory { get; set; }  // Direct text field, not a foreign key

        [Column("area")]
        [MaxLength(255)]
        public string? Area { get; set; }

        [Column("city")]
        [MaxLength(255)]
        public string? City { get; set; }        [Column("pincode")]
        [MaxLength(255)]
        public string? Pincode { get; set; }

        [Column("district")]
        [MaxLength(255)]
        public string? District { get; set; }

        [Column("state")]
        [MaxLength(255)]
        public string? State { get; set; }

        [Column("assigned_to")]
        public int? AssignedTo { get; set; }
    }
    public class SalesLeadDetails:SalesLead{
        public string? Country { get; set; }
        public new string? State { get; set; }
        public new string? Territory { get; set; }
        public new string? City { get; set; }
        public new string? Area { get; set; }
        public new string? Pincode { get; set; }
        public new string? District { get; set; }

   public List<SalesAddressDetails> Addresses { get; set; } = new();
        public List<SalesContact> Contacts { get; set; } = new();
        public List<SalesLeadsBusinessChallenge> BusinessChallenges { get; set; } = new();
        public List<SalesProducts> Products { get; set; } = new();
    }
}