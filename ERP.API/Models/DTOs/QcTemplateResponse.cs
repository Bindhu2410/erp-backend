using System;

namespace ERP.API.Models.DTOs
{
    public class QcTemplateResponse
    {
        public int Id { get; set; }

        public string TemplateName { get; set; }

        public string Description { get; set; }

        public int? UserCreated { get; set; }

        public DateTime? DateCreated { get; set; }
        public int? UserUpdated { get; set; }
        public DateTime? DateUpdated { get; set; }
    }
}
