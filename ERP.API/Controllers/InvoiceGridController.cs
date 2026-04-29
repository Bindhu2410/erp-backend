using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ERP.API.Models;
using ERP.API.Services;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InvoiceGridController : ControllerBase
    {
        private readonly IInvoiceGridService _invoiceGridService;

        public InvoiceGridController(IInvoiceGridService invoiceGridService)
        {
            _invoiceGridService = invoiceGridService;
        }

        [HttpPost("search")]
        public async Task<IActionResult> SearchInvoiceGrid([FromBody] InvoiceGridRequest request)
        {
            var (data, totalRecords) = await _invoiceGridService.GetInvoiceGridAsync(request);
            return Ok(new InvoiceGridResponse { Data = data, TotalRecords = totalRecords });
        }
    }
}
