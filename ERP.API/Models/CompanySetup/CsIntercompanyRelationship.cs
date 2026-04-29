using System;

namespace ERP.API.Models.CompanySetup
{
    public class CsIntercompanyRelationship
    {
        public int RelationshipId { get; set; }
        public int CompanyId1 { get; set; }
        public int CompanyId2 { get; set; }
        public string RelationshipType { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Notes { get; set; }
        public string Company1Name { get; set; } = string.Empty;
        public string Company2Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
