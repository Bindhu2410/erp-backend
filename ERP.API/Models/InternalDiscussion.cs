using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.API.Models
{
    [Table("internal_discussion")]
    public class InternalDiscussion : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override int? Id { get; set; }

        [Column("comment")]
        [Required]
        public string Comment { get; set; } = string.Empty;

        [Column("parent")]
        public int? Parent { get; set; }

        [Column("stage")]
        [Required]
        public string Stage { get; set; } = string.Empty;

        [Column("stage_item_id")]
        [Required]
        public string StageItemId { get; set; } = string.Empty;

        [Column("seen_by")]
        public string? SeenBy { get; set; }

        [Column("user_name")]
        [MaxLength(255)]
        [Required]
        public string UserName { get; set; } = string.Empty;
    }
}
