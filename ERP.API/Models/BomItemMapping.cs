using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ERP.API.Models
{
    [Table("bom_item_mapping", Schema = "public")]
    public class BomItemMapping
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("bom_name_id")]
        public int BomNameId { get; set; }

        [Column("item_id")]
        public int ItemId { get; set; }

        // Navigation properties (ignored during JSON binding)
        [ForeignKey("BomNameId")]
        [JsonIgnore]
        public virtual BomName? BomName { get; set; }

        [ForeignKey("ItemId")]
        [JsonIgnore]
        public virtual ItemMaster? Item { get; set; }
    }
}
