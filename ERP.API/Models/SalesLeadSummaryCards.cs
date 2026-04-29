using System;

namespace ERP.API.Models
{
    public class SalesLeadSummaryCards
    {
        public int TotalLeads { get; set; }
        public decimal TotalLeadsGrowth { get; set; }
        public int NewThisWeek { get; set; }
        public decimal NewThisWeekGrowth { get; set; }
        public int QualifiedLeads { get; set; }
        public decimal QualificationRate { get; set; }
        public int ConvertedLeads { get; set; }
        public decimal ConversionRate { get; set; }
    }
}
