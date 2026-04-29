using System.Collections.Generic;

namespace ERP.API.Models
{
    public class SalesDemoGridResponse<T>
    {
        public int TotalRecords { get; set; }
        public IEnumerable<T>? Data { get; set; }
    }
}
