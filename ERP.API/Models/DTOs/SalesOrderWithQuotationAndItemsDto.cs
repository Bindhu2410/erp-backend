using System.Collections.Generic;
using ERP.API.Models; // Ensure SalesItemResponse is available

namespace ERP.API.Models.DTOs
{
    public class SalesOrderWithQuotationAndItemsDto
    {
        public SalesOrder SalesOrder { get; set; }
        public SalesQuotation Quotation { get; set; }
        public PurchaseOrderDto PurchaseOrder { get; set; }
        public List<SalesItemResponse> Items { get; set; }
    }
}
