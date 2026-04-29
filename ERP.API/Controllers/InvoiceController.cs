using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ERP.API.Models;
using ERP.API.Services;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [ApiExplorerSettings(IgnoreApi = false)]

    public class InvoiceController : ControllerBase
    {
        private readonly IInvoiceService _invoiceService;

        public InvoiceController(IInvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }

        [HttpGet("by-delivery/{deliveryId}")]
        public async Task<IActionResult> GetInvoiceByDeliveryId(string deliveryId)
        {
            var invoice = await _invoiceService.GetInvoiceByDeliveryIdAsync(deliveryId);
            if (invoice == null)
                return NotFound();
            return Ok(invoice);
        }

        [HttpGet("by-po/{poId}")]
        public async Task<IActionResult> GetInvoiceByPoId(string poId)
        {
            var invoice = await _invoiceService.GetInvoiceByPoIdAsync(poId);
            if (invoice == null)
                return NotFound();
            return Ok(invoice);
        }

        [HttpPost]
        public async Task<IActionResult> CreateInvoice([FromBody] ERP.API.Models.DTOs.CreateInvoiceFromDeliveryRequest request)
        {
            if (string.IsNullOrEmpty(request?.DeliveryId))
                return BadRequest("delivery_id is required");
            var result = await _invoiceService.CreateInvoiceFromDeliveryAsync(request.DeliveryId);
            if (result == null)
                return BadRequest("Invalid delivery_id or related data not found.");
            return Ok(new[] { result });
        }

        [HttpGet("{invoiceId}")]
        public async Task<IActionResult> GetInvoiceById(string invoiceId)
        {
            var invoice = await _invoiceService.GetInvoiceByIdAsync(invoiceId);
            if (invoice == null)
                return NotFound();
            return Ok(invoice);
        }

        /// <summary>
        /// Get invoice by sales_invoices table id (int primary key)
        /// </summary>
        [HttpGet("id/{id}")]
        public async Task<IActionResult> GetInvoiceByIdNew(int id)
        {
            var invoice = await _invoiceService.GetInvoiceByPrimaryIdAsync(id);
            if (invoice == null)
                return NotFound();
            return Ok(invoice);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllInvoices()
        {
            var invoices = await _invoiceService.GetAllInvoicesAsync();
            return Ok(invoices);
        }
    }
}
