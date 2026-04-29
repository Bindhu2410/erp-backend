namespace ERP.API.Models.DTOs.CompanySetup
{
    public class WarehouseResponse
    {
        public int WarehouseId { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class WarehouseDropdownResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string WarehouseCode { get; set; } = string.Empty;
    }

    public class SuccessResponse
    {
        public string Message { get; set; } = string.Empty;
    }

    public class ErrorResponse
    {
        public string Message { get; set; } = string.Empty;
        public string Error { get; set; } = string.Empty;
    }
}
