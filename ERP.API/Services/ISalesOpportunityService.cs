using System.Collections.Generic;
using System.Threading.Tasks;
using ERP.API.Models;
using ERP.API.Models.DTOs;

namespace ERP.API.Services
{
    public interface ISalesOpportunityService
    {
        Task<IEnumerable<SalesOpportunityDto>> GetOpportunitiesAsync();
        Task<SalesOpportunityDto?> GetOpportunityByIdAsync(string opportunityId);
        Task<SalesOpportunityDto?> GetByIdAsync(string opportunityId); // Use string for business key
        Task<IEnumerable<SalesOpportunityDto>> GetOpportunitiesByLeadIdAsync(int leadId);
        Task<IEnumerable<SalesOpportunityDto>> GetOpportunitiesByLeadIdAsync(string leadId);
        Task<int> CreateOpportunityAsync(SalesOpportunityDto opportunity);
        Task<bool> UpdateOpportunityWithItemsAsync(string opportunityId, SalesOpportunityWithItemsRequest request);
        Task<bool> UpdateOpportunityAsync(int id, SalesOpportunityDto opportunity);
        Task<bool> UpdateOpportunityAsync(string opportunityId, SalesOpportunityDto opportunity);
        Task<bool> DeleteOpportunityAsync(int id);
        Task<(IEnumerable<SalesOpportunityGridResult> Results, int TotalRecords)> GetOpportunitiesGridAsync(
            string? searchText = null,
            string[]? customerNames = null,
            string[]? territories = null,
            string[]? statuses = null,
            string[]? stages = null,
            string[]? opportunityTypes = null,
            int pageNumber = 1,
            int pageSize = 10,
            string? orderBy = "date_created",
            string? orderDirection = "DESC");
        Task<(IEnumerable<SalesOpportunityGridResult> Results, int TotalRecords)> GetOpportunitiesGridByUserAsync(
            int currentUserId,
            string? searchText = null,
            string[]? customerNames = null,
            string[]? territories = null,
            string[]? statuses = null,
            string[]? stages = null,
            string[]? opportunityTypes = null,
            int pageNumber = 1,
            int pageSize = 10,
            string? orderBy = "date_created",
            string? orderDirection = "DESC");
        Task<(IEnumerable<SalesOpportunityGridResult> Results, int TotalRecords)> GetOpportunitiesGridByUserSPAsync(string jsonRequest);
        Task<IEnumerable<OpportunityCardDto>> GetOpportunityCardsAsync();
        Task<OpportunityCardsDto> GetOpportunityCardsStatusAsync();
        Task<OpportunityCardsDto> GetOpportunityCardsStatusByUserAsync(int currentUserId);
    }
}
