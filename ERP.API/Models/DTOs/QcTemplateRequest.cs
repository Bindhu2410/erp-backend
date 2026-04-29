using System;
using System.ComponentModel.DataAnnotations;

namespace ERP.API.Models.DTOs
{
    public class QcTemplateRequest
    {
        [Required]
        [StringLength(100)]
        public string TemplateName { get; set; }

        public string Description { get; set; }

        public int? CreatedBy { get; set; }

        public int? UpdatedBy { get; set; }
    }
}
