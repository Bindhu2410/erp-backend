using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.API.Models
{
    [Table("sales_activity_calls")]
    public class SalesActivityCall : BaseEntity
    {        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override int? Id { get; set; }

        [MaxLength(255)]
        [Column("call_title")]
        public string? CallTitle { get; set; }

        [Column("participants")]
        public string? Participants { get; set; }

        [MaxLength(255)]
        [Column("call_mode")]
        public string? CallMode { get; set; }

        [MaxLength(255)]
        [Column("call_type")]
        public string? CallType { get; set; }

        [Column("call_datetime")]
        public DateTime CallDateTime { get; set; }

        [MaxLength(255)]
        [Column("status")]
        public string? Status { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [Column("duration")]
        public TimeSpan Duration { get; set; }

        [MaxLength(255)]
        [Column("priority")]
        public string? Priority { get; set; }

        [Column("call_agenda")]
        public string? CallAgenda { get; set; }

        [MaxLength(255)]
        [Column("outcome")]
        public string? Outcome { get; set; }

        [MaxLength(255)]
        [Column("call_result")]
        public string? CallResult { get; set; }

        [MaxLength(255)]
        [Column("file_url")]
        public string? FileUrl { get; set; }

        [MaxLength(255)]
        [Column("stage_item_id")]
        [Required]
        public string StageItemId { get; set; } = string.Empty;

        [MaxLength(255)]
        [Column("stage")]
        [Required]
        public string Stage { get; set; } = string.Empty;

        [MaxLength(255)]
        [Column("call_id")]
        public string? CallId { get; set; }

        [Column("sales_activity_checklists_id")]
        public int? SalesActivityChecklistsId { get; set; }

        [Column("isactive")]
        public bool IsActive { get; set; }

        [MaxLength(255)]
        [Column("call_with")]
        public string? CallWith { get; set; }

        [Column("comments")]
        public string? Comments { get; set; }

        [MaxLength(255)]
        [Column("group_with")]
        public string? GroupWith { get; set; }

        [Column("assignedtouserid")]
        public int? Assignedtouserid { get; set; }

        [ForeignKey("Assignedtouserid")]
        [System.Text.Json.Serialization.JsonIgnore] // ADD THIS
        public virtual User? AssignedUser { get; set; }
    }
}