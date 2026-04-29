using System.Collections.Generic;
using System.Threading.Tasks;
using ERP.API.Models;

namespace ERP.API.Services
{
    public interface IDeliveryService
    {
        Task<IEnumerable<Delivery>> GetAllAsync();
        Task<Delivery> GetByIdAsync(int id);
        Task<ERP.API.Models.DeliveryResponse> CreateAsync(Delivery delivery);
        Task<Delivery> UpdateAsync(int id, Delivery delivery);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<Delivery>> GetGridAsync(int page, int pageSize, string? search);
        Task<(IEnumerable<Delivery> Data, int TotalRecords)> GetDeliveryGridAsync(DeliveryGridRequest request);

        Task<IEnumerable<Delivery>> GetByPurchaseOrderIdAsync(string purchaseOrderId);
    }
}
