using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ERP.API.Models;
using ERP.API.Services;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SalesProductGridController : ControllerBase
    {
        private readonly ISalesProductsService _salesProductsService;

        public SalesProductGridController(ISalesProductsService salesProductsService)
        {
            _salesProductsService = salesProductsService;
        }

        [HttpPost("search")]
        public async Task<IActionResult> SearchSalesProductGrid([FromBody] SalesProductGridRequest request)
        {
            var (data, totalRecords) = await _salesProductsService.GetSalesProductGridAsync(request);

            var response = new SalesProductGridResponse
            {
                TotalRecords = totalRecords,
                Data = data
            };

            return Ok(response);
        }
    }
}
