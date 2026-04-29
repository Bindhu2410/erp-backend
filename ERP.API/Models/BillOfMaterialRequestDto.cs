using System;
using System.Collections.Generic;

namespace ERP.API.Models
{
    public class BillOfMaterialRequestDto
    {
        public string? BomName { get; set; }
        public string? BomType { get; set; }
        public DateTime? EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public int? QuoteTitleId { get; set; }
        public int? TcTemplateId { get; set; }
        public string? Make { get; set; }
        public List<ChildItemDto> ChildItems { get; set; }
    }

    public class ChildItemDto
    {
        public int ChildItemId { get; set; }
        public decimal Quantity { get; set; }
        public decimal? Amount { get; set; }
    }
}
