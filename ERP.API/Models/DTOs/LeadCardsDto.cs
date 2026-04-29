using System;

namespace ERP.API.Models.DTOs
{
    public class LeadCardsDto
    {
        public long TotalLeads { get; set; }
        public long NewThisWeek { get; set; }
        public long QualifiedLeads { get; set; }
        public long Converted { get; set; }
        public long Lost { get; set; }
    }
}
