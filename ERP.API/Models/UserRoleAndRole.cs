using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.API.Models
{
    [Table("userroles")]
    public class UserRole
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("userid")]
        public int UserId { get; set; }

        [Column("roleid")]
        public int RoleId { get; set; }
    }

    [Table("roles")]
    public class Role
    {
        [Key]
        [Column("roleid")]
        public int RoleId { get; set; }

        [Column("rolename")]
        public string RoleName { get; set; } = string.Empty;
    }
}
