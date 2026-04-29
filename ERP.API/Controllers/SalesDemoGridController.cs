using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ERP.API.Models;
using ERP.API.Services;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SalesDemoGridController : ControllerBase
    {
        private readonly ISalesDemoGridService _salesDemoGridService;

        public SalesDemoGridController(ISalesDemoGridService salesDemoGridService)
        {
            _salesDemoGridService = salesDemoGridService;
        }

        [HttpPost("search")]
        public async Task<IActionResult> SearchSalesDemoGrid([FromBody] DemoGridRequest request)
        {
            // Validate UserCreated for strict user filtering
            if (request.UserCreated <= 0)
                return BadRequest("UserCreated is required and must be greater than 0.");

            // Convert DemoGridRequest to SalesDemoGridRequest if needed
            var salesRequest = new SalesDemoGridRequest
            {
                SearchText = request.SearchText,
                CustomerNames = request.CustomerNames,
                Statuses = request.Statuses,
                DemoApproaches = request.DemoApproaches,
                DemoOutcomes = request.DemoOutcomes,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                OrderBy = request.OrderBy,
                OrderDirection = request.OrderDirection,
                UserCreated = request.UserCreated,
                // Add mapping for any other properties if needed
            };

            // Explicitly deconstruct the tuple
            (var salesData, var totalRecords) = await _salesDemoGridService.GetSalesDemoGridAsync(salesRequest);

            // Map SalesDemoGrid to DemoGridResult
            var data = salesData.Select(x => new DemoGridResult
            {
                Id = x.Id,
                UserCreated = x.UserCreated,
                DateCreated = x.DateCreated,
                UserUpdated = x.UserUpdated,
                DateUpdated = x.DateUpdated,
                UserId = x.UserId,
                DemoDate = x.DemoDate,
                Status = x.Status,
                OpportunityId = x.OpportunityId?.ToString(),
                CustomerId = x.CustomerId,
                DemoContact = x.DemoContact,
                CustomerName = x.CustomerName,
                DemoName = x.DemoName,
                DemoApproach = x.DemoApproach,
                DemoOutcome = x.DemoOutcome,
                DemoFeedback = x.DemoFeedback,
                Comments = x.Comments,
                LeadId = x.LeadId,
                ContactMobileNum = x.ContactMobileNum,
                Address = x.Address,
                PresenterIds = x.PresenterIds,
                TotalRecords = x.TotalRecords
            }).ToList();

            var response = new SalesDemoGridResponse<DemoGridResult>
            {
                TotalRecords = totalRecords,
                Data = data
            };

            return Ok(response);
        }
    }
}
