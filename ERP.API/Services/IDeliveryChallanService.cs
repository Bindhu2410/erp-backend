using System.Collections.Generic;
using System.Threading.Tasks;
using ERP.API.Models;

namespace ERP.API.Services
{
    public interface IDeliveryChallanService
    {
        Task<IEnumerable<DeliveryChallanResponse>> GetAllAsync();
        Task<DeliveryChallanResponse?> GetByIdAsync(int id);
        Task<DeliveryChallanResponse> CreateAsync(DeliveryChallanRequest request);
        Task<DeliveryChallanResponse?> UpdateAsync(int id, DeliveryChallanRequest request);
        Task<bool> DeleteAsync(int id);
        Task<DeliveryChallanGridResponse> GetGridAsync(DeliveryChallanGridRequest request);
    }
}
