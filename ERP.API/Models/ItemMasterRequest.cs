using System;

namespace ERP.API.Models
{
    // DTO matching the `item_master` table columns (request body)
    public class ItemMasterRequest
    {
        public int? UserCreated { get; set; }
        public DateTime? DateCreated { get; set; }
        public int? UserUpdated { get; set; }
        public DateTime? DateUpdated { get; set; }
        public int? GroupId { get; set; }
        public int? CategoryId { get; set; }
        public int? InventoryMethodId { get; set; }
        public int? UomId { get; set; }
        public int? ValuationMethodId { get; set; }

        public string? ItemName { get; set; }
        public string? LongItemName { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemDescription { get; set; }

        // IDs in DB for related tables
        public int? MakeId { get; set; }
        public int? ModelId { get; set; }
        public int? ProductId { get; set; }

        public string? Brand { get; set; }
        public string? InventoryType { get; set; }
        public string? Specification { get; set; }
        public string? Criticality { get; set; }
        public string? StockToBank { get; set; }

        public decimal? LpRate { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? TaxPercentage { get; set; }

        public string? ValuationMethod { get; set; }
        public string? RelatedStockAccount { get; set; }
        public int? Cf { get; set; }
        public string? Hsn { get; set; }
        public bool? BomApplicable { get; set; }
        public bool? IsActive { get; set; }
        public string? ImageUrl { get; set; }
        public string? CatNo { get; set; }
    }
}
