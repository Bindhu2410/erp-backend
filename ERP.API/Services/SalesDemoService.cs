using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using ERP.API.Models;
using ERP.API.Models.DTOs;
using ERP.API.Helpers;
using Microsoft.Extensions.Logging;

namespace ERP.API.Services
{
    public class SalesDemoService : ISalesDemoService
    {
        private readonly IDbConnection _connection;
        private readonly ILogger<SalesDemoService> _logger;

        public SalesDemoService(IDbConnection connection, ILogger<SalesDemoService> logger)
        {
            _connection = connection;
            _logger = logger;
        }


        public async Task<IEnumerable<SalesDemo>> GetDemosAsync()
        {
            const string sql = @"SELECT d.*, u.username as PresenterName 
                FROM sales_demos d
                LEFT JOIN users u ON (d.presenter_ids IS NOT NULL AND u.userid = ANY(d.presenter_ids))
                ORDER BY d.date_created DESC";

            return await _connection.QueryAsync<SalesDemo>(sql);
        }

        public async Task<SalesDemo?> GetDemoByIdAsync(int id)
        {
            try
            {
                const string sql = @"SELECT d.*, u.username as PresenterName
                    FROM sales_demos d
                    LEFT JOIN users u ON (d.presenter_ids IS NOT NULL AND u.userid = ANY(d.presenter_ids))
                    WHERE d.id = @Id";

                return await _connection.QueryFirstOrDefaultAsync<SalesDemo>(sql, new { Id = id });
            }
            catch (Exception ex)
            {
                // Log the error
                _logger.LogError(ex, "Error getting demo with ID {Id}: {Message}", id, ex.Message);
                throw;
            }
        }
        private static string GetSortColumn(string? requestedColumn)
        {
            return requestedColumn?.ToLower() switch
            {
                "id" => "id",
                "customername" => "customer_name",
                "demoname" => "demo_name",
                "demotype" => "demo_type",
                "status" => "status",
                "demodatetime" => "demo_date",
                "democontact" => "demo_contact",
                "demoapproach" => "demo_approach",
                "demooutcome" => "demo_outcome",
                "demofeedback" => "demo_feedback",
                "comments" => "comments",
                "opportunityid" => "opportunity_id",
                "presenterids" => "presenter_ids",
                "presentername" => "u.username",
                "datecreated" => "date_created",
                "dateupdated" => "date_updated",
                "addressid" => "address_id",
                "customerid" => "customer_id",
                "userid" => "user_id",
                _ => "date_created"
            };
        }

        public async Task<DemoCardsDto> GetDemoCardsByUserAsync(int userId)
        {
            try
            {
                const string sql = "SELECT * FROM sp_get_salesdemo_cards_count_by_user(@UserId);";
                var result = await _connection.QueryFirstOrDefaultAsync<DemoCardsDto>(sql, new { UserId = userId });
                return result ?? new DemoCardsDto();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting Demo cards data by user: {ex.Message}", ex);
            }
        }

        public async Task<int> CreateDemoAsync(SalesDemo demo)
        {
            if (demo == null)
            {
                throw new ArgumentNullException(nameof(demo));
            }

            if (string.IsNullOrEmpty(demo.DemoName))
            {
                throw new ArgumentException("Demo name is required");
            }

            if (demo.OpportunityId == null)
            {
                throw new ArgumentException("OpportunityId is required and must reference an existing opportunity.");
            }

            demo.DateCreated = DateTime.UtcNow;
            demo.UserCreated = 1; // Set to appropriate user ID

            const string sql = @"
                INSERT INTO sales_demos (
                    user_created, date_created, demo_contact, status, 
                    demo_name, demo_approach,
                    demo_outcome, demo_feedback, comments, opportunity_id, 
                    presenter_ids, leadid, contact_mobile_num
                )
                VALUES (
                    @UserCreated, @DateCreated, @DemoContact, @Status,
                    @DemoName, @DemoApproach,
                    @DemoOutcome, @DemoFeedback, @Comments, @OpportunityId,
                    @PresenterIds, @LeadId, @ContactMobileNum
                )
                RETURNING id";

            return await _connection.ExecuteScalarAsync<int>(sql, demo);
        }

