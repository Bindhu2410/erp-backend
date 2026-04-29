using System.Collections.Generic;
using System.Threading.Tasks;
using ERP.API.Models;
using Dapper;
using Npgsql;
using Microsoft.Extensions.Configuration;

namespace ERP.API.Services
{
    public class DeliveryService : IDeliveryService
    {
        private readonly string _connectionString;

        public DeliveryService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<IEnumerable<Delivery>> GetAllAsync()
        {
            using var connection = new NpgsqlConnection(_connectionString);
            var sql = "SELECT * FROM deliveries"; // invoice_id is included by *
            return await connection.QueryAsync<Delivery>(sql);
        }

        public async Task<Delivery> GetByIdAsync(int id)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            var sql = "SELECT * FROM deliveries WHERE id = @Id"; // invoice_id is included by *
            var delivery = await connection.QueryFirstOrDefaultAsync<Delivery>(sql, new { Id = id });
            if (delivery == null)
                return null;

            // Fetch delivery items and enrich as in CreateAsync
            var dbItems = (await connection.QueryAsync<DeliveryItem>(
                "SELECT product_id AS ItemId, user_created AS UserCreated, date_created AS DateCreated, user_updated AS UserUpdated, date_updated AS DateUpdated, qty AS Qty, amount AS Amount, is_active AS IsActive, unit_price AS UnitPrice, included_child_item_ids AS IncludedChildItemIds, accessories_ids AS AccessoriesIds FROM delivery_items WHERE delivery_id = @DeliveryId ORDER BY item_id ASC",
                new { DeliveryId = delivery.DeliveryId }
            )).AsList();

            var itemIds = dbItems.Select(i => i.ItemId).Distinct().ToArray();
            var itemDetails = (await connection.QueryAsync<ItemMaster>(
                "SELECT * FROM item_master WHERE id = ANY(@Ids)", new { Ids = itemIds })).ToDictionary(x => x.Id);

            var enrichedItems = new List<SalesItemResponse>();
            foreach (var item in dbItems)
            {
                var si = new SalesItemResponse
                {
                    Id = item.ItemId,
                    UserCreated = item.UserCreated,
                    DateCreated = item.DateCreated,
                    UserUpdated = item.UserUpdated,
                    DateUpdated = item.DateUpdated,
                    Qty = item.Qty,
                    Amount = (double?)item.Amount,
                    IsActive = item.IsActive,
                    ItemId = item.ItemId,
                    UnitPrice = item.UnitPrice
                };
                if (itemDetails.TryGetValue(item.ItemId, out var master))
                {
                    si.Make = master.Make;
                    si.Model = master.Model;
                    si.Product = master.Product;
                    si.Category = master.Category;
                    si.ItemName = master.ItemName;
                    si.ItemCode = master.ItemCode;
                    si.Hsn = master.Hsn;
                    si.TaxPercentage = master.TaxPercentage;
                    si.Uom = master.Uom;
                }
                // Recursively fetch IncludedChildItems and AccessoriesItems
                var childIds = (item as DeliveryItem)?.IncludedChildItemIds;
                var accIds = (item as DeliveryItem)?.AccessoriesIds;
                si.IncludedChildItems = (childIds != null && childIds.Length > 0)
                    ? await FetchChildItemsRecursive(connection, childIds, delivery.DeliveryId)
                    : new List<SalesItemResponse>();
                si.AccessoriesItems = (accIds != null && accIds.Length > 0)
                    ? await FetchAccessoryItems(connection, accIds)
                    : new List<SalesItemResponse>();
                enrichedItems.Add(si);
            }
            delivery.Items = enrichedItems;

