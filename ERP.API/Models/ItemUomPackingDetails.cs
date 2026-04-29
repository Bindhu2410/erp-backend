using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.API.Models
{
    [Table("item_uom_packing_details", Schema = "public")]
    public class ItemUomPackingDetails
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("user_created")]
        public int? UserCreated { get; set; }

        [Column("date_created")]
        public DateTime? DateCreated { get; set; }

        [Column("user_updated")]
        public int? UserUpdated { get; set; }

        [Column("date_updated")]
        public DateTime? DateUpdated { get; set; }

        [Column("item_id")]
        public int ItemId { get; set; }

        [Column("primary_uom")]
        [MaxLength(50)]
        public string PrimaryUom { get; set; }

        [Column("buying_uom")]
        [MaxLength(50)]
        public string BuyingUom { get; set; }

        [Column("consumption_uom")]
        [MaxLength(50)]
        public string ConsumptionUom { get; set; }

        [Column("conversion_to_primary")]
        public decimal? ConversionToPrimary { get; set; }

        [Column("conversion_to_secondary")]
        public decimal? ConversionToSecondary { get; set; }
    }
}
