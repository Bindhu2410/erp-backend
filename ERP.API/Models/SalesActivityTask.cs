using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.API.Models
{
    [Table("sales_activity_tasks")]
    public class SalesActivityTask : BaseEntity
    {        
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override int? Id { get; set; }

        [MaxLength(255)]
        [Column("task_type")]
        public string? TaskType { get; set; }

        [MaxLength(255)]
        [Column("task_name")]
        public string? TaskName { get; set; }

        [Column("due_date")]
        public DateTime? DueDate { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [MaxLength(255)]
        [Column("priority")]
        public string? Priority { get; set; }

        [MaxLength(255)]
        [Column("customer_name")]
        public string? CustomerName { get; set; }

        [MaxLength(255)]
        [Column("customer_id")]
        public string? CustomerId { get; set; }

        [MaxLength(255)]
        [Column("item_link")]
        public string? ItemLink { get; set; }

        [MaxLength(255)]
        [Column("status")]
        public string? Status { get; set; }

        [Column("parent_task_id")]
        public int? ParentTaskId { get; set; }

        [Column("sub_tasks")]
        public string? SubTasks { get; set; }

        [Column("allow_completion")]
        public bool AllowCompletion { get; set; }

        [MaxLength(255)]
        [Column("stage_item_id")]
        [Required]
        public string StageItemId { get; set; } = string.Empty;

        [MaxLength(255)]
        [Column("stage")]
        [Required]
        public string Stage { get; set; } = string.Empty;

        [MaxLength(255)]
        [Column("task_id")]
        public string? TaskId { get; set; }

        [Column("sales_activity_checklists_id")]
        public int? SalesActivityChecklistsId { get; set; }

        [Column("isactive")]
        public bool IsActive { get; set; }

        [Column("to_do")]
        public string? ToDo { get; set; }

        [Column("comments")]
        public string? Comments { get; set; }

        [Column("assignedtouserid")]
        public int? Assignedtouserid { get; set; }

        [ForeignKey("Assignedtouserid")]
        [System.Text.Json.Serialization.JsonIgnore]
        public virtual User? AssignedUser { get; set; }
    }
}