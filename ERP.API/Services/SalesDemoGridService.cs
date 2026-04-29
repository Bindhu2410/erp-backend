using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using ERP.API.Models;
using System.Text.Json;

namespace ERP.API.Services
{
    public interface ISalesDemoGridService
    {
        Task<(IEnumerable<SalesDemoGrid> Data, int TotalRecords)> GetSalesDemoGridAsync(SalesDemoGridRequest request);
    }

    public class SalesDemoGridService : ISalesDemoGridService
    {
        private readonly string? _connectionString;

        public SalesDemoGridService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(_connectionString))
                throw new ArgumentException("DefaultConnection string is not configured", nameof(configuration));
        }

        public async Task<(IEnumerable<SalesDemoGrid> Data, int TotalRecords)> GetSalesDemoGridAsync(SalesDemoGridRequest request)
        {
            using var connection = new NpgsqlConnection(_connectionString);

            // Only 'ASC' (case-insensitive) is accepted, else 'DESC'
            string orderDirection = string.IsNullOrWhiteSpace(request.OrderDirection) ? "DESC" : (request.OrderDirection.ToUpper() == "ASC" ? "ASC" : "DESC");

            var parameters = new
            {
                p_search_text = request.SearchText,
                p_customer_names = request.CustomerNames ?? new string[0],
                p_statuses = request.Statuses ?? new string[0],
                p_demo_approaches = request.DemoApproaches ?? new string[0],
                p_demo_outcomes = request.DemoOutcomes ?? new string[0],
                p_selected_demo_ids = request.SelectedDemoIds ?? new int[0],
                p_user_created = request.UserCreated,
                p_page_number = request.PageNumber,
                p_page_size = request.PageSize,
                p_order_by = request.OrderBy,
                p_order_direction = orderDirection
            };

            var result = await connection.QueryAsync<SalesDemoGrid>(
                @"SELECT * FROM sales_demo_grid(
                    @p_user_created,
                    @p_search_text,
                    @p_customer_names,
                    @p_statuses,
                    @p_demo_approaches,
                    @p_demo_outcomes,
                    @p_selected_demo_ids,
                    @p_page_number,
                    @p_page_size,
                    @p_order_by,
                    @p_order_direction
                )",
                parameters
            );

            var demos = result.ToList();
            var totalRecords = demos.FirstOrDefault()?.TotalRecords ?? 0;

            return (demos, totalRecords);
        }
    }
}
