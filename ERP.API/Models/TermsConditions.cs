using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.API.Models
{
    [Table("terms_conditions")]
    public class TermsConditions
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
        
        [Column("module_name")]
        public string? ModuleName { get; set; }
        
        [Column("template_name")]
        public string? TemplateName { get; set; }
        
        [Column("template_description")]
        public string? TemplateDescription { get; set; }
        
        [NotMapped]
        public List<TermsConditionsDetail>? Details { get; set; }
    }
    
    [Table("terms_conditions_details")]
    public class TermsConditionsDetail
    {
        [Column("id")]
        public int Id { get; set; }
        
        [Column("tc_id")]
        public int TcId { get; set; }
        
        [Column("sno")]
        public int? Sno { get; set; }
        
        [Column("type")]
        public string? Type { get; set; }
        
        [Column("terms_and_conditions")]
        public string? TermsAndConditions { get; set; }
    }
}
