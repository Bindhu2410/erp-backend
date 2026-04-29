using System.Threading.Tasks;
using ERP.API.Models;
using System.Collections.Generic;
using System.Data;
using Dapper;
using Microsoft.Extensions.Configuration;

namespace ERP.API.Services
{
    public interface IInvoiceGridService
    {
        Task<(List<InvoiceDto>, int)> GetInvoiceGridAsync(InvoiceGridRequest request);
    }

    public class InvoiceGridService : IInvoiceGridService
    {
        private readonly IConfiguration _configuration;

        public InvoiceGridService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<(List<InvoiceDto>, int)> GetInvoiceGridAsync(InvoiceGridRequest request)
        {
            using (var connection = new Npgsql.NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                var filters = new List<string>();
                var parameters = new DynamicParameters();

                var searchText = string.IsNullOrWhiteSpace(request.SearchText) || request.SearchText == "string" ? null : request.SearchText;
                var statuses = (request.Statuses != null && request.Statuses.Length == 1 && request.Statuses[0] == "string") ? null : request.Statuses;

                if (!string.IsNullOrEmpty(searchText))
                {
                    filters.Add("(invoice_id ILIKE '%' || @SearchText || '%' OR po_id ILIKE '%' || @SearchText || '%' OR sales_order_id ILIKE '%' || @SearchText || '%')");
                    parameters.Add("SearchText", searchText);
                }

                if (statuses != null && statuses.Length > 0)
                {
                    var validStatuses = new HashSet<string> { "Draft", "Issued", "Paid", "Partially Paid", "Cancelled", "Refunded" };
                    var filteredStatuses = statuses.Where(s => validStatuses.Contains(s)).ToArray();
                    if (filteredStatuses.Length > 0)
                    {
                        filters.Add("status = ANY(@Statuses)");
                        parameters.Add("Statuses", filteredStatuses);
                    }
                }

                var whereClause = filters.Count > 0 ? ("WHERE " + string.Join(" AND ", filters)) : "";
                var orderBy = !string.IsNullOrEmpty(request.OrderBy) ? request.OrderBy : "date_created";
                var orderDirection = request.OrderDirection == "ASC" ? "ASC" : "DESC";

                parameters.Add("Offset", (request.PageNumber - 1) * request.PageSize);
                parameters.Add("Limit", request.PageSize);

                var sql = $@"SELECT id AS Id, invoice_id AS InvoiceId, total_amount AS TotalAmount, status AS Status, date_created AS CreatedDate, po_id AS PoId, sales_order_id AS SalesOrderId, quotation_id AS QuotationId, delivery_id AS DeliveryId, unit_price AS UnitPrice, quantity AS Quantity, amount AS Amount
                            FROM sales_invoices
                            {whereClause}
                            ORDER BY {orderBy} {orderDirection}
                            OFFSET @Offset LIMIT @Limit";

                // Debug output for troubleshooting
                Console.WriteLine("InvoiceGrid SQL: " + sql);
                foreach (var paramName in parameters.ParameterNames)
                {
                    Console.WriteLine($"Param: {paramName} = {parameters.Get<dynamic>(paramName)}");
                }

                var data = (await connection.QueryAsync<InvoiceDto>(sql, parameters)).AsList();

                // Fetch QuotationInfo and Items for each invoice
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                var quotationService = new SalesQuotationService(connectionString);
                foreach (var invoice in data)
                {
                    if (invoice.QuotationId.HasValue)
                    {
                        invoice.QuotationInfo = await quotationService.GetDetailsByQuotationIdAsync(invoice.QuotationId.Value);
                        invoice.Items = await quotationService.GetItemsByQuotationIdAsync(invoice.QuotationId.Value);
                    }
                }

                var countSql = $@"SELECT COUNT(*) FROM sales_invoices {whereClause}";
                Console.WriteLine("InvoiceGrid Count SQL: " + countSql);
                var totalRecords = await connection.ExecuteScalarAsync<int>(countSql, parameters);
                return (data, totalRecords);
            }
        }
    }
}
