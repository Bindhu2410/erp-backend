using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.API.Models
{
    [Table("uom", Schema = "public")]
    public class Uom
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

        [Column("code")]
        [Required]
        [MaxLength(255)]
        public string Code { get; set; }

        [Column("description")]
        [MaxLength(255)]
        public string Description { get; set; }
    }
}
