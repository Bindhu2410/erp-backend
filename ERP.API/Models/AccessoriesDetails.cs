using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.API.Models
{
    [Table("accessories_details")]
    public class AccessoriesDetails
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [ForeignKey("AccessoriesHeader")]
        [Column("header_id")]
        public int HeaderId { get; set; }

        [Column("accessories_name")]
        public string AccessoriesName { get; set; }

        [Column("qty")]
        public decimal? Qty { get; set; }

        [StringLength(50)]
        [Column("item_type")]
        public string ItemType { get; set; }

        public virtual AccessoriesHeader AccessoriesHeader { get; set; }
    }
}
