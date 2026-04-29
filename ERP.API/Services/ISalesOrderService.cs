
using System.Collections.Generic;
using System.Threading.Tasks;
using ERP.API.Models;
using ERP.API.Models.DTOs;

namespace ERP.API.Services
{
    public interface ISalesOrderService
    {
        /// <summary>
        /// Gets the sales order by po_id.
        /// </summary>
        Task<SalesOrder> GetSalesOrderByPoIdAsync(string poId);

        /// <summary>
        /// Updates the po_id in sales_orders for the given sales order id.
        /// </summary>
        Task<bool> UpdateSalesOrderPoIdAsync(int salesOrderId, int poId);

        Task<IEnumerable<SalesOrderGrid>> GetAllSalesOrdersAsync();
        Task<SalesOrder> GetSalesOrderByIdAsync(int id);
        Task<SalesOrder> CreateSalesOrderAsync(SalesOrder salesOrder);
        Task<bool> UpdateSalesOrderAsync(SalesOrder salesOrder);
        Task<bool> DeleteSalesOrderAsync(int id);
        Task<QuotationWithOrderResponse> GetQuotationByIdAsync(int id);
        Task CopyQuotationItemsToOrder(int quotationId, int orderId);
        Task<IEnumerable<SalesItemResponse>> GetQuotationItemsAsync(int quotationId);
        Task<SalesOrderDetailsDto> GetSalesOrderDetailsByIdAsync(int id); // Returns CustomerName
        Task<SalesOrderWithQuotationAndItemsDto> GetSalesOrderWithQuotationAndItemsAsync(int id);
        Task<object> GetSalesOrderDetailAsync(int id);
        Task<SalesOrderWithQuotationAndItemsDto> GetSalesOrderWithQuotationAndItemsByPoIdAsync(string poId);
        Task<IEnumerable<SalesOrderWithQuotationAndItemsDto>> GetAllSalesOrdersWithQuotationAndItemsAsync();
        Task<SalesOrder> GetSalesOrderByQuotationIdAsync(int quotationId);
        Task<IEnumerable<QuotationGridDto>> GetQuotationGridAsync(QuotationGridSearchRequest request);
        Task<SalesOrderDetailsDto> GetByOrderIdAsync(string orderId);
    }
}