            // --- Fetch Quotation Info and Lead Address based on poId ---
            dynamic quotationInfo = null;
            object leadAddress = null;
            if (!string.IsNullOrEmpty(delivery.PoId))
            {
                // Fetch quotation_id from purchase_order
                var poCheckSql = "SELECT quotation_id FROM purchase_order WHERE po_id = @PoId LIMIT 1;";
                var quotationId = await connection.ExecuteScalarAsync<string>(poCheckSql, new { PoId = delivery.PoId });
                if (!string.IsNullOrEmpty(quotationId))
                {
                    var quotationSql = "SELECT * FROM sales_quotations WHERE quotation_id = @QuotationId LIMIT 1;";
                    quotationInfo = await connection.QueryFirstOrDefaultAsync<dynamic>(quotationSql, new { QuotationId = quotationId });
                    if (quotationInfo != null && quotationInfo.lead_id != null)
                    {
                        var leadIdStr = System.Convert.ToString(quotationInfo.lead_id);
                        var leadSql = "SELECT address FROM lead WHERE lead_id = @LeadId LIMIT 1;";
                        leadAddress = await connection.ExecuteScalarAsync<string>(leadSql, new { LeadId = leadIdStr });
                    }
                }
            }
            // Attach to delivery if model supports it
            if (delivery is ERP.API.Models.DeliveryResponse resp)
            {
                resp.QuotationInfo = quotationInfo;
                resp.LeadAddress = leadAddress as string;
            }
            else
            {
                // If not DeliveryResponse, try to set as dynamic properties
                var deliveryType = delivery.GetType();
                var qProp = deliveryType.GetProperty("QuotationInfo");
                var lProp = deliveryType.GetProperty("LeadAddress");
                if (qProp != null) qProp.SetValue(delivery, quotationInfo);
                if (lProp != null) lProp.SetValue(delivery, leadAddress as string);
            }
            return delivery;
        }

        public async Task<ERP.API.Models.DeliveryResponse> CreateAsync(Delivery delivery)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            // Validate: po_id must have a sales_order_id in purchase_order
            string quotationId = null;
            dynamic quotationInfo = null;
            object leadAddress = null;
            if (!string.IsNullOrEmpty(delivery.PoId))
            {
                // Fetch sales_order_id and quotation_id from purchase_order
                var poCheckSql = "SELECT sales_order_id, quotation_id FROM purchase_order WHERE po_id = @PoId LIMIT 1;";
                var poResult = await connection.QueryFirstOrDefaultAsync<dynamic>(poCheckSql, new { PoId = delivery.PoId });
                if (poResult == null || poResult.sales_order_id == null || string.IsNullOrEmpty(poResult.sales_order_id.ToString()))
                {
                    throw new Exception("Cannot create delivery: The given PO does not have a sales order.");
                }
                delivery.SalesOrderId = poResult.sales_order_id.ToString();
                quotationId = poResult.quotation_id != null ? poResult.quotation_id.ToString() : null;
            }
            // Auto-generate delivery_id in format DE-YYYY-XX
            var year = DateTime.UtcNow.Year;
            var countSql = "SELECT COUNT(*) FROM deliveries WHERE EXTRACT(YEAR FROM date_created) = @Year;";
            var count = await connection.ExecuteScalarAsync<int>(countSql, new { Year = year });
            var nextNumber = count + 1;
            delivery.DeliveryId = $"DC-{year}-{nextNumber:D2}";

