using System;
using System.Collections.Generic;
using System.Linq;

namespace ERP.API.Models
{
    public static class SalesStage
    {
        public const string Lead = "Lead";
        public const string Opportunity = "Opportunity";
        public const string Quotation = "Quotation";
        public const string Demo = "Demo";
        public const string Deal = "Deal";
        public const string Customer = "Customer";

        private static readonly HashSet<string> _validStages = new(StringComparer.OrdinalIgnoreCase)
        {
            Lead,
            Opportunity,
            Quotation,
            Demo,
            Deal,
            Customer
        };

        public static IReadOnlyCollection<string> ValidStages => _validStages.ToList().AsReadOnly();

        public static bool IsValid(string stage)
        {
            return !string.IsNullOrEmpty(stage) && _validStages.Contains(stage);
        }
    }
}