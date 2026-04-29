using ERP.API.Models.CompanySetup;
using ERP.API.Models.DTOs.CompanySetup;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ERP.API.Services.CompanySetup
{
    public interface ICsBankAccountService
    {
        // Original methods
        Task<CsBankAccount> CreateBankAccountAsync(CsBankAccount bankAccount);
        Task<CsBankAccount> UpdateBankAccountAsync(CsBankAccount bankAccount);
        Task<bool> DeleteBankAccountAsync(int bankAccountId);
        Task<CsBankAccount> GetBankAccountByIdAsync(int bankAccountId);
        Task<(IEnumerable<CsBankAccount> bankAccounts, long totalCount)> GetBankAccountsByCompanyAsync(int companyId, int pageNumber = 1, int pageSize = 10);
        Task<(IEnumerable<CsBankAccount> bankAccounts, long totalCount)> SearchBankAccountsAsync(int? companyId, string? searchText = null, string? purpose = null, string? currency = null, int pageNumber = 1, int pageSize = 10);
        
        // Additional DTO-based methods
        Task<CsBankAccount> CreateBankAccountAsync(CsBankAccountDto bankAccountDto);
        Task<CsBankAccount> UpdateBankAccountAsync(int bankAccountId, CsBankAccountDto bankAccountDto);
    }
}
