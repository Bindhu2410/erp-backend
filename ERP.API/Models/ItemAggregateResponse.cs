using System.Collections.Generic;

namespace ERP.API.Models
{
    public class ItemAggregateResponse
    {
        public ItemMaster ItemMaster { get; set; }
        public ItemPlanning ItemPlanning { get; set; }
        public ItemUomPackingDetails ItemUomPackingDetails { get; set; }
        public ItemAccountingInfo ItemAccountingInfo { get; set; }
        public List<ItemLocationStock> LocationStocks { get; set; }
        public ItemQualityControl ItemQualityControl { get; set; }
        public List<SupplierResponse> Suppliers { get; set; }
    }
}
