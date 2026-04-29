using System.Collections.Generic;
using System.Threading.Tasks;
using ERP.API.Models.DTOs.CompanySetup;

namespace ERP.API.Services.CompanySetup
{
    public interface ICsBranchService
    {
        Task<CsBranchCreateResponseDto> CreateBranchAsync(CreateCsBranchDto createDto);
        Task<(bool Success, string Message)> UpdateBranchAsync(UpdateCsBranchDto updateDto);
        Task<(bool Success, string Message)> DeleteBranchAsync(int branchId, int companyId);
        Task<bool> ValidateBranchCompanyAsync(int branchId, int companyId);
        Task<IEnumerable<CsBranchDto>> GetBranchesByCompanyAsync(int companyId, bool includeInactive = false);
        Task<IEnumerable<CsBranchDropdownDto>> GetBranchesDropdownAsync(int companyId, bool activeOnly = true);
        Task<CsBranchPagedResponseDto> GetAllBranchesAsync(CsBranchPagedRequestDto request);
    }
}
