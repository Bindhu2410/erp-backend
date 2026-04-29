using System.Text.Json.Serialization;

namespace ERP.API.Models
{
    public class ItemMasterResponse
    {
        public int Id { get; set; }
        public int? UserCreated { get; set; }
        public DateTime? DateCreated { get; set; }
        public int? UserUpdated { get; set; }
        public DateTime? DateUpdated { get; set; }
        public int? GroupId { get; set; }
        public string? GroupName { get; set; }
        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public int? MakeId { get; set; }
        public string? Make { get; set; }
        public int? ModelId { get; set; }
        public string? Model { get; set; }
        public int? ProductId { get; set; }
        public string? Product { get; set; }
        public string? Brand { get; set; }
        public string? ItemName { get; set; }
        public string? ItemCode { get; set; }
        public bool IsActive { get; set; } = true;
        public string? ImageUrl { get; set; }
        public decimal? UnitPrice { get; set; }
        public int? UomId { get; set; }
        public string? UomName { get; set; }
        public string? CatNo { get; set; }
        public int? InventoryMethodId { get; set; }
        public string? InventoryMethodName { get; set; }
        public string? Hsn { get; set; }
        public decimal? TaxPercentage { get; set; }
        public int? ValuationMethodId { get; set; }
        public string? ValuationMethodName { get; set; }

        // Rate master fields
        public decimal? PurchaseRate { get; set; }
        public decimal? SaleRate { get; set; }
        public decimal? QuoteRate { get; set; }
        public int? HsnCode { get; set; }
        public int? TaxPercent { get; set; }

        // Additional missing fields from ItemMaster
        public string? LongItemName { get; set; }
        public string? ItemDescription { get; set; }
        public int? SupplierId { get; set; }
        public string? InventoryType { get; set; }
        public string? Specification { get; set; }
        public string? Criticality { get; set; }
        public string? StockToBank { get; set; }
        public decimal? LpRate { get; set; }
        public string? ValuationMethodText { get; set; }
        public string? RelatedStockAccount { get; set; }
        public int? Cf { get; set; }
        public bool? BomApplicable { get; set; }
    }
}
