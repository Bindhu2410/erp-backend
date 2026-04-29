using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Dapper;
using Newtonsoft.Json;
using ERP.API.Models;
using ERP.API.Models.DTOs;

namespace ERP.API.Services
{
    public class SalesOpportunityService : ISalesOpportunityService
    {
        private readonly ILogger<SalesOpportunityService> _logger;
        private readonly string _connectionString;

        public SalesOpportunityService(string connectionString, ILogger<SalesOpportunityService> logger)
        {
            _connectionString = connectionString;
            _logger = logger;
        }

        private Npgsql.NpgsqlConnection CreateConnection()
        {
            return new Npgsql.NpgsqlConnection(_connectionString);
        }

        // Placeholder for IdGenerator. Replace with your actual implementation.
        private static class IdGenerator
        {
            public static async Task<string> GenerateOpportunityId(Npgsql.NpgsqlConnection connection)
            {
                // TODO: Implement your ID generation logic here
                await Task.CompletedTask;
                return Guid.NewGuid().ToString();
            }
        }
        public async Task<IEnumerable<SalesOpportunityDto>> GetOpportunitiesAsync()
        {
            try
            {
                const string sql = @"
                    SELECT * FROM sales_opportunities 
                    WHERE isactive = true 
                    ORDER BY date_created DESC";
                using var connection = CreateConnection();
                var opportunities = (await connection.QueryAsync<SalesOpportunityDto>(sql))?.ToList() ?? new List<SalesOpportunityDto>();
                _logger.LogInformation("Found {Count} opportunities", opportunities.Count);
                return opportunities;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching opportunities: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<SalesOpportunityDto?> GetOpportunityByIdAsync(string opportunityId)
        {
            if (string.IsNullOrEmpty(opportunityId))
                throw new ArgumentException("OpportunityId cannot be null or empty", nameof(opportunityId));
            const string sql = @"SELECT * FROM sales_opportunities WHERE opportunity_id = @OpportunityId AND isactive = true";
            using var connection = CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<SalesOpportunityDto>(sql, new { OpportunityId = opportunityId });
        }

        public async Task<SalesOpportunityDto?> GetByIdAsync(string opportunityId)
        {
            return await GetOpportunityByIdAsync(opportunityId);
        }

        public async Task<IEnumerable<SalesOpportunityDto>> GetOpportunitiesByLeadIdAsync(int leadId)
        {
            try
            {
                const string sql = "SELECT * FROM fn_getopportunitiesbyleadid(@LeadId)";
                using var connection = CreateConnection();
                var opportunities = await connection.QueryAsync<SalesOpportunityDto>(sql, new { LeadId = leadId });
                return opportunities;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IEnumerable<SalesOpportunityDto>> GetOpportunitiesByLeadIdAsync(string leadId)
        {
            try
            {
                const string sql = "SELECT * FROM fn_getopportunitiesbyleadid(@LeadId)";
                using var connection = CreateConnection();
                var opportunities = await connection.QueryAsync<SalesOpportunityDto>(sql, new { LeadId = leadId });
                return opportunities;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int> CreateOpportunityAsync(SalesOpportunityDto opportunity)
        {
            using var connection = CreateConnection();
            // Do NOT set or pass Id, let DB sequence handle it
            opportunity.OpportunityId = await IdGenerator.GenerateOpportunityId(connection);
            opportunity.IsActive = true;

            const string sql = @"
                INSERT INTO sales_opportunities (
                    status, expected_completion, opportunity_type, opportunity_for,
                    customer_id, customer_name, customer_type, opportunity_name,
                    opportunity_id, comments, isactive, lead_id, sales_representative_id,
                    contact_name, contact_mobile_no, user_created, date_created)
                VALUES (
                    @Status, @ExpectedCompletion, @OpportunityType, @OpportunityFor,
                    @CustomerId, @CustomerName, @CustomerType, @OpportunityName,
                    @OpportunityId, @Comments, @IsActive, @LeadId, @SalesRepresentativeId,
                    @ContactName, @ContactMobileNo, 1, CURRENT_TIMESTAMP)
                RETURNING id";

            var param = new {
                opportunity.Status,
                opportunity.ExpectedCompletion,
                opportunity.OpportunityType,
                opportunity.OpportunityFor,
                opportunity.CustomerId,
                opportunity.CustomerName,
                opportunity.CustomerType,
                opportunity.OpportunityName,
                opportunity.OpportunityId,
                opportunity.Comments,
                opportunity.IsActive,
                opportunity.LeadId,
                opportunity.SalesRepresentativeId,
                opportunity.ContactName,
                opportunity.ContactMobileNo
            };

            return await connection.ExecuteScalarAsync<int>(sql, param);
        }

        public async Task<bool> UpdateOpportunityWithItemsAsync(string opportunityId, SalesOpportunityWithItemsRequest request)
        {
            if (string.IsNullOrEmpty(opportunityId))
                throw new ArgumentException("OpportunityId cannot be null or empty", nameof(opportunityId));

            // Update the opportunity
            // This method needs to be refactored to use BomId, AccessoryItemIds, and Quantity from request.
            // Remove references to Opportunity and Items.
            // TODO: Implement update logic for new request structure if needed.
            return true;
        }

        public async Task<bool> UpdateOpportunityAsync(int id, SalesOpportunityDto opportunity)
        {
            if (id != opportunity.Id)
                throw new ArgumentException($"Path parameter id ({id}) does not match opportunity.Id ({opportunity.Id})", nameof(id));

            if (opportunity.Id <= 0)
                throw new ArgumentException("Invalid opportunity Id", nameof(opportunity.Id));

            const string sql = @"
                UPDATE sales_opportunities SET
                    status = @Status,
                    expected_completion = @ExpectedCompletion,
                    opportunity_type = @OpportunityType,
                    opportunity_for = @OpportunityFor,
                    customer_id = @CustomerId,
                    customer_name = @CustomerName,
                    customer_type = @CustomerType,
                    opportunity_name = @OpportunityName,
                    opportunity_id = @OpportunityId,
                    comments = @Comments,
                    lead_id = @LeadId,
                    sales_representative_id = @SalesRepresentativeId,
                    contact_name = @ContactName,
                    contact_mobile_no = @ContactMobileNo,
                    user_updated = 1,
                    date_updated = CURRENT_TIMESTAMP
                WHERE id = @Id AND isactive = true";

            using var connection = CreateConnection();
            var rowsAffected = await connection.ExecuteAsync(sql, opportunity);
            return rowsAffected > 0;
        }

        public async Task<bool> UpdateOpportunityAsync(string opportunityId, SalesOpportunityDto opportunity)
        {
            if (opportunityId != opportunity.OpportunityId)
                throw new ArgumentException($"Path parameter opportunityId ({opportunityId}) does not match opportunity.OpportunityId ({opportunity.OpportunityId})", nameof(opportunityId));

            const string sql = @"
                UPDATE sales_opportunities SET
                    status = @Status,
                    expected_completion = @ExpectedCompletion,
                    opportunity_type = @OpportunityType,
                    opportunity_for = @OpportunityFor,
                    customer_id = @CustomerId,
                    customer_name = @CustomerName,
                    customer_type = @CustomerType,
                    opportunity_name = @OpportunityName,
                    opportunity_id = @OpportunityId,
                    comments = @Comments,
                    lead_id = @LeadId,
                    sales_representative_id = @SalesRepresentativeId,
                    contact_name = @ContactName,
                    contact_mobile_no = @ContactMobileNo,
                    user_updated = @UserUpdated,
                    date_updated = @DateUpdated
                WHERE opportunity_id = @OpportunityId AND isactive = true";

            using var connection = CreateConnection();
            var rowsAffected = await connection.ExecuteAsync(sql, new {
                Status = opportunity.Status,
                ExpectedCompletion = opportunity.ExpectedCompletion,
                OpportunityType = opportunity.OpportunityType,
                OpportunityFor = opportunity.OpportunityFor,
                CustomerId = opportunity.CustomerId,
                CustomerName = opportunity.CustomerName,
                CustomerType = opportunity.CustomerType,
                OpportunityName = opportunity.OpportunityName,
                OpportunityId = opportunity.OpportunityId,
                Comments = opportunity.Comments,
                LeadId = opportunity.LeadId,
                SalesRepresentativeId = opportunity.SalesRepresentativeId,
                ContactName = opportunity.ContactName,
                ContactMobileNo = opportunity.ContactMobileNo,
                UserUpdated = opportunity.UserUpdated ?? 1,
                DateUpdated = DateTime.UtcNow
            });
            return rowsAffected > 0;
        }
    
        public async Task<bool> DeleteOpportunityAsync(int id)
        {
            try
            {
                const string sql = @"
                    UPDATE sales_opportunities SET 
                        isactive = false,
                        user_updated = 1,
                        date_updated = CURRENT_TIMESTAMP
                    WHERE id = @Id AND isactive = true";

                using var connection = CreateConnection();
                var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });

                _logger.LogInformation(rowsAffected > 0
                    ? "Deleted opportunity: {Id}"
                    : "No opportunity found to delete: {Id}",
                    id);

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting opportunity {Id}: {Message}", id, ex.Message);
                throw;
            }
        }
        public async Task<(IEnumerable<SalesOpportunityGridResult> Results, int TotalRecords)> GetOpportunitiesGridAsync(
            string? searchText = null,
            string[]? customerNames = null,
            string[]? territories = null, // Not used in SQL, but kept for compatibility
            string[]? statuses = null,
            string[]? stages = null, // Not used in SQL, but kept for compatibility
            string[]? opportunityTypes = null,
            int pageNumber = 1,
            int pageSize = 10,
            string? orderBy = "date_created",
            string? orderDirection = "DESC")
        {
            try
            {
                // Build the request object for the SQL function
                var requestObj = new {
                    SearchText = searchText,
                    CustomerNames = customerNames,
                    Statuses = statuses,
                    OpportunityTypes = opportunityTypes,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    OrderBy = orderBy,
                    OrderDirection = orderDirection
                };
                var jsonRequest = JsonConvert.SerializeObject(requestObj);
                using var connection = CreateConnection();
                var results = await connection.QueryAsync<SalesOpportunityGridResult>(
                    "SELECT * FROM fn_get_sales_opportunities_grid(@Request::jsonb)",
                    new { Request = jsonRequest }
                );
                var resultsList = results.ToList();
                var totalRecords = resultsList.FirstOrDefault()?.TotalRecords ?? 0;
                _logger.LogInformation("Retrieved {Count} opportunities out of {Total} total records", resultsList.Count, totalRecords);
                return (resultsList, totalRecords);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetOpportunitiesGridAsync: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<(IEnumerable<SalesOpportunityGridResult> Results, int TotalRecords)> GetOpportunitiesGridByUserAsync(
            int currentUserId,
            string? searchText = null,
            string[]? customerNames = null,
            string[]? territories = null,
            string[]? statuses = null,
            string[]? stages = null,
            string[]? opportunityTypes = null,
            int pageNumber = 1,
            int pageSize = 10,
            string? orderBy = "date_created",
            string? orderDirection = "DESC")
        {
            try
            {
                // Build the request object for the SQL function, including user filter
                var requestObj = new {
                    SearchText = searchText,
                    CustomerNames = customerNames,
                    Statuses = statuses,
                    OpportunityTypes = opportunityTypes,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    OrderBy = orderBy,
                    OrderDirection = orderDirection,
                    UserCreated = currentUserId
                };
                var jsonRequest = JsonConvert.SerializeObject(requestObj);
                using var connection = CreateConnection();
                var results = await connection.QueryAsync<SalesOpportunityGridResult>(
                    "SELECT * FROM fn_get_sales_opportunities_grid_by_user(@Request::jsonb)",
                    new { Request = jsonRequest }
                );
                var resultsList = results.ToList();
                var totalRecords = resultsList.FirstOrDefault()?.TotalRecords ?? 0;
                _logger.LogInformation("Retrieved {Count} user-specific opportunities out of {Total} total records for user {UserId}", 
                    resultsList.Count, totalRecords, currentUserId);
                return (resultsList, totalRecords);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetOpportunitiesGridByUserAsync for user {UserId}: {Message}", currentUserId, ex.Message);
                throw;
            }
        }

        public async Task<(IEnumerable<SalesOpportunityGridResult> Results, int TotalRecords)> GetOpportunitiesGridByUserSPAsync(string jsonRequest)
        {
            using var connection = CreateConnection();
            var results = await connection.QueryAsync<SalesOpportunityGridResult>(
                "SELECT * FROM fn_get_sales_opportunities_grid_by_user(@Request::jsonb)",
                new { Request = jsonRequest }
            );
            var resultsList = results.ToList();
            var totalRecords = resultsList.FirstOrDefault()?.TotalRecords ?? 0;
            return (resultsList, totalRecords);
        }


        /// <summary>
        /// Returns the first opportunity and its items in the same structure as the POST response.
        /// </summary>
        public async Task<object?> GetOpportunitiesGridResponseAsync(
            string? searchText = null,
            string[]? customerNames = null,
            string[]? territories = null,
            string[]? statuses = null,
            string[]? stages = null,
            string[]? opportunityTypes = null,
            int pageNumber = 1,
            int pageSize = 10,
            string? orderBy = "date_created",
            string? orderDirection = "DESC")
        {
            // Get the grid results
            var (resultsList, totalRecords) = await GetOpportunitiesGridAsync(
                searchText, customerNames, territories, statuses, stages, opportunityTypes, pageNumber, pageSize, orderBy, orderDirection);

            var first = resultsList.FirstOrDefault();
            if (first == null)
                return null;

            // Fetch the full opportunity entity (for all fields)
            var opportunity = await GetOpportunityByIdAsync(first.OpportunityId);
            if (opportunity == null)
                return null;

            // Fetch items for this opportunity (replace with your actual item fetch logic)
            // This assumes you have a method to get items by opportunityId
            List<dynamic> items = new List<dynamic>();
            try
            {
                var mainItems = await GetItemsByOpportunityIdAsync(opportunity.OpportunityId);
                foreach (var item in mainItems)
                {
                    // Recursive mapping for includedChildItems
                    async Task<List<object>> MapChildItemsAsync(List<SalesItemDto> childItems)
                    {
                        var result = new List<object>();
                        foreach (var child in childItems)
                        {
                            var childIncluded = await GetChildItemsAsync(child.Id);
                            var childAccessories = await GetAccessoriesItemsAsync(child.Id);
                            var mappedIncludedChildItems = await MapChildItemsAsync(childIncluded);
                            var mappedAccessoriesItems = await MapChildItemsAsync(childAccessories);
                            result.Add(new {
                                includedChildItemIds = (List<int>?)null,
                                accessoriesIds = (List<int>?)null,
                                id = child.Id,
                                userCreated = child.UserCreated,
                                dateCreated = child.DateCreated,
                                userUpdated = child.UserUpdated,
                                dateUpdated = child.DateUpdated,
                                qty = child.Quantity,
                                amount = child.Total,
                                isActive = child.IsActive,
                                itemId = child.Item_Id,
                                stage = child.Stage,
                                stageItemId = child.Stage_Item_Id,
                                make = child.Make,
                                model = child.Model,
                                product = child.Product,
                                category = child.Category,
                                itemName = child.ItemName,
                                itemCode = child.ItemCode,
                                unitPrice = child.UnitPrice,
                                hsn = child.HSN,
                                taxPercentage = child.TaxPercentage,
                                uom = child.UOM,
                                parentId = child.ParentId,
                                parentItem = child.ParentItem,
                                referencedBy = child.ReferencedBy,
                                includedChildItems = mappedIncludedChildItems ?? new List<object>(),
                                accessoriesItems = mappedAccessoriesItems ?? new List<object>()
                            });
                        }
                        return result;
                    }


                    var includedChildItems = await GetChildItemsAsync(item.Id);
                    _logger.LogInformation("Included child items for item {ItemId}: {Count}", item.Id, includedChildItems.Count);

                    var accessoriesItems = await GetAccessoriesItemsAsync(item.Id);
                    _logger.LogInformation("Accessories items for item {ItemId}: {Count}", item.Id, accessoriesItems.Count);

                    var mappedIncludedChildItems = await MapChildItemsAsync(includedChildItems);
                    var mappedAccessoriesItems = await MapChildItemsAsync(accessoriesItems);
                    items.Add(new {
                        includedChildItemIds = (List<int>?)null,
                        accessoriesIds = (List<int>?)null,
                        id = item.Id,
                        userCreated = item.UserCreated,
                        dateCreated = item.DateCreated,
                        userUpdated = item.UserUpdated,
                        dateUpdated = item.DateUpdated,
                        qty = item.Quantity,
                        amount = item.Total,
                        isActive = item.IsActive,
                        itemId = item.Item_Id,
                        stage = item.Stage,
                        stageItemId = item.Stage_Item_Id,
                        make = item.Make,
                        model = item.Model,
                        product = item.Product,
                        category = item.Category,
                        itemName = item.ItemName,
                        itemCode = item.ItemCode,
                        unitPrice = item.UnitPrice,
                        hsn = item.HSN,
                        taxPercentage = item.TaxPercentage,
                        uom = item.UOM,
                        parentId = item.ParentId,
                        parentItem = item.ParentItem,
                        referencedBy = item.ReferencedBy,
                        includedChildItems = mappedIncludedChildItems != null ? mappedIncludedChildItems : new List<object>(),
                        accessoriesItems = mappedAccessoriesItems != null ? mappedAccessoriesItems : new List<object>()
                    });
                }
            }
            catch { }

            return new
            {
                opportunity,
                items
            };
        }

        // Fetch child items for a given itemId
        private async Task<List<SalesItemDto>> GetChildItemsAsync(int parentId)
        {
            const string sql = @"SELECT *, make as Make, model as Model, product as Product, hsn as HSN, tax_percentage as TaxPercentage, user_created as UserCreated, date_created as DateCreated, user_updated as UserUpdated, date_updated as DateUpdated, parent_id as ParentId, referenced_by as ReferencedBy FROM sales_products WHERE parent_id = @ParentId AND isactive = true";
            using var connection = CreateConnection();
            var items = await connection.QueryAsync<SalesItemDto>(sql, new { ParentId = parentId });
            return items?.ToList() ?? new List<SalesItemDto>();
        }

        // Fetch accessories items for a given itemId
        private async Task<List<SalesItemDto>> GetAccessoriesItemsAsync(int parentId)
        {
            const string sql = @"SELECT *, make as Make, model as Model, product as Product, hsn as HSN, tax_percentage as TaxPercentage, user_created as UserCreated, date_created as DateCreated, user_updated as UserUpdated, date_updated as DateUpdated, parent_id as ParentId, referenced_by as ReferencedBy FROM sales_products WHERE referenced_by = @ParentId AND isactive = true";
            using var connection = CreateConnection();
            var items = await connection.QueryAsync<SalesItemDto>(sql, new { ParentId = parentId });
            return items?.ToList() ?? new List<SalesItemDto>();
        }

        // Fetch items for an opportunity by OpportunityId (assumes sales_products table has opportunity_id column)
        public async Task<IEnumerable<SalesItemDto>> GetItemsByOpportunityIdAsync(string? opportunityId)
        {
            if (string.IsNullOrEmpty(opportunityId))
                return Enumerable.Empty<SalesItemDto>();

            const string sql = @"SELECT *, make as Make, model as Model, product as Product, hsn as HSN, tax_percentage as TaxPercentage, user_created as UserCreated, date_created as DateCreated, user_updated as UserUpdated, date_updated as DateUpdated, parent_id as ParentId, referenced_by as ReferencedBy FROM sales_products WHERE opportunity_id = @OpportunityId AND isactive = true";
            using var connection = CreateConnection();
            var items = await connection.QueryAsync<SalesItemDto>(sql, new { OpportunityId = opportunityId });
            return items ?? Enumerable.Empty<SalesItemDto>();
        }


        // ...existing code...

        public async Task<IEnumerable<OpportunityCardDto>> GetOpportunityCardsAsync()
        {
            // TODO: Replace with real DB logic
            return await Task.FromResult(new List<OpportunityCardDto>
            {
                new OpportunityCardDto { Status = "Open", Count = 5, TotalValue = 100000 },
                new OpportunityCardDto { Status = "Closed", Count = 2, TotalValue = 50000 }
            });
        }

        public async Task<OpportunityCardsDto> GetOpportunityCardsStatusAsync()
        {
            using var connection = CreateConnection();
            var sql = @"SELECT
                COUNT(*) FILTER (WHERE so.isactive = true AND so.status = 'Identified') AS Identified,
                COUNT(*) FILTER (WHERE so.isactive = true AND so.status = 'Solution Presentation') AS SolutionPresentation,
                COUNT(DISTINCT so.id) FILTER (WHERE so.isactive = true AND sq.opportunity_id IS NOT NULL) AS Proposal,
                COUNT(DISTINCT so.id) FILTER (WHERE so.isactive = true AND sq.opportunity_id IS NOT NULL AND LOWER(sq.status) = 'negotiation') AS Negotiation,
                COUNT(*) FILTER (WHERE so.isactive = true AND so.status = 'Closed Won') AS ClosedWon
            FROM public.sales_opportunities so
            LEFT JOIN public.sales_quotations sq ON so.opportunity_id = sq.opportunity_id AND sq.is_active = true;";
            var result = await connection.QueryFirstOrDefaultAsync<OpportunityCardsDto>(sql);
            return result ?? new OpportunityCardsDto();
        }

        public async Task<OpportunityCardsDto> GetOpportunityCardsStatusByUserAsync(int currentUserId)
        {
            try
            {
                using var connection = CreateConnection();
                // Use the simple version that only counts exact status matches
                var sql = "SELECT * FROM sp_get_opportunity_cards_count_by_user_simple(@UserId)";
                var result = await connection.QueryFirstOrDefaultAsync<OpportunityCardsDto>(sql, new { UserId = currentUserId });
                
                _logger.LogInformation("Retrieved opportunity cards data for user {UserId}", currentUserId);
                return result ?? new OpportunityCardsDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting opportunity cards data for user {UserId}: {Message}", currentUserId, ex.Message);
                throw;
            }
        }

        public string GetConnectionString()
        {
            return _connectionString;
        }
    }
}