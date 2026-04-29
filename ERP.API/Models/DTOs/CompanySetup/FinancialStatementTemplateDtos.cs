using System;

namespace ERP.API.Models.DTOs.CompanySetup
{
    public class FinancialStatementTemplateCreateDto
    {
        public string TemplateCode { get; set; }
        public string TemplateName { get; set; }
        public string TemplateType { get; set; }
        public string? TemplateDescription { get; set; }
        public int CreatedBy { get; set; }
        public string AccountingStandard { get; set; } = "INDIAN_GAAP";
        public bool IsDefault { get; set; } = false;
        public bool IsActive { get; set; } = true;
    }

    public class FinancialStatementTemplateUpdateDto
    {
        public int TemplateId { get; set; }
        public string TemplateName { get; set; }
        public string? TemplateDescription { get; set; }
        public string AccountingStandard { get; set; }
        public string TemplateType { get; set; }
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
        public int ModifiedBy { get; set; }
    }

    public class FinancialStatementTemplateDeleteDto
    {
        public int TemplateId { get; set; }
    }

    public class FinancialStatementTemplateDto : ERP.API.Models.CompanySetup.FinancialStatementTemplate { }
}
