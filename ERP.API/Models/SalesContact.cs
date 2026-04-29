using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.API.Models
{
    [Table("sales_contacts")]    
    public class SalesContact : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override int? Id { get; set; }

        [Required]
        [Column("contact_name")]
        public string ContactName { get; set; } = string.Empty;

        [Column("department_name")]
        public string? DepartmentName { get; set; }

        [Column("specialist")]
        public string? Specialist { get; set; }

        [Column("degree")]
        public string? Degree { get; set; }

        [Column("email")]
        [EmailAddress]
        public string? Email { get; set; }

        [Column("mobile_no")]
        public string? MobileNo { get; set; }

        [Column("website")]
        public string? Website { get; set; }

        [Column("isactive")]
        public bool IsActive { get; set; } = true;

        [Column("own_clinic")]
        public bool? OwnClinic { get; set; }

        [Column("visiting_hours")]
        public string? VisitingHours { get; set; }

        [Column("clinic_visiting_hours")]
        public string? ClinicVisitingHours { get; set; }

        [Column("sales_lead_id_custom")]
        public string? SalesLeadIdCustom { get; set; }

        [Column("land_line_no")]
        public string? LandLineNo { get; set; }

        [Column("fax")]
        public string? Fax { get; set; }

        [Column("salutation")]
        public string? Salutation { get; set; }

        [Column("job_title")]
        public string? JobTitle { get; set; }        [Column("is_default")]
        public bool? IsDefault { get; set; } = false;
        
        [Column("sales_lead_id")]
        public int? SalesLeadId { get; set; }

        // Navigation property for the parent SalesLead
        [ForeignKey("SalesLeadId")]
        public virtual SalesLead? SalesLead { get; set; }
    }
}