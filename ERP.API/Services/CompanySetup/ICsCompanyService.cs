using System.Collections.Generic;
using System.Threading.Tasks;
using ERP.API.Models.CompanySetup;
using ERP.API.Models.DTOs.CompanySetup;

namespace ERP.API.Services.CompanySetup
{
    public interface ICsCompanyService
    {
        Task<int> CreateCompanyAsync(CreateCsCompanyDto createDto);
        Task<bool> UpdateCompanyAsync(UpdateCsCompanyDto updateDto);
        Task<bool> DeleteCompanyAsync(int companyId, bool forceDelete = false);
        Task<CsCompanyDto?> GetCompanyByIdAsync(int companyId);
        Task<IEnumerable<CsCompanyDto>> GetAllCompaniesAsync();
        Task<IEnumerable<CsCompanyDto>> SearchCompaniesAsync(CsCompanySearchDto searchDto);
        Task<IEnumerable<CsCompanyHierarchyDto>> GetCompanyHierarchyAsync();
    }
}
