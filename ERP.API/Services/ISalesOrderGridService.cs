using System.Collections.Generic;
using System.Threading.Tasks;
using ERP.API.Models;
using ERP.API.Models.DTOs;

namespace ERP.API.Services
{
    public interface ISalesOrderGridService
    {
        Task<(IEnumerable<SalesOrderWithQuotationAndItemsDto> Data, int TotalRecords)> GetSalesOrderGridAsync(SalesOrderGridRequest request);
    }
}
