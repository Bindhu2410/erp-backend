using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
 
 
namespace ERP.API.Models
{
    [Table("sales_districts")]
    public class SalesDistrict : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override int? Id { get; set; }
 
        [MaxLength(255)]
        [Required]
        public string Name { get; init; } = string.Empty;
 
        [Column("sales_territories_id")]
        public int TerritoryId { get; set; }
    }
}
 