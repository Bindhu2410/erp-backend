using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ERP.API.Models;
using ERP.API.Services;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SalesQuotationGridController : ControllerBase
    {
        private readonly ISalesQuotationGridService _salesQuotationGridService;

        public SalesQuotationGridController(ISalesQuotationGridService salesQuotationGridService)
        {
            _salesQuotationGridService = salesQuotationGridService;
        }

        [HttpPost("search")]
        public async Task<IActionResult> SearchSalesQuotationGrid([FromBody] SalesQuotationGridRequest request)
        {
            // If not provided, try to get user from claims (like in SalesOpportunityController)
            if (request.UserCreated == null || request.UserCreated == 0)
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userIdClaim, out var userId) && userId > 0)
                {
                    request.UserCreated = userId;
                }
            }

            var (data, totalRecords) = await _salesQuotationGridService.GetSalesQuotationGridAsync(request);

            var response = new SalesQuotationGridResponse
            {
                TotalRecords = totalRecords,
                Data = data
            };

            return Ok(response);
        }
    }
}
