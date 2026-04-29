using System.Collections.Generic;

namespace ERP.API.Models
{
    public class SalesProductGridRequest
    {
        // Add filter, paging, sorting properties as needed
        public int? ProductId { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }

    public class SalesProductGridResponse
    {
        public int TotalRecords { get; set; }
        public IEnumerable<SalesProductGridDto> Data { get; set; }
    }

    public class SalesProductGridDto
    {
        public int Id { get; set; }
        public string ItemName { get; set; }
        public string ItemCode { get; set; }
        public int Quantity { get; set; }
        public double Amount { get; set; }
        // Add other fields as needed
    }
}
