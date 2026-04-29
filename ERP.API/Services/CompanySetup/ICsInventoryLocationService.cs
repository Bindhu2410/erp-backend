using System.Collections.Generic;
using System.Threading.Tasks;
using ERP.API.Models.CompanySetup;
using ERP.API.Models.DTOs.CompanySetup;
using ERP.API.Models;

namespace ERP.API.Services.CompanySetup
{
    public interface ICsInventoryLocationService
    {
        Task<IEnumerable<CsInventoryLocationDto>> GetAllAsync();
        Task<PagedResponse<CsInventoryLocationDto>> SearchAsync(CsInventoryLocationSearchDto searchDto);
        Task<CsInventoryLocationDto> GetByIdAsync(int id);
        Task<CsInventoryLocationDto> CreateAsync(CsInventoryLocationDto locationDto);
        Task UpdateAsync(CsInventoryLocationDto locationDto);
        Task DeleteAsync(int id);
    }
}
