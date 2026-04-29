using ERP.API.Models.CompanySetup;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ERP.API.Services.CompanySetup
{
    public interface ICsDefaultAccountMappingService
    {
        Task<CsDefaultAccountMapping> CreateDefaultAccountMappingAsync(CsDefaultAccountMappingRequest request);
        Task<CsDefaultAccountMapping> UpdateDefaultAccountMappingAsync(int mappingId, CsDefaultAccountMappingRequest request);
        Task<bool> DeleteDefaultAccountMappingAsync(int mappingId);
        Task<CsDefaultAccountMapping> GetDefaultAccountMappingByIdAsync(int mappingId);
        Task<CsDefaultAccountMappingResponse> GetDefaultAccountMappingsByCompanyAsync(CsDefaultAccountMappingSearchRequest request);
        Task<List<CsDefaultAccountMapping>> GetAllDefaultAccountMappingsAsync();
    }
}
