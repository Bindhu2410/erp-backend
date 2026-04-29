using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.API.Models
{
    [Table("sales_territories")]
    public class SalesTerritory : BaseEntity
    {        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override int? Id { get; set; }[MaxLength(255)]
        public string? Name { get; set; }

        [MaxLength(255)]
        public string? Alias { get; set; }
    }
}