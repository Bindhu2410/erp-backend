using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ERP.API.Models
{
    [Table("sales_activity_meetings")]
    public class SalesActivityMeeting : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [JsonPropertyName("id")]
        public override int? Id { get; set; }

        [MaxLength(255)]
        [Column("meeting_type")]
        [JsonPropertyName("meetingType")]
        public string? MeetingType { get; set; }

        [MaxLength(255)]
        [Column("customer_name")]
        [JsonPropertyName("customerName")]
        public string? CustomerName { get; set; }

        [MaxLength(255)]
        [Column("customer_id")]
        [JsonPropertyName("customerId")]
        public string? CustomerId { get; set; }

        [MaxLength(255)]
        [Column("meeting_title")]
        [JsonPropertyName("meetingTitle")]
        public string? MeetingTitle { get; set; }

        [Column("description")]
        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [Column("meeting_date_time")]
        [JsonPropertyName("meetingDateTime")]
        public DateTime MeetingDateTime { get; set; }

        [Column("duration")]
        [JsonPropertyName("duration")]
        public TimeSpan Duration { get; set; }

        [MaxLength(255)]
        [Column("status")]
        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [Column("participant")]
        [JsonPropertyName("participant")]
        public string? Participant { get; set; }

        [MaxLength(255)]
        [Column("file_url")]
        [JsonPropertyName("fileUrl")]
        public string? FileUrl { get; set; }

        [MaxLength(255)]
        [Column("stage")]
        [Required]
        [JsonPropertyName("stage")]
        public string Stage { get; set; } = string.Empty; // lead, opportunity, etc.

        [MaxLength(255)]
        [Column("stage_item_id")]
        [Required]
        [JsonPropertyName("stageItemId")]
        public string StageItemId { get; set; } = string.Empty;

        [MaxLength(255)]
        [Column("parent_meeting")]
        [JsonPropertyName("parentMeeting")]
        public string? ParentMeeting { get; set; }

        [Column("activity_check_lists_id")]
        [JsonPropertyName("activityCheckListsId")]
        public int? ActivityCheckListsId { get; set; }

        [Column("activity_parent_meetings_id")]
        [JsonPropertyName("activityParentMeetingsId")]
        public int? ActivityParentMeetingsId { get; set; }

        [MaxLength(255)]
        [Column("city")]
        [JsonPropertyName("city")]
        public string? City { get; set; }

        [MaxLength(255)]
        [Column("area")]
        [JsonPropertyName("area")]
        public string? Area { get; set; }

        [Column("address")]
        [JsonPropertyName("address")]
        public string? Address { get; set; }

        [MaxLength(255)]
        [Column("comments")]
        [JsonPropertyName("comments")]
        public string? Comments { get; set; }

        [MaxLength(255)]
        [Column("delegate")]
        [JsonPropertyName("delegate")]
        public string? Delegate { get; set; }

        //[MaxLength(255)]
        //[Column("assigned_to")]
        //[JsonPropertyName("assignedTo")]
        //public string? AssignedTo { get; set; }
        [Column("assignedtouserid")]
        public int? Assignedtouserid { get; set; }

        [ForeignKey("Assignedtouserid")]
        public virtual User? AssignedUser { get; set; }
    }
}