using ERP.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ERP.API.Services
{
    public interface IOrderAcceptanceService
    {
        Task<OrderAcceptance> CreateOrderAcceptanceAsync(OrderAcceptance orderAcceptance);
        Task<OrderAcceptance> GetOrderAcceptanceByPOAsync(string purchaseOrderId);
        Task<OrderAcceptance> CreateOrderAcceptanceFromPOAsync(int purchaseOrderId, int? userId);
    }
}
