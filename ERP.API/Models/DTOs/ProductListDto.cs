using System.ComponentModel.DataAnnotations;

namespace ERP.API.Models.DTOs
{
    public class ProductListDto
    {
        public int InventoryItemId { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
    }
}
