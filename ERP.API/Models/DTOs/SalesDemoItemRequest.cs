using System.Collections.Generic;

namespace ERP.API.Models.DTOs
{
    // Request item structure for SalesDemoWithItemsRequest, matching Opportunity API
    public class SalesDemoItemRequest
    {
        // Removed ItemId as per new requirements

        // New fields for enhanced request structure
        public string? BomId { get; set; }
        public List<int>? AccessoryItemIds { get; set; }
        public List<AccessoryItemDto>? AccessoryItems { get; set; }
        public int Quantity { get; set; }
    }

    public class AccessoryItemDto
    {
        public int AccessoryDetailId { get; set; }
        public string? AccessoriesName { get; set; }
        public int Qty { get; set; }
        public string? ItemType { get; set; }
        public int ParentChildItemId { get; set; }
    }
}
