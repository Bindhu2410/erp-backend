using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.API.Models
{
    [Table("sales_leads_business_challenges")]
    public class SalesLeadsBusinessChallenge : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public new int? Id { get; set; }

        
        [Column("solution")]
        public string? Solution { get; set; } = string.Empty;

        [Required]
        [Column("challenges")]
        public string Challenges { get; set; } = string.Empty;

        [Column("isactive")]
        public bool? IsActive { get; set; }

        [Column("sales_leads_id")]
        public int? SalesLeadsId { get; set; }

        [Column("solution_product_ids")]
        public string? SolutionProductIds { get; set; }

        [Column("solution_products")]
        public string? SolutionProducts { get; set; }
        

    }
}