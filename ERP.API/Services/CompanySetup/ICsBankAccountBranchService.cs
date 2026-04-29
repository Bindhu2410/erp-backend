using System.Collections.Generic;
using System.Threading.Tasks;
using ERP.API.Models.CompanySetup;

namespace ERP.API.Services.CompanySetup
{
    public interface ICsBankAccountBranchService
    {
        Task<CsBankAccountBranch> CreateBankAccountBranchAsync(int bankAccountId, int branchId);
        Task<bool> DeleteBankAccountBranchAsync(int bankAccountId, int branchId);
        Task<IEnumerable<CsBankAccountBranch>> GetBranchesByBankAccountAsync(int bankAccountId);
        Task<(IEnumerable<CsBankAccountBranchDetail> accounts, long totalCount)> GetBankAccountsByBranchAsync(
            int branchId, int pageNumber = 1, int pageSize = 10);
    }
}
