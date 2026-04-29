using System;
using System.ComponentModel.DataAnnotations;

namespace ERP.API.Models.DTOs
{
    public class CreateSalesDocumentDto
    {
        [Required]
        [MaxLength(255)]
        public string? FileUrl { get; set; }

        [Required]
        [MaxLength(255)]
        public string? Title { get; set; }

        [MaxLength(255)]
        public string? FileType { get; set; }

        [MaxLength(255)]
        public string? FileName { get; set; }

        [MaxLength(255)]
        public string? IconUrl { get; set; }

        public string? Description { get; set; }

        [MaxLength(255)]
        public string? DocumentId { get; set; }

        [MaxLength(255)]
        public string? Stage { get; set; }

        [MaxLength(255)]
        public string? StageItemId { get; set; }
    }
}
