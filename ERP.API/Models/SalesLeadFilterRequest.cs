namespace ERP.API.Models
{
    public class SalesLeadFilterRequest
    {
        public string? Territory { get; set; }
        public string? CustomerName { get; set; }
        public string? Status { get; set; }
        public string? Score { get; set; }
        public string? LeadType { get; set; }
        
        // Sorting options
        public string SortField { get; set; } = "id";
        public string SortDirection { get; set; } = "ASC";
        
        // Pagination options
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
