using System.Collections.Generic;

namespace ERP.API.Models
{    public class SalesOrderGridRequest
    {
        public string? SearchText { get; set; }
        public string[]? CustomerNames { get; set; }
        public string[]? Statuses { get; set; }
        public string[]? OrderIds { get; set; }
        [System.ComponentModel.DataAnnotations.Range(1, int.MaxValue, ErrorMessage = "Page number must be greater than 0")]
        public int PageNumber { get; set; } = 1;
        [System.ComponentModel.DataAnnotations.Range(1, int.MaxValue, ErrorMessage = "Page size must be greater than 0")]
        public int PageSize { get; set; } = 10;
        public string OrderBy { get; set; } = "order_date";
        [System.ComponentModel.DataAnnotations.RegularExpression("^(ASC|DESC)$", ErrorMessage = "Order direction must be either 'ASC' or 'DESC'")]
        public string OrderDirection { get; set; } = "DESC";
    }

    public class SalesOrderGridResponse
    {
        public int TotalRecords { get; set; }
        public IEnumerable<ERP.API.Models.DTOs.SalesOrderWithQuotationAndItemsDto> Data { get; set; }
    }
}
