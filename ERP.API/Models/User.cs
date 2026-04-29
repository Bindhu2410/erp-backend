using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.API.Models
{
    [Table("users")]
    public class User
    {
        [Key]
        [Column("userid")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }

        [Required]
        [MaxLength(255)]
        [Column("username")]
        public string Username { get; set; } = string.Empty;

        [MaxLength(255)]
        [Column("email")]
        public string? Email { get; set; }

        [Column("datecreated")]  
        public DateTime? DateCreated { get; set; }

        [Column("lastlogindate")]
        public DateTime? DateUpdated { get; set; }

        [Column("isactive")]
        public bool IsActive { get; set; } = true;
    }
}