using System;

namespace ERP.API.Models
{
    public class DemoChecklist
    {
        public int Id { get; set; }
        public int ChecklistId { get; set; }
        public string ChecklistName { get; set; }
        public int DemoId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsActive { get; set; }
    }

    public class DemoChecklistItem
    {
        public int Id { get; set; }
        public string ChecklistName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
