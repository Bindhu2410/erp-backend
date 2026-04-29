using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.API.Models
{
    [Table("item_location", Schema = "public")]
    public class ItemLocation
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("item_id")]
        public int ItemId { get; set; }

        [NotMapped]
        public string ItemName { get; set; }

        [Required]
        [Column("warehouse_id")]
        public int WarehouseId { get; set; }

        [Column("rack")]
        [MaxLength(50)]
        public string Rack { get; set; }

        [Column("shelf")]
        [MaxLength(50)]
        public string Shelf { get; set; }

        [Column("column_no")]
        [MaxLength(50)]
        public string ColumnNo { get; set; }

    [Column("in_place")]
    [MaxLength(100)]
    public string InPlace { get; set; }

    [Column("user_created")]
    public int? UserCreated { get; set; }

    [Column("date_created")]
    public DateTime? DateCreated { get; set; }

    [Column("user_updated")]
    public int? UserUpdated { get; set; }

    [Column("date_updated")]
    public DateTime? DateUpdated { get; set; }
    }
}
