using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ERP.API.Models.CompanySetup;
using ERP.API.Models.DTOs.CompanySetup;

namespace ERP.API.Services.CompanySetup
{
    public interface ICsGstRateService
    {
        Task<IEnumerable<CsGstRateWithCompany>> GetAllAsync();
        Task<CsGstRate?> GetByIdAsync(int gstRateId);
        Task<(IEnumerable<CsGstRate> Data, int TotalRecords, int FilteredRecords)> GetByCompanyAsync(CsGstRateSearchDto searchDto);
        Task<CsGstRate?> GetByHsnSacAsync(int companyId, string hsnSacCode, bool isHsn, DateTime effectiveDate);
        Task<int> CreateAsync(CsGstRate gstRate);
        Task<bool> UpdateAsync(CsGstRate gstRate);
        Task<bool> DeleteAsync(int gstRateId);
    }
}
