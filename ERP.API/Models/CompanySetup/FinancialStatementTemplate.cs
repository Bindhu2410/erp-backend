using System;

namespace ERP.API.Models.CompanySetup
{
    public class FinancialStatementTemplate
    {
        public int TemplateId { get; set; }
        public string TemplateCode { get; set; }
        public string TemplateName { get; set; }
        public string TemplateType { get; set; }
        public string? TemplateDescription { get; set; }
        public string AccountingStandard { get; set; }
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}
