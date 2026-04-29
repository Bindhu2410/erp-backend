using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.API.Models
{
    [Table("demo_assignments", Schema = "public")]
    public class SalesDemoAssignment
    {
        [Key]
        public int Id { get; set; }
        public int? UserCreated { get; set; }
        public DateTime? DateCreated { get; set; }
        public int? UserUpdated { get; set; }
        public DateTime? DateUpdated { get; set; }
        public int DemoItemId { get; set; }
        public string AssignedToType { get; set; }
        public int AssignedToId { get; set; }
        public DateTime? AssignmentStartDate { get; set; }
        public DateTime? ExpectedReturnDate { get; set; }
        public DateTime? ActualReturnDate { get; set; }
        public string Status { get; set; }
    }
}
