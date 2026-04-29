using ERP.API.Models.DTOs;
using ERP.API.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderProcessingController : ControllerBase
    {
        private readonly IPurchaseOrderService _purchaseOrderService;
        private readonly IInvoiceService _invoiceService;

        public OrderProcessingController(IPurchaseOrderService purchaseOrderService, IInvoiceService invoiceService)
        {
            _purchaseOrderService = purchaseOrderService;
            _invoiceService = invoiceService;
        }

      

       
        [HttpPost("summary/message")]
        public async Task<IActionResult> GetOrderProcessingSummaryMessageObject([FromBody] string poId)
        {
            var poDetails = await _purchaseOrderService.GetByPoIdAsync(poId);
            if (poDetails?.PurchaseOrder == null)
                return NotFound("Purchase Order not found");

            var po = poDetails.PurchaseOrder;
            var messages = new List<string>();

            // PO Created message
            messages.Add($"PO created - {po.PoId} - {po.DateCreated:yyyy-MM-dd}");

            // PO Status updated message (if status is not the initial status)
            if (!string.IsNullOrEmpty(po.Status) && po.Status != "Created")
            {
                messages.Add($"PO status is updated {po.Status}");
            }

            // OA Created message (assuming OA info is in poDetails.OA or similar)
                // OA Created message removed as per requirement

            // Invoice message
            var invoiceId = await _purchaseOrderService.GetInvoiceIdByPoIdAsync(poId);
            DateTime? invoiceDate = null;
            string invoiceIdValue = null;
            if (!string.IsNullOrEmpty(invoiceId))
            {
                var invoice = await _invoiceService.GetInvoiceByIdAsync(invoiceId);
                invoiceDate = invoice?.CreatedDate;
                invoiceIdValue = invoice?.InvoiceId;
            }
            if (!string.IsNullOrEmpty(invoiceIdValue) && invoiceDate.HasValue)
            {
                messages.Add($"Invoice Generated - {invoiceIdValue} - {invoiceDate:yyyy-MM-ddTHH:mm:ss}");
            }

            var result = new {
                Messages = messages
            };
            return Ok(result);
        }

       
    }
}
