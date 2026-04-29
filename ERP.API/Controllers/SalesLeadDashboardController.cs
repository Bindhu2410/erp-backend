using System;
using System.Threading.Tasks;
using ERP.API.Models;
using ERP.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ERP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SalesLeadDashboardController : ControllerBase
    {
        private readonly ISalesLeadApiService _salesLeadService;
        private readonly ILogger<SalesLeadDashboardController> _logger;

        public SalesLeadDashboardController(ISalesLeadApiService salesLeadService, ILogger<SalesLeadDashboardController> logger)
        {
            _salesLeadService = salesLeadService;
            _logger = logger;
        }

        // GET: api/SalesLeadDashboard/summary
        [HttpGet("summary")]
        public async Task<ActionResult<SalesLeadSummaryCards>> GetSummaryCards()
        {
            try
            {
                var summaryCards = await _salesLeadService.GetSummaryCardsAsync();
                return Ok(summaryCards);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting summary cards: {Message}", ex.Message);
                return StatusCode(500, $"Failed to retrieve summary cards: {ex.Message}");
            }
        }

        // POST: api/SalesLeadDashboard/filter
        [HttpPost("filter")]
        public async Task<ActionResult<SalesLeadFilterResponse>> FilterLeads(SalesLeadFilterRequest request)
        {
            try
            {
                var filteredLeads = await _salesLeadService.FilterLeadsAsync(request);
                return Ok(filteredLeads);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while filtering leads: {Message}", ex.Message);
                return StatusCode(500, $"Failed to filter leads: {ex.Message}");
            }
        }

        // GET: api/SalesLeadDashboard/my-leads/{userId}
        [HttpPost("my-leads/{userId}")]
        public async Task<ActionResult<SalesLeadFilterResponse>> GetMyLeads(int userId, SalesLeadFilterRequest request)
        {
            try
            {
                var myLeads = await _salesLeadService.GetMyLeadsAsync(userId, request);
                return Ok(myLeads);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting user's leads: {Message}", ex.Message);
                return StatusCode(500, $"Failed to retrieve user's leads: {ex.Message}");
            }
        }

        // GET: api/SalesLeadDashboard/dropdown-options
        [HttpGet("dropdown-options")]
        public async Task<ActionResult<SalesLeadDropdownOptions>> GetDropdownOptions()
        {
            try
            {
                var options = await _salesLeadService.GetFilterDropdownOptionsAsync();
                return Ok(options);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting dropdown options: {Message}", ex.Message);
                return StatusCode(500, $"Failed to retrieve dropdown options: {ex.Message}");
            }
        }
    }
}
