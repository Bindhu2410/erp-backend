using System.Threading.Tasks;
using ERP.API.Models;
using System.Collections.Generic;

namespace ERP.API.Services
{
    public interface IPurchaseOrderService
    {
        Task<IEnumerable<PurchaseOrderDto>> GetAllAsync();
        Task<PurchaseOrderDto> GetByIdAsync(int id);
        Task<PurchaseOrderDto> CreateAsync(PurchaseOrderDto purchaseOrder);
        Task<PurchaseOrderDto> UpdateAsync(int id, PurchaseOrderDto purchaseOrder);
        Task<bool> DeleteAsync(int id);

        // Restored methods for compatibility
        Task<PurchaseOrderDetailsDto> GetByPoIdAsync(string poId);
        Task<List<SalesItemResponse>> GetDetailsByPoIdAsync(string poId);
        Task<string> GetInvoiceIdByPoIdAsync(string poId);
        Task<IEnumerable<PurchaseRequisition>> GetApprovedPRsForDropdown();

        // New method for dropdown items
        Task<List<SalesItemResponse>> GetItemsByRequisitionIdAsync(int requisitionId);

        // New method to create purchase order from quotation
        Task<PurchaseOrderDto> CreatePurchaseOrderFromQuotationAsync(int quotationId, int? userId);

        // New method to get purchase order by quotation ID
        Task<PurchaseOrderDetailsDto> GetByQuotationIdAsync(int quotationId);

        // Update only the status of a purchase order
        Task<bool> UpdateStatusAsync(int id, string status);
    }
}
