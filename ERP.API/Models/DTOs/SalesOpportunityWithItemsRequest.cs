using System.Collections.Generic;
using ERP.API.Models;

namespace ERP.API.Models.DTOs
{
   

    public class SalesOpportunityWithItemsRequest
    {
    public SalesOpportunityDto Opportunity { get; set; } = new SalesOpportunityDto();
    public List<SalesOpportunityItemDto> Items { get; set; } = new List<SalesOpportunityItemDto>();
    }
}
