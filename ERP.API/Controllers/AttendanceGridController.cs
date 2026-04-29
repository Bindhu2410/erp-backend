using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Dapper;
using Npgsql;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AttendanceGridController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AttendanceGridController> _logger;

        public AttendanceGridController(IConfiguration configuration, ILogger<AttendanceGridController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Get attendance list with hierarchy-based filtering
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> GetAttendanceGrid([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] string? role = "Admin")
        {
            try
            {
                var currentUserId = GetCurrentUserId();

                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                using var connection = new NpgsqlConnection(connectionString);

                string whereClause = "WHERE 1=1";
                var parameters = new DynamicParameters();

                // Apply hierarchy-based filtering based on role parameter
                if (role == "Sales Manager")
                {
                    // Sales Manager sees their team's attendance
                    var teamEmployeeIds = await GetTeamEmployeeIdsAsync(currentUserId, connection);
                    if (teamEmployeeIds.Any())
                    {
                        whereClause += " AND e.id = ANY(@TeamIds)";
                        parameters.Add("TeamIds", teamEmployeeIds.ToArray());
                    }
                    else
                    {
                        whereClause += " AND 1=0"; // No team members
                    }
                }
                else if (role == "Sales Rep" || role == "Sales Representative")
                {
                    // Sales Rep sees only their own attendance
                    var employeeId = await GetEmployeeIdByUserIdAsync(currentUserId, connection);
                    if (employeeId.HasValue)
                    {
                        whereClause += " AND e.id = @EmployeeId";
                        parameters.Add("EmployeeId", employeeId.Value);
                    }
                    else
                    {
                        whereClause += " AND 1=0"; // No employee record
                    }
                }
                // Admin sees all (no additional filtering)

                // Add date filtering
                if (startDate.HasValue)
                {
                    whereClause += " AND a.attendance_date >= @StartDate";
                    parameters.Add("StartDate", startDate.Value);
                }
                if (endDate.HasValue)
                {
                    whereClause += " AND a.attendance_date <= @EndDate";
                    parameters.Add("EndDate", endDate.Value);
                }

                var sql = $@"
                    SELECT 
                        a.id,
                        a.employee_id,
                        CONCAT(e.first_name, ' ', COALESCE(e.last_name, '')) as employee_name,
                        e.employee_id as employee_code,
                        a.attendance_date,
                        a.check_in_time,
                        a.check_out_time,
                        a.status,
                        a.remarks,
                        a.user_created,
                        a.date_created,
                        a.user_updated,
                        a.date_updated
                    FROM attendance a
                    INNER JOIN employees e ON a.employee_id = e.id
                    {whereClause}
                    ORDER BY a.attendance_date DESC, e.first_name";

                var result = await connection.QueryAsync(sql, parameters);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving attendance grid");
                return StatusCode(500, new { message = "Failed to retrieve attendance data", error = ex.Message });
            }
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => 
                c.Type == ClaimTypes.NameIdentifier || 
                c.Type == "userid" || 
                c.Type == "sub");
            
            return userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId) ? userId : 1;
        }

        /// <summary>
        /// Debug endpoint to check attendance data
        /// </summary>
        [HttpGet("debug")]
        public async Task<ActionResult> Debug()
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                using var connection = new NpgsqlConnection(connectionString);

                var attendanceCount = await connection.QueryFirstOrDefaultAsync<int>("SELECT COUNT(*) FROM attendance");
                var employeeCount = await connection.QueryFirstOrDefaultAsync<int>("SELECT COUNT(*) FROM employees WHERE active = true");
                
                var sampleAttendance = await connection.QueryAsync(@"
                    SELECT a.id, a.attendance_date, e.first_name, e.last_name 
                    FROM attendance a 
                    INNER JOIN employees e ON a.employee_id = e.id 
                    ORDER BY a.attendance_date DESC LIMIT 5");

                return Ok(new {
                    attendanceCount,
                    employeeCount,
                    sampleAttendance
                });
            }
            catch (Exception ex)
            {
                return Ok(new { error = ex.Message });
            }
        }

        private async Task<List<int>> GetTeamEmployeeIdsAsync(int managerId, NpgsqlConnection connection)
        {
            // Get manager's employee record
            var managerEmployeeId = await GetEmployeeIdByUserIdAsync(managerId, connection);
            if (!managerEmployeeId.HasValue) return new List<int>();

            var managerEmployee = await connection.QueryFirstOrDefaultAsync<dynamic>(
                "SELECT employee_id FROM employees WHERE id = @Id", 
                new { Id = managerEmployeeId.Value });

            if (managerEmployee == null) return new List<int>();

            // Get team members who report to this manager
            var sql = @"
                SELECT id 
                FROM employees 
                WHERE report_manager = @ManagerCode AND active = true";

            var teamMembers = await connection.QueryAsync<int>(sql, new { ManagerCode = managerEmployee.employee_id });
            var teamList = teamMembers.ToList();

            // Include manager's own attendance
            teamList.Add(managerEmployeeId.Value);

            return teamList;
        }

        private async Task<int?> GetEmployeeIdByUserIdAsync(int userId, NpgsqlConnection connection)
        {
            // For testing, return first active employee
            var sql = "SELECT id FROM employees WHERE active = true ORDER BY id LIMIT 1";
            return await connection.QueryFirstOrDefaultAsync<int?>(sql);
        }
    }
}