        public async Task<bool> UpdateDemoAsync(int id, SalesDemo demo)
        {
            if (demo == null)
            {
                throw new ArgumentNullException(nameof(demo));
            }

            if (id != demo.Id)
            {
                throw new ArgumentException("ID mismatch");
            }

            if (string.IsNullOrEmpty(demo.DemoName))
            {
                throw new ArgumentException("Demo name is required");
            }

            demo.DateUpdated = DateTime.UtcNow;
            demo.UserUpdated = 1; // Set to appropriate user ID

            const string sql = @"
                UPDATE sales_demos SET
                    user_updated = @UserUpdated,
                    date_updated = @DateUpdated,
                    demo_contact = @DemoContact,
                    status = @Status,
                    demo_name = @DemoName,
                    demo_approach = @DemoApproach,
                    demo_outcome = @DemoOutcome,
                    demo_feedback = @DemoFeedback,
                    comments = @Comments,
                    opportunity_id = @OpportunityId,
                    presenter_ids = @PresenterIds,
                    leadid = @LeadId,
                    contact_mobile_num = @ContactMobileNum
                WHERE id = @Id";

            var rowsAffected = await _connection.ExecuteAsync(sql, demo);
            return rowsAffected > 0;
        }
        public async Task<bool> DeleteDemoAsync(int id)
        {
            const string sql = @"
                DELETE FROM sales_demos 
                WHERE id = @Id";

            var rowsAffected = await _connection.ExecuteAsync(sql, new { Id = id });
            return rowsAffected > 0;
        }

        public async Task<IEnumerable<SalesDemo>> GetDemosByOpportunityIdAsync(string? opportunityId)
        {
            const string sql = @"SELECT d.*, u.username as PresenterName FROM sales_demos d LEFT JOIN users u ON (d.presenter_ids IS NOT NULL AND u.userid = ANY(d.presenter_ids)) WHERE d.opportunity_id = @OpportunityId ORDER BY d.date_created DESC";
            return await _connection.QueryAsync<SalesDemo>(sql, new { OpportunityId = opportunityId });
        }

