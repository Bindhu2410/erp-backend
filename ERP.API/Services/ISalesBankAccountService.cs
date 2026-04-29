using System.Collections.Generic;
using System.Threading.Tasks;
using ERP.API.Models;

namespace ERP.API.Services
{
    public interface ISalesBankAccountService
    {
        Task<IEnumerable<SalesBankAccount>> GetAllAsync();
        Task<SalesBankAccount> GetByIdAsync(int id);
        Task<int> CreateAsync(SalesBankAccount bankAccount);
        Task UpdateAsync(SalesBankAccount bankAccount);
        Task DeleteAsync(int id);
    }
}
