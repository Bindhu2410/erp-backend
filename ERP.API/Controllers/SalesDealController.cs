using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ERP.API.Models;
using ERP.API.Models.DTOs;
using ERP.API.Services;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SalesDealController : ControllerBase
    {
        private readonly ISalesDealService _dealService;
        private readonly ILogger<SalesDealController> _logger;

        public SalesDealController(ISalesDealService dealService, ILogger<SalesDealController> logger)
        {
            _dealService = dealService;
            _logger = logger;
        }        // GET: api/SalesDeal/summary
        [HttpGet("summary")]
        public async Task<ActionResult<DealSummary>> GetDealsSummary()
        {
            try
            {
                var summary = await _dealService.GetDealsSummaryAsync();
                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting deals summary: {Message}", ex.Message);
                return StatusCode(500, $"Failed to retrieve deals summary: {ex.Message}");
            }
        }
        
        // GET: api/SalesDeal
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SalesDeal>>> GetAllDeals()
        {
            try
            {
                var deals = await _dealService.GetAllDealsAsync();
                return Ok(deals);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all deals: {Message}", ex.Message);
                return StatusCode(500, $"Failed to retrieve deals: {ex.Message}");
            }
        }

        // GET: api/SalesDeal/filtered
        [HttpGet("filtered")]
        public async Task<ActionResult<PagedResult<SalesDealDto>>> GetFilteredDeals(
            [FromQuery] DealFilterCriteria filterCriteria,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var dealsPage = await _dealService.GetFilteredDealsAsync(filterCriteria, pageNumber, pageSize);
                return Ok(dealsPage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting filtered deals: {Message}", ex.Message);
                return StatusCode(500, $"Failed to retrieve deals: {ex.Message}");
            }
        }

        // GET: api/SalesDeal/5
        [HttpGet("{id}")]
        public async Task<ActionResult<SalesDeal>> GetDeal(int id)
        {
            try
            {
                var deal = await _dealService.GetDealByIdAsync(id);
                
                if (deal == null)
                {
                    _logger.LogInformation("Deal with ID {Id} not found", id);
                    return NotFound($"Deal with ID {id} not found");
                }

                return Ok(deal);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting deal {Id}: {Message}", id, ex.Message);
                return StatusCode(500, $"Failed to retrieve deal {id}: {ex.Message}");
            }
        }        // POST: api/SalesDeal
        [HttpPost]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<int>> CreateDeal([FromBody] SalesDealRequestDto dealDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        message = "Invalid model state",
                        statusCode = 400,
                        errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                    });
                }

                var deal = new SalesDeal
                {
                    UserCreated = dealDto.UserCreated ?? 1, // Default to 1 if not provided
                    DateCreated = dealDto.DateCreated ?? DateTime.UtcNow,
                    UserUpdated = dealDto.UserUpdated ?? 1,
                    DateUpdated = dealDto.DateUpdated ?? DateTime.UtcNow,
                    DealName = dealDto.DealName,
                    Amount = dealDto.Amount,
                    ExpectedRevenue = dealDto.ExpectedRevenue,
                    StartDate = dealDto.StartDate,
                    DealFor = dealDto.DealFor,
                    CloseDate = dealDto.CloseDate,
                    Status = dealDto.Status,
                    IsActive = dealDto.IsActive ?? true,
                    Comments = dealDto.Comments,
                    OpportunityId = dealDto.OpportunityId,
                    CustomerId = dealDto.CustomerId,
                    SalesRepresentativeId = dealDto.SalesRepresentativeId
                };
                
                var id = await _dealService.CreateDealAsync(deal);
                dealDto.Id = id;                _logger.LogInformation("Created new deal with ID {Id}", id);
                return Created($"api/SalesDeal/{id}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating deal: {Message}", ex.Message);
                return StatusCode(500, new 
                { 
                    message = "Failed to create deal",
                    statusCode = 500,
                    errors = new[] { ex.Message }
                });
            }
        }

        // PUT: api/SalesDeal/5
        [HttpPut("{id}")]
        [Consumes("application/json")]
        [Produces("application/json")]        [ProducesResponseType(typeof(SalesDealRequestDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateDeal(int id, [FromBody] SalesDealRequestDto dealDto)
        {
            try
            {
                if (id != dealDto.Id)
                {
                    return BadRequest(new
                    {
                        message = "ID mismatch",
                        statusCode = 400,
                        errors = new[] { $"URL ID {id} does not match body ID {dealDto.Id}" }
                    });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        message = "Invalid model state",
                        statusCode = 400,
                        errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                    });
                }

                var deal = new SalesDeal
                {
                    Id = id,
                    UserCreated = dealDto.UserCreated,
                    DateCreated = dealDto.DateCreated,
                    UserUpdated = dealDto.UserUpdated ?? 1,
                    DateUpdated = DateTime.UtcNow,
                    DealName = dealDto.DealName,
                    Amount = dealDto.Amount,
                    ExpectedRevenue = dealDto.ExpectedRevenue,
                    StartDate = dealDto.StartDate,
                    DealFor = dealDto.DealFor,
                    CloseDate = dealDto.CloseDate,
                    Status = dealDto.Status,
                    IsActive = dealDto.IsActive ?? true,
                    Comments = dealDto.Comments,
                    OpportunityId = dealDto.OpportunityId,
                    CustomerId = dealDto.CustomerId,
                    SalesRepresentativeId = dealDto.SalesRepresentativeId
                };
                
                var success = await _dealService.UpdateDealAsync(id, deal);

                if (!success)
                {
                    return NotFound(new
                    {
                        message = $"Deal with ID {id} not found",
                        statusCode = 404
                    });
                }                _logger.LogInformation("Updated deal with ID {Id}", id);
                var updatedDeal = await _dealService.GetDealByIdAsync(id);
                return Ok(updatedDeal);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating deal {Id}: {Message}", id, ex.Message);
                return StatusCode(500, new
                {
                    message = $"Failed to update deal {id}",
                    statusCode = 500,
                    errors = new[] { ex.Message }
                });
            }
        }

        // DELETE: api/SalesDeal/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDeal(int id)
        {
            try
            {
                // The user who performed the delete (this would typically come from auth)
                int userUpdated = 1; // Replace with actual user ID from auth
                
                var success = await _dealService.DeleteDealAsync(id, userUpdated);

                if (!success)
                {
                    _logger.LogWarning("Deal with ID {Id} not found for deletion", id);
                    return NotFound($"Deal with ID {id} not found");
                }

                _logger.LogInformation("Deleted deal with ID {Id}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting deal {Id}: {Message}", id, ex.Message);
                return StatusCode(500, $"Failed to delete deal {id}: {ex.Message}");
            }
        }
    }
}