        public async Task<DemoCardsDto> GetDemoCardsAsync()
        {
            try
            {
                const string sql = "SELECT * FROM sp_get_demo_cards_count()";
                var cards = await _connection.QueryFirstOrDefaultAsync<DemoCardsDto>(sql);
                return cards ?? new DemoCardsDto();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting Demo cards data: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<SalesDemoWithItemsResponse>> GetDemosWithItemsAsync()
        {
            const string demoSql = @"SELECT d.id, d.user_created, d.date_created, d.user_updated, d.date_updated, d.user_id, d.demo_date, d.status, d.address_id, d.opportunity_id, d.demo_contact, d.demo_name, d.customer_name, d.demo_approach, d.demo_outcome, d.demo_feedback, d.comments, d.contact_mobile_num, d.leadid FROM sales_demos d ORDER BY d.date_created DESC";
            // Return all sales_demo_items rows for the demo (ordered by id DESC so latest rows are first)
            const string itemsSql = @"SELECT * FROM sales_demo_items WHERE demo_id = @DemoId ORDER BY id DESC";
            // Removed unused productSql

            var demos = (await _connection.QueryAsync<SalesDemoWithItemsResponse>(demoSql)).AsList();
            foreach (var demo in demos)
            {
                var items = (await _connection.QueryAsync<SalesItemResponse>(itemsSql, new { DemoId = demo.Id })).AsList();
                var mappedItems = new List<SalesDemoItemResponse>();
                foreach (var item in items)
                {
                    var mapped = new SalesDemoItemResponse
                    {
                        BomId = item.BomId ?? string.Empty,
                        BomName = item.BomName ?? string.Empty,
                        BomType = item.BomType ?? string.Empty,
                        BomChildItems = item.ChildItems != null ? item.ChildItems.ConvertAll(child => new BomChildItemDto
                        {
                            ChildItemId = child.ChildItemId,
                            Quantity = child.Quantity,
                            Make = child.Make ?? string.Empty,
                            Model = child.Model ?? string.Empty,
                            Product = child.Product ?? string.Empty,
                            CategoryName = child.CategoryName ?? string.Empty,
                            UnitPrice = child.UnitPrice ?? 0,
                            ItemName = child.ItemName ?? string.Empty,
                            ItemCode = child.ItemCode ?? string.Empty,
                            CatNo = child.CatNo ?? string.Empty,
                            UomName = child.UomName ?? string.Empty,
                            Hsn = child.Hsn ?? string.Empty
                        }) : new List<BomChildItemDto>(),
                        AccessoryItemIds = item.AccessoriesIds != null ? new List<int>(item.AccessoriesIds) : new List<int>(),
                        AccessoryItems = item.AccessoriesItems != null ? item.AccessoriesItems.ConvertAll(acc => new AccessoryItemResponseDto
                        {
                            Id = acc.Id,
                            Make = acc.Make ?? string.Empty,
                            Model = acc.Model ?? string.Empty,
                            Product = acc.Product ?? string.Empty,
                            ItemName = acc.ItemName ?? string.Empty,
                            ItemCode = acc.ItemCode ?? string.Empty,
                            UnitPrice = acc.UnitPrice ?? 0,
                            Hsn = acc.Hsn ?? string.Empty,
                            TaxPercentage = acc.TaxPercentage ?? 0,
                            CategoryName = acc.CategoryName ?? acc.Category ?? string.Empty
                        }) : new List<AccessoryItemResponseDto>(),
                        Quantity = item.Qty ?? 0
                    };
                    mappedItems.Add(mapped);
                }
                demo.Items = mappedItems;
            }
            return demos;
        }

        public async Task<SalesDemoWithItemsResponse?> GetDemoWithItemsByIdAsync(int id)
        {
            const string demoSql = @"SELECT d.id, d.user_created, d.date_created, d.user_updated, d.date_updated, d.user_id, d.demo_date, d.status, d.address_id, d.opportunity_id, d.demo_contact, d.demo_name, d.customer_name, d.demo_approach, d.demo_outcome, d.demo_feedback, d.comments, d.contact_mobile_num, d.leadid,
                a.address_line1, a.address_line2, a.city, a.state, a.zipcode, a.country
                FROM sales_demos d
                LEFT JOIN address a ON d.address_id = a.id
                WHERE d.id = @Id";
            // Return all sales_demo_items rows for the demo (ordered by id DESC so latest rows are first)
            const string itemsSql = @"SELECT * FROM sales_demo_items WHERE demo_id = @DemoId ORDER BY id DESC";
            // Removed unused productSql
            const string presenterIdsSql = @"SELECT presenter_id FROM sales_demo_presenters WHERE demo_id = @DemoId";

            var demo = await _connection.QueryFirstOrDefaultAsync<SalesDemoWithItemsResponse>(demoSql, new { Id = id });
            if (demo != null)
            {
                var items = (await _connection.QueryAsync<SalesItemResponse>(itemsSql, new { DemoId = demo.Id })).AsList();
                var mappedItems = new List<SalesDemoItemResponse>();
                foreach (var item in items)
                {
                    var mapped = new SalesDemoItemResponse
                    {
                        BomId = item.BomId,
                        BomName = item.BomName,
                        BomType = item.BomType,
                        BomChildItems = item.ChildItems != null ? item.ChildItems.ConvertAll(child => new BomChildItemDto
                        {
                            ChildItemId = child.ChildItemId,
                            Quantity = child.Quantity,
                            Make = child.Make,
                            Model = child.Model,
                            Product = child.Product,
                            CategoryName = child.CategoryName,
                            ValuationMethodName = child.ValuationMethodName,
                            InventoryMethodName = child.InventoryMethodName,
                            InventoryTypeName = child.InventoryTypeName,
                            UnitPrice = child.UnitPrice,
                            ItemName = child.ItemName,
                            ItemCode = child.ItemCode,
                            CatNo = child.CatNo,
                            UomName = child.UomName,
                            PurchaseRate = child.PurchaseRate,
                            SaleRate = child.SaleRate,
                            QuoteRate = child.QuoteRate,
                            Hsn = child.Hsn,
                            Tax = child.Tax
                        }) : new List<BomChildItemDto>(),
                        AccessoryItemIds = item.AccessoriesIds != null ? new List<int>(item.AccessoriesIds) : new List<int>(),
                        AccessoryItems = item.AccessoriesItems != null ? item.AccessoriesItems.ConvertAll(acc => new AccessoryItemResponseDto
                        {
                            Id = acc.Id,
                            Make = acc.Make ?? string.Empty,
                            Model = acc.Model ?? string.Empty,
                            Product = acc.Product ?? string.Empty,
                            ItemName = acc.ItemName ?? string.Empty,
                            ItemCode = acc.ItemCode ?? string.Empty,
                            UnitPrice = acc.UnitPrice ?? 0,
                            Hsn = acc.Hsn ?? string.Empty,
                            TaxPercentage = acc.TaxPercentage ?? 0,
                            CategoryName = acc.CategoryName ?? acc.Category ?? string.Empty
                        }) : new List<AccessoryItemResponseDto>(),
                        Quantity = item.Qty ?? 0
                    };
                    mappedItems.Add(mapped);
                }
                demo.Items = mappedItems;
    
                // Fetch presenter IDs from sales_demo_presenters
                var presenterIds = (await _connection.QueryAsync<int>(presenterIdsSql, new { DemoId = demo.Id })).AsList();
                demo.PresenterIds = presenterIds;

                // Fetch presenter names from users table
                if (presenterIds.Count > 0)
                {
                    // If you need presenter names, implement the query here. For now, assign empty list.
                    demo.PresenterNames = new List<string>();
                }
                // Try to map from joined address first
                bool hasJoinedAddress = (demo as dynamic)?.address_line1 != null ||
                    (demo as dynamic)?.address_line2 != null ||
                    (demo as dynamic)?.city != null ||
                    (demo as dynamic)?.state != null ||
                    (demo as dynamic)?.zipcode != null ||
                    (demo as dynamic)?.area != null ||
                    (demo as dynamic)?.block != null ||
                    (demo as dynamic)?.landmark != null ||
                    (demo as dynamic)?.is_default != null ||
                    (demo as dynamic)?.department != null ||
                    (demo as dynamic)?.opportunity_id != null;
                if (hasJoinedAddress)
                {
                    demo.CustomerAddress = new ERP.API.Models.DTOs.SalesAddressDto
                    {
                        DoorNo = (demo as dynamic)?.address_line1,
                        Street = (demo as dynamic)?.address_line2,
                        Area = (demo as dynamic)?.area,
                        Block = (demo as dynamic)?.block,
                        City = (demo as dynamic)?.city,
                        State = (demo as dynamic)?.state,
                        Pincode = (demo as dynamic)?.zipcode,
                        Landmark = (demo as dynamic)?.landmark,
                        IsDefault = (demo as dynamic)?.is_default,
                        Department = (demo as dynamic)?.department,
                        OpportunityId = (demo as dynamic)?.opportunity_id
                    };
                }
                else
                {
                    // Fallback: fetch address by leadId or opportunityId as in POST
                    string? leadIdToUse = demo.LeadId;
                    if (string.IsNullOrEmpty(leadIdToUse) && !string.IsNullOrEmpty(demo.OpportunityId))
                    {
                        var opp = await _connection.QueryFirstOrDefaultAsync<dynamic>(
                            "SELECT lead_id FROM sales_opportunities WHERE opportunity_id = @OpportunityId LIMIT 1",
                            new { OpportunityId = demo.OpportunityId });
                        if (opp != null && opp.lead_id != null)
                        {
                            var leadIdStr = opp.lead_id as string;
                            if (!string.IsNullOrEmpty(leadIdStr))
                            {
                                leadIdToUse = leadIdStr;
                            }
                        }
                    }
                    if (!string.IsNullOrEmpty(leadIdToUse))
                    {
                        var addresses = await _connection.QueryAsync<dynamic>(
                            "SELECT * FROM sales_addresses WHERE sales_lead_id = @LeadId ORDER BY is_default DESC, id LIMIT 1",
                            new { LeadId = leadIdToUse });
                        var address = addresses.FirstOrDefault();
                        if (address != null)
                        {
                            demo.CustomerAddress = new ERP.API.Models.DTOs.SalesAddressDto
                            {
                                DoorNo = address.door_no,
                                Street = address.street,
                                Area = address.area,
                                Block = address.block,
                                City = address.city,
                                State = address.state,
                                Pincode = address.pincode,
                                Landmark = address.landmark,
                                IsDefault = address.is_default,
                                Department = address.department,
                                OpportunityId = address.opportunity_id
                            };
                        }
                    }
                    // If still not found, try by opportunityId (as in POST)
                    if (demo.CustomerAddress == null && !string.IsNullOrEmpty(demo.OpportunityId))
                    {
                        var addresses = await _connection.QueryAsync<dynamic>(
                            "SELECT * FROM sales_addresses WHERE opportunity_id = @OpportunityId ORDER BY is_default DESC, id LIMIT 1",
                            new { OpportunityId = demo.OpportunityId });
                        var address = addresses.FirstOrDefault();
                        if (address != null)
                        {
                            demo.CustomerAddress = new ERP.API.Models.DTOs.SalesAddressDto
                            {
                                DoorNo = address.door_no,
                                Street = address.street,
                                Area = address.area,
                                Block = address.block,
                                City = address.city,
                                State = address.state,
                                Pincode = address.pincode,
                                Landmark = address.landmark,
                                IsDefault = address.is_default,
                                Department = address.department,
                                OpportunityId = address.opportunity_id
                            };
                        }
                    }
                    // Final fallback: try by customerName if still not found (robust like POST)
                    if (demo.CustomerAddress == null && !string.IsNullOrEmpty(demo.CustomerName))
                    {
                        var addresses = await _connection.QueryAsync<dynamic>(
                            "SELECT * FROM sales_addresses WHERE customer_name = @CustomerName ORDER BY is_default DESC, id LIMIT 1",
                            new { CustomerName = demo.CustomerName });
                        var address = addresses.FirstOrDefault();
                        if (address != null)
                        {
                            demo.CustomerAddress = new ERP.API.Models.DTOs.SalesAddressDto
                            {
                                DoorNo = address.door_no,
                                Street = address.street,
                                Area = address.area,
                                Block = address.block,
                                City = address.city,
                                State = address.state,
                                Pincode = address.pincode,
                                Landmark = address.landmark,
                                IsDefault = address.is_default,
                                Department = address.department,
                                OpportunityId = address.opportunity_id
                            };
                        }
                    }
                }
            }
            return demo;
        }

        public async Task<IEnumerable<SalesDemoWithItemsResponse>> GetDemosWithItemsByOpportunityIdAsync(string? opportunityId)
        {
            const string demoSql = @"SELECT d.id, d.user_created, d.date_created, d.user_updated, d.date_updated, d.user_id, d.demo_date, d.status, d.address_id, d.opportunity_id, d.demo_contact, d.demo_name, d.customer_name, d.demo_approach, d.demo_outcome, d.demo_feedback, d.comments, d.contact_mobile_num, d.leadid, d.presenter_ids FROM sales_demos d WHERE d.opportunity_id = @OpportunityId ORDER BY d.date_created DESC";
            const string itemsSql = @"SELECT * FROM sales_demo_items WHERE demo_id = @DemoId";

            var demos = (await _connection.QueryAsync<SalesDemoWithItemsResponse>(demoSql, new { OpportunityId = opportunityId })).AsList();
            foreach (var demo in demos)
            {
                var items = (await _connection.QueryAsync<SalesItemResponse>(itemsSql, new { DemoId = demo.Id })).AsList();
                var mappedItems = new List<SalesDemoItemResponse>();
                foreach (var item in items)
                {
                    var mapped = new SalesDemoItemResponse
                    {
                        BomId = item.BomId ?? string.Empty,
                        BomName = item.BomName ?? string.Empty,
                        BomType = item.BomType ?? string.Empty,
                        BomChildItems = item.ChildItems != null ? item.ChildItems.ConvertAll(child => new BomChildItemDto
                        {
                            ChildItemId = child.ChildItemId,
                            Quantity = child.Quantity,
                            Make = child.Make ?? string.Empty,
                            Model = child.Model ?? string.Empty,
                            Product = child.Product ?? string.Empty,
                            CategoryName = child.CategoryName ?? string.Empty,
                            UnitPrice = child.UnitPrice ?? 0,
                            ItemName = child.ItemName ?? string.Empty,
                            ItemCode = child.ItemCode ?? string.Empty,
                            CatNo = child.CatNo ?? string.Empty,
                            UomName = child.UomName ?? string.Empty,
                            Hsn = child.Hsn ?? string.Empty
                        }) : new List<BomChildItemDto>(),
                        AccessoryItemIds = item.AccessoriesIds != null ? new List<int>(item.AccessoriesIds) : new List<int>(),
                        AccessoryItems = item.AccessoriesItems != null ? item.AccessoriesItems.ConvertAll(acc => new AccessoryItemResponseDto
                        {
                            Id = acc.Id,
                            Make = acc.Make ?? string.Empty,
                            Model = acc.Model ?? string.Empty,
                            Product = acc.Product ?? string.Empty,
                            ItemName = acc.ItemName ?? string.Empty,
                            ItemCode = acc.ItemCode ?? string.Empty,
                            UnitPrice = acc.UnitPrice ?? 0,
                            Hsn = acc.Hsn ?? string.Empty,
                            TaxPercentage = acc.TaxPercentage ?? 0,
                            CategoryName = acc.CategoryName ?? acc.Category ?? string.Empty
                        }) : new List<AccessoryItemResponseDto>(),
                        Quantity = item.Qty ?? 0
                    };
                    mappedItems.Add(mapped);
                }
                demo.Items = mappedItems;
            }
            return demos;
        }
    }
}