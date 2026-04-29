using ERP.API.Models.CompanySetup;

namespace ERP.API.Services.CompanySetup
{
    public interface ICsCostCentreService
    {
        Task<CsCostCentre> CreateCostCentreAsync(int companyId, CsCostCentreDto createDto);
        Task<CsCostCentre> UpdateCostCentreAsync(int costCentreId, CsCostCentreDto updateDto);
        Task<bool> DeleteCostCentreAsync(int costCentreId);
        Task<CsCostCentre?> GetCostCentreByIdAsync(int costCentreId);
        Task<CsCostCentrePagedResponse> GetCostCentresByCompanyAsync(int companyId, int pageNumber = 1, int pageSize = 10);
        Task<CsCostCentrePagedResponse> SearchCostCentresAsync(int companyId, CsCostCentreSearchRequest searchRequest);
        Task<List<CsCostCentreHierarchyItem>> GetCostCentreHierarchyAsync(int companyId);
        Task<List<CsCostCentreDropdownItem>> GetCostCentresDropdownAsync(int companyId);
        Task<List<CsCostCentre>> GetAllCostCentresAsync();
    }
}
