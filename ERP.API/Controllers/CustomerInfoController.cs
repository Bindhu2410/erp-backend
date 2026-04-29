
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using ERP.API.Models;
using ERP.API.Models.DTOs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Dapper;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route("api/customer-info")]
    public class CustomerInfoController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<CustomerInfoController> _logger;
        private readonly string _connectionString;

        public CustomerInfoController(IConfiguration configuration, ILogger<CustomerInfoController> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _connectionString = _configuration.GetConnectionString("DefaultConnection") ??
                throw new System.InvalidOperationException("DefaultConnection string is not configured");
        }

        /// <summary>
        /// Get all unique customer names from active opportunities
        /// </summary>
        [HttpGet("opportunity-customer-names")]
        public async Task<ActionResult<IEnumerable<string>>> GetOpportunityCustomerNames()
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var customerNames = (await connection.QueryAsync<string>(
                    "SELECT DISTINCT customer_name FROM sales_opportunities WHERE isactive = true AND customer_name IS NOT NULL AND customer_name <> '' ORDER BY customer_name ASC")).ToList();

                return Ok(customerNames);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error fetching opportunity customer names: {Message}", ex.Message);
                return StatusCode(500, new { message = "An error occurred while fetching customer names", error = ex.Message });
            }
        }

        /// <summary>
        /// Get all opportunity customer names and their lead addresses
        /// </summary>
        [HttpGet("opportunity-customers-addresses")]
        public async Task<ActionResult<IEnumerable<object>>> GetOpportunityCustomersAndAddresses()
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                // Get all active opportunities with customer name and lead id
                var opportunities = (await connection.QueryAsync<dynamic>(
                    "SELECT opportunity_id, customer_name, lead_id FROM sales_opportunities WHERE isactive = true AND customer_name IS NOT NULL AND lead_id IS NOT NULL")).ToList();

                var result = new List<object>();
                foreach (var opp in opportunities)
                {
                    // Fetch address fields from sales_lead for this lead_id
                    var leadAddress = await connection.QueryFirstOrDefaultAsync<dynamic>(
                        "SELECT pincode, area, state, district, city, door_no, street FROM sales_lead WHERE lead_id = @LeadId LIMIT 1",
                        new { LeadId = (string)opp.lead_id });

                    result.Add(new
                    {
                        OpportunityId = opp.opportunity_id,
                        CustomerName = opp.customer_name,
                        LeadId = opp.lead_id,
                        Address = leadAddress
                    });
                }
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error fetching opportunity customer names and addresses: {Message}", ex.Message);
                return StatusCode(500, new { message = "An error occurred while fetching data", error = ex.Message });
            }
        }
    }
}
