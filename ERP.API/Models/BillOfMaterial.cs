using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.API.Models
{
    [Table("bill_of_materials")]
    public class BillOfMaterial
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("bom_id")]
        public string? BomId { get; set; }

        [Column("bom_name")]
        public string? BomName { get; set; }

        [Column("bom_type")]
        public string? BomType { get; set; }

        [Column("effective_from")]
        public DateTime? EffectiveFrom { get; set; }

        [Column("effective_to")]
        public DateTime? EffectiveTo { get; set; }

        [Column("quote_title_id")]
        public int? QuoteTitleId { get; set; }

        [Column("tc_template_id")]
        public int? TcTemplateId { get; set; }

        [Column("make")]
        public string? Make { get; set; }

        public List<BillOfMaterialChildItem> ChildItems { get; set; } = new List<BillOfMaterialChildItem>();
        public List<BillOfMaterialOptionalItem> OptionalItems { get; set; } = new List<BillOfMaterialOptionalItem>();
    }
}
