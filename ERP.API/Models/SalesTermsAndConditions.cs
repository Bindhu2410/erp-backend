using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.API.Models
{
    [Table("sales_terms_and_conditions")]
    public class SalesTermsAndConditions
    {
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
        
        [Column("taxes")]
        public string? Taxes { get; set; }
        
        [Column("freight_charges")]
        public string? FreightCharges { get; set; }
        
        [Column("delivery")]
        public string? Delivery { get; set; }
        
        [Column("payment")]
        public string? Payment { get; set; }
        
        [Column("warranty")]
        public string? Warranty { get; set; }
        
        [Column("template_name")]
        public string? TemplateName { get; set; }
        
        [Column("is_default")]
        public bool? IsDefault { get; set; }
        
        [Column("is_active")]
        public bool? IsActive { get; set; }
        
        [Column("quotation_id")]
        public int? QuotationId { get; set; }
    }
}
