using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.API.Models
{
    [Table("attendance")]
    public class Attendance : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override int? Id { get; set; }

        [Required]
        [Column("employee_id")]
        [ForeignKey("SalesEmployee")]
        public int EmployeeId { get; set; }

        [Required]
        [Column("attendance_date")]
        public DateTime AttendanceDate { get; set; }

        [Column("check_in_time")]
        public TimeSpan? CheckInTime { get; set; }

        [Column("check_out_time")]
        public TimeSpan? CheckOutTime { get; set; }

        [Column("status")]
        [MaxLength(50)]
        public string? Status { get; set; }

        [Column("remarks")]
        public string? Remarks { get; set; }

        // Navigation property
        public virtual SalesEmployee? SalesEmployee { get; set; }
    }
}
