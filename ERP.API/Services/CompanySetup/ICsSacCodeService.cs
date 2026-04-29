        
using System.Collections.Generic;
using System.Threading.Tasks;
using ERP.API.Models;
using ERP.API.Models.DTOs.CompanySetup;

namespace ERP.API.Services.CompanySetup
{
    public interface ICsSacCodeService
    {
        Task<PagedResponse<CsSacCodeDto>> SearchAsync(CsSacCodeSearchDto searchDto);
        Task<CsSacCodeDto> GetByIdAsync(int id);
        Task<int> CreateAsync(CsSacCodeDto dto);
        Task<bool> UpdateAsync(CsSacCodeDto dto);
        Task<IEnumerable<CsSacCodeDto>> GetAllAsync();
        Task<bool> DeleteAsync(int id);
        Task<PagedResponse<CsSacCodeDto>> GetByCompanyAsync(int companyId, int pageNumber = 1, int pageSize = 10);
    }
}
