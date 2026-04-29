using System.Threading.Tasks;
using System.Collections.Generic;
using ERP.API.Models;
using ERP.API.Models.DTOs.CompanySetup;

namespace ERP.API.Services.CompanySetup
{
    public interface ICsPaymentTermService
    {
        Task<PagedResponse<CsPaymentTermDto>> SearchAsync(CsPaymentTermSearchDto searchDto);
        Task<CsPaymentTermDto> GetByIdAsync(int id);
        Task<int> CreateAsync(CsPaymentTermDto dto);
        Task<bool> UpdateAsync(CsPaymentTermDto dto);
        Task<bool> DeleteAsync(int id);
        Task<PagedResponse<CsPaymentTermDto>> GetByCompanyAsync(int companyId, int pageNumber = 1, int pageSize = 10);
        Task<IEnumerable<CsPaymentTermDto>> GetAllPaymentTermsAsync();
    }
}
