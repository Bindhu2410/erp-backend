using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.API.Models
{    [Table("sales_external_comments")]
    public class SalesExternalComment : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override int? Id { get; set; }

        [MaxLength(255)]
        [Column("title")]
        public string? Title { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [Column("date_time")]
        public DateTime? DateTime { get; set; }

        [MaxLength(255)]
        [Column("stage")]
        public string? Stage { get; set; }

        [MaxLength(255)]
        [Column("stage_item_id")]
        public string? StageItemId { get; set; }

        [Column("isactive")]
        public bool IsActive { get; set; }

        [MaxLength(255)]
        [Column("activity_id")]
        public string? ActivityId { get; set; }
    }
}