using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.API.Models
{
    [Table("sales_areas")]
    public class SalesArea : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override int? Id { get; set; }

        [MaxLength(255)]
        [Required]
        public string Name { get; set; } = string.Empty;

        [Column("sales_cities_id")]
        public int CityId { get; set; }

        public int? Pincode { get; set; }

        [Required]
        public string Description { get; set; } = string.Empty;
    }
}