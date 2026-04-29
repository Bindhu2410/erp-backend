namespace ERP.API.Models.DTOs
{
    public class SalesLeadCardsDto
    {
        public long TotalLeads { get; set; }
        public long NewLeads { get; set; }
        public long QualifiedLeads { get; set; }
        public long UnqualifiedLeads { get; set; }
    }
}
