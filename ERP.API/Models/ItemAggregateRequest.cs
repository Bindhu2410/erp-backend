using System.Collections.Generic;

namespace ERP.API.Models
{
    public class ItemAggregateRequest
    {
        // Use a request DTO for ItemMaster so POST body matches table structure
        public ItemMasterRequest ItemMaster { get; set; }
        public ItemPlanning ItemPlanning { get; set; }
        public ItemUomPackingDetails ItemUomPackingDetails { get; set; }
        public ItemAccountingInfo ItemAccountingInfo { get; set; }
        public List<ItemLocationStock> LocationStocks { get; set; }
        public ItemQualityControl ItemQualityControl { get; set; }
        public List<SupplierPayload> Suppliers { get; set; }
    }
}
