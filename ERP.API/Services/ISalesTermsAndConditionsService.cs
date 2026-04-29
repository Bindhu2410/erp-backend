using System.Collections.Generic;
using System.Threading.Tasks;
using ERP.API.Models;

namespace ERP.API.Services
{
    public interface ISalesTermsAndConditionsService
    {
        Task<IEnumerable<SalesTermsAndConditions>> GetAllAsync();
        Task<SalesTermsAndConditions> GetByIdAsync(int id);
        Task<int> CreateAsync(SalesTermsAndConditions termsAndConditions);
        Task UpdateAsync(SalesTermsAndConditions termsAndConditions);
        Task DeleteAsync(int id);
        Task<SalesTermsAndConditions> GetDefaultTemplateAsync();
        Task<SalesTermsAndConditions> GetByQuotationIdAsync(int quotationId);
        Task<IEnumerable<SalesTermsAndConditions>> GetTermsByQuotationIdAsync(int quotationId);
    }
}
