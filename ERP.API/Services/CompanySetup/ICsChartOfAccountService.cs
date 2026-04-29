using ERP.API.Models.CompanySetup;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ERP.API.Services.CompanySetup
{
    public interface ICsChartOfAccountService
    {
        Task<IEnumerable<CsChartOfAccount>> GetAllAsync();
        Task<CsChartOfAccount> CreateChartOfAccountAsync(CsChartOfAccount chartOfAccount);
        Task<CsChartOfAccount> UpdateChartOfAccountAsync(int accountId, CsChartOfAccount chartOfAccount);
        Task<bool> DeleteChartOfAccountAsync(int accountId);
        Task<CsChartOfAccount> GetChartOfAccountByIdAsync(int accountId);
        Task<CsChartOfAccountPagedResponse> GetChartOfAccountsByCompanyAsync(CsChartOfAccountSearchRequest searchRequest);
        Task<IEnumerable<CsChartOfAccountDto>> GetChartOfAccountsHierarchyAsync(int companyId, bool includeInactive = false);
        Task<IEnumerable<CsChartOfAccountDropdownItem>> GetChartOfAccountsDropdownAsync(int companyId, string? accountType = null);
    }
}
