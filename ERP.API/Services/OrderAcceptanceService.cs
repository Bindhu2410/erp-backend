using ERP.API.Models;
using System.Threading.Tasks;
using Dapper;
using System.Data;

namespace ERP.API.Services
{
    public class OrderAcceptanceService : IOrderAcceptanceService
    {
        private readonly IDbConnection _dbConnection;
        public OrderAcceptanceService(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<OrderAcceptance> CreateOrderAcceptanceAsync(OrderAcceptance orderAcceptance)
        {
            // Check if the related PurchaseOrder status is 'Approved'
            var poStatusSql = @"SELECT status FROM public.purchase_order WHERE po_id = @PurchaseOrderId LIMIT 1;";
            string poStatus = await _dbConnection.ExecuteScalarAsync<string>(poStatusSql, new { PurchaseOrderId = orderAcceptance.PurchaseOrderId });
            if (poStatus != "Approved")
            {
                throw new InvalidOperationException("Order Acceptance can only be created when the Purchase Order status is 'Approved'.");
            }

            // Automatically fetch sales_order_id based on po_id
            var salesOrderIdSql = @"SELECT sales_order_id FROM public.purchase_order WHERE po_id = @PurchaseOrderId LIMIT 1;";
            orderAcceptance.SalesOrderId = await _dbConnection.ExecuteScalarAsync<string>(salesOrderIdSql, new { PurchaseOrderId = orderAcceptance.PurchaseOrderId });

            // Auto-generate OrderAcceptanceId: OA-YYYY-NN (NN = next sequence for year)
            int year = DateTime.UtcNow.Year;
            var countSql = @"SELECT COUNT(*) FROM public.order_acceptance WHERE EXTRACT(YEAR FROM date_created) = @Year;";
            int count = await _dbConnection.ExecuteScalarAsync<int>(countSql, new { Year = year });
            string nextSeq = (count + 1).ToString("D2");
            orderAcceptance.OrderAcceptanceId = $"OA-{year}-{nextSeq}";

            var sql = @"INSERT INTO public.order_acceptance (order_acceptance_id, user_created, date_created, user_updated, date_updated, subject, purchase_order_id, comments, fileUrl, fileName, quotation_id, sales_order_id)
                        VALUES (@OrderAcceptanceId, @UserCreated, @DateCreated, @UserUpdated, @DateUpdated, @Subject, @PurchaseOrderId, @Comments, @FileUrl, @FileName, @QuotationId, @SalesOrderId)
                        RETURNING id;";
            orderAcceptance.Id = await _dbConnection.ExecuteScalarAsync<int>(sql, orderAcceptance);
            return orderAcceptance;
        }

        public async Task<OrderAcceptance> GetOrderAcceptanceByPOAsync(string purchaseOrderId)
        {
            var sql = @"SELECT * FROM public.order_acceptance WHERE purchase_order_id = @PurchaseOrderId LIMIT 1;";
            return await _dbConnection.QueryFirstOrDefaultAsync<OrderAcceptance>(sql, new { PurchaseOrderId = purchaseOrderId });
        }

        public async Task<OrderAcceptance> CreateOrderAcceptanceFromPOAsync(int purchaseOrderId, int? userId)
        {
            // Get PO details
            var po = await _dbConnection.QueryFirstOrDefaultAsync<dynamic>(
                "SELECT po_id, quotation_id, sales_order_id FROM purchase_order WHERE id = @Id",
                new { Id = purchaseOrderId });
            
            if (po == null)
                throw new InvalidOperationException($"Purchase Order with ID {purchaseOrderId} not found");

            // Check if OA already exists
            var existing = await GetOrderAcceptanceByPOAsync(po.po_id);
            if (existing != null)
                throw new InvalidOperationException($"Order Acceptance already exists for PO {po.po_id}");

            // Generate OA ID in format OA/FY-FY/NN
            var now = DateTime.UtcNow;
            var fiscalYearStart = now.Month >= 4 ? now.Year : now.Year - 1;
            var fiscalYearEnd = fiscalYearStart + 1;
            var fyShort = fiscalYearStart % 100;
            var fyEndShort = fiscalYearEnd % 100;
            var seqSql = "SELECT COALESCE(MAX(CAST(SPLIT_PART(order_acceptance_id, '/', 3) AS INTEGER)), 0) + 1 FROM order_acceptance WHERE order_acceptance_id LIKE @fyPattern";
            var fyPattern = $"OA/{fyShort}-{fyEndShort}/%";
            var seq = await _dbConnection.ExecuteScalarAsync<int>(seqSql, new { fyPattern });
            string oaId = $"OA/{fyShort}-{fyEndShort}/{seq:D2}";

            var orderAcceptance = new OrderAcceptance
            {
                OrderAcceptanceId = oaId,
                PurchaseOrderId = po.po_id,
                QuotationId = po.quotation_id,
                SalesOrderId = po.sales_order_id,
                Subject = $"Order Acceptance for PO {po.po_id}",
                UserCreated = userId ?? 1,
                DateCreated = DateTime.UtcNow,
                UserUpdated = userId,
                DateUpdated = DateTime.UtcNow
            };

            // Check if Sales Order already exists for this PO
            var existingSO = await _dbConnection.QueryFirstOrDefaultAsync<string>(
                "SELECT order_id FROM sales_orders WHERE po_id = @PoId", new { PoId = po.po_id });
            
            string soId;
            if (!string.IsNullOrEmpty(existingSO))
            {
                soId = existingSO;
            }
            else
            {
                // Create Sales Order first
                var soFyPattern = $"SO/{fyShort}-{fyEndShort}/%";
                var soSeqSql = "SELECT COALESCE(MAX(CAST(SPLIT_PART(order_id, '/', 3) AS INTEGER)), 0) + 1 FROM sales_orders WHERE order_id LIKE @fyPattern";
                var soSeq = await _dbConnection.ExecuteScalarAsync<int>(soSeqSql, new { fyPattern = soFyPattern });
                soId = $"SO/{fyShort}-{fyEndShort}/{soSeq:D2}";
                
                var soSql = @"INSERT INTO public.sales_orders (order_id, customer_id, order_date, status, quotation_id, po_id, acceptance_date, user_created, date_created, user_updated, date_updated)
                             VALUES (@OrderId, @CustomerId, @OrderDate, @Status, @QuotationId, @PoId, @AcceptanceDate, @UserCreated, @DateCreated, @UserUpdated, @DateUpdated)
                             RETURNING id;";
                
                var soParams = new
                {
                    OrderId = soId,
                    CustomerId = (int?)null,
                    OrderDate = DateTime.UtcNow.Date,
                    Status = "Pending",
                    QuotationId = po.quotation_id,
                    PoId = po.po_id,
                    AcceptanceDate = DateTime.UtcNow.Date,
                    UserCreated = userId ?? 1,
                    DateCreated = DateTime.UtcNow,
                    UserUpdated = userId,
                    DateUpdated = DateTime.UtcNow
                };
                
                await _dbConnection.ExecuteScalarAsync<int>(soSql, soParams);
            }
            orderAcceptance.SalesOrderId = soId;

            var sql = @"INSERT INTO public.order_acceptance (order_acceptance_id, user_created, date_created, user_updated, date_updated, subject, purchase_order_id, quotation_id, sales_order_id)
                        VALUES (@OrderAcceptanceId, @UserCreated, @DateCreated, @UserUpdated, @DateUpdated, @Subject, @PurchaseOrderId, @QuotationId, @SalesOrderId)
                        RETURNING id;";
            orderAcceptance.Id = await _dbConnection.ExecuteScalarAsync<int>(sql, orderAcceptance);
            return orderAcceptance;
        }
    }
}