            var sql = @"INSERT INTO deliveries (
                user_created, sales_order_id, po_id, delivery_id, delivery_date, delivery_status, dispatch_address, priority, transporter_name, vehicle_no, driver_name, driver_contact, mode_of_delivery, invoice_id
            ) VALUES (
                @UserCreated, @SalesOrderId, @PoId, @DeliveryId, @DeliveryDate, @DeliveryStatus, @DispatchAddress, @Priority, @TransporterName, @VehicleNo, @DriverName, @DriverContact, @ModeOfDelivery, @InvoiceId
            ) RETURNING *;";
            var createdDelivery = await connection.QueryFirstOrDefaultAsync<Delivery>(sql, delivery);

            // Insert delivery items if any
            if (delivery.Items != null && delivery.Items.Count > 0)
            {
                foreach (var item in delivery.Items)
                {
                    // Map request fields to DeliveryItem for DB insert, including child/accessory IDs
                    var deliveryItem = new DeliveryItem
                    {
                        ItemId = item.ItemId ?? 0,
                        UserCreated = item.UserCreated ?? 0,
                        DateCreated = item.DateCreated ?? System.DateTime.UtcNow,
                        UserUpdated = item.UserUpdated ?? 0,
                        DateUpdated = item.DateUpdated ?? System.DateTime.UtcNow,
                        Qty = item.Qty ?? 0,
                        Amount = (decimal)(item.Amount ?? 0),
                        IsActive = item.IsActive ?? true,
                        UnitPrice = (decimal)(item.UnitPrice ?? 0),
                        IncludedChildItemIds = item.IncludedChildItemIds ?? new int[0],
                        AccessoriesIds = item.AccessoriesIds ?? new int[0]
                    };
                    var itemSql = @"INSERT INTO delivery_items (
                        delivery_id, product_id, user_created, date_created, user_updated, date_updated, qty, amount, is_active, unit_price, included_child_item_ids, accessories_ids
                    ) VALUES (
                        @DeliveryId, @ProductId, @UserCreated, @DateCreated, @UserUpdated, @DateUpdated, @Qty, @Amount, @IsActive, @UnitPrice, @IncludedChildItemIds, @AccessoriesIds
                    );";
                    await connection.ExecuteAsync(itemSql, new
                    {
                        DeliveryId = createdDelivery.DeliveryId,
                        ProductId = deliveryItem.ItemId,
                        deliveryItem.UserCreated,
                        deliveryItem.DateCreated,
                        deliveryItem.UserUpdated,
                        deliveryItem.DateUpdated,
                        deliveryItem.Qty,
                        deliveryItem.Amount,
                        deliveryItem.IsActive,
                        deliveryItem.UnitPrice,
                        IncludedChildItemIds = deliveryItem.IncludedChildItemIds,
                        AccessoriesIds = deliveryItem.AccessoriesIds,
                    }
                );
                }
            }

            // Fetch enriched items with join to item_master for response (like GetByIdAsync)
            var enrichedItems = (await connection.QueryAsync<SalesItemResponse>(
                @"SELECT di.product_id AS ItemId,
                         di.user_created AS UserCreated,
                         di.date_created AS DateCreated,
                         di.user_updated AS UserUpdated,
                         di.date_updated AS DateUpdated,
                         di.qty AS Qty,
                         di.amount AS Amount,
                         di.is_active AS IsActive,
                         di.unit_price AS UnitPrice,
                         di.included_child_item_ids AS IncludedChildItemIds,
                         di.accessories_ids AS AccessoriesIds,
                         m.name AS Make,
                         mo.name AS Model,
                         p.name AS Product,
                         im.category AS Category,
                         im.item_name AS ItemName,
                         im.item_code AS ItemCode,
                         im.hsn AS Hsn,
                         im.tax_percentage AS TaxPercentage,
                         im.uom AS Uom
                    FROM delivery_items di
                    JOIN item_master im ON di.product_id = im.id
                    LEFT JOIN make m ON im.make_id = m.id
                    LEFT JOIN model mo ON im.model_id = mo.id
                    LEFT JOIN product p ON im.product_id = p.id
                   WHERE di.delivery_id = @DeliveryId
                ORDER BY di.item_id ASC",
                new { DeliveryId = createdDelivery.DeliveryId }
            )).ToList();
            foreach (var item in enrichedItems)
            {
                // Always fetch if IncludedChildItemIds or AccessoriesIds is not null and has items
                if (item.IncludedChildItemIds != null && item.IncludedChildItemIds.Length > 0)
                    item.IncludedChildItems = await FetchChildItemsRecursive(connection, item.IncludedChildItemIds, createdDelivery.DeliveryId);
                else
                    item.IncludedChildItems = new List<SalesItemResponse>();
                if (item.AccessoriesIds != null && item.AccessoriesIds.Length > 0)
                    item.AccessoriesItems = await FetchAccessoryItems(connection, item.AccessoriesIds);
                else
                    item.AccessoriesItems = new List<SalesItemResponse>();
            }
            createdDelivery.Items = enrichedItems;


            // --- Fetch Quotation Info using stored procedure for createdDelivery.PoId ---
            var createdPoId = createdDelivery.PoId;
            if (!string.IsNullOrEmpty(createdPoId))
            {
                var spSql = "SELECT * FROM sp_get_quotation_info_by_po_id(@PoId);";
                var spResult = await connection.QueryFirstOrDefaultAsync(spSql, new { PoId = createdPoId });
                if (spResult != null)
                {
                    quotationInfo = spResult; // All columns are now flat fields
                    // Try to fetch lead address as in /api/purchaseorder/get-by-po-quotationid
                    string opportunityId = null;
                    if (spResult.quotation_id != null)
                    {
                        // Get opportunity_id from sales_quotations
                        var oppIdSql = "SELECT opportunity_id FROM sales_quotations WHERE quotation_id = @QuotationId LIMIT 1;";
                        opportunityId = await connection.ExecuteScalarAsync<string>(oppIdSql, new { QuotationId = spResult.quotation_id });
                    }
                    string leadId = null;
                    if (!string.IsNullOrEmpty(opportunityId))
                    {
                        // Get lead_id from sales_opportunities
                        var leadIdSql = "SELECT lead_id FROM sales_opportunities WHERE opportunity_id = @OpportunityId LIMIT 1;";
                        leadId = await connection.ExecuteScalarAsync<string>(leadIdSql, new { OpportunityId = opportunityId });
                    }
                    if (!string.IsNullOrEmpty(leadId))
                    {
                        // Get address from sales_lead
                        var leadSql = "SELECT pincode, area, state, district, city, door_no, street, landmark FROM sales_lead WHERE lead_id = @LeadId LIMIT 1;";
                        leadAddress = await connection.QueryFirstOrDefaultAsync(leadSql, new { LeadId = leadId });
                    }
                    // Fallback: if still null, try direct lead_id from spResult
                    if (leadAddress == null && spResult.lead_id != null)
                    {
                        var leadIdStr = System.Convert.ToString(spResult.lead_id);
                        if (!string.IsNullOrEmpty(leadIdStr))
                        {
                            var leadSql = "SELECT pincode, area, state, district, city, door_no, street, landmark FROM sales_lead WHERE lead_id = @LeadId LIMIT 1;";
                            leadAddress = await connection.QueryFirstOrDefaultAsync(leadSql, new { LeadId = leadIdStr });
                        }
                    }
                }
            }

            // Return as DeliveryResponse (with QuotationInfo and LeadAddress)
            var response = new ERP.API.Models.DeliveryResponse();
            // Copy all properties from createdDelivery to response
            foreach (var prop in typeof(Delivery).GetProperties())
            {
                var value = prop.GetValue(createdDelivery);
                typeof(ERP.API.Models.DeliveryResponse).GetProperty(prop.Name)?.SetValue(response, value);
            }
            response.QuotationInfo = quotationInfo;
            response.LeadAddress = leadAddress as string;
            return response;
        }
        // End of CreateAsync
        public async Task<Delivery> UpdateAsync(int id, Delivery delivery)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            // Ensure SalesOrderId is set from PoId before update
            if (!string.IsNullOrEmpty(delivery.PoId))
            {
                var poCheckSql = "SELECT sales_order_id FROM purchase_order WHERE po_id = @PoId LIMIT 1;";
                var salesOrderId = await connection.ExecuteScalarAsync<string>(poCheckSql, new { PoId = delivery.PoId });
                if (string.IsNullOrEmpty(salesOrderId))
                {
                    throw new Exception("Cannot update delivery: The given PO does not have a sales order.");
                }
                delivery.SalesOrderId = salesOrderId;
            }
            var sql = @"UPDATE deliveries SET
                user_updated = @UserUpdated,
                date_updated = @DateUpdated,
                sales_order_id = @SalesOrderId,
                po_id = @PoId,
                delivery_id = @DeliveryId,
                delivery_date = @DeliveryDate,
                delivery_status = @DeliveryStatus,
                dispatch_address = @DispatchAddress,
                priority = @Priority,
                transporter_name = @TransporterName,
                vehicle_no = @VehicleNo,
                driver_name = @DriverName,
                driver_contact = @DriverContact,
                mode_of_delivery = @ModeOfDelivery,
                invoice_id = @InvoiceId
            WHERE id = @Id RETURNING *;";
            delivery.Id = id;
            var updatedDelivery = await connection.QueryFirstOrDefaultAsync<Delivery>(sql, delivery);

            // Update delivery items: for simplicity, delete and re-insert
            if (updatedDelivery != null && delivery.Items != null)
            {
                await connection.ExecuteAsync("DELETE FROM delivery_items WHERE delivery_id = @DeliveryId", new { DeliveryId = updatedDelivery.DeliveryId });
                foreach (var item in delivery.Items)
                {
                    var deliveryItem = new DeliveryItem
                    {
                        ItemId = item.ItemId ?? 0,
                        UserCreated = item.UserCreated ?? 0,
                        DateCreated = item.DateCreated ?? System.DateTime.UtcNow,
                        UserUpdated = item.UserUpdated ?? 0,
                        DateUpdated = item.DateUpdated ?? System.DateTime.UtcNow,
                        Qty = item.Qty ?? 0,
                        Amount = (decimal)(item.Amount ?? 0),
                        IsActive = item.IsActive ?? true,
                        UnitPrice = (decimal)(item.UnitPrice ?? 0),
                        IncludedChildItemIds = item.IncludedChildItemIds ?? new int[0],
                        AccessoriesIds = item.AccessoriesIds ?? new int[0]
                    };
                    var itemSql = @"INSERT INTO delivery_items (
                        delivery_id, product_id, user_created, date_created, user_updated, date_updated, qty, amount, is_active, unit_price, included_child_item_ids, accessories_ids
                    ) VALUES (
                        @DeliveryId, @ProductId, @UserCreated, @DateCreated, @UserUpdated, @DateUpdated, @Qty, @Amount, @IsActive, @UnitPrice, @IncludedChildItemIds, @AccessoriesIds
                    );";
                    await connection.ExecuteAsync(itemSql, new
                    {
                        DeliveryId = updatedDelivery.DeliveryId,
                        ProductId = deliveryItem.ItemId,
                        deliveryItem.UserCreated,
                        deliveryItem.DateCreated,
                        deliveryItem.UserUpdated,
                        deliveryItem.DateUpdated,
                        deliveryItem.Qty,
                        deliveryItem.Amount,
                        deliveryItem.IsActive,
                        deliveryItem.UnitPrice,
                        IncludedChildItemIds = deliveryItem.IncludedChildItemIds,
                        AccessoriesIds = deliveryItem.AccessoriesIds,
                    });
                }
                // Fetch delivery items and enrich as in GetByIdAsync
                var dbItems = (await connection.QueryAsync<DeliveryItem>(
                    "SELECT product_id AS ItemId, user_created AS UserCreated, date_created AS DateCreated, user_updated AS UserUpdated, date_updated AS DateUpdated, qty AS Qty, amount AS Amount, is_active AS IsActive, unit_price AS UnitPrice, included_child_item_ids AS IncludedChildItemIds, accessories_ids AS AccessoriesIds FROM delivery_items WHERE delivery_id = @DeliveryId ORDER BY item_id ASC",
                    new { DeliveryId = updatedDelivery.DeliveryId }
                )).AsList();
                var itemIds = dbItems.Select(i => i.ItemId).Distinct().ToArray();
                var itemDetails = (await connection.QueryAsync<ItemMaster>(
                    "SELECT * FROM item_master WHERE id = ANY(@Ids)", new { Ids = itemIds })).ToDictionary(x => x.Id);
                var enrichedItems = new List<SalesItemResponse>();
                foreach (var item in dbItems)
                {
                    var si = new SalesItemResponse
                    {
                        Id = item.ItemId,
                        UserCreated = item.UserCreated,
                        DateCreated = item.DateCreated,
                        UserUpdated = item.UserUpdated,
                        DateUpdated = item.DateUpdated,
                        Qty = item.Qty,
                        Amount = (double?)item.Amount,
                        IsActive = item.IsActive,
                        ItemId = item.ItemId,
                        UnitPrice = item.UnitPrice
                    };
                    if (itemDetails.TryGetValue(item.ItemId, out var master))
                    {
                        si.Make = master.Make;
                        si.Model = master.Model;
                        si.Product = master.Product;
                        si.Category = master.Category;
                        si.ItemName = master.ItemName;
                        si.ItemCode = master.ItemCode;
                        si.Hsn = master.Hsn;
                        si.TaxPercentage = master.TaxPercentage;
                        si.Uom = master.Uom;
                    }
                    // Always recursively fetch IncludedChildItems and AccessoriesItems for each item using DeliveryItem properties
                    var childIds = (item as DeliveryItem)?.IncludedChildItemIds;
                    var accIds = (item as DeliveryItem)?.AccessoriesIds;
                    si.IncludedChildItems = (childIds != null && childIds.Length > 0)
                        ? await FetchChildItemsRecursive(connection, childIds, updatedDelivery.DeliveryId)
                        : new List<SalesItemResponse>();
                    si.AccessoriesItems = (accIds != null && accIds.Length > 0)
                        ? await FetchAccessoryItems(connection, accIds)
                        : new List<SalesItemResponse>();
                    enrichedItems.Add(si);
                }
                updatedDelivery.Items = enrichedItems;
            }
            return updatedDelivery;
        }
        

                            public async Task<bool> DeleteAsync(int id)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            var sql = "DELETE FROM deliveries WHERE id = @Id";
            var rows = await connection.ExecuteAsync(sql, new { Id = id });
            return rows > 0;
        }

        public async Task<IEnumerable<Delivery>> GetGridAsync(int page, int pageSize, string? search)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            var offset = (page - 1) * pageSize;
            var sql = @"SELECT id, user_created, date_created, user_updated, date_updated, sales_order_id, po_id, delivery_id, delivery_date, delivery_status, dispatch_address, priority, transporter_name FROM deliveries ";
            if (!string.IsNullOrEmpty(search))
            {
                sql += "WHERE delivery_id ILIKE @Search OR delivery_status ILIKE @Search OR sales_order_id ILIKE @Search OR po_id ILIKE @Search OR dispatch_address ILIKE @Search OR priority ILIKE @Search OR transporter_name ILIKE @Search ";
            }
            sql += "ORDER BY id DESC OFFSET @Offset LIMIT @Limit";
            return await connection.QueryAsync<Delivery>(sql, new { Search = $"%{search}%", Offset = offset, Limit = pageSize });
        }

    public async Task<(IEnumerable<Delivery> Data, int TotalRecords)> GetDeliveryGridAsync(DeliveryGridRequest request)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        // Prepare the request as JSON for the stored procedure
        var gridRequest = new
        {
            SearchText = request.SearchText,
            Statuses = request.Statuses ?? System.Array.Empty<string>(),
            DeliveryIds = request.DeliveryIds ?? System.Array.Empty<string>(),
            PoIds = request.PoIds ?? System.Array.Empty<string>(),
            PageNumber = request.PageNumber > 0 ? request.PageNumber : 1,
            PageSize = request.PageSize > 0 ? request.PageSize : 10,
            OrderBy = !string.IsNullOrWhiteSpace(request.OrderBy) ? request.OrderBy : "date_created",
            OrderDirection = request.OrderDirection == "ASC" ? "ASC" : "DESC"
        };
        var parameters = new { p_request = System.Text.Json.JsonSerializer.Serialize(gridRequest) };
        var result = await connection.QueryAsync<Delivery>(
            "SELECT * FROM fn_get_deliveries_grid(@p_request::jsonb)",
            parameters
        );
        var deliveries = result.ToList();
        var totalRecords = deliveries.FirstOrDefault()?.TotalRecords ?? 0;
        return (deliveries, totalRecords);
    }
        public async Task<IEnumerable<Delivery>> GetByPurchaseOrderIdAsync(string purchaseOrderId)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            var sql = "SELECT * FROM deliveries WHERE po_id = @PoId";
            var deliveries = (await connection.QueryAsync<Delivery>(sql, new { PoId = purchaseOrderId })).ToList();
            foreach (var delivery in deliveries)
            {
                var dbItems = (await connection.QueryAsync<DeliveryItem>(
                    "SELECT product_id AS ItemId, user_created AS UserCreated, date_created AS DateCreated, user_updated AS UserUpdated, date_updated AS DateUpdated, qty AS Qty, amount AS Amount, is_active AS IsActive, unit_price AS UnitPrice, included_child_item_ids AS IncludedChildItemIds, accessories_ids AS AccessoriesIds FROM delivery_items WHERE delivery_id = @DeliveryId ORDER BY item_id ASC",
                    new { DeliveryId = delivery.DeliveryId }
                )).AsList();
                var itemIds = dbItems.Select(i => i.ItemId).Distinct().ToArray();
                var itemDetails = (await connection.QueryAsync<ItemMaster>(
                    "SELECT * FROM item_master WHERE id = ANY(@Ids)", new { Ids = itemIds })).ToDictionary(x => x.Id);
                var enrichedItems = new List<SalesItemResponse>();
                foreach (var item in dbItems)
                {
                    var si = new SalesItemResponse
                    {
                        Id = item.ItemId,
                        UserCreated = item.UserCreated,
                        DateCreated = item.DateCreated,
                        UserUpdated = item.UserUpdated,
                        DateUpdated = item.DateUpdated,
                        Qty = item.Qty,
                        Amount = (double?)item.Amount,
                        IsActive = item.IsActive,
                        ItemId = item.ItemId,
                        UnitPrice = item.UnitPrice
                    };
                    if (itemDetails.TryGetValue(item.ItemId, out var master))
                    {
                        si.Make = master.Make;
                        si.Model = master.Model;
                        si.Product = master.Product;
                        si.Category = master.Category;
                        si.ItemName = master.ItemName;
                        si.ItemCode = master.ItemCode;
                        si.Hsn = master.Hsn;
                        si.TaxPercentage = master.TaxPercentage;
                        si.Uom = master.Uom;
                    }
                    var childIds = (item as DeliveryItem)?.IncludedChildItemIds;
                    var accIds = (item as DeliveryItem)?.AccessoriesIds;
                    si.IncludedChildItems = (childIds != null && childIds.Length > 0)
                        ? await FetchChildItemsRecursive(connection, childIds, delivery.DeliveryId)
                        : new List<SalesItemResponse>();
                    si.AccessoriesItems = (accIds != null && accIds.Length > 0)
                        ? await FetchAccessoryItems(connection, accIds)
                        : new List<SalesItemResponse>();
                    enrichedItems.Add(si);
                }
                delivery.Items = enrichedItems;
            }
            return deliveries;
        }

        // --- Helper methods for recursion ---
        private async Task<List<SalesItemResponse>> FetchChildItemsRecursive(NpgsqlConnection conn, int[] childIds, string deliveryId)
        {
            var childItems = (await conn.QueryAsync<SalesItemResponse>(
                @"SELECT di.product_id AS ItemId,
                         di.user_created AS UserCreated,
                         di.date_created AS DateCreated,
                         di.user_updated AS UserUpdated,
                         di.date_updated AS DateUpdated,
                         di.qty AS Qty,
                         di.amount AS Amount,
                         di.is_active AS IsActive,
                         di.unit_price AS UnitPrice,
                         di.included_child_item_ids AS IncludedChildItemIds,
                         di.accessories_ids AS AccessoriesIds,
                         m.name AS Make,
                         mo.name AS Model,
                         p.name AS Product,
                         im.category AS Category,
                         im.item_name AS ItemName,
                         im.item_code AS ItemCode,
                         im.hsn AS Hsn,
                         im.tax_percentage AS TaxPercentage,
                         im.uom AS Uom
                    FROM delivery_items di
                    JOIN item_master im ON di.product_id = im.id
                    LEFT JOIN make m ON im.make_id = m.id
                    LEFT JOIN model mo ON im.model_id = mo.id
                    LEFT JOIN product p ON im.product_id = p.id
                   WHERE di.product_id = ANY(@Ids) AND di.delivery_id = @DeliveryId",
                new { Ids = childIds, DeliveryId = deliveryId }
            )).ToList();
            foreach (var childItem in childItems)
            {
                childItem.IncludedChildItems = (childItem.IncludedChildItemIds != null && childItem.IncludedChildItemIds.Length > 0)
                    ? await FetchChildItemsRecursive(conn, childItem.IncludedChildItemIds, deliveryId)
                    : new List<SalesItemResponse>();
                childItem.AccessoriesItems = (childItem.AccessoriesIds != null && childItem.AccessoriesIds.Length > 0)
                    ? await FetchAccessoryItems(conn, childItem.AccessoriesIds)
                    : new List<SalesItemResponse>();
            }
            return childItems;
        }

        private async Task<List<SalesItemResponse>> FetchAccessoryItems(NpgsqlConnection conn, int[] accessoryIds)
        {
            var accItems = (await conn.QueryAsync<SalesItemResponse>(
                @"SELECT im.id AS ItemId,
                         m.name AS Make,
                         mo.name AS Model,
                         p.name AS Product,
                         im.category AS Category,
                         im.item_name AS ItemName,
                         im.item_code AS ItemCode,
                         im.hsn AS Hsn,
                         im.tax_percentage AS TaxPercentage,
                         im.uom AS Uom
                    FROM item_master im
                    LEFT JOIN make m ON im.make_id = m.id
                    LEFT JOIN model mo ON im.model_id = mo.id
                    LEFT JOIN product p ON im.product_id = p.id
                   WHERE im.id = ANY(@Ids)",
                new { Ids = accessoryIds }
            )).ToList();
            return accItems;
        }
    }
}
