namespace ERP.API.Models.DTOs.CompanySetup
{
    public class CsBranchPagedRequestDto
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int? CompanyId { get; set; }
        public bool? IsActive { get; set; }
    }

    public class CsBranchPagedResponseDto
    {
        public IEnumerable<CsBranchDto> Branches { get; set; } = new List<CsBranchDto>();
        public long TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }
}
