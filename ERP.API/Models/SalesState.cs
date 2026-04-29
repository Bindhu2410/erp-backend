using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.API.Models
{
    [Table("sales_states")]
    public class SalesState : BaseEntity
    {        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override int? Id { get; set; }

        [MaxLength(255)]
        public string? Name { get; set; }

        [Column("sales_countries_id")]
        public int CountryId { get; set; }
    }
}