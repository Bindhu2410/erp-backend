using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.API.Models
{
    [Table("item_accounting_info", Schema = "public")]
    public class ItemAccountingInfo
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

        [Column("asset_account")]
        [MaxLength(100)]
        public string AssetAccount { get; set; }

        [Column("depreciation_account")]
        [MaxLength(100)]
        public string DepreciationAccount { get; set; }

        [Column("purchase_account")]
        [MaxLength(100)]
        public string PurchaseAccount { get; set; }

        [Column("sales_account")]
        [MaxLength(100)]
        public string SalesAccount { get; set; }
    }
}
