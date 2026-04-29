using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.API.Models
{
    [Table("teamhierarchy")]
    public class TeamHierarchy
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("userid")]
        public int UserId { get; set; }

        [Column("region")]
        public string Region { get; set; } = string.Empty;

        [Column("role")]
        public string Role { get; set; } = string.Empty;
    }
}
