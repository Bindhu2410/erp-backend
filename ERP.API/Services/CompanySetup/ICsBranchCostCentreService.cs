using ERP.API.Models.CompanySetup;

namespace ERP.API.Services.CompanySetup
{
    public interface ICsBranchCostCentreService
    {
        Task<CsBranchCostCentre> CreateBranchCostCentreAsync(int branchId, int costCentreId);
        Task<bool> DeleteBranchCostCentreAsync(int branchId, int costCentreId);
        Task<IEnumerable<CsBranchCostCentreDetail>> GetCostCentresByBranchAsync(int branchId);
        Task<CsBranchCostCentrePagedResponse> GetBranchesByCostCentreAsync(int costCentreId, int pageNumber = 1, int pageSize = 10);
        Task<IEnumerable<CsBranchCostCentreDropdownItem>> GetBranchCostCentresDropdownAsync(int branchId);
    }
}
