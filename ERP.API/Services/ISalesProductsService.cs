using System.Collections.Generic;
using System.Threading.Tasks;
using ERP.API.Models;
using ERP.API.Services; // Update with the actual namespace for SalesProductGridDto
using ERP.API.Services; // Update with the actual namespace for SalesProductGridRequest

namespace ERP.API.Services
{
    public interface ISalesProductsService
    {
        // ...existing methods...

        Task<(IEnumerable<SalesProductGridDto> Data, int TotalRecords)> GetSalesProductGridAsync(SalesProductGridRequest request);
    }
}