using System.Collections.Generic;
using System.Threading.Tasks;
using ERP.API.Models.CompanySetup;
using ERP.API.Models.DTOs.CompanySetup;

namespace ERP.API.Services.CompanySetup
{
    public interface ICsHsnCodeService
    {
        Task<CsHsnCode?> GetByIdAsync(int hsnCodeId);
        Task<(IEnumerable<CsHsnCode> Data, int TotalRecords, int FilteredRecords)> GetByCompanyAsync(CsHsnCodeSearchDto searchDto);
        Task<int> CreateAsync(CsHsnCode hsnCode);
        Task<bool> UpdateAsync(CsHsnCode hsnCode);
        Task<bool> DeleteAsync(int hsnCodeId);
    }
}
