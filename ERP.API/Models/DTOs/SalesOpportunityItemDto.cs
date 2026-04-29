using System;
using System.Collections.Generic;

namespace ERP.API.Models.DTOs
{
    public class AccessoryItemWithParentDto
    {
        public int AccessoryDetailId { get; set; }
        public string AccessoriesName { get; set; } = string.Empty;
        public int Qty { get; set; }
        public string ItemType { get; set; } = string.Empty;
        public int? ParentChildItemId { get; set; }
    }

    public class SalesOpportunityItemDto
    {
        public string BomId { get; set; } = string.Empty;
        public List<int> AccessoryItemIds { get; set; } = new List<int>();
        public List<AccessoryItemWithParentDto> AccessoryItems { get; set; } = new List<AccessoryItemWithParentDto>();
        public int Quantity { get; set; }
    }
}
