using Microsoft.AspNetCore.Mvc;
using Dapper;
using Npgsql;
using Microsoft.Extensions.Configuration;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public EmployeeController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Get employee cards with total count and new employees this month
        /// </summary>
        /// <returns>Employee statistics</returns>
        [HttpGet("cards")]
        public async Task<ActionResult> GetEmployeeCards()
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                using var connection = new NpgsqlConnection(connectionString);
                await connection.OpenAsync();

                // Get total employee count
                var totalCount = await connection.ExecuteScalarAsync<int>(
                    "SELECT COUNT(*) FROM employees WHERE active = true");

                // Get new employees this month
                var currentMonth = DateTime.UtcNow.Month;
                var currentYear = DateTime.UtcNow.Year;
                var newThisMonth = await connection.ExecuteScalarAsync<int>(
                    @"SELECT COUNT(*) FROM employees 
                      WHERE active = true 
                      AND EXTRACT(MONTH FROM date_created) = @Month 
                      AND EXTRACT(YEAR FROM date_created) = @Year",
                    new { Month = currentMonth, Year = currentYear });

                return Ok(new
                {
                    totalEmployees = totalCount,
                    newThisMonth = newThisMonth
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to fetch employee cards", details = ex.Message });
            }
        }
    }
}