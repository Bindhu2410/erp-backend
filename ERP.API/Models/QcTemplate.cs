using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.API.Models
{
    [Table("qc_templates")]
    public class QcTemplate : BaseEntity
    {
        [Column("template_name")]
        [StringLength(100)]
        [Required]
        public string TemplateName { get; set; }

        [Column("description")]
        public string Description { get; set; }

    }
}
