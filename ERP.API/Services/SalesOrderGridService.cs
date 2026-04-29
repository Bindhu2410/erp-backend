using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using ERP.API.Models;
using ERP.API.Models.DTOs;

namespace ERP.API.Services
{
    public class SalesOrderGridService : ISalesOrderGridService
    {        private readonly string? _connectionString;

        public SalesOrderGridService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(_connectionString))
                throw new ArgumentException("DefaultConnection string is not configured", nameof(configuration));
        }        public async Task<(IEnumerable<SalesOrderWithQuotationAndItemsDto> Data, int TotalRecords)> GetSalesOrderGridAsync(SalesOrderGridRequest request)
        {
            using var connection = new NpgsqlConnection(_connectionString);

            // Build SQL query for sales order grid
            var orderByColumn = request.OrderBy;
            // Ensure orderByColumn is prefixed with the correct table alias
            if (orderByColumn == "date_created") orderByColumn = "o.order_date"; // Map to actual column
            else if (orderByColumn == "order_date") orderByColumn = "o.order_date";
            else if (orderByColumn == "customer_name") orderByColumn = "c.name";
            else if (orderByColumn == "order_id") orderByColumn = "o.order_id";
            else if (orderByColumn == "status") orderByColumn = "o.status";
            else if (orderByColumn == "grand_total") orderByColumn = "o.grand_total";
            // Default fallback
            else orderByColumn = "o.order_date"; // Default to order_date instead of invalid column

            // Normalize filters: treat "string" as null/empty
            var searchText = string.IsNullOrWhiteSpace(request.SearchText) || request.SearchText == "string" ? null : request.SearchText;
            
            // More robust array normalization
            var customerNames = request.CustomerNames?.Where(x => !string.IsNullOrWhiteSpace(x) && x != "string" && x != "string1" && x != "string2").ToArray();
            customerNames = (customerNames == null || customerNames.Length == 0) ? null : customerNames;
            
            var statuses = request.Statuses?.Where(x => !string.IsNullOrWhiteSpace(x) && x != "string" && x != "string1" && x != "string2").ToArray();
            statuses = (statuses == null || statuses.Length == 0) ? null : statuses;
            
            var orderIds = request.OrderIds?.Where(x => !string.IsNullOrWhiteSpace(x) && x != "string" && x != "string1" && x != "string2").ToArray();
            orderIds = (orderIds == null || orderIds.Length == 0) ? null : orderIds;

            var whereClauses = new List<string>();
            
            // Only add search text filter if it's actually provided
            if (!string.IsNullOrEmpty(searchText))
                whereClauses.Add("o.order_id ILIKE '%' || @searchText || '%'");
                
            if (customerNames != null && customerNames.Length > 0)
                whereClauses.Add("c.name = ANY(@customerNames)");
            if (statuses != null && statuses.Length > 0)
                whereClauses.Add("o.status = ANY(@statuses)");
            if (orderIds != null && orderIds.Length > 0)
                whereClauses.Add("o.order_id = ANY(@orderIds)");
                
            var whereSql = whereClauses.Count > 0 ? string.Join(" AND ", whereClauses) : "1=1";

            var sql = $@"SELECT 
                o.id,
                o.order_id AS OrderId,
                o.customer_id AS CustomerId,
                o.order_date AS OrderDate,
                o.expected_delivery_date AS ExpectedDeliveryDate,
                o.status,
                o.quotation_id AS QuotationId,
                COALESCE(o.po_id, '') AS PoId,
                o.acceptance_date AS AcceptanceDate,
                COALESCE(o.total_amount, 0) AS TotalAmount,
                COALESCE(o.tax_amount, 0) AS TaxAmount,
                COALESCE(o.grand_total, 0) AS GrandTotal,
                COALESCE(o.notes, '') AS Notes,
                COALESCE(o.user_created, 0) AS UserCreated,
                o.date_created AS DateCreated,
                COALESCE(o.user_updated, 0) AS UserUpdated,
                o.date_updated AS DateUpdated,
                COALESCE(c.name, '') AS CustomerName,
                '' AS MobileNum
            FROM sales_orders o 
            LEFT JOIN sales_customers c ON o.customer_id = c.id 
            WHERE {whereSql} 
            ORDER BY {orderByColumn} {request.OrderDirection} 
            OFFSET (@pageNumber - 1) * @pageSize LIMIT @pageSize";

            var parameters = new
            {
                searchText = searchText,
                customerNames = customerNames?.ToArray(),
                statuses = statuses?.ToArray(),
                orderIds = orderIds?.ToArray(),
                pageNumber = request.PageNumber,
                pageSize = request.PageSize
            };

            var result = await connection.QueryAsync<SalesOrder>(sql, parameters);

            // Get total records for pagination
            var countSql = $@"SELECT COUNT(*) FROM sales_orders o 
                             LEFT JOIN sales_customers c ON o.customer_id = c.id
                             WHERE {whereSql}";
            var totalRecords = await connection.ExecuteScalarAsync<int>(countSql, parameters);

            if (result == null || !result.Any())
                return (new List<SalesOrderWithQuotationAndItemsDto>(), 0);

            var gridData = new List<SalesOrderWithQuotationAndItemsDto>();
            foreach (var so in result)
            {
                // Enhance customer information if missing
                if (string.IsNullOrEmpty(so.CustomerName) || string.IsNullOrEmpty(so.MobileNum))
                {
                    // Try to get customer info from sales_customers
                    if (so.CustomerId.HasValue)
                    {
                        var customer = await connection.QueryFirstOrDefaultAsync<dynamic>(
                            "SELECT name, mobile FROM sales_customers WHERE id = @CustomerId",
                            new { CustomerId = so.CustomerId });
                        
                        if (customer != null)
                        {
                            if (string.IsNullOrEmpty(so.CustomerName) && customer.name != null)
                                so.CustomerName = (string)customer.name;
                            if (string.IsNullOrEmpty(so.MobileNum) && customer.mobile != null)
                                so.MobileNum = (string)customer.mobile;
                        }
                    }
                }

                // Fetch quotation with explicit column selection to avoid problematic columns
                SalesQuotation quotation = null;
                if (so.QuotationId.HasValue)
                {
                    quotation = await connection.QueryFirstOrDefaultAsync<SalesQuotation>(
                        @"SELECT 
                            id, quotation_id, customer_id, customer_name, status, 
                            quotation_date, valid_till, opportunity_id, lead_id,
                            user_created, date_created, user_updated, date_updated,
                            version, terms, quotation_for, lost_reason, quotation_type,
                            order_type, comments, delivery_within, delivery_after,
                            contact_name, contact_mobile_no
                        FROM sales_quotations WHERE id = @QuotationId",
                        new { QuotationId = so.QuotationId });
                        
                    // Try to get mobile number from quotation if still missing
                    if (string.IsNullOrEmpty(so.MobileNum) && quotation != null && !string.IsNullOrEmpty(quotation.ContactMobileNo))
                        so.MobileNum = quotation.ContactMobileNo;
                        
                    // Try to get customer name from quotation if still missing
                    if (string.IsNullOrEmpty(so.CustomerName) && quotation != null && !string.IsNullOrEmpty(quotation.CustomerName))
                        so.CustomerName = quotation.CustomerName;
                }

                // Fetch items with safe column selection
                var items = new List<SalesItemResponse>();
                if (so.QuotationId.HasValue)
                {
                    items = (await connection.QueryAsync<SalesItemResponse>(
                        @"SELECT 
                            sp.id, 
                            sp.user_created as UserCreated, 
                            sp.date_created as DateCreated, 
                            sp.user_updated as UserUpdated, 
                            sp.date_updated as DateUpdated, 
                            sp.qty, 
                            sp.amount, 
                            sp.is_active as IsActive, 
                            sp.item_id as ItemId, 
                            sp.stage, 
                            sp.stage_item_id as StageItemId,
                            sp.unit_price as UnitPrice,
                            COALESCE(mk.name, '') as Make, 
                            COALESCE(md.name, '') as Model, 
                            COALESCE(pd.name, '') as Product, 
                            COALESCE(im.category, '') as Category, 
                            COALESCE(im.item_name, '') AS ItemName, 
                            COALESCE(im.item_code, '') AS ItemCode,
                            COALESCE(im.hsn, '') as Hsn,
                            '' as TaxPercentage,
                            COALESCE(im.uom, '') as Uom,
                            '' as Taxes
                        FROM sales_product sp
                        LEFT JOIN item_master im ON sp.item_id = im.id
                        LEFT JOIN make mk ON im.make_id = mk.id
                        LEFT JOIN model md ON im.model_id = md.id
                        LEFT JOIN product pd ON im.product_id = pd.id
                        WHERE sp.stage = 'Quotation' AND sp.stage_item_id = @QuotationId",
                        new { QuotationId = so.QuotationId.ToString() })).AsList();

                    // Fetch child items and accessories for each item
                    for (int i = 0; i < items.Count; i++)
                    {
                        items[i] = ReplaceNullsWithDefaults(items[i]);
                        
                        // Fetch child items
                        var childItems = (await connection.QueryAsync<SalesItemResponse>(
                            @"SELECT 
                                spci.id,
                                NULL as UserCreated, 
                                NULL as DateCreated, 
                                NULL as UserUpdated, 
                                NULL as DateUpdated, 
                                NULL as Qty, 
                                NULL as Amount, 
                                im.is_active as IsActive, 
                                im.id as ItemId, 
                                NULL as Stage, 
                                NULL as StageItemId,
                                im.unit_price as UnitPrice,
                                COALESCE(mk.name, '') as Make, 
                                COALESCE(md.name, '') as Model, 
                                COALESCE(pd.name, '') as Product, 
                                COALESCE(im.category, '') as Category, 
                                COALESCE(im.item_name, '') AS ItemName, 
                                COALESCE(im.item_code, '') AS ItemCode,
                                COALESCE(im.hsn, '') as Hsn,
                                '' as TaxPercentage,
                                COALESCE(im.uom, '') as Uom,
                                '' as Taxes
                            FROM sales_product_child_items spci 
                            JOIN item_master im ON spci.child_item_id = im.id 
                            LEFT JOIN make mk ON im.make_id = mk.id
                            LEFT JOIN model md ON im.model_id = md.id
                            LEFT JOIN product pd ON im.product_id = pd.id
                            WHERE spci.sales_product_id = @SalesProductId",
                            new { SalesProductId = items[i].Id })).AsList();

                        // Fetch accessory items
                        var accessoryItems = (await connection.QueryAsync<SalesItemResponse>(
                            @"SELECT 
                                spa.id,
                                NULL as UserCreated, 
                                NULL as DateCreated, 
                                NULL as UserUpdated, 
                                NULL as DateUpdated, 
                                NULL as Qty, 
                                NULL as Amount, 
                                im.is_active as IsActive, 
                                im.id as ItemId, 
                                NULL as Stage, 
                                NULL as StageItemId,
                                im.unit_price as UnitPrice,
                                COALESCE(mk.name, '') as Make, 
                                COALESCE(md.name, '') as Model, 
                                COALESCE(pd.name, '') as Product, 
                                COALESCE(im.category, '') as Category, 
                                COALESCE(im.item_name, '') AS ItemName, 
                                COALESCE(im.item_code, '') AS ItemCode,
                                COALESCE(im.hsn, '') as Hsn,
                                '' as TaxPercentage,
                                COALESCE(im.uom, '') as Uom,
                                '' as Taxes
                            FROM sales_product_accessories spa 
                            JOIN item_master im ON spa.accessories_item_id = im.id 
                            LEFT JOIN make mk ON im.make_id = mk.id
                            LEFT JOIN model md ON im.model_id = md.id
                            LEFT JOIN product pd ON im.product_id = pd.id
                            WHERE spa.sales_product_id = @SalesProductId",
                            new { SalesProductId = items[i].Id })).AsList();

                        // Apply defaults to child and accessory items
                        for (int j = 0; j < childItems.Count; j++)
                            childItems[j] = ReplaceNullsWithDefaults(childItems[j]);
                        
                        for (int k = 0; k < accessoryItems.Count; k++)
                            accessoryItems[k] = ReplaceNullsWithDefaults(accessoryItems[k]);

                        // Assign to the main item
                        items[i].IncludedChildItems = childItems;
                        items[i].AccessoriesItems = accessoryItems;
                    }
                }

                gridData.Add(new SalesOrderWithQuotationAndItemsDto
                {
                    SalesOrder = so,
                    Quotation = quotation,
                    Items = items
                });
            }
            return (gridData, totalRecords);
        }

        public static T ReplaceNullsWithDefaults<T>(T obj)
        {
            if (obj == null) return obj;
            var props = typeof(T).GetProperties();
            foreach (var prop in props)
            {
                if (!prop.CanWrite) continue;
                var value = prop.GetValue(obj);
                if (value == null)
                {
                    if (prop.PropertyType == typeof(int) || prop.PropertyType == typeof(int?))
                        prop.SetValue(obj, 0);
                    else if (prop.PropertyType == typeof(decimal) || prop.PropertyType == typeof(decimal?))
                        prop.SetValue(obj, 0m);
                    else if (prop.PropertyType == typeof(double) || prop.PropertyType == typeof(double?))
                        prop.SetValue(obj, 0d);
                    else if (prop.PropertyType == typeof(float) || prop.PropertyType == typeof(float?))
                        prop.SetValue(obj, 0f);
                    else if (prop.PropertyType == typeof(DateTime) || prop.PropertyType == typeof(DateTime?))
                        prop.SetValue(obj, DateTime.MinValue);
                    else if (prop.PropertyType == typeof(DateTimeOffset) || prop.PropertyType == typeof(DateTimeOffset?))
                        prop.SetValue(obj, DateTimeOffset.MinValue);
                    else if (prop.PropertyType == typeof(bool) || prop.PropertyType == typeof(bool?))
                        prop.SetValue(obj, false);
                }
            }
            return obj;
        }
    }
}
