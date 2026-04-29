namespace ERP.API.Models.DTOs.CompanySetup
{
    public class CsInventoryLocationSearchDto
    {
        public int? WarehouseId { get; set; }
        public string? SearchText { get; set; }
        public string? LocationCategory { get; set; }
        public bool? IsActive { get; set; }
        public int PageSize { get; set; } = 10;
        public int PageNumber { get; set; } = 1;
    }
}
