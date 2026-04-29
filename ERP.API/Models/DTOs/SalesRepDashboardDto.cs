using System.Collections.Generic;

namespace ERP.API.Models.DTOs
{
    public class SalesRepDashboardDto
    {
        public int TotalOpportunities { get; set; }
        public int ClosingSoon { get; set; }
        public List<SalesRepStageDto> Stages { get; set; } = new();
    }

 

    public class SalesRepStageDto
    {
        public string Stage { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}
