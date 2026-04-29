
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using ERP.API.Models;

namespace ERP.API.Services
{
    public interface ISalesQuotationGridService
    {
        Task<(IEnumerable<SalesQuotationGrid> Data, int TotalRecords)> GetSalesQuotationGridAsync(SalesQuotationGridRequest request);
    }

    public class SalesQuotationGridService : ISalesQuotationGridService
    {
        private readonly string? _connectionString;

        public SalesQuotationGridService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(_connectionString))
                throw new ArgumentException("DefaultConnection string is not configured", nameof(configuration));
        }

        public async Task<(IEnumerable<SalesQuotationGrid> Data, int TotalRecords)> GetSalesQuotationGridAsync(SalesQuotationGridRequest request)
        {
            using var connection = new NpgsqlConnection(_connectionString);

            // If PageSize is 0 or less, fetch all records by setting a very large page size
            int effectivePageSize = request.PageSize > 0 ? request.PageSize : 1000000;

            // Match opportunity grid conventions for request serialization
            var p_request = System.Text.Json.JsonSerializer.Serialize(new {
                SearchText = request.SearchText,
                CustomerNames = request.CustomerNames ?? new string[0],
                Statuses = request.Statuses ?? new string[0],
                QuotationIds = request.QuotationIds ?? new string[0],
                PageNumber = request.PageNumber,
                PageSize = effectivePageSize,
                OrderBy = string.IsNullOrWhiteSpace(request.OrderBy) ? "date_created" : request.OrderBy,
                OrderDirection = string.IsNullOrWhiteSpace(request.OrderDirection) ? "DESC" : request.OrderDirection,
                UserCreated = request.UserCreated
            });

            var parameters = new DynamicParameters();
            parameters.Add("p_request", p_request);

            var sql = @"SELECT * FROM fn_get_sales_quotations_grid(@p_request::json)";
            var result = await connection.QueryAsync<SalesQuotationGrid>(sql, parameters);
            var resultList = result.ToList();
            int totalRecords = resultList.FirstOrDefault()?.TotalRecords ?? 0;
            return (resultList, totalRecords);
        }
    }
}
