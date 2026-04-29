using System;

namespace ERP.API.Models
{
    public class TaskModel
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int? UserCreated { get; set; }
        public int? UserUpdated { get; set; }
        public string? TaskId { get; set; }
        public string? TaskName { get; set; }
        public int? ParentTaskId { get; set; }
        public string? Description { get; set; }
        public string? TaskType { get; set; }
        public string? Status { get; set; }
        public string? Priority { get; set; }
        public DateTime? DueDate { get; set; }
        public string? Stage { get; set; }
        public string? StageItemId { get; set; }
        public int OwnerId { get; set; }
        public int? AssigneeId { get; set; }
        public string? OwnerName { get; set; }
        public string? AssigneeName { get; set; }
        public string? ActivityStatus { get; set; }
        public string? ActivityId { get; set; }
    }
}
