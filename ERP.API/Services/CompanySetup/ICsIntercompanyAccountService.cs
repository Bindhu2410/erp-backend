using System.Collections.Generic;
using System.Threading.Tasks;
using ERP.API.Models.CompanySetup;
using ERP.API.Models.DTOs.CompanySetup;

namespace ERP.API.Services.CompanySetup
{
    public interface ICsIntercompanyAccountService
    {
        Task<CsIntercompanyAccount?> GetByIdAsync(int accountId);
        Task<(IEnumerable<CsIntercompanyAccount> Data, int TotalRecords, int FilteredRecords)> GetByRelationshipAsync(CsIntercompanyAccountSearchDto searchDto);
        Task<int> CreateAsync(CsIntercompanyAccount account);
        Task<bool> UpdateAsync(CsIntercompanyAccount account);
        Task<bool> DeleteAsync(int accountId);
    }
}
