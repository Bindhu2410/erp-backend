using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Dapper;
using Npgsql;
using ERP.API.Models;

namespace ERP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DemoChecklistController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public DemoChecklistController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Get all demo checklist names (from demo_checklist_items)
        /// </summary>
        [HttpGet("all-names")]
        public async Task<ActionResult<IEnumerable<string>>> GetAllDemoChecklistNames()
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString))
                return StatusCode(500, "Could not resolve connection string.");

            using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();
            try
            {
                var items = await connection.QueryAsync<dynamic>(
                    "SELECT checklist_name, COALESCE(is_active, FALSE) AS is_active FROM demo_checklist_items ORDER BY checklist_name");
                return Ok(items);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { message = "Failed to fetch checklist names", error = ex.Message });
            }
        }
    }
}
