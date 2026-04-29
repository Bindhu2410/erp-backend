using System;

namespace ERP.API.Models
{
    public class SalesItemDto
    {
        // Fields from sales_product
        public int Id { get; set; }
        public int Item_Id { get; set; }
        public string? Stage { get; set; }
        public int Stage_Item_Id { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Discount { get; set; }
        public decimal Tax { get; set; }
        public decimal Total { get; set; }
        public string? Remarks { get; set; }
        // Add other sales_product fields as needed

        // Additional fields for API response
        public int? UserCreated { get; set; }
        public DateTime? DateCreated { get; set; }
        public int? UserUpdated { get; set; }
        public DateTime? DateUpdated { get; set; }
        public string? Make { get; set; }
        public string? Model { get; set; }
        public string? Product { get; set; }
        public string? HSN { get; set; }
        public decimal? TaxPercentage { get; set; }
        public int? ParentId { get; set; }
        public string? ParentItem { get; set; }
        public int? ReferencedBy { get; set; }

        // Fields from item_master
        public int ItemMaster_Id { get; set; }
        public string? ItemName { get; set; }
        public string? ItemCode { get; set; }
        public string? Description { get; set; }
        public string? Category { get; set; }
        public string? UOM { get; set; }
        public decimal UnitPrice { get; set; }
        public bool IsActive { get; set; }
        // Add other item_master fields as needed
    }
}
