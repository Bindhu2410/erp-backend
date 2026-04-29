using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using ERP.API.Models;
using ERP.API.Models.DTOs;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace ERP.API.Services
{
    public class SalesOrderService : ISalesOrderService
    {
        private readonly string? _connectionString;


        public async Task<SalesOrderWithQuotationAndItemsDto> GetSalesOrderWithQuotationAndItemsByPoIdAsync(string poId)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            // Debug: Check if sales order exists with different case sensitivity or whitespace
            var salesOrder = await connection.QueryFirstOrDefaultAsync<SalesOrder>(
                "SELECT * FROM sales_orders WHERE TRIM(po_id) = TRIM(@PoId)",
                new { PoId = poId });
            if (salesOrder == null) return null;
            var quotation = await connection.QueryFirstOrDefaultAsync<SalesQuotation>(
                "SELECT * FROM sales_quotations WHERE id = @QuotationId",
                new { QuotationId = salesOrder.QuotationId });
            string customerName = string.Empty;
            string mobileNum = string.Empty;
            if (quotation != null && !string.IsNullOrEmpty(quotation.OpportunityId))
            {
                var opportunity = await connection.QueryFirstOrDefaultAsync<SalesOpportunity>(
                    "SELECT * FROM sales_opportunities WHERE opportunity_id = @OpportunityId",
                    new { OpportunityId = quotation.OpportunityId });
                if (opportunity != null)
                {
                    customerName = opportunity.CustomerName ?? string.Empty;
                    mobileNum = opportunity.ContactMobileNo ?? string.Empty;
                }
            }
            if (string.IsNullOrEmpty(customerName) && quotation != null && !string.IsNullOrEmpty(quotation.CustomerName))
            {
                customerName = quotation.CustomerName;
            }
            salesOrder.CustomerName = customerName;
            salesOrder.MobileNum = mobileNum;
            // Fetch items based on quotation id, not sales order id, and build hierarchy
            var items = (await connection.QueryAsync<SalesItemResponse>(
                "SELECT sp.*, mk.name as make, md.name as model, pd.name as product, c.name as category, im.item_name as ItemName, im.item_code as ItemCode, im.unit_price as UnitPrice, u.code as Uom, im.tax_percentage as TaxPercentage " +
                "FROM sales_product sp JOIN item_master im ON sp.item_id = im.id LEFT JOIN make mk ON im.make_id = mk.id LEFT JOIN model md ON im.model_id = md.id LEFT JOIN product pd ON im.product_id = pd.id LEFT JOIN categories c ON im.category_id = c.id LEFT JOIN uom u ON im.uom_id = u.id WHERE sp.stage = 'Quotation' AND sp.stage_item_id = @QuotationId",
                new { QuotationId = salesOrder.QuotationId.ToString() })).AsList();


            // Populate IncludedChildItems and AccessoriesItems for each item (non-recursive, correct join)
            foreach (var item in items)
            {
                // IncludedChildItems: join sales_product_child_items -> item_master only
                item.IncludedChildItems = (await connection.QueryAsync<SalesItemResponse>(
                    @"SELECT im.id, NULL as UserCreated, NULL as DateCreated, NULL as UserUpdated, NULL as DateUpdated, NULL as Qty, NULL as Amount, im.is_active as IsActive, im.id as ItemId, NULL as Stage, NULL as StageItemId, mk.name as make, md.name as model, pd.name as product, c.name as category, im.item_name as ItemName, im.item_code as ItemCode, im.unit_price as UnitPrice, im.hsn, im.tax_percentage as TaxPercentage, u.code as uom, NULL as ParentId, NULL as ParentItem, NULL as ReferencedBy, NULL as IncludedChildItems, NULL as AccessoriesItems
                      FROM sales_product_child_items spci
                      JOIN item_master im ON spci.child_item_id = im.id
                      LEFT JOIN make mk ON im.make_id = mk.id LEFT JOIN model md ON im.model_id = md.id LEFT JOIN product pd ON im.product_id = pd.id LEFT JOIN categories c ON im.category_id = c.id LEFT JOIN uom u ON im.uom_id = u.id
                      WHERE spci.sales_product_id = @SalesProductId",
                    new { SalesProductId = item.Id })).ToList();

                // AccessoriesItems: join sales_product_accessories -> item_master only
                item.AccessoriesItems = (await connection.QueryAsync<SalesItemResponse>(
                    @"SELECT im.id, NULL as UserCreated, NULL as DateCreated, NULL as UserUpdated, NULL as DateUpdated, NULL as Qty, NULL as Amount, im.is_active as IsActive, im.id as ItemId, NULL as Stage, NULL as StageItemId, mk.name as make, md.name as model, pd.name as product, c.name as category, im.item_name as ItemName, im.item_code as ItemCode, im.unit_price as UnitPrice, im.hsn, im.tax_percentage as TaxPercentage, u.code as uom, NULL as ParentId, NULL as ParentItem, NULL as ReferencedBy, NULL as IncludedChildItems, NULL as AccessoriesItems
                      FROM sales_product_accessories spa
                      JOIN item_master im ON spa.accessories_item_id = im.id
                      LEFT JOIN make mk ON im.make_id = mk.id LEFT JOIN model md ON im.model_id = md.id LEFT JOIN product pd ON im.product_id = pd.id LEFT JOIN categories c ON im.category_id = c.id LEFT JOIN uom u ON im.uom_id = u.id
                      WHERE spa.sales_product_id = @SalesProductId",
                    new { SalesProductId = item.Id })).ToList();
            }

            // Only top-level items (parent_id = 0)
            var topLevelItems = items.Where(i => i.ParentId == 0 || i.ParentId == null).ToList();
            // Replace nulls with defaults for all items (flat list)
            for (int i = 0; i < items.Count; i++)
                items[i] = ReplaceNullsWithDefaults(items[i]);
            return new SalesOrderWithQuotationAndItemsDto
            {
                SalesOrder = salesOrder,
                Quotation = quotation,
                Items = topLevelItems
            };
        }

        /// <summary>
        /// Gets the sales order by po_id.
        /// </summary>
        public async Task<SalesOrder> GetSalesOrderByPoIdAsync(string poId)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<SalesOrder>(
                "SELECT * FROM sales_orders WHERE TRIM(po_id) = TRIM(@PoId)",
                new { PoId = poId });
        }
        /// <summary>
        /// Updates the po_id in sales_orders for the given sales order id.
        /// </summary>
        public async Task<bool> UpdateSalesOrderPoIdAsync(int salesOrderId, int poId)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            var sql = "UPDATE sales_orders SET po_id = @PoId WHERE id = @SalesOrderId";
            var rowsAffected = await connection.ExecuteAsync(sql, new { PoId = poId, SalesOrderId = salesOrderId });
            return rowsAffected > 0;
        }

        public SalesOrderService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(_connectionString))
                throw new ArgumentException("DefaultConnection string is not configured", nameof(configuration));
        }

        public async Task<IEnumerable<SalesOrderGrid>> GetAllSalesOrdersAsync()
        {
            using var connection = new NpgsqlConnection(_connectionString);
            var salesOrders = (await connection.QueryAsync<SalesOrderGrid>(
                "SELECT so.id, so.order_id as OrderId, so.customer_name as CustomerName, so.mobile_num as MobileNum, so.order_date as OrderDate, so.expected_delivery_date as ExpectedDeliveryDate, " +
                "so.status, so.po_id as PoId, so.grand_total as GrandTotal, so.quotation_id as QuotationId " +
                "FROM sales_orders so " +
                "ORDER BY so.order_date DESC")).ToList();

            // No need to manually set CustomerName and MobileNum, as they are now selected directly from the database.
            return salesOrders;
        }

        public async Task<SalesOrder> GetSalesOrderByIdAsync(int id)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<SalesOrder>(
                "SELECT * FROM sales_orders WHERE id = @Id",
                new { Id = id });
        }        public async Task<SalesOrder> CreateSalesOrderAsync(SalesOrder salesOrder)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            var sql = @"
                SELECT * FROM fn_create_sales_order(
                    @CustomerId, @OrderDate, @ExpectedDeliveryDate,
                    @Status, @QuotationId, @PoId, @AcceptanceDate,
                    @TotalAmount, @TaxAmount, @GrandTotal, @Notes,
                    @UserCreated
                )";

            return await connection.QueryFirstOrDefaultAsync<SalesOrder>(sql, salesOrder);
        }

        public async Task<bool> UpdateSalesOrderAsync(SalesOrder salesOrder)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            var sql = @"
                UPDATE sales_orders
                SET order_id = @OrderId,
                    customer_id = @CustomerId,
                    order_date = @OrderDate,
                    expected_delivery_date = @ExpectedDeliveryDate,
                    status = @Status,
                    quotation_id = @QuotationId,
                    po_id = @PoId,
                    acceptance_date = @AcceptanceDate,
                    total_amount = @TotalAmount,
                    tax_amount = @TaxAmount,
                    grand_total = @GrandTotal,
                    notes = @Notes,
                    user_updated = @UserUpdated,
                    date_updated = CURRENT_TIMESTAMP
                WHERE id = @Id";

            var rowsAffected = await connection.ExecuteAsync(sql, salesOrder);
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteSalesOrderAsync(int id)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            var rowsAffected = await connection.ExecuteAsync(
                "DELETE FROM sales_orders WHERE id = @Id",
                new { Id = id });
            return rowsAffected > 0;
        }

        public async Task<QuotationWithOrderResponse> GetQuotationByIdAsync(int id)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            var sql = "SELECT * FROM fn_get_quotation_with_order(@Id)";

            var result = await connection.QueryAsync<QuotationDetails, SalesOrderDetails, QuotationWithOrderResponse>(
                sql,
                (quotation, salesOrder) => new QuotationWithOrderResponse 
                { 
                    Quotation = quotation, 
                    SalesOrder = salesOrder 
                },
                new { Id = id },
                splitOn: "sales_order_id"
            );

            return result.FirstOrDefault() ?? new QuotationWithOrderResponse 
            { 
                Quotation = new QuotationDetails(),
                SalesOrder = null 
            };
        }

        public async Task CopyQuotationItemsToOrder(int quotationId, int orderId)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();
            // Fetch all items for the quotation, joining item_master to get make/model/product/category/item_name/item_code
                                var items = (await connection.QueryAsync<SalesItemResponse>(
                                        @"SELECT sp.*, mk.name as make, md.name as model, pd.name as product, c.name as category, im.item_name as ItemName, im.item_code as ItemCode
                                            FROM sales_product sp
                                            JOIN item_master im ON sp.item_id = im.id
                                            LEFT JOIN make mk ON im.make_id = mk.id
                                            LEFT JOIN model md ON im.model_id = md.id
                                            LEFT JOIN product pd ON im.product_id = pd.id
                                            LEFT JOIN categories c ON im.category_id = c.id
                                            WHERE sp.stage = 'Quotation' AND sp.stage_item_id = @QuotationId",
                                        new { QuotationId = quotationId.ToString() })).ToList();


            foreach (var item in items)
            {
                // Insert as new order item
                await connection.ExecuteAsync(
                    @"INSERT INTO sales_product (qty, amount, is_active, item_id, stage_item_id, stage, unit_price)
                      VALUES (@Qty, @Amount, @IsActive, @ItemId, @StageItemId, @Stage, @UnitPrice)",
                    new {
                        item.Qty,
                        item.Amount,
                        IsActive = true,
                        item.ItemId,
                        StageItemId = orderId.ToString(),
                        Stage = "Order",
                        item.UnitPrice
                    });
            }
        }

        public async Task<SalesOrder> CreateSalesOrderFromQuotationAsync(int quotationId, int userCreated)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            // Fetch the quotation
            var quotation = await connection.QueryFirstOrDefaultAsync<QuotationDetails>(
                "SELECT * FROM sales_quotations WHERE id = @Id",
                new { Id = quotationId });

            if (quotation == null)
                throw new Exception("Quotation not found.");

            if (!quotation.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase))
                throw new Exception("Quotation is not approved.");

            // Create sales order using the quotation info
            var sql = @"
                SELECT * FROM fn_create_sales_order(
                    @CustomerId, @OrderDate, @ExpectedDeliveryDate,
                    @Status, @QuotationId, @PoId, @AcceptanceDate,
                    @TotalAmount, @TaxAmount, @GrandTotal, @Notes,
                    @UserCreated
                )";

            var salesOrder = await connection.QueryFirstOrDefaultAsync<SalesOrder>(sql, new
            {
                CustomerId = quotation.CustomerId,
                OrderDate = DateTime.UtcNow,
                ExpectedDeliveryDate = quotation.ValidTill,
                Status = "Draft",
                QuotationId = quotation.Id,
                PoId = (string)null,
                AcceptanceDate = (DateTime?)null,
                TotalAmount = 0.00m,
                TaxAmount = 0.00m,
                GrandTotal = 0.00m,
                Notes = quotation.Comments,
                UserCreated = userCreated
            });

            // Optionally copy items from quotation to order
            if (salesOrder != null)
            {
                await CopyQuotationItemsToOrder(quotationId, salesOrder.Id);
            }

            return salesOrder;
        }

        public async Task<IEnumerable<SalesItemResponse>> GetQuotationItemsAsync(int quotationId)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            return await connection.QueryAsync<SalesItemResponse>(
                @"SELECT sp.*, mk.name as make, md.name as model, pd.name as product, c.name as category, im.item_name as ItemName, im.item_code as ItemCode
                  FROM sales_product sp
                  JOIN item_master im ON sp.item_id = im.id
                  LEFT JOIN make mk ON im.make_id = mk.id
                  LEFT JOIN model md ON im.model_id = md.id
                  LEFT JOIN product pd ON im.product_id = pd.id
                  LEFT JOIN categories c ON im.category_id = c.id
                  WHERE sp.stage = 'Quotation' AND sp.stage_item_id = @QuotationId",
                new { QuotationId = quotationId.ToString() });
        }

        public async Task<SalesOrderDetailsDto> GetSalesOrderDetailsByIdAsync(int id)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            var salesOrder = await connection.QueryFirstOrDefaultAsync<SalesOrder>(
                "SELECT * FROM sales_orders WHERE id = @Id",
                new { Id = id });
            if (salesOrder == null)
                return new SalesOrderDetailsDto {
                    SalesOrder = new SalesOrder(),
                    Quotation = new SalesQuotation(),
                    Items = new List<SalesProduct>(),
                    CustomerName = string.Empty
                };


            SalesQuotation quotation = null;
            List<SalesItemResponse> items = new List<SalesItemResponse>();
            List<SalesProduct> mappedItems = new List<SalesProduct>();

            // Helper: recursively fetch child and accessory items for a given sales_product id
            async Task<List<SalesProduct>> FetchChildProductsAsync(int parentProductId, NpgsqlConnection conn)
            {
                var childProducts = (await conn.QueryAsync<SalesProduct>(
                    @"SELECT sp.*, mk.name as MakeName, md.name as ModelName, pd.name as ProductName, c.name as CategoryName, im.item_name as ItemName, im.item_code as ItemCode
                      FROM sales_product_child_items spci
                      JOIN sales_product sp ON spci.child_item_id = sp.id
                      JOIN item_master im ON sp.item_id = im.id
                      LEFT JOIN make mk ON im.make_id = mk.id
                      LEFT JOIN model md ON im.model_id = md.id
                      LEFT JOIN product pd ON im.product_id = pd.id
                      LEFT JOIN categories c ON im.category_id = c.id
                      WHERE spci.sales_product_id = @ParentId",
                    new { ParentId = parentProductId })).ToList();
                foreach (var child in childProducts)
                {
                    child.IncludedChildItems = await FetchChildProductsAsync(child.Id ?? 0, conn);
                    child.AccessoriesItems = await FetchAccessoryProductsAsync(child.Id ?? 0, conn);
                }
                return childProducts;
            }

            async Task<List<SalesProduct>> FetchAccessoryProductsAsync(int productId, NpgsqlConnection conn)
            {
                var accessories = (await conn.QueryAsync<SalesProduct>(
                    @"SELECT sp.*, im.make as MakeName, im.model as ModelName, im.product as ProductName, im.category as CategoryName, im.item_name as ItemName, im.item_code as ItemCode
                      FROM sales_product_accessories spa
                      JOIN sales_product sp ON spa.accessories_item_id = sp.id
                      JOIN item_master im ON sp.item_id = im.id
                      WHERE spa.sales_product_id = @ProductId",
                    new { ProductId = productId })).ToList();
                foreach (var acc in accessories)
                {
                    acc.IncludedChildItems = await FetchChildProductsAsync(acc.Id ?? 0, conn);
                    acc.AccessoriesItems = await FetchAccessoryProductsAsync(acc.Id ?? 0, conn);
                }
                return accessories;
            }
            string customerName = salesOrder.CustomerName ?? string.Empty;
            string mobileNum = salesOrder.MobileNum ?? string.Empty;
            if (salesOrder.QuotationId.HasValue)
            {
                quotation = await connection.QueryFirstOrDefaultAsync<SalesQuotation>(
                    "SELECT * FROM sales_quotations WHERE id = @Id",
                    new { Id = salesOrder.QuotationId });
                if (quotation != null)
                {
                    // Try to get customerName from opportunity if available
                    if (!string.IsNullOrEmpty(quotation.OpportunityId))
                    {
                        var opportunity = await connection.QueryFirstOrDefaultAsync<SalesOpportunity>(
                            "SELECT * FROM sales_opportunities WHERE opportunity_id = @OpportunityId",
                            new { OpportunityId = quotation.OpportunityId });
                        if (opportunity != null)
                        {
                            customerName = string.IsNullOrEmpty(opportunity.CustomerName) ? customerName : opportunity.CustomerName;
                            mobileNum = string.IsNullOrEmpty(opportunity.ContactMobileNo) ? mobileNum : opportunity.ContactMobileNo;
                        }
                    }
                    // Fallback to quotation customer name if not found in opportunity
                    if (string.IsNullOrEmpty(customerName) && !string.IsNullOrEmpty(quotation.CustomerName))
                    {
                        customerName = quotation.CustomerName;
                    }
                }
                // Debug: Log the QuotationId used for item query
                Console.WriteLine($"[DEBUG] Fetching items for QuotationId: {salesOrder.QuotationId.Value}");
                // Use same logic as by-quotation endpoint to fetch items
                items = (await connection.QueryAsync<SalesItemResponse>(
                    @"SELECT sp.*, im.make, im.model, im.product, im.category, im.item_name as ItemName, im.item_code as ItemCode
                      FROM sales_product sp
                      JOIN item_master im ON sp.item_id = im.id
                      WHERE sp.stage = 'Quotation' AND sp.stage_item_id = @QuotationId",
                    new { QuotationId = salesOrder.QuotationId.Value.ToString() })).ToList();
                Console.WriteLine($"[DEBUG] Items fetched: {items.Count}");
                items = items.Select(item => new SalesItemResponse
                {
                    Id = item.Id,
                    UserCreated = item.UserCreated ?? 0,
                    DateCreated = item.DateCreated,
                    UserUpdated = item.UserUpdated ?? 0,
                    DateUpdated = item.DateUpdated,
                    Qty = item.Qty ?? 0,
                    Amount = item.Amount ?? 0,
                    IsActive = item.IsActive ?? true,
                    ItemId = item.ItemId ?? 0,
                    Stage = item.Stage ?? "Quotation",
                    UnitPrice = item.UnitPrice ?? 0,
                    StageItemId = item.StageItemId ?? salesOrder.QuotationId.Value.ToString(),
                    Make = item.Make ?? string.Empty,
                    Model = item.Model ?? string.Empty,
                    Product = item.Product ?? string.Empty,
                    Category = item.Category ?? string.Empty,
                    ItemName = item.ItemName ?? string.Empty,
                    ItemCode = item.ItemCode ?? string.Empty
                }).ToList();
                mappedItems = new List<SalesProduct>();
                foreach (var item in items)
                {
                    var prod = new SalesProduct
                    {
                        Id = item.Id,
                        Quantity = item.Qty,
                        Amount = item.Amount,
                        InventoryItemsId = item.ItemId ?? 0,
                        Stage = item.Stage ?? "Quotation",
                        StageItemId = item.StageItemId ?? salesOrder.QuotationId.Value.ToString(),
                        IsActive = item.IsActive,
                        MakeName = item.Make ?? string.Empty,
                        ModelName = item.Model ?? string.Empty,
                        ProductName = item.Product ?? string.Empty,
                        CategoryName = item.Category ?? string.Empty,
                        ItemName = item.ItemName ?? string.Empty,
                        ItemCode = item.ItemCode ?? string.Empty
                    };
                    // Recursively fetch children and accessories for each product
                    prod.IncludedChildItems = await FetchChildProductsAsync(prod.Id ?? 0, connection);
                    prod.AccessoriesItems = await FetchAccessoryProductsAsync(prod.Id ?? 0, connection);
                    mappedItems.Add(prod);
                }
                Console.WriteLine($"[DEBUG] Mapped items count: {mappedItems.Count}");
            }
            else
            {
                quotation = new SalesQuotation();
                mappedItems = new List<SalesProduct>();
            }

            // Set the values on the SalesOrder object for API response
            salesOrder.CustomerName = string.IsNullOrEmpty(customerName) ? (salesOrder.CustomerName ?? string.Empty) : customerName;
            salesOrder.MobileNum = string.IsNullOrEmpty(mobileNum) ? (salesOrder.MobileNum ?? string.Empty) : mobileNum;
            return new SalesOrderDetailsDto
            {
                SalesOrder = salesOrder,
                Quotation = quotation,
                Items = mappedItems,
                CustomerName = salesOrder.CustomerName ?? string.Empty
            };
        }

        public async Task<object> GetSalesOrderDetailAsync(int id)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            var salesOrder = await connection.QueryFirstOrDefaultAsync<SalesOrder>(
                "SELECT * FROM sales_orders WHERE id = @Id",
                new { Id = id });
            if (salesOrder == null) return null;

            var quotation = await connection.QueryFirstOrDefaultAsync<SalesQuotation>(
                "SELECT * FROM sales_quotations WHERE id = @QuotationId",
                new { QuotationId = salesOrder.QuotationId });

            string customerName = string.Empty;
            string mobileNum = string.Empty;
            if (quotation != null && !string.IsNullOrEmpty(quotation.OpportunityId))
            {
                var opportunity = await connection.QueryFirstOrDefaultAsync<SalesOpportunity>(
                    "SELECT * FROM sales_opportunities WHERE opportunity_id = @OpportunityId",
                    new { OpportunityId = quotation.OpportunityId });
                if (opportunity != null)
                {
                    customerName = opportunity.CustomerName ?? string.Empty;
                    mobileNum = opportunity.ContactMobileNo ?? string.Empty;
                }
            }
            if (string.IsNullOrEmpty(customerName) && quotation != null && !string.IsNullOrEmpty(quotation.CustomerName))
                customerName = quotation.CustomerName;

            salesOrder.CustomerName = customerName;
            salesOrder.MobileNum = mobileNum;

            PurchaseOrderDto purchaseOrder = null;
            if (!string.IsNullOrEmpty(salesOrder.PoId))
            {
                purchaseOrder = await connection.QueryFirstOrDefaultAsync<PurchaseOrderDto>(
                    "SELECT * FROM purchase_order WHERE TRIM(po_id) = TRIM(@PoId)",
                    new { PoId = salesOrder.PoId });
            }

            // Fetch BOM-grouped items from sales_product for the linked quotation
            var rawItems = (await connection.QueryAsync<dynamic>(
                @"SELECT sp.id, sp.bom_id, sp.qty, sp.bom_child_item_ids, sp.bom_accessory_item_ids,
                         bom.bom_name, bom.bom_type, bom.quote_title_id, bom.tc_template_id
                  FROM sales_product sp
                  LEFT JOIN bill_of_materials bom ON sp.bom_id = bom.bom_id
                  WHERE sp.stage = 'Quotation' AND sp.stage_item_id = @QuotationId",
                new { QuotationId = salesOrder.QuotationId.ToString() })).AsList();

            var items = new System.Collections.Generic.List<object>();
            foreach (var qItem in rawItems)
            {
                if (string.IsNullOrWhiteSpace(qItem.bom_id?.ToString()) ||
                    string.IsNullOrWhiteSpace(qItem.bom_name?.ToString()))
                    continue;

                int[] bomChildItemIds = System.Array.Empty<int>();
                int[] bomAccessoryItemIds = System.Array.Empty<int>();
                try { bomChildItemIds = qItem.bom_child_item_ids != null ? System.Text.Json.JsonSerializer.Deserialize<int[]>(qItem.bom_child_item_ids.ToString()) : System.Array.Empty<int>(); } catch { }
                try { bomAccessoryItemIds = qItem.bom_accessory_item_ids != null ? System.Text.Json.JsonSerializer.Deserialize<int[]>(qItem.bom_accessory_item_ids.ToString()) : System.Array.Empty<int>(); } catch { }

                var childItemList = new System.Collections.Generic.List<object>();

                if (bomChildItemIds.Length > 0)
                {
                    var childRows = await connection.QueryAsync<dynamic>(
                        @"SELECT bci.child_item_id as item_id, bci.quantity,
                                 im.item_name, im.item_code, im.unit_price, im.hsn, im.tax_percentage, im.cat_no,
                                 c.name as category_name,
                                 u.code as uom_name,
                                 m.name as make, mo.name as model, p.name as product,
                                 vm.name as valuation_method_name,
                                 ime.name as inventory_method_name,
                                 itt.name as inventory_type_name,
                                 rmi.purchase_rate, rmi.sales_rate as sale_rate, rmi.quotation_rate as quote_rate
                          FROM bill_of_material_child_items bci
                          JOIN item_master im ON bci.child_item_id = im.id
                          LEFT JOIN categories c ON im.category_id = c.id
                          LEFT JOIN uom u ON im.uom_id = u.id
                          LEFT JOIN make m ON im.make_id = m.id
                          LEFT JOIN model mo ON im.model_id = mo.id
                          LEFT JOIN product p ON im.product_id = p.id
                          LEFT JOIN valuation_method vm ON im.valuation_method_id = vm.id
                          LEFT JOIN inventory_method ime ON im.inventory_method_id = ime.id
                          LEFT JOIN inventory_types itt ON im.group_id = itt.id
                          LEFT JOIN rate_master_items rmi ON im.id = rmi.item_id
                          WHERE bci.bill_of_material_id = (SELECT id FROM bill_of_materials WHERE bom_id = @BomId LIMIT 1)",
                        new { BomId = qItem.bom_id });
                    foreach (var ci in childRows)
                    {
                        childItemList.Add(new
                        {
                            itemId = (int)ci.item_id,
                            quantity = ci.quantity,
                            categoryName = (string)ci.category_name,
                            groupName = (string)ci.inventory_type_name,
                            valuationMethodName = (string)ci.valuation_method_name,
                            inventoryMethodName = (string)ci.inventory_method_name,
                            inventoryTypeName = (string)ci.inventory_type_name,
                            unitPrice = ci.unit_price,
                            make = (string)ci.make,
                            model = (string)ci.model,
                            product = (string)ci.product,
                            itemName = (string)ci.item_name,
                            itemCode = (string)ci.item_code,
                            catNo = (string)ci.cat_no,
                            uomName = (string)ci.uom_name,
                            purchaseRate = ci.purchase_rate,
                            saleRate = ci.sale_rate,
                            quoteRate = ci.quote_rate,
                            hsn = (string)ci.hsn,
                            taxPercentage = ci.tax_percentage == -1 ? 0 : ci.tax_percentage,
                            quoteTitleId = (int?)null,
                            tcTemplateId = (int?)null
                        });
                    }
                }

                if (bomAccessoryItemIds.Length > 0)
                {
                    var accRows = await connection.QueryAsync<dynamic>(
                        @"SELECT im.id as item_id, im.item_name, im.item_code, im.unit_price, im.hsn, im.tax_percentage, im.cat_no,
                                 c.name as category_name, u.code as uom_name,
                                 m.name as make, mo.name as model, p.name as product,
                                 vm.name as valuation_method_name,
                                 ime.name as inventory_method_name,
                                 itt.name as inventory_type_name,
                                 rmi.purchase_rate, rmi.sales_rate as sale_rate, rmi.quotation_rate as quote_rate
                          FROM item_master im
                          LEFT JOIN categories c ON im.category_id = c.id
                          LEFT JOIN uom u ON im.uom_id = u.id
                          LEFT JOIN make m ON im.make_id = m.id
                          LEFT JOIN model mo ON im.model_id = mo.id
                          LEFT JOIN product p ON im.product_id = p.id
                          LEFT JOIN valuation_method vm ON im.valuation_method_id = vm.id
                          LEFT JOIN inventory_method ime ON im.inventory_method_id = ime.id
                          LEFT JOIN inventory_types itt ON im.group_id = itt.id
                          LEFT JOIN rate_master_items rmi ON im.id = rmi.item_id
                          WHERE im.id = ANY(@Ids)",
                        new { Ids = bomAccessoryItemIds });
                    foreach (var ai in accRows)
                    {
                        childItemList.Add(new
                        {
                            itemId = (int)ai.item_id,
                            quantity = (decimal)1,
                            categoryName = (string)ai.category_name,
                            groupName = (string)ai.inventory_type_name,
                            valuationMethodName = (string)ai.valuation_method_name,
                            inventoryMethodName = (string)ai.inventory_method_name,
                            inventoryTypeName = (string)ai.inventory_type_name,
                            unitPrice = ai.unit_price,
                            make = (string)ai.make,
                            model = (string)ai.model,
                            product = (string)ai.product,
                            itemName = (string)ai.item_name,
                            itemCode = (string)ai.item_code,
                            catNo = (string)ai.cat_no,
                            uomName = (string)ai.uom_name,
                            purchaseRate = ai.purchase_rate,
                            saleRate = ai.sale_rate,
                            quoteRate = ai.quote_rate,
                            hsn = (string)ai.hsn,
                            taxPercentage = ai.tax_percentage == -1 ? 0 : ai.tax_percentage,
                            quoteTitleId = (int?)null,
                            tcTemplateId = (int?)null
                        });
                    }
                }

                if (childItemList.Count > 0)
                {
                    items.Add(new
                    {
                        bomId = qItem.bom_id?.ToString() ?? "",
                        bomName = qItem.bom_name?.ToString() ?? "",
                        bomType = qItem.bom_type?.ToString() ?? "",
                        childItems = childItemList
                    });
                }
            }

            return new
            {
                salesOrder,
                quotation,
                purchaseOrder,
                items
            };
        }

        public async Task<SalesOrderWithQuotationAndItemsDto> GetSalesOrderWithQuotationAndItemsAsync(int id)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            var salesOrder = await connection.QueryFirstOrDefaultAsync<SalesOrder>(
                "SELECT * FROM sales_orders WHERE id = @Id",
                new { Id = id });
            if (salesOrder == null) return null;

            var quotation = await connection.QueryFirstOrDefaultAsync<SalesQuotation>(
                "SELECT * FROM sales_quotations WHERE id = @QuotationId",
                new { QuotationId = salesOrder.QuotationId });

            string customerName = string.Empty;
            string mobileNum = string.Empty;
            if (quotation != null && !string.IsNullOrEmpty(quotation.OpportunityId))
            {
                var opportunity = await connection.QueryFirstOrDefaultAsync<SalesOpportunity>(
                    "SELECT * FROM sales_opportunities WHERE opportunity_id = @OpportunityId",
                    new { OpportunityId = quotation.OpportunityId });
                if (opportunity != null)
                {
                    customerName = opportunity.CustomerName ?? string.Empty;
                    mobileNum = opportunity.ContactMobileNo ?? string.Empty;
                }
            }
            if (string.IsNullOrEmpty(customerName) && quotation != null && !string.IsNullOrEmpty(quotation.CustomerName))
                customerName = quotation.CustomerName;

            salesOrder.CustomerName = customerName;
            salesOrder.MobileNum = mobileNum;

            // Fetch PO details linked to this sales order
            PurchaseOrderDto purchaseOrder = null;
            if (!string.IsNullOrEmpty(salesOrder.PoId))
            {
                purchaseOrder = await connection.QueryFirstOrDefaultAsync<PurchaseOrderDto>(
                    "SELECT * FROM purchase_order WHERE TRIM(po_id) = TRIM(@PoId)",
                    new { PoId = salesOrder.PoId });
            }

            // Fetch items with full detail (make, model, product, category, uom, pricing)
            var items = (await connection.QueryAsync<SalesItemResponse>(
                @"SELECT sp.*, mk.name as make, md.name as model, pd.name as product, c.name as category,
                         im.item_name as ItemName, im.item_code as ItemCode, im.unit_price as UnitPrice,
                         u.code as Uom, im.tax_percentage as TaxPercentage
                  FROM sales_product sp
                  JOIN item_master im ON sp.item_id = im.id
                  LEFT JOIN make mk ON im.make_id = mk.id
                  LEFT JOIN model md ON im.model_id = md.id
                  LEFT JOIN product pd ON im.product_id = pd.id
                  LEFT JOIN categories c ON im.category_id = c.id
                  LEFT JOIN uom u ON im.uom_id = u.id
                  WHERE sp.stage = 'Quotation' AND sp.stage_item_id = @QuotationId",
                new { QuotationId = salesOrder.QuotationId.ToString() })).AsList();

            // Populate IncludedChildItems and AccessoriesItems for each item
            foreach (var item in items)
            {
                item.IncludedChildItems = (await connection.QueryAsync<SalesItemResponse>(
                    @"SELECT im.id, NULL as UserCreated, NULL as DateCreated, NULL as UserUpdated, NULL as DateUpdated,
                             NULL as Qty, NULL as Amount, im.is_active as IsActive, im.id as ItemId,
                             NULL as Stage, NULL as StageItemId,
                             mk.name as make, md.name as model, pd.name as product, c.name as category,
                             im.item_name as ItemName, im.item_code as ItemCode, im.unit_price as UnitPrice,
                             im.hsn, im.tax_percentage as TaxPercentage, u.code as uom,
                             NULL as ParentId, NULL as ParentItem, NULL as ReferencedBy,
                             NULL as IncludedChildItems, NULL as AccessoriesItems
                      FROM sales_product_child_items spci
                      JOIN item_master im ON spci.child_item_id = im.id
                      LEFT JOIN make mk ON im.make_id = mk.id
                      LEFT JOIN model md ON im.model_id = md.id
                      LEFT JOIN product pd ON im.product_id = pd.id
                      LEFT JOIN categories c ON im.category_id = c.id
                      LEFT JOIN uom u ON im.uom_id = u.id
                      WHERE spci.sales_product_id = @SalesProductId",
                    new { SalesProductId = item.Id })).ToList();

                item.AccessoriesItems = (await connection.QueryAsync<SalesItemResponse>(
                    @"SELECT im.id, NULL as UserCreated, NULL as DateCreated, NULL as UserUpdated, NULL as DateUpdated,
                             NULL as Qty, NULL as Amount, im.is_active as IsActive, im.id as ItemId,
                             NULL as Stage, NULL as StageItemId,
                             mk.name as make, md.name as model, pd.name as product, c.name as category,
                             im.item_name as ItemName, im.item_code as ItemCode, im.unit_price as UnitPrice,
                             im.hsn, im.tax_percentage as TaxPercentage, u.code as uom,
                             NULL as ParentId, NULL as ParentItem, NULL as ReferencedBy,
                             NULL as IncludedChildItems, NULL as AccessoriesItems
                      FROM sales_product_accessories spa
                      JOIN item_master im ON spa.accessories_item_id = im.id
                      LEFT JOIN make mk ON im.make_id = mk.id
                      LEFT JOIN model md ON im.model_id = md.id
                      LEFT JOIN product pd ON im.product_id = pd.id
                      LEFT JOIN categories c ON im.category_id = c.id
                      LEFT JOIN uom u ON im.uom_id = u.id
                      WHERE spa.sales_product_id = @SalesProductId",
                    new { SalesProductId = item.Id })).ToList();
            }

            // Only return top-level items
            var topLevelItems = items.Where(i => i.ParentId == 0 || i.ParentId == null).ToList();
            for (int i = 0; i < topLevelItems.Count; i++)
                topLevelItems[i] = ReplaceNullsWithDefaults(topLevelItems[i]);

            return new SalesOrderWithQuotationAndItemsDto
            {
                SalesOrder = salesOrder,
                Quotation = quotation,
                PurchaseOrder = purchaseOrder,
                Items = topLevelItems
            };
        }

        public async Task<IEnumerable<SalesOrderWithQuotationAndItemsDto>> GetAllSalesOrdersWithQuotationAndItemsAsync()
        {
            using var connection = new NpgsqlConnection(_connectionString);
            var salesOrders = (await connection.QueryAsync<SalesOrder>("SELECT * FROM sales_orders")).ToList();
            var result = new List<SalesOrderWithQuotationAndItemsDto>();
            foreach (var so in salesOrders)
            {
                var quotation = await connection.QueryFirstOrDefaultAsync<SalesQuotation>(
                    "SELECT * FROM sales_quotations WHERE id = @QuotationId",
                    new { QuotationId = so.QuotationId });
                string customerName = string.Empty;
                string mobileNum = string.Empty;
                if (quotation != null && !string.IsNullOrEmpty(quotation.OpportunityId))
                {
                    var opportunity = await connection.QueryFirstOrDefaultAsync<SalesOpportunity>(
                        "SELECT * FROM sales_opportunities WHERE opportunity_id = @OpportunityId",
                        new { OpportunityId = quotation.OpportunityId });
                    if (opportunity != null)
                    {
                        customerName = opportunity.CustomerName ?? string.Empty;
                        mobileNum = opportunity.ContactMobileNo ?? string.Empty;
                    }
                }
                if (string.IsNullOrEmpty(customerName) && quotation != null && !string.IsNullOrEmpty(quotation.CustomerName))
                {
                    customerName = quotation.CustomerName;
                }
                so.CustomerName = customerName;
                so.MobileNum = mobileNum;
                // Fetch items based on quotation id, not sales order id
                var items = (await connection.QueryAsync<SalesItemResponse>(
                    "SELECT sp.*, im.make, im.model, im.product, im.category, im.item_name as ItemName, im.item_code as ItemCode FROM sales_product sp JOIN item_master im ON sp.item_id = im.id WHERE sp.stage = 'Quotation' AND sp.stage_item_id = @QuotationId",
                    new { QuotationId = so.QuotationId.ToString() })).AsList();
                // Replace nulls with defaults
                for (int i = 0; i < items.Count; i++)
                    items[i] = ReplaceNullsWithDefaults(items[i]);
                result.Add(new SalesOrderWithQuotationAndItemsDto
                {
                    SalesOrder = so,
                    Quotation = quotation,
                    Items = items
                });
            }
            return result;
        }

        public async Task<SalesItemResponse> GetItemDetailsByItemIdAsync(int itemId)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            var item = await connection.QueryFirstOrDefaultAsync<SalesItemResponse>(
                "SELECT sp.*, im.make, im.model, im.product, im.category, im.item_name as ItemName, im.item_code as ItemCode FROM sales_product sp JOIN item_master im ON sp.item_id = im.id WHERE sp.item_id = @ItemId",
                new { ItemId = itemId });
            return ReplaceNullsWithDefaults(item);
        }

        public async Task<SalesOrder> GetSalesOrderByQuotationIdAsync(int quotationId)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<SalesOrder>(
                "SELECT * FROM sales_orders WHERE quotation_id = @QuotationId",
                new { QuotationId = quotationId });
        }

        public async Task<IEnumerable<QuotationGridDto>> GetQuotationGridAsync(QuotationGridSearchRequest request)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            var sql = "SELECT id, quotation_id, customer_name, date_created, status FROM sales_quotations WHERE is_active = true ORDER BY date_created DESC";
            var result = await connection.QueryAsync<QuotationGridDto>(sql);
            return result;
        }

        // Stub: Fetch Sales Order by Order ID
        public async Task<SalesOrderDetailsDto> GetByOrderIdAsync(string orderId)
        {
            // Stub: Replace with actual DB logic
            return new SalesOrderDetailsDto();
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
                    if (prop.PropertyType == typeof(string))
                        prop.SetValue(obj, "");
                    else if (prop.PropertyType == typeof(int) || prop.PropertyType == typeof(int?))
                        prop.SetValue(obj, 0);
                    else if (prop.PropertyType == typeof(decimal) || prop.PropertyType == typeof(decimal?))
                        prop.SetValue(obj, 0m);
                    else if (prop.PropertyType == typeof(bool) || prop.PropertyType == typeof(bool?))
                        prop.SetValue(obj, false);
                    else if (prop.PropertyType == typeof(DateTime) || prop.PropertyType == typeof(DateTime?))
                        prop.SetValue(obj, DateTime.MinValue);
                }
            }
            return obj;
        }
    }
}
