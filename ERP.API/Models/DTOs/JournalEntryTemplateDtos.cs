
using System;
using ERP.API.Models.CompanySetup;

namespace ERP.API.Models.DTOs
{
    public class JournalEntryTemplateCreateDto
    {
        public int CompanyId { get; set; }
        public string TemplateCode { get; set; }
        public string TemplateName { get; set; }
        public string TemplateDescription { get; set; }
        public int? TemplateCategoryId { get; set; }
        public string Frequency { get; set; }
        public bool? IsActive { get; set; }
        public bool? AutoReverse { get; set; }
        public int? AutoReverseDays { get; set; }
        public bool? ApprovalRequired { get; set; }
        public int? ApprovalWorkflowId { get; set; }
        public bool? AutoGenerate { get; set; }
        public DateTime? NextGenerationDate { get; set; }
        public DateTime? LastGeneratedDate { get; set; }
        public int? GenerationCount { get; set; }
        public string[] Tags { get; set; }
        public int CreatedBy { get; set; }
    }

    public class JournalEntryTemplateUpdateDto
    {
        public int TemplateId { get; set; }
        public string TemplateName { get; set; }
        public string TemplateCode { get; set; }
        public string TemplateDescription { get; set; }
        public int? TemplateCategoryId { get; set; }
        public string Frequency { get; set; }
        public bool? IsActive { get; set; }
        public bool? AutoReverse { get; set; }
        public int? AutoReverseDays { get; set; }
        public bool? ApprovalRequired { get; set; }
        public int? ApprovalWorkflowId { get; set; }
        public bool? AutoGenerate { get; set; }
        public DateTime? NextGenerationDate { get; set; }
        public DateTime? LastGeneratedDate { get; set; }
        public int? GenerationCount { get; set; }
        public string[] Tags { get; set; }
        public int ModifiedBy { get; set; }
    }

    public class JournalEntryTemplateDeleteDto
    {
        public int TemplateId { get; set; }
    }

    public class JournalEntryTemplateDto : JournalEntryTemplate { }
}
