using System.Collections.Generic;
using System.Threading.Tasks;
using ERP.API.Models;
using ERP.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PaymentResponse>> GetPaymentById(int id)
        {
            var payment = await _paymentService.GetPaymentByIdAsync(id);
            if (payment == null)
                return NotFound();
            return Ok(payment);
        }

        [HttpPost("by-invoice")]
        public async Task<ActionResult<List<PaymentResponse>>> GetPaymentsByInvoiceId([FromBody] string invoiceId)
        {
            var payments = await _paymentService.GetPaymentsByInvoiceIdAsync(invoiceId);
            return Ok(payments);
        }

        [HttpPost]
        public async Task<ActionResult<PaymentResponse>> CreatePayment([FromBody] Payment payment)
        {
            var created = await _paymentService.CreatePaymentAsync(payment);
            return Ok(created);
        }
        [HttpPost("payment-grid/search")]
        public async Task<ActionResult<PaymentGridResponse>> SearchPaymentGrid([FromBody] PaymentGridRequest request)
        {
            // Normalize filters: treat "string" as null/empty for all string and string[] fields
            if (request.SearchText == "string") request.SearchText = null;
            if (request.CustomerNames != null && request.CustomerNames.Length == 1 && request.CustomerNames[0] == "string") request.CustomerNames = null;
            if (request.Statuses != null && request.Statuses.Length == 1 && request.Statuses[0] == "string") request.Statuses = null;

            // Add normalization for any other string[] fields in the future
            var stringArrayProps = request.GetType().GetProperties()
                .Where(p => p.PropertyType == typeof(string[]));
            foreach (var prop in stringArrayProps)
            {
                var arr = prop.GetValue(request) as string[];
                if (arr != null && arr.Length == 1 && arr[0] == "string")
                {
                    prop.SetValue(request, null);
                }
            }

            var (data, totalRecords) = await _paymentService.GetPaymentGridAsync(request);
            return Ok(new PaymentGridResponse { Data = data, TotalRecords = totalRecords });
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<PaymentResponse>> UpdatePayment(int id, [FromBody] Payment payment)
        {
            if (id != payment.Id)
                return BadRequest("Payment ID mismatch.");
            var updated = await _paymentService.UpdatePaymentAsync(payment);
            if (updated == null)
                return NotFound();
            return Ok(updated);
        }
    }

    // Removed InvoiceIdRequest class since it's no longer needed
}
