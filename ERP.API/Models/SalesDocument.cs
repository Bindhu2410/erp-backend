using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.API.Models
{
    [Table("sales_documents")]
    public class SalesDocument : BaseEntity
    {        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override int? Id { get; set; }

        [MaxLength(255)]
        [Column("file_url")]
        public string? FileUrl { get; set; }

        [MaxLength(255)]
        [Column("title")]
        public string? Title { get; set; }

        [MaxLength(255)]
        [Column("file_type")]
        public string? FileType { get; set; }

        [MaxLength(255)]
        [Column("file_name")]
        public string? FileName { get; set; }

        [MaxLength(255)]
        [Column("icon_url")]
        public string? IconUrl { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [Column("isactive")]
        public bool IsActive { get; set; }

        [MaxLength(255)]
        [Column("document_id")]
        public string? DocumentId { get; set; }

        [MaxLength(255)]
        [Column("stage")]
        public string? Stage { get; set; }

        [MaxLength(255)]
        [Column("stage_item_id")]
        public string? StageItemId { get; set; }
    }
}