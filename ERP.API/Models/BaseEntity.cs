using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.API.Models
{    public class BaseEntity
    {        
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public virtual int? Id { get; set; }

        [Column("user_created")]
        public virtual int? UserCreated { get; set; }

        [Column("date_created")]
        public virtual DateTime? DateCreated { get; set; }

        [Column("user_updated")]
        public virtual int? UserUpdated { get; set; }

        [Column("date_updated")]
        public virtual DateTime? DateUpdated { get; set; }

        //[Column("active")]
        //public virtual bool? Active { get; set; } = true;
    }
}