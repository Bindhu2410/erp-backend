using System;
using System.ComponentModel.DataAnnotations;

namespace ERP.API.Models.DTOs.CompanySetup
{
    public class CsIntercompanyRelationshipDto
    {
        public int RelationshipId { get; set; }

        [Required]
        public int CompanyId1 { get; set; }
        
        [Required]
        public int CompanyId2 { get; set; }

        public string Company1Name { get; set; } = string.Empty;
        
        public string Company2Name { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string RelationshipType { get; set; } = string.Empty;

        [Required]
        public DateTime EffectiveDate { get; set; }

        public DateTime? EndDate { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; }
        
        public DateTime? UpdatedAt { get; set; }
    }
}
