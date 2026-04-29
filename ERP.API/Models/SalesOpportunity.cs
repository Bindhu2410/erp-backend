using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.API.Models
{
    [Table("sales_opportunities")]
    public class SalesOpportunity : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public override int? Id { get; set; }

        [Column("user_created")]
        public override int? UserCreated { get; set; }

        [Column("date_created")]
        public override DateTime? DateCreated { get; set; }

        [Column("user_updated")]
        public override int? UserUpdated { get; set; }

        [Column("date_updated")]
        public override DateTime? DateUpdated { get; set; }

        [Required(ErrorMessage = "Status is required")]
        [Column("status")]
        [MaxLength(255)]
        public string Status { get; set; } = string.Empty;

        [Column("expected_completion")]
        public DateTime? ExpectedCompletion { get; set; }

        [Required(ErrorMessage = "Opportunity Type is required")]
        [Column("opportunity_type")]
        [MaxLength(255)]
        public string OpportunityType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Opportunity For is required")]
        [Column("opportunity_for")]
        [MaxLength(255)]
        public string OpportunityFor { get; set; } = string.Empty;

        [Column("customer_id")]
        [MaxLength(255)]
        public string? CustomerId { get; set; }

        [Column("customer_name")]
        [MaxLength(255)]
        public string? CustomerName { get; set; }

        [Column("customer_type")]
        [MaxLength(255)]
        public string? CustomerType { get; set; }

        [Column("opportunity_name")]
        [MaxLength(255)]
        public string OpportunityName { get; set; } = string.Empty;

        [Column("opportunity_id")]
        [MaxLength(255)]
        public string? OpportunityId { get; set; }

        [Column("comments")]
        public string? Comments { get; set; }

        [Column("isactive")]
        public bool IsActive { get; set; } = false;

        [Column("lead_id")]
        [MaxLength(255)]
        public string? LeadId { get; set; }

        [Column("sales_representative_id")]
        public int? SalesRepresentativeId { get; set; }

        [Column("contact_name")]
        [MaxLength(255)]
        public string? ContactName { get; set; }        [Column("contact_mobile_no")]
        [MaxLength(255)]
        [Phone]
        public string? ContactMobileNo { get; set; }
    }
}
