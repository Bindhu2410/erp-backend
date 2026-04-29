using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.API.Models
{
    [Table("sales_activity_events")]
    public class SalesActivityEvent : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override int? Id { get; set; }

        [MaxLength(255)]
        [Column("event_title")]
        public string? EventTitle { get; set; }

        [Column("guests")]
        public string? Guests { get; set; }

        [Column("start_date")]
        [Required]
        public DateTime StartDate { get; set; }

        [Column("end_date")]
        [Required]
        public DateTime EndDate { get; set; }

        [Column("start_time")]
        [Required]
        public TimeSpan StartTime { get; set; }

        [Column("end_time")]
        [Required]
        public TimeSpan EndTime { get; set; }

        [Column("participant")]
        public string? Participant { get; set; }

        [MaxLength(255)]
        [Column("event_location")]
        public string? EventLocation { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [MaxLength(255)]
        [Column("status")]
        public string? Status { get; set; }

        [MaxLength(255)]
        [Column("priority")]
        public string? Priority { get; set; }

        [MaxLength(255)]
        [Column("file_url")]
        public string? FileUrl { get; set; }

        [MaxLength(255)]
        [Column("stage")]
        [Required]
        public string Stage { get; set; } = string.Empty;

        [MaxLength(255)]
        [Column("stage_item_id")]
        [Required]
        public string StageItemId { get; set; } = string.Empty;

        [MaxLength(255)]
        [Column("event_id")]
        public string? EventId { get; set; }

        [Column("sales_activity_checklists_id")]
        public int? ActivityCheckListsId { get; set; }

        [Column("isactive")]
        public bool IsActive { get; set; }

        [Column("comments")]
        public string? Comments { get; set; }


        [Column("assignedtouserid")]
        public int? Assignedtouserid { get; set; }

        // Navigation Property
        [ForeignKey("Assignedtouserid")]
        public virtual User? AssignedUser { get; set; }
    }
}