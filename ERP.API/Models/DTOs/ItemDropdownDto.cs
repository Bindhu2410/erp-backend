using System.ComponentModel.DataAnnotations;

namespace ERP.API.Models.DTOs
{
    public class ItemDropdownDto
    {
    public int ItemId { get; set; }
    public decimal? Quantity { get; set; }
    public string? CategoryName { get; set; }
    public string? GroupName { get; set; }
    public string? ValuationMethodName { get; set; }
    public string? InventoryMethodName { get; set; }
    public string? InventoryTypeName { get; set; }
    public decimal? UnitPrice { get; set; }
    public string? Make { get; set; }
    public string? Model { get; set; }
    public string? Product { get; set; }
    public string? ItemName { get; set; }
    public string? ItemCode { get; set; }
    public string? CatNo { get; set; }
    public string? UomName { get; set; }
    public decimal? PurchaseRate { get; set; }
    public decimal? SaleRate { get; set; }
    public decimal? QuoteRate { get; set; }
    public string? HSN { get; set; }
    public decimal? TaxPercentage { get; set; }
    public int? QuoteTitleId { get; set; }
    public int? TcTemplateId { get; set; }
    }
}
