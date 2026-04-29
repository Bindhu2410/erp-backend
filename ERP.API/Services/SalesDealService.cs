using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using ERP.API.Models;
using ERP.API.Models.DTOs;
using Microsoft.Extensions.Configuration;

namespace ERP.API.Services
{    public class SalesDealService : BaseDataService<SalesDeal>, ISalesDealService
    {
        public SalesDealService(IConfiguration configuration) 
            : base(configuration.GetConnectionString("DefaultConnection") ?? throw new ArgumentNullException("DefaultConnection"), "sales_deals")
        {
        }

        public override async Task<SalesDeal?> GetByIdAsync(int? id)
        {
            try
            {
                using var connection = CreateConnection();
                
                var result = await connection.QueryFirstOrDefaultAsync<SalesDeal>(
                    @"SELECT * FROM sales_deals WHERE id = @Id",
                    new { Id = id });
                
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving deal {id}: {ex.Message}");
                throw;
            }
        }

        public override async Task<bool> UpdateAsync(SalesDeal deal)
        {
            try
            {
                using var connection = CreateConnection();

                var sql = @"
                    UPDATE sales_deals SET
                        user_updated = @UserUpdated,
                        date_updated = CURRENT_TIMESTAMP,
                        status = @Status,
                        deal_name = @DealName,
                        amount = @Amount, 
                        expected_revenue = @ExpectedRevenue,
                        start_date = @StartDate,
                        deal_for = @DealFor,
                        close_date = @CloseDate,
                        isactive = @IsActive,
                        comments = @Comments,
                        opportunity_id = @OpportunityId,
                        customer_id = @CustomerId,
                        sales_representative_id = @SalesRepresentativeId
                    WHERE id = @Id";

                var rowsAffected = await connection.ExecuteAsync(sql, deal);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update deal {deal.Id}: {ex.Message}", ex);
            }
        }

        protected override string GenerateInsertQuery()
        {
            return @"
                INSERT INTO sales_deals (
                    user_created, date_created, user_updated, date_updated,
                    status, deal_name, amount, expected_revenue, start_date,
                    deal_for, close_date, isactive, comments,
                    opportunity_id, customer_id, sales_representative_id
                ) VALUES (
                    @UserCreated, @DateCreated, @UserUpdated, @DateUpdated,
                    @Status, @DealName, @Amount, @ExpectedRevenue, @StartDate,
                    @DealFor, @CloseDate, @IsActive, @Comments,
                    @OpportunityId, @CustomerId, @SalesRepresentativeId
                ) RETURNING id";
        }

        protected override string GenerateUpdateQuery()
        {
            return @"
                UPDATE sales_deals SET
                    user_updated = @UserUpdated,
                    date_updated = CURRENT_TIMESTAMP,
                    status = @Status,
                    deal_name = @DealName,
                    amount = @Amount, 
                    expected_revenue = @ExpectedRevenue,
                    start_date = @StartDate,
                    deal_for = @DealFor,
                    close_date = @CloseDate,
                    isactive = @IsActive,
                    comments = @Comments,
                    opportunity_id = @OpportunityId,
                    customer_id = @CustomerId,
                    sales_representative_id = @SalesRepresentativeId
                WHERE id = @Id";
        }

        public override async Task<int> CreateAsync(SalesDeal deal)
        {
            try
            {
                using var connection = CreateConnection();

                var sql = @"
                    INSERT INTO sales_deals (
                        user_created, date_created, user_updated, date_updated,
                        status, deal_name, amount, expected_revenue, start_date,
                        deal_for, close_date, isactive, comments,
                        opportunity_id, customer_id, sales_representative_id
                    ) VALUES (
                        @UserCreated, @DateCreated, @UserUpdated, @DateUpdated,
                        @Status, @DealName, @Amount, @ExpectedRevenue, @StartDate,
                        @DealFor, @CloseDate, @IsActive, @Comments,
                        @OpportunityId, @CustomerId, @SalesRepresentativeId
                    ) RETURNING id";

                var id = await connection.ExecuteScalarAsync<int>(sql, deal);
                return id;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create deal: {ex.Message}", ex);
            }
        }        public override async Task<bool> DeleteAsync(int id)
        {
            try
            {
                using var connection = CreateConnection();

                var sql = "DELETE FROM sales_deals WHERE id = @Id";
                var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete deal {id}: {ex.Message}", ex);
            }
        }

        public async Task<SalesDealDetails?> GetDealDetailsByIdAsync(int id)
        {
            try
            {
                using var connection = CreateConnection();
                
                var query = @"
                    SELECT 
                        sd.*,
                        so.opportunity_name as OpportunityName,
                        sc.name as CustomerName,
                        se.name as SalesRepresentativeName
                    FROM sales_deals sd
                    LEFT JOIN sales_opportunities so ON sd.opportunity_id = so.id
                    LEFT JOIN sales_customers sc ON sd.customer_id = sc.id
                    LEFT JOIN sales_employees se ON sd.sales_representative_id = se.id
                    WHERE sd.id = @Id";

                var dealDetails = await connection.QueryFirstOrDefaultAsync<SalesDealDetails>(query, new { Id = id });
                return dealDetails;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving deal details {id}: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<SalesDeal>> GetByOpportunityIdAsync(int opportunityId)
        {
            using var connection = CreateConnection();
            
            return await connection.QueryAsync<SalesDeal>(
                "SELECT * FROM sales_deals WHERE opportunity_id = @OpportunityId",
                new { OpportunityId = opportunityId });
        }

        public async Task<IEnumerable<SalesDeal>> GetByCustomerIdAsync(int customerId)
        {
            using var connection = CreateConnection();
            
            return await connection.QueryAsync<SalesDeal>(
                "SELECT * FROM sales_deals WHERE customer_id = @CustomerId",
                new { CustomerId = customerId });
        }

        public async Task<IEnumerable<SalesDeal>> GetBySalesRepIdAsync(int salesRepId)
        {
            using var connection = CreateConnection();
            
            return await connection.QueryAsync<SalesDeal>(
                "SELECT * FROM sales_deals WHERE sales_representative_id = @SalesRepId",
                new { SalesRepId = salesRepId });
        }

        public async Task<int> CreateDealAsync(SalesDeal deal)
        {
            try
            {
                return await CreateAsync(deal);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create deal: {ex.Message}", ex);
            }
        }

        public async Task<SalesDeal?> GetDealByIdAsync(int id)
        {
            try
            {
                return await GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to retrieve deal {id}: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<SalesDeal>> GetAllDealsAsync()
        {
            try
            {
                return await GetAllAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to retrieve all deals: {ex.Message}", ex);
            }
        }

        public async Task<bool> UpdateDealAsync(int id, SalesDeal deal)
        {
            try
            {
                if (deal.Id != id)
                {
                    throw new ArgumentException($"Deal ID mismatch. URL ID: {id}, Deal ID: {deal.Id}");
                }

                return await UpdateAsync(deal);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update deal {id}: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteDealAsync(int id, int userUpdated)
        {
            try
            {
                using var connection = CreateConnection();
                
                // Soft delete: update isactive flag and user info
                var sql = @"
                    UPDATE sales_deals 
                    SET isactive = false,
                        user_updated = @UserUpdated,
                        date_updated = CURRENT_TIMESTAMP
                    WHERE id = @Id";

                var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id, UserUpdated = userUpdated });
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete deal {id}: {ex.Message}", ex);
            }
        }

        public async Task<PagedResult<SalesDealDto>> GetFilteredDealsAsync(DealFilterCriteria filterCriteria, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                using var connection = CreateConnection();

                var sql = @"
                    SELECT 
                        d.*,
                        so.opportunity_name,
                        sc.name as customer_name,
                        se.name as sales_representative_name,
                        st.name as territory_name
                    FROM sales_deals d
                    LEFT JOIN sales_opportunities so ON d.opportunity_id = so.id
                    LEFT JOIN sales_customers sc ON d.customer_id = sc.id
                    LEFT JOIN sales_employees se ON d.sales_representative_id = se.id
                    LEFT JOIN sales_territories st ON d.territory_id = st.id
                    WHERE d.isactive = true";

                var parameters = new DynamicParameters();

                if (filterCriteria.TerritoryId.HasValue)
                {
                    sql += " AND d.territory_id = @TerritoryId";
                    parameters.Add("@TerritoryId", filterCriteria.TerritoryId);
                }

                if (filterCriteria.ZoneId.HasValue)
                {
                    sql += " AND st.zone_id = @ZoneId";
                    parameters.Add("@ZoneId", filterCriteria.ZoneId);
                }

                if (filterCriteria.DivisionId.HasValue)
                {
                    sql += " AND d.division_id = @DivisionId";
                    parameters.Add("@DivisionId", filterCriteria.DivisionId);
                }

                if (!string.IsNullOrEmpty(filterCriteria.CustomerName))
                {
                    sql += " AND (sc.name ILIKE @CustomerName OR d.deal_name ILIKE @CustomerName)";
                    parameters.Add("@CustomerName", $"%{filterCriteria.CustomerName}%");
                }

                if (!string.IsNullOrEmpty(filterCriteria.Status))
                {
                    sql += " AND d.status = @Status";
                    parameters.Add("@Status", filterCriteria.Status);
                }

                if (!string.IsNullOrEmpty(filterCriteria.LeadType))
                {
                    sql += " AND d.deal_for = @LeadType";
                    parameters.Add("@LeadType", filterCriteria.LeadType);
                }

                if (!string.IsNullOrEmpty(filterCriteria.SearchTerm))
                {
                    sql += @" AND (
                        d.deal_name ILIKE @SearchTerm OR
                        d.comments ILIKE @SearchTerm OR
                        sc.name ILIKE @SearchTerm OR
                        se.name ILIKE @SearchTerm
                    )";
                    parameters.Add("@SearchTerm", $"%{filterCriteria.SearchTerm}%");
                }

                // Count total records
                var countSql = $"SELECT COUNT(*) FROM ({sql}) as filtered_deals";
                var totalRecords = await connection.ExecuteScalarAsync<int>(countSql, parameters);

                // Add sorting
                var sortColumn = !string.IsNullOrEmpty(filterCriteria.SortColumn) 
                    ? filterCriteria.SortColumn.ToLower() 
                    : "id";
                var sortDirection = !string.IsNullOrEmpty(filterCriteria.SortDirection) 
                    && filterCriteria.SortDirection.ToUpper() == "DESC" 
                    ? "DESC" 
                    : "ASC";
                sql += $" ORDER BY d.{sortColumn} {sortDirection}";

                // Add pagination
                sql += " LIMIT @PageSize OFFSET @Offset";
                parameters.Add("@PageSize", pageSize);
                parameters.Add("@Offset", (pageNumber - 1) * pageSize);                // Execute the query and ensure we have a List<SalesDealDto>
                var deals = (await connection.QueryAsync<SalesDealDto>(sql, parameters)).ToList();

                return new PagedResult<SalesDealDto>
                {
                    Items = deals,
                    TotalCount = totalRecords,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to retrieve filtered deals: {ex.Message}", ex);
            }
        }

        public async Task<DealSummary> GetDealsSummaryAsync()
        {
            try
            {
                using var connection = CreateConnection();

                var sql = @"
                    SELECT
                        COUNT(*) FILTER (WHERE status = 'Current') as current_deals,
                        COUNT(*) FILTER (WHERE status = 'Won') as won_deals,
                        COUNT(*) FILTER (WHERE status = 'Lost') as lost_deals,
                        COUNT(*) FILTER (WHERE status = 'On Hold') as on_hold_deals
                    FROM sales_deals
                    WHERE isactive = true";

                var summary = await connection.QueryFirstOrDefaultAsync<DealSummary>(sql);
                return summary ?? new DealSummary();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to retrieve deals summary: {ex.Message}", ex);
            }
        }
    }
}