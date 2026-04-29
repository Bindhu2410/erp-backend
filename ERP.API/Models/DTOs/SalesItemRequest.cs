using System.Collections.Generic;
namespace ERP.API.Models.DTOs
{
    // Detailed accessory entry with parent child item tracking
    public class AccessoryItemRequest
    {
        public int AccessoryDetailId { get; set; }
        public string AccessoriesName { get; set; }
        public int Qty { get; set; }
        public string ItemType { get; set; }
        public int? ParentChildItemId { get; set; }
    }

    // Canonical SalesItemRequest for all API usage
    public class SalesItemRequest
    {
        public string BomId { get; set; }
        public int Quantity { get; set; }
        public List<int> AccessoryItemIds { get; set; }
        // Detailed accessory list with ParentChildItemId for scoped display
        public List<AccessoryItemRequest> AccessoryItems { get; set; }
    }
}
