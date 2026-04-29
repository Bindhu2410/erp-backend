using System;
using System.ComponentModel.DataAnnotations;

namespace ERP.API.Models
{
    public class AttendanceCreateDto
    {
        [Required]
        public int EmployeeId { get; set; }

        [Required]
        public DateTime AttendanceDate { get; set; }

        public TimeSpan? CheckInTime { get; set; }

        public TimeSpan? CheckOutTime { get; set; }

        [MaxLength(50)]
        public string? Status { get; set; }

        public string? Remarks { get; set; }

        public int? UserCreated { get; set; }
    }

    public class AttendanceUpdateDto
    {
        public TimeSpan? CheckInTime { get; set; }

        public TimeSpan? CheckOutTime { get; set; }

        [MaxLength(50)]
        public string? Status { get; set; }

        public string? Remarks { get; set; }

        public int? UserUpdated { get; set; }
    }

    public class AttendanceBulkCreateDto
    {
        [Required]
        public DateTime AttendanceDate { get; set; }

        [Required]
        public List<AttendanceRecordDto> Records { get; set; }

        public int? UserCreated { get; set; }
    }

    public class AttendanceRecordDto
    {
        [Required]
        public int EmployeeId { get; set; }

        public TimeSpan? CheckInTime { get; set; }

        public TimeSpan? CheckOutTime { get; set; }

        [MaxLength(50)]
        public string? Status { get; set; }

        public string? Remarks { get; set; }
    }
    
    public class AttendanceFilterDto
    {
        public int? UserId { get; set; }
    }
}
