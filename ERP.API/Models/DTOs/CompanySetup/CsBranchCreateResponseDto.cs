namespace ERP.API.Models.DTOs.CompanySetup
{
    public class CsBranchCreateResponseDto
    {
        public int? OutBranchId { get; set; }
        public string? OutBranchCode { get; set; }
        public string OutMessage { get; set; } = string.Empty;
        public bool Success => OutBranchId.HasValue;
    }
}
