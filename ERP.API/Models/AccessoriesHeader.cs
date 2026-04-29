using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.API.Models
{
    [Table("accessories_header")]
    public class AccessoriesHeader
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [StringLength(50)]
        [Column("accesory_id")]
        public string? AccesoryId { get; set; }

        [Column("date")]
        public DateTime? Date { get; set; }

        [Column("item_id")]
        public int ItemId { get; set; }

        [Column("item_description")]
        public string ItemDescription { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<AccessoriesDetails> AccessoriesDetails { get; set; }
    }
}
