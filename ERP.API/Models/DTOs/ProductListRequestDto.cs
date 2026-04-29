namespace ERP.API.Models.DTOs
{
    public class ProductListRequestDto
    {
        public string? Search { get; set; }
        public int? Page { get; set; }
        public int? PageSize { get; set; }
    }
}