using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.API.Models
{
    [Table("sales_product")]
    public class SalesProductResponse
    {
        public int Id { get; set; }
        public int? UserCreated { get; set; }
        public DateTime? DateCreated { get; set; }
        public int? UserUpdated { get; set; }
        public DateTime? DateUpdated { get; set; }
        public int? Qty { get; set; }
        public double? Amount { get; set; }
        public bool IsActive { get; set; }
        public int? ItemId { get; set; }
        public string? Stage { get; set; }
        public decimal? UnitPrice { get; set; }
        public string? StageItemId { get; set; }

        // ItemMaster fields
        public string? Category { get; set; }
        public string? Brand { get; set; }
        public string? Make { get; set; }
        public string? Model { get; set; }
        public string? ItemName { get; set; }
        public string? ItemCode { get; set; }
        public string? UnitOfMeasure { get; set; }
    }
}
