using System.Collections.Generic;
using System.Threading.Tasks;
using ERP.API.Models;

namespace ERP.API.Services
{
    public interface ITermsConditionsService
    {
        Task<IEnumerable<TermsConditions>> GetAllAsync();
        Task<TermsConditions> GetByIdAsync(int id);
        Task<int> CreateAsync(TermsConditions termsConditions);
        Task UpdateAsync(TermsConditions termsConditions);
        Task DeleteAsync(int id);
    }
}
