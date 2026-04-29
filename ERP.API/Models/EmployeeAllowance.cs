using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.API.Models
{
    [Table("employee_allowances")]
    public class EmployeeAllowance : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override int? Id { get; set; }

        [Required]
        [Column("employee_id")]
        [ForeignKey("SalesEmployee")]
        public int EmployeeId { get; set; }

        [Column("allow_type")]
        [MaxLength(100)]
        public string? AllowanceType { get; set; }

        [Column("allow_amount")]
        public decimal? AllowanceAmount { get; set; }

        [Column("allow_eff_from")]
        public DateTime? AllowEffFrom { get; set; }

        [Column("allow_eff_to")]
        public DateTime? AllowEffTo { get; set; }

        // Navigation property
        public virtual SalesEmployee? SalesEmployee { get; set; }
        public bool Active { get; internal set; }
    }
}
