using System.Collections.Generic;
using System.Threading.Tasks;
using ERP.API.Models;
using ERP.API.Models.DTOs;

namespace ERP.API.Services
{    public interface ISalesDealService
    {        // CRUD operations
        Task<int> CreateDealAsync(SalesDeal deal);
        Task<SalesDeal?> GetDealByIdAsync(int id);
        Task<IEnumerable<SalesDeal>> GetAllDealsAsync();
        Task<bool> UpdateDealAsync(int id, SalesDeal deal);
        Task<bool> DeleteDealAsync(int id, int userUpdated);
        
        // Search, filter, and pagination
        Task<PagedResult<SalesDealDto>> GetFilteredDealsAsync(
            DealFilterCriteria filterCriteria,
            int pageNumber = 1,
            int pageSize = 10);
        
        // Dashboard statistics
        Task<DealSummary> GetDealsSummaryAsync();
    }
}