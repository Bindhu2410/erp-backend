using System.ComponentModel.DataAnnotations;

namespace ERP.API.Models
{
    public class InvoiceGridRequest
    {
        public string? SearchText { get; set; }
        public string[]? Statuses { get; set; }
        [System.ComponentModel.DataAnnotations.Range(1, int.MaxValue, ErrorMessage = "Page number must be greater than 0")]
        public int PageNumber { get; set; } = 1;
        [System.ComponentModel.DataAnnotations.Range(1, int.MaxValue, ErrorMessage = "Page size must be greater than 0")]
        public int PageSize { get; set; } = 10;
        public string OrderBy { get; set; } = "date_created";
        [System.ComponentModel.DataAnnotations.RegularExpression("^(ASC|DESC)$", ErrorMessage = "Order direction must be either 'ASC' or 'DESC'")]
        public string OrderDirection { get; set; } = "DESC";
        // Add filter fields as needed
    }

    public class InvoiceGridResponse
    {
        public object Data { get; set; }
        public int TotalRecords { get; set; }
    }
}
