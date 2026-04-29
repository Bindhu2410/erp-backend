using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.API.Models
{
    [Table("sales_summaries")]
    public class SalesSummary : BaseEntity
    {        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override int? Id { get; set; }

        [MaxLength(255)]
        [Column("icon_url")]
        public string? IconUrl { get; set; }

        [MaxLength(255)]
        [Column("title")]
        public string? Title { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [Column("date_time")]
        public DateTime? DateTime { get; set; }

        [Column("isactive")]
        public bool IsActive { get; set; }

        [MaxLength(255)]
        [Column("stage_item_id")]
        public string? StageItemId { get; set; }

        [MaxLength(255)]
        [Column("stage")]
        public string? Stage { get; set; }

        [Column("entities")]
        public string? Entities { get; set; }
    }
}