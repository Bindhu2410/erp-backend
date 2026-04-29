using System;

namespace ERP.API.Models.DTOs.CompanySetup
{
    public class CsIntercompanyRelationshipSearchDto
    {
        public int? CompanyId { get; set; }
        public string? SearchText { get; set; }
        public string? RelationshipType { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public bool? ActiveOnly { get; set; }
    }
}
