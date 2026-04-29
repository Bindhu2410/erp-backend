using System;
using System.ComponentModel.DataAnnotations;

namespace ERP.API.Models
{
    public class SupplierItemDto
    {
        public int ItemId { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public string? LongItemName { get; set; }
        public string? ItemDescription { get; set; }
        public int? SupplierId { get; set; }
        public string? SupplierName { get; set; }
        public string? SupplierCode { get; set; }
        public decimal? UnitPrice { get; set; }
        public int? UomId { get; set; }
        public string? UomName { get; set; }
        public string? Brand { get; set; }
        public string? Specification { get; set; }
        public bool IsActive { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }
        public string? SupplierCity { get; set; }
        public string? SupplierState { get; set; }
        public string? SupplierCountry { get; set; }
        public string? SupplierContact { get; set; }
    }

    public class SupplierItemsResponse
    {
        public int SupplierId { get; set; }
        public string? SupplierName { get; set; }
        public string? SupplierCode { get; set; }
        public string? SupplierCity { get; set; }
        public string? SupplierState { get; set; }
        public string? SupplierCountry { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
        public bool IsActive { get; set; }
        public List<SupplierItemDto>? Items { get; set; } = new List<SupplierItemDto>();
    }

    public class SupplierItemRequest
    {
        public int? SupplierId { get; set; }
        public int? ItemId { get; set; }
        public bool? IsActive { get; set; } = true;
    }
}
