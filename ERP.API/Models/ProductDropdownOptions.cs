using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.API.Models
{    
    [Table("product_dropdown_options")]
    public class ProductDropdownOptions
    {
        public int MakeId { get; set; }
        public string? MakeName { get; set; }
        public int ModelId { get; set; }
        public string? ModelName { get; set; }
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
    }
}
