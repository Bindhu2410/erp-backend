using System;

// DTO for API response with only required fields
public class SalesItemResponseDto
{
    public string? ValuationMethodName { get; set; }
    public string? Brand { get; set; }
    public string? CatNo { get; set; }
    public int Id { get; set; }
    public int? Qty { get; set; }
    public double? Amount { get; set; }
    public bool? IsActive { get; set; }
    public int? ItemId { get; set; }
    public string? Make { get; set; }
    public string? Model { get; set; }
    public string? Product { get; set; }
    public string? Category { get; set; }
    public string? CategoryName { get; set; }
    public string? ItemName { get; set; }
    public string? ItemCode { get; set; }
    public decimal? UnitPrice { get; set; }
    public string? Hsn { get; set; }
    public decimal? TaxPercentage { get; set; }
    public string? UomName { get; set; }
}

namespace ERP.API.Models
{
        public class SalesItemResponse
        {
            public string? ValuationMethodName { get; set; }
    // For Delivery/Quotation: store child and accessory IDs for recursive fetch
    public string? Brand { get; set; }
    public string? ImageUrl { get; set; }
    public string? CatNo { get; set; }
        public int[]? IncludedChildItemIds { get; set; }
        public int[]? AccessoriesIds { get; set; }
        public int Id { get; set; }
        public int? UserCreated { get; set; }
        public DateTime? DateCreated { get; set; }
        public int? UserUpdated { get; set; }
        public DateTime? DateUpdated { get; set; }
        public int? Qty { get; set; }
        public double? Amount { get; set; }
        public bool? IsActive { get; set; }
        public int? ItemId { get; set; }
        public string? Stage { get; set; }
        public string? StageItemId { get; set; }
        public string? Make { get; set; }
        public string? Model { get; set; }
        public string? Product { get; set; }
    public string? Category { get; set; }
    public string? CategoryName { get; set; }
        public string? ItemName { get; set; }
        public string? ItemCode { get; set; }
        public decimal? UnitPrice { get; set; }
        public string? Hsn { get; set; }
    public decimal? TaxPercentage { get; set; }
        public string? Taxes { get; set; }
    public string? Uom { get; set; }
    public string? UomName { get; set; }
        
        // BOM-related fields
        public string? BomId { get; set; }
        public string? BomName { get; set; }
        public string? BomType { get; set; }
        public DateTime? EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public int? QuoteTitleId { get; set; }
        public string? QuoteTitleName { get; set; }
        public int? TcTemplateId { get; set; }
        public string? TcTemplateName { get; set; }
        
        // Rate fields
        public decimal? PurchaseRate { get; set; }
        public decimal? SaleRate { get; set; }
        public decimal? QuoteRate { get; set; }
        
        public int? ParentId { get; set; }
       public SalesItemResponse? ParentItem { get; set; }
       public List<SalesItemResponse>? ReferencedBy { get; set; }

       // Added for SalesQuotationController: full objects for included child and accessories
       [Newtonsoft.Json.JsonProperty("includedChildItems")]
       public List<SalesItemResponse>? IncludedChildItems { get; set; }
       [Newtonsoft.Json.JsonProperty("accessoriesItems")]
       public List<SalesItemResponse>? AccessoriesItems { get; set; }
       
       // BOM child items with detailed information
       public List<BomChildItemResponse>? ChildItems { get; set; }
    }
    
    // New class for BOM child item details
    public class BomChildItemResponse
    {
        public int ChildItemId { get; set; }
        public int Quantity { get; set; }
        public string? Make { get; set; }
        public string? Model { get; set; }
        public string? Product { get; set; }
        public string? CategoryName { get; set; }
        public string? ValuationMethodName { get; set; }
        public string? InventoryMethodName { get; set; }
        public string? InventoryTypeName { get; set; }
        public decimal? UnitPrice { get; set; }
        public string? ItemName { get; set; }
        public string? ItemCode { get; set; }
        public string? CatNo { get; set; }
        public string? UomName { get; set; }
        public decimal? PurchaseRate { get; set; }
        public decimal? SaleRate { get; set; }
        public decimal? QuoteRate { get; set; }
        public string? Hsn { get; set; }
        public decimal? Tax { get; set; }
    }

    // DTO for API response with only required fields
    public class SalesItemResponseDto
    {
        public string? ValuationMethodName { get; set; }
        public string? Brand { get; set; }
        public string? CatNo { get; set; }
        public int Id { get; set; }
        public int? Qty { get; set; }
        public double? Amount { get; set; }
        public bool? IsActive { get; set; }
        public int? ItemId { get; set; }
        public string? Make { get; set; }
        public string? Model { get; set; }
        public string? Product { get; set; }
        public string? Category { get; set; }
        public string? CategoryName { get; set; }
        public string? ItemName { get; set; }
        public string? ItemCode { get; set; }
        public decimal? UnitPrice { get; set; }
        public string? Hsn { get; set; }
        public decimal? TaxPercentage { get; set; }
        public string? UomName { get; set; }
    }
}
