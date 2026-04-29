using System.Collections.Generic;
using System.Threading.Tasks;
using ERP.API.Models;

namespace ERP.API.Services
{
    public interface IPaymentService
    {
        Task<List<PaymentResponse>> GetPaymentsByInvoiceIdAsync(string invoiceId);
        Task<(IEnumerable<PaymentResponse> Data, int TotalRecords)> GetPaymentGridAsync(PaymentGridRequest request);
        Task<PaymentResponse> CreatePaymentAsync(Payment payment);
        Task<PaymentResponse> GetPaymentByIdAsync(int id);
        Task<PaymentResponse> UpdatePaymentAsync(Payment payment);
    }
}
