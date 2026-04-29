using System.Collections.Generic;

namespace ERP.API.Models.DTOs
{
    public class SalesDemoItemResponse
    {
        public int Id { get; set; }
        public string? BomId { get; set; }
        public string? BomName { get; set; }
        public string? BomType { get; set; }
        public List<BomChildItemDto>? BomChildItems { get; set; }
        public List<int>? AccessoryItemIds { get; set; }
        public List<AccessoryItemResponseDto>? AccessoryItems { get; set; }
        public int Quantity { get; set; }
    }

    public class BomChildItemDto
    {
        public int Id { get; set; }
        public int ChildItemId { get; set; }
        public int Quantity { get; set; }
        public string? Make { get; set; }
        public string? Model { get; set; }
        public string? Product { get; set; }
        public string? Category { get; set; }
        public string? CategoryName { get; set; }
        public string? ValuationMethodName { get; set; }
        public string? InventoryMethodName { get; set; }
        public string? InventoryTypeName { get; set; }
        public decimal? UnitPrice { get; set; }
        public string? ItemName { get; set; }
        public string? ItemCode { get; set; }
        public string? CatNo { get; set; }
        public string? Uom { get; set; }
        public string? UomName { get; set; }
        public decimal? PurchaseRate { get; set; }
        public decimal? SaleRate { get; set; }
        public decimal? QuoteRate { get; set; }
        public string? Hsn { get; set; }
        public decimal? Tax { get; set; }
        public decimal? TaxPercentage { get; set; }
    }

    public class AccessoryItemResponseDto
    {
        public int Id { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public string Product { get; set; }
        public string ItemName { get; set; }
        public string ItemCode { get; set; }
        public decimal UnitPrice { get; set; }
        public string Hsn { get; set; }
        public decimal TaxPercentage { get; set; }
        public string CategoryName { get; set; }
    }
}
