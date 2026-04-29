using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.API.Models
{
    [Table("sales_cities")]    
    public class SalesCity : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override int? Id { get; set; }

        [MaxLength(255)]
        public string? Name { get; set; }

        [Column("sales_districts_id")]
        public int DistrictId { get; set; }

        [Column("city_code")]
        [MaxLength(255)]
        public string? CityCode { get; set; }

        public string? Description { get; set; }
    }
}