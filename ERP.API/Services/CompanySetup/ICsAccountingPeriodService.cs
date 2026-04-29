using ERP.API.Models.DTOs.CompanySetup;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ERP.API.Services.CompanySetup
{
    public interface ICsAccountingPeriodService
    {
        Task<CsAccountingPeriodResponse> CreateAccountingPeriodAsync(CsAccountingPeriodDto createDto);
        Task<CsAccountingPeriodResponse> UpdateAccountingPeriodAsync(int periodId, CsAccountingPeriodDto updateDto);
        Task<bool> DeleteAccountingPeriodAsync(int periodId);
        Task<CsAccountingPeriodResponse?> GetAccountingPeriodByIdAsync(int periodId);
        Task<CsAccountingPeriodPagedResponse> GetAccountingPeriodsByCompanyAsync(int companyId, int pageNumber = 1, int pageSize = 10);
        Task<CsAccountingPeriodPagedResponse> SearchAccountingPeriodsAsync(int companyId, CsAccountingPeriodSearchRequest searchRequest);
        Task<List<CsAccountingPeriodResponse>> GetAllAccountingPeriodsAsync();
    }
}
