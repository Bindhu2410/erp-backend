using System.Collections.Generic;
using System.Threading.Tasks;
using ERP.API.Models.DTOs;
using Dapper;
using System.Linq;
using Microsoft.Extensions.Logging;
using ERP.API.Models;

namespace ERP.API.Services
{
    public class SalesRepDashboardService : ISalesRepDashboardService
    {
        private readonly string _connectionString;
        private readonly ILogger<SalesRepDashboardService> _logger;

        public SalesRepDashboardService(string connectionString, ILogger<SalesRepDashboardService> logger)
        {
            _connectionString = connectionString;
            _logger = logger;
        }

        private Npgsql.NpgsqlConnection CreateConnection() => new Npgsql.NpgsqlConnection(_connectionString);

        public async Task<SalesRepDashboardDto> GetDashboardDataAsync(int userId)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();

            // Total Opportunities, Closing Soon
            var summarySql = @"
                SELECT COUNT(*) AS TotalOpportunities,
                       COUNT(*) FILTER (WHERE so.expected_completion BETWEEN CURRENT_DATE AND CURRENT_DATE + INTERVAL '7 days') AS ClosingSoon
                FROM sales_opportunities so
                WHERE so.isactive = true AND so.user_created = @UserId;
            ";
            var summary = await connection.QueryFirstOrDefaultAsync(summarySql, new { UserId = userId });
            // Bar chart: Demo (status=Scheduled), Quotation (status=Negotiation, Submitted) for this user
            // Demo Scheduled
            var demoSql = @"
                SELECT COUNT(*) FROM sales_demos WHERE user_created = @UserId AND status = 'Demo Scheduled';
            ";
            var demoCount = await connection.ExecuteScalarAsync<long>(demoSql, new { UserId = userId });
   
            // Quotation Negotiation
            var quoteNegotiationSql = @"
                SELECT COUNT(*) FROM sales_quotations WHERE is_active = true AND user_created = @UserId AND status = 'Negotiation';
            ";
            var quoteNegotiationCount = await connection.ExecuteScalarAsync<long>(quoteNegotiationSql, new { UserId = userId });
           
            // Quotation Submitted
            var quoteSubmittedSql = @"
                SELECT COUNT(*) FROM sales_quotations WHERE is_active = true AND user_created = @UserId AND status = 'Submitted';
            ";
            var quoteSubmittedCount 
            = await connection.ExecuteScalarAsync<long>(quoteSubmittedSql, new { UserId = userId });
            var stages = new List<dynamic>
            {
                new { stage = "Demo Scheduled", count = demoCount },
                new { stage = "Quote Sent", count = quoteSubmittedCount },
                new { stage = "Negotiation", count = quoteNegotiationCount }
            };

            return new SalesRepDashboardDto
            {
                TotalOpportunities = summary?.totalopportunities != null ? (int)summary.totalopportunities : 0,
                ClosingSoon = summary?.closingsoon != null ? (int)summary.closingsoon : 0,
                Stages = stages.Select(s => new SalesRepStageDto { Stage = s.stage, Count = s.count != null ? (int)s.count : 0 }).ToList()
            };
        }
    }
}
