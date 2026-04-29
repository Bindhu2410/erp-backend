using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.API.Models
{
    [Table("pincodes")]
    public class SalesPincode : BaseEntity
    {        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override int? Id { get; set; }

        [MaxLength(10)]
        public string? Pincode { get; set; }

        [Column("sales_areas_id")]
        public int AreaId { get; set; }
    }
}