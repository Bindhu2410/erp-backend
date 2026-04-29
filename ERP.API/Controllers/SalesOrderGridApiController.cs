using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ERP.API.Models;
using ERP.API.Services;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SalesOrderGridApiController : ControllerBase
    {
        private readonly ISalesOrderGridService _salesOrderGridService;

        public SalesOrderGridApiController(ISalesOrderGridService salesOrderGridService)
        {
            _salesOrderGridService = salesOrderGridService;
        }

        [HttpPost]
        public async Task<IActionResult> GetSalesOrderGrid([FromBody] SalesOrderGridRequest request)
        {
            var (data, totalRecords) = await _salesOrderGridService.GetSalesOrderGridAsync(request);
            var response = new SalesOrderGridResponse
            {
                TotalRecords = totalRecords,
                Data = data
            };
            return Ok(response);
        }
    }
}
