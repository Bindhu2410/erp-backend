using System.Collections.Generic;
using System.Threading.Tasks;
using ERP.API.Models.CompanySetup;
using ERP.API.Models.DTOs.CompanySetup;

namespace ERP.API.Services.CompanySetup
{
    public interface ICsIntercompanyRelationshipService
    {
        Task<CsIntercompanyRelationship?> GetByIdAsync(int relationshipId);
        Task<(IEnumerable<CsIntercompanyRelationship> Data, int TotalRecords, int FilteredRecords)> SearchAsync(CsIntercompanyRelationshipSearchDto searchDto);
        Task<IEnumerable<CsIntercompanyRelationship>> GetByCompanyAsync(int companyId, bool activeOnly = true);
        Task<int> CreateAsync(CsIntercompanyRelationship relationship);
        Task<bool> UpdateAsync(CsIntercompanyRelationship relationship);
        Task<bool> DeleteAsync(int relationshipId);
    }
}
