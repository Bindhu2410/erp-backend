using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.API.Models
{
    [Table("rate_master")]
    public class RateMaster
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        
        [Column("rate_master_id")]
        public string RateMasterId { get; set; }
        
        [Column("doc_date")]
        public DateTime? DocDate { get; set; }
        
        [Column("effective_date")]
        public DateTime? EffectiveDate { get; set; }
        
        [Column("inventory_group_id")]
        public int? InventoryGroupId { get; set; }
        
        [Column("type")]
        public string? Type { get; set; }
        
        [Column("remarks")]
        public string? Remarks { get; set; }
        
        [Column("user_created")]
        public int? UserCreated { get; set; }
        
        [Column("date_created")]
        public DateTime? DateCreated { get; set; }
        
        [Column("user_updated")]
        public int? UserUpdated { get; set; }
        
        [Column("date_updated")]
        public DateTime? DateUpdated { get; set; }
        
        [NotMapped]
        public List<RateMasterItem>? Items { get; set; }
    }
    
    [Table("rate_master_items")]
    public class RateMasterItem
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        
        [Column("rate_master_id")]
        public int RateMasterId { get; set; }
        
        [Column("item_id")]
        public int ItemId { get; set; }
        
        [Column("supplier_id")]
        public int? SupplierId { get; set; }
        
        [Column("currency_type")]
        public string? CurrencyType { get; set; }
        
        [Column("purchase_rate")]
        public decimal? PurchaseRate { get; set; }
        
        [Column("hsn_code")]
        public string? HsnCode { get; set; }
        
        [Column("tax")]
        public decimal? Tax { get; set; }
        
        [Column("sales_rate")]
        public decimal? SalesRate { get; set; }
        
        [Column("kl_rate")]
        public decimal? KlRate { get; set; }
        
        [Column("quotation_rate")]
        public decimal? QuotationRate { get; set; }
    }
}