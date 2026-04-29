using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using ERP.API.Models;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace ERP.API.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IConfiguration _configuration;
        public PaymentService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<List<PaymentResponse>> GetPaymentsByInvoiceIdAsync(string invoiceId)
        {
            using var connection = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            var sql = @"SELECT p.id, p.user_created, p.date_created, p.user_updated, p.date_updated, p.invoice_id, p.payment_date, p.due_date, p.payment_method, p.amount_paid, p.payment_status, p.outstanding_amount, p.total_amount, sq.customer_name FROM payments p LEFT JOIN sales_invoices si ON p.invoice_id = si.invoice_id LEFT JOIN sales_quotations sq ON si.quotation_id = sq.id WHERE p.invoice_id = @InvoiceId";
            var payments = await connection.QueryAsync<ERP.API.Models.PaymentResponse>(sql, new { InvoiceId = invoiceId });
            return payments.AsList();
        }
        
        public async Task<(IEnumerable<PaymentResponse> Data, int TotalRecords)> GetPaymentGridAsync(PaymentGridRequest request)
        {
            using var connection = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            var whereClauses = new List<string>();
            var parameters = new DynamicParameters();
            if (!string.IsNullOrEmpty(request.SearchText))
            {
                whereClauses.Add("(payment_method ILIKE @SearchText OR payment_status ILIKE @SearchText)");
                parameters.Add("SearchText", "%" + request.SearchText + "%");
            }
            if (request.Statuses != null && request.Statuses.Length > 0)
            {
                whereClauses.Add("payment_status = ANY(@Statuses)");
                parameters.Add("Statuses", request.Statuses);
            }

            var whereSql = whereClauses.Count > 0 ? ("WHERE " + string.Join(" AND ", whereClauses)) : "";
            var sql = $@"SELECT p.id, p.user_created, p.date_created, p.user_updated, p.date_updated, p.invoice_id, p.payment_date, p.due_date, p.payment_method, p.amount_paid, p.payment_status, p.outstanding_amount, p.total_amount, sq.customer_name FROM payments p LEFT JOIN sales_invoices si ON p.invoice_id = si.invoice_id LEFT JOIN sales_quotations sq ON si.quotation_id = sq.id {whereSql} ORDER BY {(string.IsNullOrEmpty(request.OrderBy) ? "p.date_created" : "p." + request.OrderBy)} {(request.OrderDirection == "ASC" ? "ASC" : "DESC")} OFFSET @Offset LIMIT @Limit";
            parameters.Add("Offset", (request.PageNumber - 1) * request.PageSize);
            parameters.Add("Limit", request.PageSize);
            var data = await connection.QueryAsync<ERP.API.Models.PaymentResponse>(sql, parameters);
            var countSql = $@"SELECT COUNT(*) FROM payments p LEFT JOIN sales_invoices si ON p.invoice_id = si.invoice_id LEFT JOIN sales_quotations sq ON si.quotation_id = sq.id {whereSql}";
            var totalRecords = await connection.ExecuteScalarAsync<int>(countSql, parameters);
            return (data, totalRecords);
        }

        public async Task<PaymentResponse> GetPaymentByIdAsync(int id)
        {
            using var connection = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            var sql = @"SELECT p.id, p.user_created, p.date_created, p.user_updated, p.date_updated, p.invoice_id, p.payment_date, p.due_date, p.payment_method, p.amount_paid, p.payment_status, p.outstanding_amount, p.total_amount, sq.customer_name FROM payments p LEFT JOIN sales_invoices si ON p.invoice_id = si.invoice_id LEFT JOIN sales_quotations sq ON si.quotation_id = sq.id WHERE p.id = @Id";
            return await connection.QueryFirstOrDefaultAsync<ERP.API.Models.PaymentResponse>(sql, new { Id = id });
        }
        public async Task<PaymentResponse> CreatePaymentAsync(Payment payment)
        {
            using var connection = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            var sql = @"INSERT INTO payments (invoice_id, payment_date, due_date, payment_method, amount_paid, payment_status, outstanding_amount, total_amount, user_created, date_created) VALUES (@InvoiceId, @PaymentDate, @DueDate, @PaymentMethod, @AmountPaid, @PaymentStatus, @OutstandingAmount, @TotalAmount, @UserCreated, @DateCreated) RETURNING id;";
            var id = await connection.ExecuteScalarAsync<int>(sql, new {
                InvoiceId = payment.InvoiceId,
                PaymentDate = payment.PaymentDate,
                DueDate = payment.DueDate,
                PaymentMethod = payment.PaymentMethod,
                AmountPaid = payment.AmountPaid,
                PaymentStatus = payment.PaymentStatus,
                OutstandingAmount = payment.OutstandingAmount,
                TotalAmount = payment.TotalAmount,
                UserCreated = payment.UserCreated,
                DateCreated = payment.DateCreated
            });
            var customerNameSql = @"SELECT sq.customer_name FROM sales_invoices si LEFT JOIN sales_quotations sq ON si.quotation_id = sq.id WHERE si.invoice_id = @InvoiceId";
            var customerName = await connection.ExecuteScalarAsync<string>(customerNameSql, new { InvoiceId = payment.InvoiceId });
            var response = new ERP.API.Models.PaymentResponse
            {
                Id = id,
                UserCreated = payment.UserCreated,
                DateCreated = payment.DateCreated,
                UserUpdated = payment.UserUpdated,
                DateUpdated = payment.DateUpdated,
                InvoiceId = payment.InvoiceId,
                PaymentDate = payment.PaymentDate,
                DueDate = payment.DueDate,
                PaymentMethod = payment.PaymentMethod,
                AmountPaid = payment.AmountPaid,
                PaymentStatus = payment.PaymentStatus,
                OutstandingAmount = payment.OutstandingAmount,
                TotalAmount = payment.TotalAmount,
                CustomerName = customerName
            };
            return response;
        }

        public async Task<PaymentResponse> UpdatePaymentAsync(Payment payment)
        {
            using var connection = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            var sql = @"UPDATE payments SET user_updated = @UserUpdated, date_updated = @DateUpdated, invoice_id = @InvoiceId, payment_date = @PaymentDate, due_date = @DueDate, payment_method = @PaymentMethod, amount_paid = @AmountPaid, payment_status = @PaymentStatus, outstanding_amount = @OutstandingAmount, total_amount = @TotalAmount WHERE id = @Id";
            var affected = await connection.ExecuteAsync(sql, new {
                Id = payment.Id,
                UserUpdated = payment.UserUpdated,
                DateUpdated = payment.DateUpdated,
                InvoiceId = payment.InvoiceId,
                PaymentDate = payment.PaymentDate,
                DueDate = payment.DueDate,
                PaymentMethod = payment.PaymentMethod,
                AmountPaid = payment.AmountPaid,
                PaymentStatus = payment.PaymentStatus,
                OutstandingAmount = payment.OutstandingAmount,
                TotalAmount = payment.TotalAmount
            });
            if (affected == 0)
                return null;
            var customerNameSql = @"SELECT sq.customer_name FROM sales_invoices si LEFT JOIN sales_quotations sq ON si.quotation_id = sq.id WHERE si.invoice_id = @InvoiceId";
            var customerName = await connection.ExecuteScalarAsync<string>(customerNameSql, new { InvoiceId = payment.InvoiceId });
            var response = new ERP.API.Models.PaymentResponse
            {
                Id = payment.Id,
                UserCreated = payment.UserCreated,
                DateCreated = payment.DateCreated,
                UserUpdated = payment.UserUpdated,
                DateUpdated = payment.DateUpdated,
                InvoiceId = payment.InvoiceId,
                PaymentDate = payment.PaymentDate,
                DueDate = payment.DueDate,
                PaymentMethod = payment.PaymentMethod,
                AmountPaid = payment.AmountPaid,
                PaymentStatus = payment.PaymentStatus,
                OutstandingAmount = payment.OutstandingAmount,
                TotalAmount = payment.TotalAmount,
                CustomerName = customerName
            };
            return response;
        }
    }
}
