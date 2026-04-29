using System.Threading.Tasks;
using ERP.API.Models;
using ERP.API.Models.DTOs.CompanySetup;

namespace ERP.API.Services.CompanySetup
{
    public interface ICsOpeningBalanceService
    {
        Task<PagedResponse<CsOpeningBalanceDto>> SearchAsync(CsOpeningBalanceSearchDto searchDto);
        Task<CsOpeningBalanceDto> GetByIdAsync(int id);
        Task<int> CreateAsync(CsOpeningBalanceDto dto);
        Task<bool> UpdateAsync(CsOpeningBalanceDto dto);
        Task<bool> DeleteAsync(int id);
        Task<PagedResponse<CsOpeningBalanceDto>> GetByCompanyPeriodAsync(int companyId, int periodId, int pageNumber, int pageSize);
    }
}
