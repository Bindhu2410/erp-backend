using ERP.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AttendanceController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AttendanceController(AppDbContext context)
        {
            _context = context;
        }

     
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetAll()
        {
            var currentUserId = GetCurrentUserId();
            var userRole = await GetUserRoleAsync(currentUserId);
            
            IQueryable<Attendance> attendanceQuery = _context.Attendances;
            
            // Apply role-based filtering
            if (userRole == "Sales Manager")
            {
                var teamEmployeeIds = await GetTeamEmployeeIdsAsync(currentUserId);
                attendanceQuery = attendanceQuery.Where(a => teamEmployeeIds.Contains(a.EmployeeId));
            }
            
            var attendances = await attendanceQuery.ToListAsync();
            var employeeIds = attendances.Select(a => a.EmployeeId).Distinct().ToList();
            var employees = await _context.SalesEmployees.Where(e => e.Id.HasValue && employeeIds.Contains(e.Id.Value)).ToListAsync();

            var result = attendances.Select(att =>
            {
                var employee = employees.FirstOrDefault(e => e.Id == att.EmployeeId);
                return new
                {
                    att.Id,
                    att.EmployeeId,
                    EmployeeName = employee != null ? $"{employee.FirstName} {employee.LastName}".Trim() : null,
                    EmployeeCode = employee?.EmployeeId,
                    att.AttendanceDate,
                    att.CheckInTime,
                    att.CheckOutTime,
                    att.Status,
                    att.Remarks,
                    att.UserCreated,
                    att.DateCreated,
                    att.UserUpdated,
                    att.DateUpdated
                };
            });

            return Ok(result);
        }

        /// <summary>
        /// Get attendance record by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetById(int id)
        {
            var attendance = await _context.Attendances.FirstOrDefaultAsync(a => a.Id == id);
            if (attendance == null)
                return NotFound(new { message = $"Attendance record with ID {id} not found." });

            var employee = await _context.SalesEmployees.FirstOrDefaultAsync(e => e.Id == attendance.EmployeeId);

            var result = new
            {
                attendance.Id,
                attendance.EmployeeId,
                EmployeeName = employee != null ? $"{employee.FirstName} {employee.LastName}".Trim() : null,
                EmployeeCode = employee?.EmployeeId,
                attendance.AttendanceDate,
                attendance.CheckInTime,
                attendance.CheckOutTime,
                attendance.Status,
                attendance.Remarks,
                attendance.UserCreated,
                attendance.DateCreated,
                attendance.UserUpdated,
                attendance.DateUpdated
            };

            return Ok(result);
        }

        /// <summary>
        /// Get attendance records for a specific employee
        /// </summary>
        [HttpGet("employee/{employeeId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetByEmployeeId(int employeeId)
        {
            var employee = await _context.SalesEmployees.FirstOrDefaultAsync(e => e.Id == employeeId);
            if (employee == null)
                return NotFound(new { message = $"Employee with ID {employeeId} not found." });

            var attendances = await _context.Attendances
                .Where(a => a.EmployeeId == employeeId)
                .OrderByDescending(a => a.AttendanceDate)
                .ToListAsync();

            var result = attendances.Select(att => new
            {
                att.Id,
                att.EmployeeId,
                EmployeeName = $"{employee.FirstName} {employee.LastName}".Trim(),
                EmployeeCode = employee.EmployeeId,
                att.AttendanceDate,
                att.CheckInTime,
                att.CheckOutTime,
                att.Status,
                att.Remarks,
                att.UserCreated,
                att.DateCreated,
                att.UserUpdated,
                att.DateUpdated
            });

            return Ok(result);
        }

        /// <summary>
        /// Get attendance records for a date range
        /// </summary>
        [HttpGet("range")]
        public async Task<ActionResult<IEnumerable<object>>> GetByDateRange([FromBody] DateTime startDate, [FromBody] DateTime endDate)
        {
            if (startDate > endDate)
                return BadRequest(new { message = "Start date must be before end date." });

            var attendances = await _context.Attendances
                .Where(a => a.AttendanceDate >= startDate && a.AttendanceDate <= endDate)
                .OrderByDescending(a => a.AttendanceDate)
                .ToListAsync();

            if (attendances.Count == 0)
                return NotFound(new { message = "No attendance records found for the specified date range." });

            var employeeIds = attendances.Select(a => a.EmployeeId).Distinct().ToList();
            var employees = await _context.SalesEmployees.Where(e => e.Id.HasValue && employeeIds.Contains(e.Id.Value)).ToListAsync();

            var result = attendances.Select(att =>
            {
                var employee = employees.FirstOrDefault(e => e.Id == att.EmployeeId);
                return new
                {
                    att.Id,
                    att.EmployeeId,
                    EmployeeName = employee != null ? $"{employee.FirstName} {employee.LastName}".Trim() : null,
                    EmployeeCode = employee?.EmployeeId,
                    att.AttendanceDate,
                    att.CheckInTime,
                    att.CheckOutTime,
                    att.Status,
                    att.Remarks,
                    att.UserCreated,
                    att.DateCreated,
                    att.UserUpdated,
                    att.DateUpdated
                };
            });

            return Ok(result);
        }

        /// <summary>
        /// Get attendance for a specific date
        /// </summary>
        [HttpGet("date/{date}")]
        public async Task<ActionResult<IEnumerable<object>>> GetByDate(DateTime date)
        {
            var attendances = await _context.Attendances
                .Where(a => a.AttendanceDate == date)
                .ToListAsync();

            if (attendances.Count == 0)
                return NotFound(new { message = $"No attendance records found for {date:yyyy-MM-dd}." });

            var employeeIds = attendances.Select(a => a.EmployeeId).Distinct().ToList();
            var employees = await _context.SalesEmployees.Where(e => e.Id.HasValue && employeeIds.Contains(e.Id.Value)).ToListAsync();

            var result = attendances.Select(att =>
            {
                var employee = employees.FirstOrDefault(e => e.Id == att.EmployeeId);
                return new
                {
                    att.Id,
                    att.EmployeeId,
                    EmployeeName = employee != null ? $"{employee.FirstName} {employee.LastName}".Trim() : null,
                    EmployeeCode = employee?.EmployeeId,
                    att.AttendanceDate,
                    att.CheckInTime,
                    att.CheckOutTime,
                    att.Status,
                    att.Remarks,
                    att.UserCreated,
                    att.DateCreated,
                    att.UserUpdated,
                    att.DateUpdated
                };
            });

            return Ok(result);
        }

        /// <summary>
        /// Create a new attendance record
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<object>> Create([FromBody] AttendanceCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Verify employee exists
            var employee = await _context.SalesEmployees.FirstOrDefaultAsync(e => e.Id == dto.EmployeeId);
            if (employee == null)
                return BadRequest(new { message = $"Employee with ID {dto.EmployeeId} not found." });

            // Check if attendance already exists for this employee on this date
            var existingAttendance = await _context.Attendances
                .FirstOrDefaultAsync(a => a.EmployeeId == dto.EmployeeId && a.AttendanceDate == dto.AttendanceDate);
            if (existingAttendance != null)
                return BadRequest(new { message = $"Attendance already recorded for this employee on {dto.AttendanceDate:yyyy-MM-dd}." });

            var attendance = new Attendance
            {
                EmployeeId = dto.EmployeeId,
                AttendanceDate = DateTime.SpecifyKind(dto.AttendanceDate, DateTimeKind.Utc),
                CheckInTime = dto.CheckInTime,
                CheckOutTime = dto.CheckOutTime,
                Status = dto.Status ?? "Present",
                Remarks = dto.Remarks,
                UserCreated = dto.UserCreated,
                DateCreated = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc),
                UserUpdated = dto.UserCreated,
                DateUpdated = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc)
            };

            _context.Attendances.Add(attendance);
            await _context.SaveChangesAsync();

            var createdAttendance = await _context.Attendances.FirstOrDefaultAsync(a => a.Id == attendance.Id);

            var result = new
            {
                createdAttendance.Id,
                createdAttendance.EmployeeId,
                EmployeeName = $"{employee.FirstName} {employee.LastName}".Trim(),
                EmployeeCode = employee.EmployeeId,
                createdAttendance.AttendanceDate,
                createdAttendance.CheckInTime,
                createdAttendance.CheckOutTime,
                createdAttendance.Status,
                createdAttendance.Remarks,
                createdAttendance.UserCreated,
                createdAttendance.DateCreated,
                createdAttendance.UserUpdated,
                createdAttendance.DateUpdated
            };

            return CreatedAtAction(nameof(GetById), new { id = attendance.Id }, result);
        }

        /// <summary>
        /// Create attendance records in bulk for a date
        /// </summary>
        [HttpPost("bulk")]
        public async Task<ActionResult<object>> CreateBulk([FromBody] AttendanceBulkCreateDto dto)
        {
            if (!ModelState.IsValid || dto.Records == null || dto.Records.Count == 0)
                return BadRequest(new { message = "Records are required." });

            var employeeIds = dto.Records.Select(r => r.EmployeeId).Distinct().ToList();
            var employees = await _context.SalesEmployees.Where(e => e.Id.HasValue && employeeIds.Contains(e.Id.Value)).ToListAsync();

            if (employees.Count != employeeIds.Count)
                return BadRequest(new { message = "One or more employees not found." });

            var existingRecords = await _context.Attendances
                .Where(a => a.AttendanceDate == dto.AttendanceDate && employeeIds.Contains(a.EmployeeId))
                .ToListAsync();

            if (existingRecords.Count > 0)
                return BadRequest(new { message = "Attendance already exists for some employees on this date." });

            var attendances = dto.Records.Select(record => new Attendance
            {
                EmployeeId = record.EmployeeId,
                AttendanceDate = DateTime.SpecifyKind(dto.AttendanceDate, DateTimeKind.Utc),
                CheckInTime = record.CheckInTime,
                CheckOutTime = record.CheckOutTime,
                Status = record.Status ?? "Present",
                Remarks = record.Remarks,
                UserCreated = dto.UserCreated,
                DateCreated = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc),
                UserUpdated = dto.UserCreated,
                DateUpdated = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc)
            }).ToList();

            _context.Attendances.AddRange(attendances);
            await _context.SaveChangesAsync();

            var results = attendances.Select(att =>
            {
                var emp = employees.FirstOrDefault(e => e.Id == att.EmployeeId);
                return new
                {
                    att.Id,
                    att.EmployeeId,
                    EmployeeName = emp != null ? $"{emp.FirstName} {emp.LastName}".Trim() : null,
                    EmployeeCode = emp?.EmployeeId,
                    att.AttendanceDate,
                    att.CheckInTime,
                    att.CheckOutTime,
                    att.Status,
                    att.Remarks
                };
            });

            return CreatedAtAction(nameof(GetByDate), new { date = dto.AttendanceDate }, new { message = "Bulk attendance created successfully.", records = results });
        }

        /// <summary>
        /// Update an attendance record
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] AttendanceUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var attendance = await _context.Attendances.FirstOrDefaultAsync(a => a.Id == id);
            if (attendance == null)
                return NotFound(new { message = $"Attendance record with ID {id} not found." });

            // Ensure existing DateTime properties have UTC kind
            if (attendance.DateCreated.HasValue)
                attendance.DateCreated = DateTime.SpecifyKind(attendance.DateCreated.Value, DateTimeKind.Utc);
            attendance.AttendanceDate = DateTime.SpecifyKind(attendance.AttendanceDate, DateTimeKind.Utc);

            if (dto.CheckInTime.HasValue)
                attendance.CheckInTime = dto.CheckInTime;
            if (dto.CheckOutTime.HasValue)
                attendance.CheckOutTime = dto.CheckOutTime;
            if (!string.IsNullOrEmpty(dto.Status))
                attendance.Status = dto.Status;
            if (!string.IsNullOrEmpty(dto.Remarks))
                attendance.Remarks = dto.Remarks;

            attendance.UserUpdated = dto.UserUpdated;
            attendance.DateUpdated = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);

            _context.Attendances.Update(attendance);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Attendance record updated successfully.", id = attendance.Id });
        }

        /// <summary>
        /// Delete an attendance record
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var attendance = await _context.Attendances.FirstOrDefaultAsync(a => a.Id == id);
            if (attendance == null)
                return NotFound(new { message = $"Attendance record with ID {id} not found." });

            _context.Attendances.Remove(attendance);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Attendance record deleted successfully." });
        }

        /// <summary>
        /// Get attendance summary (present, absent, late counts)
        /// </summary>
        [HttpGet("summary/employee/{employeeId}")]
        public async Task<ActionResult<object>> GetAttendanceSummary(int employeeId, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var employee = await _context.SalesEmployees.FirstOrDefaultAsync(e => e.Id == employeeId);
            if (employee == null)
                return NotFound(new { message = $"Employee with ID {employeeId} not found." });

            var query = _context.Attendances.Where(a => a.EmployeeId == employeeId);

            if (startDate.HasValue)
                query = query.Where(a => a.AttendanceDate >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(a => a.AttendanceDate <= endDate.Value);

            var attendances = await query.ToListAsync();

            var summary = new
            {
                EmployeeId = employeeId,
                EmployeeName = $"{employee.FirstName} {employee.LastName}".Trim(),
                EmployeeCode = employee.EmployeeId,
                TotalDays = attendances.Count,
                Present = attendances.Count(a => a.Status == "Present"),
                Absent = attendances.Count(a => a.Status == "Absent"),
                Late = attendances.Count(a => a.Status == "Late"),
                HalfDay = attendances.Count(a => a.Status == "Half Day"),
                Leave = attendances.Count(a => a.Status == "Leave"),
                PeriodStart = startDate?.ToString("yyyy-MM-dd"),
                PeriodEnd = endDate?.ToString("yyyy-MM-dd")
            };

            return Ok(summary);
        }

        /// <summary>
        /// Get monthly attendance report with role-based access
        /// </summary>
        [HttpGet("report/monthly")]
        public async Task<ActionResult<IEnumerable<object>>> GetMonthlyReport([FromQuery] int month, [FromQuery] int year)
        {
            if (month < 1 || month > 12)
                return BadRequest(new { message = "Invalid month. Must be between 1 and 12." });

            var currentUserId = GetCurrentUserId();
            var userRole = await GetUserRoleAsync(currentUserId);
            
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            IQueryable<Attendance> attendanceQuery = _context.Attendances
                .Where(a => a.AttendanceDate >= startDate && a.AttendanceDate <= endDate);
            
            // Apply role-based filtering
            if (userRole == "Sales Manager")
            {
                var teamEmployeeIds = await GetTeamEmployeeIdsAsync(currentUserId);
                attendanceQuery = attendanceQuery.Where(a => teamEmployeeIds.Contains(a.EmployeeId));
            }
            
            var attendances = await attendanceQuery.ToListAsync();

            if (attendances.Count == 0)
                return NotFound(new { message = $"No attendance records found for {month}/{year}." });

            var employeeIds = attendances.Select(a => a.EmployeeId).Distinct().ToList();
            var employees = await _context.SalesEmployees.Where(e => e.Id.HasValue && employeeIds.Contains(e.Id.Value)).ToListAsync();

            var report = employees.Select(emp =>
            {
                var empAttendances = attendances.Where(a => a.EmployeeId == emp.Id).ToList();
                return new
                {
                    emp.Id,
                    emp.EmployeeId,
                    EmployeeName = $"{emp.FirstName} {emp.LastName}".Trim(),
                    TotalDays = empAttendances.Count,
                    Present = empAttendances.Count(a => a.Status == "Present"),
                    Absent = empAttendances.Count(a => a.Status == "Absent"),
                    Late = empAttendances.Count(a => a.Status == "Late"),
                    HalfDay = empAttendances.Count(a => a.Status == "Half Day"),
                    Leave = empAttendances.Count(a => a.Status == "Leave"),
                    Month = $"{month}/{year}"
                };
            });

            return Ok(report);
        }
        
        /// <summary>
        /// Helper method to get current user ID from claims
        /// </summary>
        private int GetCurrentUserId()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => 
                c.Type == ClaimTypes.NameIdentifier || 
                c.Type == "userid" || 
                c.Type == "sub");
            
            return userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId) ? userId : 1;
        }
        
        /// <summary>
        /// Helper method to get user role
        /// </summary>
        private async Task<string> GetUserRoleAsync(int userId)
        {
            var userRole = await _context.Set<UserRole>()
                .Join(_context.Set<Role>(), ur => ur.RoleId, r => r.RoleId, (ur, r) => new { ur.UserId, r.RoleName })
                .FirstOrDefaultAsync(ur => ur.UserId == userId);
            
            return userRole?.RoleName ?? "User";
        }
        
        /// <summary>
        /// Helper method to get team employee IDs for sales managers
        /// </summary>
        private async Task<List<int>> GetTeamEmployeeIdsAsync(int managerId)
        {
            // Note: UserId relationship removed from new schema, using alternative approach
            var manager = await _context.SalesEmployees.FirstOrDefaultAsync(e => e.Id == managerId);
            if (manager == null) return new List<int>();
            
            // Get team members who report to this manager
            var teamMembers = await _context.SalesEmployees
                .Where(e => e.ReportManager == manager.EmployeeId && e.Active)
                .Select(e => e.Id.Value)
                .ToListAsync();
            
            // Include manager's own attendance
            if (manager.Id.HasValue)
                teamMembers.Add(manager.Id.Value);
            
            return teamMembers;
        }
        
        /// <summary>
        /// Helper method to check if a user is in the manager's team
        /// </summary>
        private async Task<bool> IsUserInTeamAsync(int managerId, int employeeId)
        {
            var teamEmployeeIds = await GetTeamEmployeeIdsAsync(managerId);
            return teamEmployeeIds.Contains(employeeId);
        }
    }
}