using System.Collections.Generic;

namespace ERP.API.Models
{
    public class DeliveryGridRequest
    {
        public string? SearchText { get; set; }
        public string[]? Statuses { get; set; }
        public string[]? DeliveryIds { get; set; }
        public string[]? PoIds { get; set; }
        [System.ComponentModel.DataAnnotations.Range(1, int.MaxValue, ErrorMessage = "Page number must be greater than 0")]
        public int PageNumber { get; set; } = 1;
        [System.ComponentModel.DataAnnotations.Range(1, int.MaxValue, ErrorMessage = "Page size must be greater than 0")]
        public int PageSize { get; set; } = 10;
        public string OrderBy { get; set; } = "delivery_date";
        [System.ComponentModel.DataAnnotations.RegularExpression("^(ASC|DESC)$", ErrorMessage = "Order direction must be either 'ASC' or 'DESC'")]
        public string OrderDirection { get; set; } = "DESC";
    }

    public class DeliveryGridResponse
    {
        public int TotalRecords { get; set; }
        public IEnumerable<ERP.API.Models.Delivery> Data { get; set; }
    }
}
