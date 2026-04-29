using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.API.Models
{
    [Table("bom_name", Schema = "public")]
    public class BomName
    {
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("user_created")]
    public int? UserCreated { get; set; }

    [Column("date_created")]
    public DateTime? DateCreated { get; set; }

    [Column("user_updated")]
    public int? UserUpdated { get; set; }

    [Column("date_updated")]
    public DateTime? DateUpdated { get; set; }

    [Required]
    [Column("name")]
    public string Name { get; set; }

    [Column("type")]
    public string[]? Type { get; set; }
    }
}
