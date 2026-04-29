using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using System.Linq;
using Dapper;
using ERP.API.Models.CompanySetup;
using ERP.API.Services.CompanySetup;

namespace ERP.API.Services.Implementation.CompanySetup
{
    public class CsBankAccountBranchService : ICsBankAccountBranchService
    {
        private readonly IDbConnection _dbConnection;

        public CsBankAccountBranchService(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<CsBankAccountBranch> CreateBankAccountBranchAsync(int bankAccountId, int branchId)
        {
            var result = await _dbConnection.QueryAsync<CsBankAccountBranch>(
                "SELECT * FROM public.sp_create_cs_bank_account_branch(@BankAccountId, @BranchId)",
                new { BankAccountId = bankAccountId, BranchId = branchId });
            return result.FirstOrDefault();
        }

        public async Task<bool> DeleteBankAccountBranchAsync(int bankAccountId, int branchId)
        {
            return await _dbConnection.QueryFirstOrDefaultAsync<bool>(
                "SELECT * FROM public.sp_delete_cs_bank_account_branch(@BankAccountId, @BranchId)",
                new { BankAccountId = bankAccountId, BranchId = branchId });
        }

        public async Task<IEnumerable<CsBankAccountBranch>> GetBranchesByBankAccountAsync(int bankAccountId)
        {
            return await _dbConnection.QueryAsync<CsBankAccountBranch>(
                "SELECT * FROM public.sp_get_cs_bank_account_branches_by_account(@BankAccountId)",
                new { BankAccountId = bankAccountId });
        }

        public async Task<(IEnumerable<CsBankAccountBranchDetail> accounts, long totalCount)> GetBankAccountsByBranchAsync(
            int branchId, int pageNumber = 1, int pageSize = 10)
        {
            var results = await _dbConnection.QueryAsync<CsBankAccountBranchDetail>(
                "SELECT * FROM public.sp_get_cs_bank_accounts_by_branch(@BranchId, @PageNumber, @PageSize)",
                new { BranchId = branchId, PageNumber = pageNumber, PageSize = pageSize });

            var accountsList = results.ToList();
            return (accountsList, accountsList.Any() ? accountsList.First().TotalCount : 0);
        }
    }
}
