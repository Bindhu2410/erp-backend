using System;
using System.Collections.Generic;

namespace ERP.API.Models
{
    public static class SalesStageType
    {
        public const string Lead = "Lead";
        public const string Opportunity = "Opportunity";
        public const string Quotation = "Quotation";
        public const string Deal = "Deal";

        private static readonly HashSet<string> ValidStages = new(StringComparer.OrdinalIgnoreCase)
        {
            Lead,
            Opportunity,
            Quotation,
            Deal
        };

        public static bool IsValid(string stage) => 
            !string.IsNullOrEmpty(stage) && ValidStages.Contains(stage);

        public static string GetValidStagesString() => 
            string.Join(", ", ValidStages);
    }
}
