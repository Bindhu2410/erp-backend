using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ERP.API.Models
{
    [Table("sales_demos")]
    public class SalesDemo : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override int? Id { get; set; }

        [Column("user_created")]
        [JsonIgnore]
        public override int? UserCreated { get; set; }

        [Column("date_created")]
        [JsonIgnore]
        public override DateTime? DateCreated { get; set; }

        [Column("user_updated")]
        [JsonIgnore]
        public override int? UserUpdated { get; set; }

        [Column("date_updated")]
        [JsonIgnore]
        public override DateTime? DateUpdated { get; set; }

        [Column("user_id")]
        [JsonIgnore]
        public int? UserId { get; set; }  // Agent/Manager who created request

        [Column("demo_date")]
        [Required]
        public DateTime DemoDate { get; set; }        [Column("status")]
        [Required]
        [MaxLength(100)]
        public string Status { get; set; } = string.Empty;

        [Column("address")]
        [MaxLength(100)]
        public string Address { get; set; } = string.Empty;

        [Column("opportunity_id")]
        public string? OpportunityId { get; set; }

        [Column("demo_contact")]
        [MaxLength(255)]
        public string DemoContact { get; set; } = string.Empty;

        [Column("demo_name")]
        [MaxLength(255)]
        public string DemoName { get; set; } = string.Empty;

        [Column("demo_approach")]
        [MaxLength(255)]
        public string DemoApproach { get; set; } = string.Empty;

        [Column("demo_outcome")]
        [MaxLength(255)]
        public string DemoOutcome { get; set; } = string.Empty;

        [Column("demo_feedback")]
        [MaxLength(255)]
        public string DemoFeedback { get; set; } = string.Empty;

        [Column("comments")]
        [MaxLength(255)]
        public string Comments { get; set; } = string.Empty;

        // Main presenter (single, for legacy or primary presenter)
        [Column("presenter_id")]
        public int? PresenterId { get; set; }

        // Navigation properties
        [ForeignKey("UserCreated")]
        [JsonIgnore]
        public virtual User CreatedByUser { get; set; } = null!;

        [ForeignKey("UserUpdated")]
        [JsonIgnore]
        public virtual User UpdatedByUser { get; set; } = null!;

        [ForeignKey("UserId")]
        [JsonIgnore]
        public virtual User User { get; set; } = null!;

        [ForeignKey("PresenterId")]
        [JsonIgnore]
        public virtual User? Presenter { get; set; }

        // For display only
        [NotMapped]
        public string? PresenterName { get; set; }

        [Column("leadid")]
        public string? LeadId { get; set; }

        [Column("contact_mobile_num")]
        public string? ContactMobileNum { get; set; }

        [Column("presenter_ids", TypeName = "integer[]")]
        public int[] PresenterIds { get; set; } = Array.Empty<int>();

        [Column("customer_name")]
        public string? CustomerName { get; set; }

        [Column("demo_time")]
        public TimeSpan? DemoTime { get; set; }
    }
}
