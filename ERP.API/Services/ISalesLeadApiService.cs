using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using ERP.API.Models;
using ERP.API.Models.DTOs;

namespace ERP.API.Services
{
    public interface ISalesLeadApiService    {
        Task<IEnumerable<SalesLeadDto>> GetAllSalesLeadsAsync();
        Task<SalesLeadDto?> GetSalesLeadByIdAsync(int id);
        Task<SalesLeadDto> CreateSalesLeadAsync(CreateSalesLeadDto createSalesLeadDto);
        Task<SalesLeadDto?> UpdateSalesLeadAsync(int id, UpdateSalesLeadDto updateSalesLeadDto);
        Task<bool> DeleteSalesLeadAsync(int id, int userUpdated);
        
        // Dashboard and filtering
        Task<SalesLeadSummaryCards> GetSummaryCardsAsync();
        Task<SalesLeadFilterResponse> FilterLeadsAsync(SalesLeadFilterRequest request);
        Task<SalesLeadFilterResponse> GetMyLeadsAsync(int userId, SalesLeadFilterRequest request);
        Task<SalesLeadDropdownOptions> GetFilterDropdownOptionsAsync();
        
        // Grid and Dropdown
        Task<(IEnumerable<SalesLeadGridResult> Results, int TotalRecords)> GetSalesLeadsGridAsync(
            string? searchText = null,
            string[]? customerNames = null,
            string[]? statuses = null,
            string[]? scores = null,
            string[]? leadTypes = null,
            int? pageNumber = 1,
            int? pageSize = 10,
            string? orderBy = "created_date",
            string? orderDirection = "DESC");

        Task<(IEnumerable<LeadsDropdownResult> Results, int TotalRecords)> GetLeadsDropdownAsync(
            string? searchText = null,
            int? pageNumber = 1,
            int? pageSize = 10);
        
        // Required for database operations
        IDbConnection GetConnection();
    }
}
