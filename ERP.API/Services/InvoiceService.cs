using System;
using System.Threading.Tasks;
using ERP.API.Models;
using System.Collections.Generic;
using ERP.API.Services;
using System.Data;
using Dapper;
using Microsoft.Extensions.Configuration;

namespace ERP.API.Services
{
    public interface IInvoiceService
    {
        Task<InvoiceResponse> CreateInvoiceAsync(CreateInvoiceRequest request);
        Task<InvoiceResponse> CreateInvoiceFromDeliveryAsync(string deliveryId);
        Task<InvoiceResponse> GetInvoiceByIdAsync(string invoiceId);
        Task<InvoiceResponse> GetInvoiceByPrimaryIdAsync(int id);
        Task<InvoiceResponse> GetInvoiceByDeliveryIdAsync(string deliveryId);
        Task<List<InvoiceResponse>> GetAllInvoicesAsync();
        Task<InvoiceResponse> GetInvoiceByPoIdAsync(string poId);
    }
}

    public class InvoiceService : IInvoiceService
    {
        public async Task<InvoiceResponse> GetInvoiceByPrimaryIdAsync(int id)
        {
            using (var connection = new Npgsql.NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                var sql = @"SELECT * FROM sales_invoices WHERE id = @Id LIMIT 1;";
                var invoiceRecord = await connection.QueryFirstOrDefaultAsync(sql, new { Id = id.ToString() });
                if (invoiceRecord == null)
                    return null;

                string poId = string.Empty;
                if (invoiceRecord.po_id is int i)
                    poId = i.ToString();
                else if (invoiceRecord.po_id is string s)
                    poId = s;
                else if (invoiceRecord.po_id is int?)
                    poId = ((int?)invoiceRecord.po_id)?.ToString() ?? string.Empty;
                else
                    poId = Convert.ToString(invoiceRecord.po_id) ?? string.Empty;

                string salesOrderId = string.Empty;
                if (invoiceRecord.sales_order_id is int i2)
                    salesOrderId = i2.ToString();
                else if (invoiceRecord.sales_order_id is string s2)
                    salesOrderId = s2;
                else
                    salesOrderId = Convert.ToString(invoiceRecord.sales_order_id) ?? string.Empty;
                int quotationId = invoiceRecord.quotation_id ?? 0;
                object purchaseOrderInfo = null;
                object fullQuotationInfo = null;
                List<object> fullItems = new List<object>();
                List<SalesTermsAndConditions> fullTerms = new List<SalesTermsAndConditions>();
                ERP.API.Models.Delivery? delivery = null;

                if (!string.IsNullOrEmpty(poId))
                    purchaseOrderInfo = await _purchaseOrderService.GetDetailsByPoIdAsync(poId);
                if (quotationId != 0)
                {
                    fullQuotationInfo = await _salesQuotationService.GetDetailsByQuotationIdAsync(quotationId);
                    var itemsResult = await _salesQuotationService.GetItemsByQuotationIdAsync(quotationId);
                    if (itemsResult is IEnumerable<object> itemsEnumerable)
                        fullItems = itemsEnumerable.ToList();
                    var termsResult = await _termsService.GetTermsByQuotationIdAsync(quotationId);
                    if (termsResult is IEnumerable<SalesTermsAndConditions> termsEnumerable)
                        fullTerms = termsEnumerable.ToList();
                }

                if (!string.IsNullOrEmpty(poId))
                {
                    var deliveries = await _deliveryService.GetByPurchaseOrderIdAsync(poId);
                    delivery = deliveries?.OrderByDescending(d => d.DeliveryDate).FirstOrDefault();
                }

                return new InvoiceResponse
                {
                    Id = invoiceRecord.id is int ? (int?)invoiceRecord.id : int.TryParse(invoiceRecord.id?.ToString(), out int _id) ? (int?)_id : null,
                    InvoiceId = invoiceRecord.invoice_id,
                    Status = invoiceRecord.status,
                    CreatedDate = invoiceRecord.date_created,
                    po_id = poId,
                    sales_order_id = salesOrderId,
                    quotation_id = invoiceRecord.quotation_id ?? 0,
                    PurchaseOrderInfo = purchaseOrderInfo ?? new object(),
                    QuotationInfo = fullQuotationInfo ?? new object(),
                    Items = fullItems ?? new List<object>(),
                    TermsAndConditions = fullTerms ?? new List<SalesTermsAndConditions>(),
                    TotalAmount = invoiceRecord.total_amount != null ? (decimal)invoiceRecord.total_amount : 0,
                    Delivery = delivery,
                    delivery_id = invoiceRecord.delivery_id?.ToString() ?? string.Empty
                };
            }
        }

        public async Task<InvoiceResponse> GetInvoiceByPoIdAsync(string poId)
        {
            using (var connection = new Npgsql.NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                var sql = @"SELECT * FROM sales_invoices WHERE po_id = @PoId ORDER BY id DESC LIMIT 1;";
                var invoiceRecord = await connection.QueryFirstOrDefaultAsync(sql, new { PoId = poId });
                if (invoiceRecord == null)
                    return null;

                string salesOrderId = string.Empty;
                if (invoiceRecord.sales_order_id is int i2)
                    salesOrderId = i2.ToString();
                else if (invoiceRecord.sales_order_id is string s2)
                    salesOrderId = s2;
                else if (invoiceRecord.sales_order_id is int?)
                    salesOrderId = ((int?)invoiceRecord.sales_order_id)?.ToString() ?? string.Empty;
                else
                    salesOrderId = Convert.ToString(invoiceRecord.sales_order_id) ?? string.Empty;
                int quotationId = invoiceRecord.quotation_id ?? 0;
                object purchaseOrderInfo = null;
                object fullQuotationInfo = null;
                List<object> fullItems = new List<object>();
                List<SalesTermsAndConditions> fullTerms = new List<SalesTermsAndConditions>();
                ERP.API.Models.Delivery? delivery = null;

                if (!string.IsNullOrEmpty(poId))
                    purchaseOrderInfo = await _purchaseOrderService.GetDetailsByPoIdAsync(poId);
                if (quotationId != 0)
                {
                    fullQuotationInfo = await _salesQuotationService.GetDetailsByQuotationIdAsync(quotationId);
                    var itemsResult = await _salesQuotationService.GetItemsByQuotationIdAsync(quotationId);
                    if (itemsResult is IEnumerable<object> itemsEnumerable)
                        fullItems = itemsEnumerable.ToList();
                    var termsResult = await _termsService.GetTermsByQuotationIdAsync(quotationId);
                    if (termsResult is IEnumerable<SalesTermsAndConditions> termsEnumerable)
                        fullTerms = termsEnumerable.ToList();
                }

                if (!string.IsNullOrEmpty(poId))
                {
                    var deliveries = await _deliveryService.GetByPurchaseOrderIdAsync(poId);
                    delivery = deliveries?.OrderByDescending(d => d.DeliveryDate).FirstOrDefault();
                }

                return new InvoiceResponse
                {
                    Id = invoiceRecord.id is int ? (int?)invoiceRecord.id : int.TryParse(invoiceRecord.id?.ToString(), out int _id) ? (int?)_id : null,
                    InvoiceId = invoiceRecord.invoice_id,
                    Status = invoiceRecord.status,
                    CreatedDate = invoiceRecord.date_created,
                    po_id = poId,
                    sales_order_id = salesOrderId,
                    quotation_id = invoiceRecord.quotation_id ?? 0,
                    PurchaseOrderInfo = purchaseOrderInfo ?? new object(),
                    QuotationInfo = fullQuotationInfo ?? new object(),
                    Items = fullItems ?? new List<object>(),
                    TermsAndConditions = fullTerms ?? new List<SalesTermsAndConditions>(),
                    TotalAmount = invoiceRecord.total_amount != null ? (decimal)invoiceRecord.total_amount : 0,
                    Delivery = delivery,
                    delivery_id = invoiceRecord.delivery_id?.ToString() ?? string.Empty
                };
            }
        }

        public async Task<InvoiceResponse> GetInvoiceByDeliveryIdAsync(string deliveryId)
        {
            using (var connection = new Npgsql.NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                var sql = @"SELECT * FROM sales_invoices WHERE delivery_id = @DeliveryId ORDER BY id DESC LIMIT 1;";
                var invoiceRecord = await connection.QueryFirstOrDefaultAsync(sql, new { DeliveryId = deliveryId });
                if (invoiceRecord == null)
                    return null;

                // Fetch related data (PurchaseOrderInfo, QuotationInfo, Items, TermsAndConditions, Delivery)
                string poId = string.Empty;
                if (invoiceRecord.po_id is int i)
                    poId = i.ToString();
                else if (invoiceRecord.po_id is string s)
                    poId = s;
                else if (invoiceRecord.po_id is int?)
                    poId = ((int?)invoiceRecord.po_id)?.ToString() ?? string.Empty;
                else
                    poId = Convert.ToString(invoiceRecord.po_id) ?? string.Empty;

                string salesOrderId = string.Empty;
                if (invoiceRecord.sales_order_id is int i2)
                    salesOrderId = i2.ToString();
                else if (invoiceRecord.sales_order_id is string s2)
                    salesOrderId = s2;
                else
                    salesOrderId = Convert.ToString(invoiceRecord.sales_order_id) ?? string.Empty;
                int quotationId = invoiceRecord.quotation_id ?? 0;
                object purchaseOrderInfo = null;
                object fullQuotationInfo = null;
                List<object> fullItems = new List<object>();
                List<SalesTermsAndConditions> fullTerms = new List<SalesTermsAndConditions>();
                ERP.API.Models.Delivery? delivery = null;

                if (!string.IsNullOrEmpty(poId))
                    purchaseOrderInfo = await _purchaseOrderService.GetDetailsByPoIdAsync(poId);
                if (quotationId != 0)
                {
                    fullQuotationInfo = await _salesQuotationService.GetDetailsByQuotationIdAsync(quotationId);
                    var itemsResult = await _salesQuotationService.GetItemsByQuotationIdAsync(quotationId);
                    if (itemsResult is IEnumerable<object> itemsEnumerable)
                        fullItems = itemsEnumerable.ToList();
                    var termsResult = await _termsService.GetTermsByQuotationIdAsync(quotationId);
                    if (termsResult is IEnumerable<SalesTermsAndConditions> termsEnumerable)
                        fullTerms = termsEnumerable.ToList();
                }

                // Fetch delivery by deliveryId
                var sqlDelivery = "SELECT * FROM deliveries WHERE delivery_id = @DeliveryId LIMIT 1;";
                delivery = await connection.QueryFirstOrDefaultAsync<ERP.API.Models.Delivery>(sqlDelivery, new { DeliveryId = deliveryId });

                return new InvoiceResponse
                {
                    Id = invoiceRecord.id is int ? (int?)invoiceRecord.id : int.TryParse(invoiceRecord.id?.ToString(), out int _id) ? (int?)_id : null,
                    InvoiceId = invoiceRecord.invoice_id,
                    Status = invoiceRecord.status,
                    CreatedDate = invoiceRecord.date_created,
                    po_id = poId,
                    sales_order_id = salesOrderId,
                    quotation_id = invoiceRecord.quotation_id ?? 0,
                    PurchaseOrderInfo = purchaseOrderInfo ?? new object(),
                    QuotationInfo = fullQuotationInfo ?? new object(),
                    Items = fullItems ?? new List<object>(),
                    TermsAndConditions = fullTerms ?? new List<SalesTermsAndConditions>(),
                    TotalAmount = invoiceRecord.total_amount != null ? (decimal)invoiceRecord.total_amount : 0,
                    Delivery = delivery,
                    delivery_id = invoiceRecord.delivery_id != null ? invoiceRecord.delivery_id.ToString() : string.Empty
                };
            }
        }
        private readonly IPurchaseOrderService _purchaseOrderService;
        private readonly ISalesOrderService _salesOrderService;
        private readonly ISalesQuotationService _salesQuotationService;
        private readonly ISalesTermsAndConditionsService _termsService;
        private readonly IDeliveryService _deliveryService;
        private readonly IConfiguration _configuration;

        public InvoiceService(
            IPurchaseOrderService purchaseOrderService,
            ISalesOrderService salesOrderService,
            ISalesQuotationService salesQuotationService,
            ISalesTermsAndConditionsService termsService,
            IDeliveryService deliveryService,
            IConfiguration configuration)
        {
            _purchaseOrderService = purchaseOrderService;
            _salesOrderService = salesOrderService;
            _salesQuotationService = salesQuotationService;
            _termsService = termsService;
            _deliveryService = deliveryService;
            _configuration = configuration;
        }

        public async Task<InvoiceResponse> CreateInvoiceFromDeliveryAsync(string deliveryId)
        {
            // Fetch delivery details
            var deliveries = await _deliveryService.GetAllAsync();
            var delivery = deliveries?.FirstOrDefault(d => d.DeliveryId == deliveryId);
            if (delivery == null)
                return null;

            // Use delivery to get related PO, SO, Quotation
            string poId = delivery.PoId;
            string salesOrderId = delivery.SalesOrderId;
            int? quotationId = null;
            if (!string.IsNullOrEmpty(poId))
            {
                var poDetails = await _purchaseOrderService.GetByPoIdAsync(poId);
                quotationId = poDetails?.PurchaseOrder?.QuotationId;
            }
            else if (!string.IsNullOrEmpty(salesOrderId))
            {
                var soDetails = await _salesOrderService.GetByOrderIdAsync(salesOrderId);
                quotationId = soDetails?.SalesOrder?.QuotationId;
            }

            // Fetch related data
            var purchaseOrderInfo = !string.IsNullOrEmpty(poId) ? await _purchaseOrderService.GetDetailsByPoIdAsync(poId) : null;
            object fullQuotationInfo = null;
            List<object> fullItems = new List<object>();
            List<SalesTermsAndConditions> fullTerms = new List<SalesTermsAndConditions>();
            if (quotationId.HasValue)
            {
                var quotationDetails = await _salesQuotationService.GetDetailsByQuotationIdAsync(quotationId.Value);
                if (quotationDetails != null)
                    fullQuotationInfo = quotationDetails;
                var itemsResult = await _salesQuotationService.GetItemsByQuotationIdAsync(quotationId.Value);
                if (itemsResult is IEnumerable<object> itemsEnumerable)
                    fullItems = itemsEnumerable.ToList();
                var termsResult = await _termsService.GetTermsByQuotationIdAsync(quotationId.Value);
                if (termsResult is IEnumerable<SalesTermsAndConditions> termsEnumerable)
                    fullTerms = termsEnumerable.ToList();
            }

            // Calculate total amount from delivery items if available
            decimal totalAmount = 0;
            if (delivery.Items != null)
            {
                foreach (var item in delivery.Items)
                {
                    if (item.Amount.HasValue)
                        totalAmount += (decimal)item.Amount.Value;
                }
            }

            // Auto-generate invoice_id
            string invoiceId = $"INV-{DateTime.Now:yyyy}-{new Random().Next(100, 999)}";
            // Insert invoice for each delivery item to satisfy NOT NULL constraints

            if (delivery.Items != null && delivery.Items.Count > 0)
            {
                foreach (var item in delivery.Items)
                {
                    decimal unitPrice = item.UnitPrice != null ? Convert.ToDecimal(item.UnitPrice) : 0;
                    decimal amount = item.Amount != null ? Convert.ToDecimal(item.Amount) : 0;
                    decimal quantity = item.Qty != null ? Convert.ToDecimal(item.Qty) : 0;
                    await InsertInvoiceAsync(invoiceId, quotationId, poId, salesOrderId, totalAmount, unitPrice, quantity, amount, deliveryId);
                }
            }
            else
            {
                // Fallback: insert with zeroes if no items (should not happen)
                await InsertInvoiceAsync(invoiceId, quotationId, poId, salesOrderId, totalAmount, 0, 0, 0, deliveryId);
            }

            // Update sales_invoices with delivery_id
            using (var connection = new Npgsql.NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                await connection.ExecuteAsync(
                    "UPDATE sales_invoices SET delivery_id = @DeliveryId WHERE invoice_id = @InvoiceId",
                    new { DeliveryId = deliveryId, InvoiceId = invoiceId }
                );

                // Also update purchase_order with the new invoice_id if poId is present
                if (!string.IsNullOrEmpty(poId))
                {
                    await connection.ExecuteAsync(
                        "UPDATE purchase_order SET invoice_id = @InvoiceId WHERE po_id = @PoId",
                        new { InvoiceId = invoiceId, PoId = poId }
                    );
                }
            }

            // Fetch the last inserted invoice id for this invoiceId
            int? id = null;
            using (var connection = new Npgsql.NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                var sql = @"SELECT id FROM sales_invoices WHERE invoice_id = @InvoiceId ORDER BY id DESC LIMIT 1;";
                id = await connection.ExecuteScalarAsync<int?>(sql, new { InvoiceId = invoiceId });
            }

            return new InvoiceResponse
            {
                Id = id,
                InvoiceId = invoiceId,
                Status = "Draft",
                CreatedDate = DateTime.Now,
                po_id = poId ?? string.Empty,
                sales_order_id = salesOrderId ?? string.Empty,
                quotation_id = quotationId ?? 0,
                PurchaseOrderInfo = purchaseOrderInfo ?? new object(),
                QuotationInfo = fullQuotationInfo ?? new object(),
                Items = fullItems ?? new List<object>(),
                TermsAndConditions = fullTerms ?? new List<SalesTermsAndConditions>(),
                TotalAmount = totalAmount,
                Delivery = delivery,
                delivery_id = deliveryId != null ? deliveryId.ToString() : string.Empty
            };
        }

        // Removed duplicate InsertInvoiceAsync signature
        private async Task<int> InsertInvoiceAsync(string invoiceId, int? quotationId, string poId, string salesOrderId, decimal totalAmount, decimal unitPrice, decimal quantity, decimal amount, string deliveryId)
        {
            using (var connection = new Npgsql.NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                var sql = @"INSERT INTO sales_invoices (invoice_id, quotation_id, po_id, sales_order_id, total_amount, status, date_created, unit_price, quantity, amount, delivery_id)
                            VALUES (@InvoiceId, @QuotationId, @PoId, @SalesOrderId, @TotalAmount, 'Draft', NOW(), @UnitPrice, @Quantity, @Amount, @DeliveryId) RETURNING id;";
                var parameters = new
                {
                    InvoiceId = invoiceId,
                    QuotationId = quotationId,
                    PoId = poId,
                    SalesOrderId = salesOrderId,
                    TotalAmount = totalAmount,
                    UnitPrice = unitPrice,
                    Quantity = quantity,
                    Amount = amount,
                    DeliveryId = deliveryId
                };
                var newId = await connection.ExecuteScalarAsync<int>(sql, parameters);
                return newId;
            }
        }

        // Helper method to fetch PO and SO IDs by quotationId
        private async Task<(string poId, string salesOrderId)> GetPoAndSalesOrderIdByQuotationIdAsync(int quotationId)
        {
            using (var connection = new Npgsql.NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                var sql = @"SELECT po_id, sales_order_id FROM purchase_order WHERE quotation_id = @QuotationId LIMIT 1;";
                var result = await connection.QueryFirstOrDefaultAsync(sql, new { QuotationId = quotationId });
                string poId = result?.po_id != null ? result.po_id.ToString() : string.Empty;
                string salesOrderId = result?.sales_order_id != null ? result.sales_order_id.ToString() : string.Empty;
                return (poId, salesOrderId);
            }
        }

        public async Task<InvoiceResponse> CreateInvoiceAsync(CreateInvoiceRequest request)
        {
            // ...existing code up to invoice creation...
            int? quotationId = request.QuotationId;
            string poId = request.PoId;
            string salesOrderId = request.SalesOrderId;

            // If po_id or sales_order_id is "string", fetch from purchase_order table
            if (quotationId.HasValue && (poId == "string" || string.IsNullOrEmpty(poId) || salesOrderId == "string" || string.IsNullOrEmpty(salesOrderId)))
            {
                var ids = await GetPoAndSalesOrderIdByQuotationIdAsync(quotationId.Value);
                if (poId == "string" || string.IsNullOrEmpty(poId)) poId = ids.poId;
                if (salesOrderId == "string" || string.IsNullOrEmpty(salesOrderId)) salesOrderId = ids.salesOrderId;
            }

            // Declare variables before use
            object fullQuotationInfo = null;
            List<object> fullItems = new List<object>();
            List<SalesTermsAndConditions> fullTerms = new List<SalesTermsAndConditions>();
            object purchaseOrderInfo = null;

            // Auto-fetch PO and SO IDs based on quotationId
            if (quotationId.HasValue)
            {
                // If PO ID or SO ID is not provided, fetch from purchase_order table
                if (string.IsNullOrEmpty(poId) || string.IsNullOrEmpty(salesOrderId))
                {
                    var (fetchedPoId, fetchedSalesOrderId) = await GetPoAndSalesOrderIdByQuotationIdAsync(quotationId.Value);
                    if (string.IsNullOrEmpty(poId)) poId = fetchedPoId;
                    if (string.IsNullOrEmpty(salesOrderId)) salesOrderId = fetchedSalesOrderId;
                }

                // Fetch quotation info, items, and terms and conditions using quotationId
                fullQuotationInfo = await _salesQuotationService.GetDetailsByQuotationIdAsync(quotationId.Value);
                var itemsResult = await _salesQuotationService.GetItemsByQuotationIdAsync(quotationId.Value);
                if (itemsResult is IEnumerable<object> itemsEnumerable)
                    fullItems = itemsEnumerable.ToList();
                var termsResult = await _termsService.GetTermsByQuotationIdAsync(quotationId.Value);
                if (termsResult is IEnumerable<SalesTermsAndConditions> termsEnumerable)
                    fullTerms = termsEnumerable.ToList();
            }
            else if (!string.IsNullOrEmpty(poId))
            {
                var poDetails = await _purchaseOrderService.GetByPoIdAsync(poId);
                quotationId = poDetails?.PurchaseOrder?.QuotationId;
                var soId = poDetails?.PurchaseOrder?.SalesOrderId;
                salesOrderId = soId != null ? soId.ToString() : null;
            }
            else if (!string.IsNullOrEmpty(salesOrderId))
            {
                var soDetails = await _salesOrderService.GetByOrderIdAsync(salesOrderId);
                quotationId = soDetails?.SalesOrder?.QuotationId;
            }

            // Fix: convert int? to string for salesOrderId if needed
            if (salesOrderId is int)
                salesOrderId = salesOrderId.ToString();

            // Auto-generate invoice_id
            string invoiceId = $"INV-{DateTime.Now:yyyy}-{new Random().Next(100, 999)}";
            decimal totalAmount = 0;
            if (fullItems is IEnumerable<dynamic> itemList)
            {
                foreach (var item in itemList)
                {
                    if (item != null && item.GetType().GetProperty("Total") != null)
                    {
                        totalAmount += (decimal)item.GetType().GetProperty("Total").GetValue(item);
                    }
                }
            }
            // Insert into sales_invoices table (fallback: insert with zeroes for required NOT NULL columns)
            string validPoId = string.IsNullOrEmpty(poId) || poId == "string" ? null : poId;
            string validSalesOrderId = string.IsNullOrEmpty(salesOrderId) || salesOrderId == "string" ? null : salesOrderId;
            await InsertInvoiceAsync(invoiceId, quotationId, validPoId, validSalesOrderId, totalAmount, 0, 0, 0, request.DeliveryId);

            // Fetch the full invoice record from the DB after insert
            dynamic invoiceRecord = null;
            using (var connection = new Npgsql.NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                var sql = @"SELECT * FROM sales_invoices WHERE invoice_id = @InvoiceId ORDER BY id DESC LIMIT 1;";
                invoiceRecord = await connection.QueryFirstOrDefaultAsync(sql, new { InvoiceId = invoiceId });
            }

            // Return all invoice fields in the response, plus related info
            // After invoice is created, if the request contains a DeliveryId, update the delivery's invoice_id
            if (!string.IsNullOrEmpty(request.DeliveryId))
            {
                using (var connection = new Npgsql.NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    await connection.OpenAsync();
                    await connection.ExecuteAsync(
                        "UPDATE deliveries SET invoice_id = @InvoiceId WHERE delivery_id = @DeliveryId",
                        new { InvoiceId = invoiceRecord?.invoice_id ?? invoiceId, DeliveryId = request.DeliveryId }
                    );
                }
            }
            string finalPoId = string.Empty;
            if (invoiceRecord?.po_id is int i)
                finalPoId = i.ToString();
            else if (invoiceRecord?.po_id is string s)
                finalPoId = s;
            else if (poId != null)
                finalPoId = poId;
            else
                finalPoId = Convert.ToString(invoiceRecord?.po_id) ?? string.Empty;

            string finalSalesOrderId = string.Empty;
            if (invoiceRecord?.sales_order_id is int i2)
                finalSalesOrderId = i2.ToString();
            else if (invoiceRecord?.sales_order_id is string s2)
                finalSalesOrderId = s2;
            else if (salesOrderId != null)
                finalSalesOrderId = salesOrderId;
            else
                finalSalesOrderId = Convert.ToString(invoiceRecord?.sales_order_id) ?? string.Empty;

            return new InvoiceResponse
            {
                InvoiceId = invoiceRecord?.invoice_id ?? invoiceId,
                Status = invoiceRecord?.status ?? "Draft",
                CreatedDate = invoiceRecord?.date_created ?? DateTime.Now,
                po_id = finalPoId ?? string.Empty,
                sales_order_id = finalSalesOrderId ?? string.Empty,
                quotation_id = invoiceRecord?.quotation_id ?? quotationId ?? 0,
                PurchaseOrderInfo = purchaseOrderInfo ?? new object(),
                QuotationInfo = fullQuotationInfo ?? new object(),
                Items = fullItems ?? new List<object>(),
                TermsAndConditions = fullTerms ?? new List<SalesTermsAndConditions>(),
                TotalAmount = invoiceRecord?.total_amount != null ? (decimal)invoiceRecord.total_amount : totalAmount
            };
        }

        public async Task<InvoiceResponse> GetInvoiceByIdAsync(string invoiceId)
        {
            using (var connection = new Npgsql.NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                var sql = @"SELECT * FROM sales_invoices WHERE invoice_id = @InvoiceId ORDER BY id DESC LIMIT 1;";
                var invoiceRecord = await connection.QueryFirstOrDefaultAsync(sql, new { InvoiceId = invoiceId });
                if (invoiceRecord == null)
                    return null;

                // Fetch related data (PurchaseOrderInfo, QuotationInfo, Items, TermsAndConditions, Delivery)
                string poId = string.Empty;
                if (invoiceRecord.po_id is int i)
                    poId = i.ToString();
                else if (invoiceRecord.po_id is string s)
                    poId = s;
                else
                    poId = Convert.ToString(invoiceRecord.po_id) ?? string.Empty;

                string salesOrderId = string.Empty;
                if (invoiceRecord.sales_order_id is int i2)
                    salesOrderId = i2.ToString();
                else if (invoiceRecord.sales_order_id is string s2)
                    salesOrderId = s2;
                else
                    salesOrderId = Convert.ToString(invoiceRecord.sales_order_id) ?? string.Empty;
                int quotationId = invoiceRecord.quotation_id ?? 0;
                object purchaseOrderInfo = null;
                object fullQuotationInfo = null;
                List<object> fullItems = new List<object>();
                List<SalesTermsAndConditions> fullTerms = new List<SalesTermsAndConditions>();
                ERP.API.Models.Delivery? delivery = null;

                if (!string.IsNullOrEmpty(poId))
                    purchaseOrderInfo = await _purchaseOrderService.GetDetailsByPoIdAsync(poId);
                if (quotationId != 0)
                {
                    fullQuotationInfo = await _salesQuotationService.GetDetailsByQuotationIdAsync(quotationId);
                    var itemsResult = await _salesQuotationService.GetItemsByQuotationIdAsync(quotationId);
                    if (itemsResult is IEnumerable<object> itemsEnumerable)
                        fullItems = itemsEnumerable.ToList();
                    var termsResult = await _termsService.GetTermsByQuotationIdAsync(quotationId);
                    if (termsResult is IEnumerable<SalesTermsAndConditions> termsEnumerable)
                        fullTerms = termsEnumerable.ToList();
                }

                // Fetch delivery by po_id, then sales_order_id, then latest
                if (!string.IsNullOrEmpty(poId))
                {
                    var deliveries = await _deliveryService.GetByPurchaseOrderIdAsync(poId);
                    delivery = deliveries?.OrderByDescending(d => d.DeliveryDate).FirstOrDefault();
                }
                if (delivery == null && !string.IsNullOrEmpty(salesOrderId))
                {
                    var sqlDelivery = "SELECT * FROM deliveries WHERE sales_order_id = @SalesOrderId ORDER BY delivery_date DESC LIMIT 1;";
                    var result = await connection.QueryFirstOrDefaultAsync<ERP.API.Models.Delivery>(sqlDelivery, new { SalesOrderId = salesOrderId });
                    delivery = result;
                }
                if (delivery == null)
                {
                    var sqlDelivery = "SELECT * FROM deliveries ORDER BY delivery_date DESC LIMIT 1;";
                    var result = await connection.QueryFirstOrDefaultAsync<ERP.API.Models.Delivery>(sqlDelivery);
                    delivery = result;
                }

                return new InvoiceResponse
                {
                    Id = invoiceRecord.id is int ? (int?)invoiceRecord.id : int.TryParse(invoiceRecord.id?.ToString(), out int _id) ? (int?)_id : null,
                    InvoiceId = invoiceRecord.invoice_id,
                    Status = invoiceRecord.status,
                    CreatedDate = invoiceRecord.date_created,
                    po_id = poId,
                    sales_order_id = salesOrderId,
                    quotation_id = invoiceRecord.quotation_id ?? 0,
                    PurchaseOrderInfo = purchaseOrderInfo ?? new object(),
                    QuotationInfo = fullQuotationInfo ?? new object(),
                    Items = fullItems ?? new List<object>(),
                    TermsAndConditions = fullTerms ?? new List<SalesTermsAndConditions>(),
                    TotalAmount = invoiceRecord.total_amount != null ? (decimal)invoiceRecord.total_amount : 0,
                    Delivery = delivery,
                    delivery_id = invoiceRecord.delivery_id?.ToString() ?? string.Empty
                };
            }
        }

        public async Task<List<InvoiceResponse>> GetAllInvoicesAsync()
        {
            var invoices = new List<InvoiceResponse>();
            using (var connection = new Npgsql.NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                var sql = @"SELECT * FROM sales_invoices;";
                var results = await connection.QueryAsync(sql);
                foreach (var invoice in results)
                {
                    int quotationId = invoice.quotation_id ?? 0;
                    string poId = string.Empty;
                    if (invoice.po_id is int i)
                        poId = i.ToString();
                    else if (invoice.po_id is string s)
                        poId = s;
                    else
                        poId = Convert.ToString(invoice.po_id) ?? string.Empty;

                    string salesOrderId = string.Empty;
                    if (invoice.sales_order_id is int i2)
                        salesOrderId = i2.ToString();
                    else if (invoice.sales_order_id is string s2)
                        salesOrderId = s2;
                    else
                        salesOrderId = Convert.ToString(invoice.sales_order_id) ?? string.Empty;
                    object purchaseOrderInfo = null;
                    object quotationInfo = null;
                    object items = null;
                    object termsAndConditions = null;
                    if (!string.IsNullOrEmpty(poId))
                        purchaseOrderInfo = await _purchaseOrderService.GetDetailsByPoIdAsync(poId);
                    if (quotationId != 0)
                    {
                        quotationInfo = await _salesQuotationService.GetDetailsByQuotationIdAsync(quotationId);
                        items = await _salesQuotationService.GetItemsByQuotationIdAsync(quotationId);
                        termsAndConditions = await _termsService.GetTermsByQuotationIdAsync(quotationId);
                    }
                    invoices.Add(new InvoiceResponse
                    {
                        Id = invoice.id is int ? (int?)invoice.id : int.TryParse(invoice.id?.ToString(), out int _id) ? (int?)_id : null,
                        InvoiceId = invoice.invoice_id,
                        Status = invoice.status,
                        CreatedDate = invoice.date_created,
                        po_id = poId,
                        sales_order_id = salesOrderId,
                        quotation_id = quotationId,
                        PurchaseOrderInfo = purchaseOrderInfo ?? new object(),
                        QuotationInfo = quotationInfo ?? new object(),
                        Items = items ?? new object(),
                        TermsAndConditions = termsAndConditions ?? new object(),
                        TotalAmount = invoice.total_amount != null ? (decimal)invoice.total_amount : 0,
                        delivery_id = invoice.delivery_id?.ToString() ?? string.Empty
                    });
                }
            }
            return invoices;
        }
    }
