using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.API.Models
{
    [Table("sales_locations")]
    public class SalesLocation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RowId { get; set; }

        [MaxLength(255)]
        public string? Country { get; set; }

        [Column("country_id")]
        public int? CountryId { get; set; }

        [MaxLength(255)]
        public string? State { get; set; }

        [Column("state_id")]
        public int? StateId { get; set; }

        [MaxLength(255)]
        public string? Territory { get; set; }

        [Column("territory_id")]
        public int? TerritoryId { get; set; }

        [MaxLength(255)]
        public string? TerritoryAlias { get; set; }

        [MaxLength(255)]
        public string? District { get; set; }

        [Column("district_id")]
        public int? DistrictId { get; set; }

        [MaxLength(255)]
        public string? City { get; set; }

        [Column("city_id")]
        public int? CityId { get; set; }

        [MaxLength(255)]
        public string? Area { get; set; }

        [Column("area_id")]
        public int? AreaId { get; set; }

        [MaxLength(10)]
        public string? Pincode { get; set; }

        [Column("pincode_id")]
        public int? PincodeId { get; set; }
    }
}
