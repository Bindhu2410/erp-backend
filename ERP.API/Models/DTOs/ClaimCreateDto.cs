using System.Collections.Generic;

namespace ERP.API.Models.DTOs
{
    public class ClaimCreateDto
    {
        public string? ClaimNo { get; set; }
        public DateTime ClaimDate { get; set; }
        public string? UserName { get; set; }
        public string? ClaimType { get; set; }
        public string? ModeOfTravel { get; set; }
        public List<ClaimItemDto>? Items { get; set; }
    }
}
