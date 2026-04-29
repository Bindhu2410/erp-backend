using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ERP.API.Models;
using ERP.API.Services;
using Microsoft.AspNetCore.Http;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route("api/print")]
    public class PrintController : ControllerBase
    {
        private readonly QuotationPdfService _pdfService;

        public PrintController(QuotationPdfService pdfService)
        {
            _pdfService = pdfService;
        }

        [HttpPost("quotation")]
        [Produces("application/pdf")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> PrintQuotation([FromBody] QuotationModel model)
        {
            if (model == null)
                return BadRequest(new { message = "Invalid request body" });

            var pdf = await _pdfService.GeneratePdfAsync(model);
            return File(pdf, "application/pdf", $"Quotation-{model.QuotationNumber}.pdf");
        }
    }
}
