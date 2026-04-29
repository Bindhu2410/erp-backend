using System.Threading.Tasks;
using ERP.API.Models;
using ERP.API.Models.DTOs.CompanySetup;

namespace ERP.API.Services.CompanySetup
{
    public interface ICsTdsRateService
    {
        Task<CsTdsRateDto> GetByIdAsync(int id);
        Task<PagedResponse<CsTdsRateDto>> SearchAsync(CsTdsRateSearchDto searchDto);
        Task<PagedResponse<CsTdsRateDto>> GetByCompanyAsync(int companyId, int pageNumber = 1, int pageSize = 10);
        Task<List<CsTdsRateDto>> GetAllItemsAsync();
        Task<int> CreateAsync(CsTdsRateDto tdsRate);
        Task<bool> UpdateAsync(CsTdsRateDto tdsRate);
        Task<bool> DeleteAsync(int id);
    }
}
