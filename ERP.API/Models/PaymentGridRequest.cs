using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ERP.API.Models
{
    public class PaymentGridRequest
    {
        // Add filter, paging, sorting properties as needed
        public string? SearchText { get; set; }
        public string[]? CustomerNames { get; set; }
        public string[]? Statuses { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Page number must be greater than 0")]
        public int PageNumber { get; set; } = 1;
        [Range(1, int.MaxValue, ErrorMessage = "Page size must be greater than 0")]
        public int PageSize { get; set; } = 10;
        public string OrderBy { get; set; } = "date_created";
        [RegularExpression("^(ASC|DESC)$", ErrorMessage = "Order direction must be either 'ASC' or 'DESC'")]
        public string OrderDirection { get; set; } = "DESC";
    }

    public class PaymentGridResponse
    {
        public IEnumerable<PaymentResponse> Data { get; set; }
        public int TotalRecords { get; set; }
    }
}
