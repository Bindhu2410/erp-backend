using System.Threading.Tasks;
using ERP.API.Models;
using Dapper;
using System.Data;
using Npgsql;

namespace ERP.API.Services
{
    public class PurchaseOrderService : IPurchaseOrderService
    {
        private readonly IDbConnection _db;
        public PurchaseOrderService(IDbConnection db)
        {
            _db = db;
        }

        // Restored for compatibility with existing codebase
        public async Task<PurchaseOrderDetailsDto> GetByPoIdAsync(string poId)
        {
            var po = await _db.QueryFirstOrDefaultAsync<PurchaseOrderDto>(
                "SELECT * FROM purchase_order WHERE po_id = @poId", new { poId });
            if (po == null)
                return null;

            string vendorName = null;
            if (po.SupplierId != null)
            {
                vendorName = await _db.ExecuteScalarAsync<string>("SELECT vendor_name FROM suppliers WHERE id = @id", new { id = po.SupplierId });
            }

            var poItemsRaw = po != null ? (await _db.QueryAsync<PurchaseOrderItemDto>(
                "SELECT * FROM purchase_order_items WHERE purchase_order_id = @poId", new { poId = po.Id })).AsList() : new List<PurchaseOrderItemDto>();
            var poItems = new List<ERP.API.Models.SalesItemResponseDto>();
            if (poItemsRaw != null && poItemsRaw.Count > 0)
            {
                foreach (var item in poItemsRaw)
                {
                    var itemDetails = await _db.QueryFirstOrDefaultAsync<ItemMaster>(
                        "SELECT * FROM item_master WHERE id = @itemId", new { itemId = item.ItemId });
                    string? categoryName = null;
                    string? uomName = null;
                    if (itemDetails?.CategoryId != null)
                    {
                        var category = await _db.QueryFirstOrDefaultAsync<dynamic>("SELECT name FROM categories WHERE id = @id", new { id = itemDetails.CategoryId });
                        categoryName = category?.name;
                    }
                    if (itemDetails?.UomId != null)
                    {
                        try
                        {
                            var uom = await _db.QueryFirstOrDefaultAsync<dynamic>("SELECT code FROM uom WHERE id = @id", new { id = itemDetails.UomId });
                            uomName = uom?.code;
                        }
                        catch (Npgsql.PostgresException ex) when (ex.SqlState == "42P01")
                        {
                            uomName = null;
                        }
                    }
                    if (itemDetails != null)
                    {
                        poItems.Add(new ERP.API.Models.SalesItemResponseDto
                        {
                            Id = itemDetails.Id,
                            ItemId = itemDetails.Id,
                            Qty = item.Quantity,
                            Make = itemDetails.Make,
                            Model = itemDetails.Model,
                            CategoryName = categoryName,
                            Product = itemDetails.Product,
                            Brand = itemDetails.Brand,
                            ItemName = itemDetails.ItemName,
                            ItemCode = itemDetails.ItemCode,
                            UnitPrice = itemDetails.UnitPrice,
                            Hsn = itemDetails.Hsn,
                            TaxPercentage = itemDetails.TaxPercentage ?? 0,
                            UomName = uomName,
                            CatNo = itemDetails.CatNo,
                            ValuationMethodName = null // Add logic if needed
                        });
                    }
                }
            }
            else
            {
                poItems = new List<ERP.API.Models.SalesItemResponseDto>();
            }
            return new PurchaseOrderDetailsDto
            {
                PurchaseOrder = po,
                Items = poItems,
                VendorName = vendorName
            };
        }
        private async Task<string?> GetValuationMethodName(int? valuationMethodId)
        {
            if (valuationMethodId == null) return null;
            try
            {
                var method = await _db.QueryFirstOrDefaultAsync<dynamic>("SELECT name FROM valuation_methods WHERE id = @id", new { id = valuationMethodId });
                return method?.name;
            }
            catch (Npgsql.PostgresException ex) when (ex.SqlState == "42P01")
            {
                return null;
            }
        }

        public async Task<List<SalesItemResponse>> GetDetailsByPoIdAsync(string poId)
        {
            // Example stub: fetch items for a PO
            var po = await _db.QueryFirstOrDefaultAsync<PurchaseOrderDto>(
                "SELECT * FROM purchase_order WHERE po_id = @poId", new { poId });
            if (po == null)
                return new List<SalesItemResponse>();
            var poItemsRaw = (await _db.QueryAsync<PurchaseOrderItemDto>(
                "SELECT * FROM purchase_order_items WHERE purchase_order_id = @poId", new { poId = po.Id })).AsList();
            var items = new List<SalesItemResponse>();
            foreach (var item in poItemsRaw)
            {
                var itemDetails = await _db.QueryFirstOrDefaultAsync<ItemMaster>(
                    "SELECT * FROM item_master WHERE id = @itemId", new { itemId = item.ItemId });
                string? categoryName = null;
                string? uomName = null;
                string? valuationMethodName = null;
                if (itemDetails?.CategoryId != null)
                {
                    var category = await _db.QueryFirstOrDefaultAsync<dynamic>("SELECT name FROM categories WHERE id = @id", new { id = itemDetails.CategoryId });
                    categoryName = category?.name;
                }
                if (itemDetails?.UomId != null)
                {
                    var uom = await _db.QueryFirstOrDefaultAsync<dynamic>("SELECT code FROM uoms WHERE id = @id", new { id = itemDetails.UomId });
                    uomName = uom?.code;
                }
                if (itemDetails?.ValuationMethodId != null)
                {
                    var valuationMethod = await _db.QueryFirstOrDefaultAsync<dynamic>("SELECT name FROM valuation_methods WHERE id = @id", new { id = itemDetails.ValuationMethodId });
                    valuationMethodName = valuationMethod?.name;
                }
                if (itemDetails != null)
                {
                    items.Add(new SalesItemResponse
                    {
                        Id = itemDetails.Id,
                        ItemId = itemDetails.Id,
                        Qty = item.Quantity,
                        Make = itemDetails.Make,
                        Model = itemDetails.Model,
                        Category = categoryName,
                        Product = itemDetails.Product,
                        Brand = itemDetails.Brand,
                        ItemName = itemDetails.ItemName,
                        ItemCode = itemDetails.ItemCode,
                        UnitPrice = itemDetails.UnitPrice,
                        Hsn = itemDetails.Hsn,
                        TaxPercentage = itemDetails.TaxPercentage,
                        Uom = uomName,
                        CatNo = itemDetails.CatNo,
                        ValuationMethodName = valuationMethodName,
                        IsActive = itemDetails.IsActive,
                        ImageUrl = itemDetails.ImageUrl
                    });
                }
            }
            return items;
        }

        public async Task<string> GetInvoiceIdByPoIdAsync(string poId)
        {
            return await _db.ExecuteScalarAsync<string>(
                "SELECT invoice_id FROM purchase_order WHERE po_id = @poId LIMIT 1", new { poId });
        }

        public async Task<IEnumerable<PurchaseOrderDto>> GetAllAsync()
        {
            var sql = "SELECT * FROM purchase_order";
            var purchaseOrders = await _db.QueryAsync<PurchaseOrderDto>(sql);
            return purchaseOrders;
        }

        public async Task<PurchaseOrderDto> GetByIdAsync(int id)
        {
            var sql = "SELECT * FROM purchase_order WHERE id = @id";
            var purchaseOrder = await _db.QueryFirstOrDefaultAsync<PurchaseOrderDto>(sql, new { id });
            return purchaseOrder;
        }

        public async Task<PurchaseOrderDto> CreateAsync(PurchaseOrderDto purchaseOrder)
        {
            var now = System.DateTime.Now;
            var fiscalYearStart = now.Month >= 4 ? now.Year : now.Year - 1;
            var fiscalYearEnd = fiscalYearStart + 1;
            var fyShort = fiscalYearStart % 100;
            var fyEndShort = fiscalYearEnd % 100;
            var seqSql = "SELECT COALESCE(MAX(CAST(SPLIT_PART(po_id, '/', 3) AS INTEGER)), 0) + 1 FROM purchase_order WHERE po_id LIKE @fyPattern";
            var fyPattern = $"PO/{fyShort}-{fyEndShort}/%";
            var seq = await _db.ExecuteScalarAsync<int>(seqSql, new { fyPattern });
            var poId = $"PO/{fyShort}-{fyEndShort}/{seq:D2}";
            var insertSql = @"INSERT INTO purchase_order (sales_order_id, status, po_id, quotation_id, delivery_date, user_created, date_created, user_updated, date_updated, purchase_requisition_id, supplier_id, description) VALUES (@SalesOrderId, @Status, @PoId, @QuotationId, @DeliveryDate, @UserCreated, NOW(), @UserUpdated, NOW(), @PurchaseRequisitionId, @SupplierId, @Description) RETURNING *";
            var created = await _db.QueryFirstOrDefaultAsync<PurchaseOrderDto>(insertSql, new
            {
                SalesOrderId = purchaseOrder.SalesOrderId,
                Status = purchaseOrder.Status ?? "Received",
                PoId = poId,
                QuotationId = purchaseOrder.QuotationId,
                DeliveryDate = purchaseOrder.DeliveryDate,
                UserCreated = purchaseOrder.UserCreated,
                UserUpdated = purchaseOrder.UserUpdated,
                PurchaseRequisitionId = purchaseOrder.PurchaseRequisitionId,
                SupplierId = purchaseOrder.SupplierId,
                Description = purchaseOrder.Description
            });

            // Store items in purchase_order_items table
            if (created != null && purchaseOrder.Items != null && purchaseOrder.Items.Count > 0)
            {
                foreach (var item in purchaseOrder.Items)
                {
                    var insertItemSql = @"INSERT INTO purchase_order_items (purchase_order_id, item_id, quantity) VALUES (@PurchaseOrderId, @ItemId, @Quantity)";
                    await _db.ExecuteAsync(insertItemSql, new
                    {
                        PurchaseOrderId = created.Id,
                        ItemId = item.ItemId,
                        Quantity = item.Quantity
                    });
                }
            }

            // Update the purchase requisition status to "PO Created"
            if (created != null && !string.IsNullOrEmpty(purchaseOrder.PurchaseRequisitionId))
            {
                await UpdatePurchaseRequisitionStatus(purchaseOrder.PurchaseRequisitionId, "PO Created");
            }

            return created;
        }

        public async Task<PurchaseOrderDto> UpdateAsync(int id, PurchaseOrderDto purchaseOrder)
        {
            var sql = @"UPDATE purchase_order SET sales_order_id = @SalesOrderId, status = @Status, quotation_id = @QuotationId, delivery_date = @DeliveryDate, user_updated = @UserUpdated, date_updated = NOW() WHERE id = @Id RETURNING *";
            var updated = await _db.QueryFirstOrDefaultAsync<PurchaseOrderDto>(sql, new
            {
                Id = id,
                SalesOrderId = purchaseOrder.SalesOrderId,
                Status = purchaseOrder.Status,
                QuotationId = purchaseOrder.QuotationId,
                DeliveryDate = purchaseOrder.DeliveryDate,
                UserUpdated = purchaseOrder.UserUpdated
            });
            return updated;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var sql = "DELETE FROM purchase_order WHERE id = @id";
            var rows = await _db.ExecuteAsync(sql, new { id });
            return rows > 0;
        }

        public async Task<bool> UpdateStatusAsync(int id, string status)
        {
            var sql = "UPDATE purchase_order SET status = @Status, date_updated = NOW() WHERE id = @Id";
            var rows = await _db.ExecuteAsync(sql, new { Id = id, Status = status });
            return rows > 0;
        }

        private async Task UpdatePurchaseRequisitionStatus(string prId, string status)
        {
            var sql = "UPDATE purchase_requisitions SET status = @status WHERE purchase_requisition_id = @prId";
            await _db.ExecuteAsync(sql, new { status, prId });
        }

        public async Task<IEnumerable<PurchaseRequisition>> GetApprovedPRsForDropdown()
        {
            var sql = @"SELECT * FROM purchase_requisitions pr
                        WHERE pr.status = 'Approved'
                        AND NOT EXISTS (
                            SELECT 1 FROM purchase_order po
                            WHERE po.purchase_requisition_id = pr.purchase_requisition_id
                        )";
            var prList = (await _db.QueryAsync<PurchaseRequisition>(sql)).ToList();
            // Only return PRs, items can be fetched separately by id
            return prList;
        }

        // Fetch items for a requisition for dropdown
        public async Task<List<SalesItemResponse>> GetItemsByRequisitionIdAsync(int requisitionId)
        {
            var items = new List<SalesItemResponse>();
            // Fetch items directly from purchase_requisition_bom for the requisition
            var bomSql = "SELECT * FROM purchase_requisition_boms WHERE purchase_requisition_id = @requisitionId";
            var bomItems = (await _db.QueryAsync<PurchaseRequisitionBom>(bomSql, new { requisitionId })).ToList();
            foreach (var bom in bomItems)
            {
                var itemDetails = await _db.QueryFirstOrDefaultAsync<ItemMaster>("SELECT * FROM item_master WHERE id = @itemId", new { itemId = bom.ItemId });
                string categoryName = null;
                string uomName = null;
                if (itemDetails?.CategoryId != null)
                {
                    var category = await _db.QueryFirstOrDefaultAsync<dynamic>("SELECT name FROM categories WHERE id = @id", new { id = itemDetails.CategoryId });
                    categoryName = category?.name;
                }
                if (itemDetails?.UomId != null)
                {
                    var uom = await _db.QueryFirstOrDefaultAsync<dynamic>("SELECT code FROM uom WHERE id = @id", new { id = itemDetails.UomId });
                    uomName = uom?.code;
                }
                if (itemDetails != null)
                {
                    items.Add(new SalesItemResponse
                    {
                        Id = itemDetails.Id,
                        ItemId = itemDetails.Id,
                        Qty = bom.Quantity,
                        Make = itemDetails.Make,
                        Model = itemDetails.Model,
                        CategoryName = categoryName,
                        Product = itemDetails.Product,
                        Brand = itemDetails.Brand,
                        ItemName = itemDetails.ItemName,
                        ItemCode = itemDetails.ItemCode,
                        UnitPrice = itemDetails.UnitPrice,
                        Hsn = itemDetails.Hsn,
                        TaxPercentage = itemDetails.TaxPercentage,
                        UomName = uomName,
                        CatNo = itemDetails.CatNo,
                        ValuationMethodName = null // Add logic if needed
                    });
                }
            }
            return items;
        }

        /// <summary>
        /// Creates a purchase order from a quotation with the given quotation ID.
        /// Extracts items from the quotation and creates a PO with auto-generated PO ID.
        /// </summary>
        /// <param name="quotationId">The ID of the quotation to create PO from</param>
        /// <param name="userId">The user ID for audit purposes</param>
        /// <returns>The created PurchaseOrderDto or null if quotation not found</returns>
        public async Task<PurchaseOrderDto> CreatePurchaseOrderFromQuotationAsync(int quotationId, int? userId)
        {
            // Fetch the quotation with all details
            var quotation = await _db.QueryFirstOrDefaultAsync<dynamic>(
                @"SELECT sq.id, sq.valid_till, sq.quotation_id
                 FROM sales_quotations sq 
                 WHERE sq.id = @QuotationId",
                new { QuotationId = quotationId });

            if (quotation == null)
                return null;

            // Check if PO already exists for this quotation
            var existingPo = await _db.QueryFirstOrDefaultAsync<PurchaseOrderDto>(
                "SELECT * FROM purchase_order WHERE quotation_id = @QuotationId",
                new { QuotationId = quotationId });

            if (existingPo != null)
                return existingPo; // PO already exists, return it

            // Fetch items associated with this quotation from sales_product
            var quotationItems = await _db.QueryAsync<dynamic>(
                @"SELECT sp.id, sp.bom_id, sp.qty
                  FROM sales_product sp
                  LEFT JOIN bill_of_materials bom ON sp.bom_id = bom.bom_id
                  WHERE sp.stage = 'Quotation' AND sp.stage_item_id = @QuotationIdStr",
                new { QuotationIdStr = quotationId.ToString() });

            // Get BOM child items (these are the actual items to purchase)
            var itemsToAdd = new List<PurchaseOrderItemDto>();
            foreach (var qItem in quotationItems)
            {
                // Cast bom_id to int, handling both int and string types
                int? bomId = null;
                if (qItem.bom_id != null)
                {
                    if (int.TryParse(qItem.bom_id.ToString(), out int parsedBomId))
                    {
                        bomId = parsedBomId;
                    }
                }

                if (bomId.HasValue)
                {
                    // Fetch child items from BOM
                    var bomChildItems = await _db.QueryAsync<dynamic>(
                        @"SELECT child_item_id FROM bill_of_material_child_items 
                          WHERE bill_of_material_id = (SELECT id FROM bill_of_materials WHERE bom_id = @BomId)",
                        new { BomId = bomId });

                    foreach (var childItem in bomChildItems)
                    {
                        itemsToAdd.Add(new PurchaseOrderItemDto
                        {
                            ItemId = (int)childItem.child_item_id,
                            Quantity = qItem.qty ?? 1 // Use quantity from sales_product
                        });
                    }
                }
            }

            // Generate PO ID in format PO/FY-FY/##
            var now = DateTime.Now;
            var fiscalYearStart = now.Month >= 4 ? now.Year : now.Year - 1;
            var fiscalYearEnd = fiscalYearStart + 1;
            var fyShort = fiscalYearStart % 100;
            var fyEndShort = fiscalYearEnd % 100;
            var seqSql = "SELECT COALESCE(MAX(CAST(SPLIT_PART(po_id, '/', 3) AS INTEGER)), 0) + 1 FROM purchase_order WHERE po_id LIKE @fyPattern";
            var fyPattern = $"PO/{fyShort}-{fyEndShort}/%";
            var seq = await _db.ExecuteScalarAsync<int>(seqSql, new { fyPattern });
            var poId = $"PO/{fyShort}-{fyEndShort}/{seq:D2}";

            // Use valid_till as delivery_date if available, otherwise use NULL
            DateTime? deliveryDate = quotation.valid_till;

            // Create the purchase order without supplier (can be set later)
            var insertSql = @"INSERT INTO purchase_order 
                (po_id, quotation_id, status, supplier_id, delivery_date, user_created, date_created, user_updated, date_updated, description) 
                VALUES (@PoId, @QuotationId, @Status, @SupplierId, @DeliveryDate, @UserCreated, NOW(), @UserUpdated, NOW(), @Description) 
                RETURNING *";
            
            var createdPo = await _db.QueryFirstOrDefaultAsync<PurchaseOrderDto>(insertSql, new
            {
                PoId = poId,
                QuotationId = quotationId,
                Status = "Open",
                SupplierId = (int?)null,
                DeliveryDate = deliveryDate,
                UserCreated = userId,
                UserUpdated = userId,
                Description = $"Created from Quotation {quotation.quotation_id}"
            });

            // Insert items into purchase_order_items if there are any
            if (createdPo != null && itemsToAdd.Count > 0)
            {
                foreach (var item in itemsToAdd)
                {
                    var insertItemSql = @"INSERT INTO purchase_order_items (purchase_order_id, item_id, quantity) 
                                         VALUES (@PurchaseOrderId, @ItemId, @Quantity)";
                    await _db.ExecuteAsync(insertItemSql, new
                    {
                        PurchaseOrderId = createdPo.Id,
                        ItemId = item.ItemId,
                        Quantity = item.Quantity
                    });
                }
            }

            return createdPo;
        }

        /// <summary>
        /// Gets purchase order details by quotation ID (internal quotation id).
        /// </summary>
        /// <param name="quotationId">The internal quotation ID</param>
        /// <returns>PurchaseOrderDetailsDto with PO and items from quotation's sales_product, or null if not found</returns>
        public async Task<PurchaseOrderDetailsDto> GetByQuotationIdAsync(int quotationId)
        {
            var po = await _db.QueryFirstOrDefaultAsync<PurchaseOrderDto>(
                "SELECT * FROM purchase_order WHERE quotation_id = @QuotationId", new { QuotationId = quotationId });
            
            if (po == null)
                return null;

            // Get vendor name if supplier is set
            string vendorName = null;
            if (po.SupplierId != null)
            {
                vendorName = await _db.ExecuteScalarAsync<string>("SELECT vendor_name FROM suppliers WHERE id = @id", new { id = po.SupplierId });
            }

            // Get items from sales_product (quotation items) instead of purchase_order_items
            var quotationItems = await _db.QueryAsync<dynamic>(
                @"SELECT sp.id, sp.bom_id, sp.qty, bom.bom_name, bom.bom_type
                  FROM sales_product sp
                  LEFT JOIN bill_of_materials bom ON sp.bom_id = bom.bom_id
                  WHERE sp.stage = 'Quotation' AND sp.stage_item_id = @QuotationId",
                new { QuotationId = quotationId.ToString() });
            
            var poItems = new List<ERP.API.Models.SalesItemResponseDto>();
            foreach (var qItem in quotationItems)
            {
                // Get BOM child items details
                var bomChildItems = await _db.QueryAsync<dynamic>(
                    @"SELECT im.id, im.item_name, im.item_code, im.unit_price, im.hsn, im.tax_percentage, 
                             im.category_id, im.uom_id, im.make_id, im.model_id, im.product_id, 
                             im.brand, im.cat_no, c.name as category_name, u.code as uom_code,
                             m.name as make, mo.name as model, p.name as product
                      FROM item_master im
                      LEFT JOIN categories c ON im.category_id = c.id
                      LEFT JOIN uom u ON im.uom_id = u.id
                      LEFT JOIN make m ON im.make_id = m.id
                      LEFT JOIN model mo ON im.model_id = mo.id
                      LEFT JOIN product p ON im.product_id = p.id
                      WHERE im.id IN (
                        SELECT child_item_id FROM bill_of_material_child_items 
                        WHERE bill_of_material_id = (SELECT id FROM bill_of_materials WHERE bom_id = @BomId)
                      )",
                    new { BomId = qItem.bom_id });

                foreach (var childItem in bomChildItems)
                {
                    poItems.Add(new ERP.API.Models.SalesItemResponseDto
                    {
                        Id = childItem.id,
                        ItemId = childItem.id,
                        Qty = qItem.qty ?? 1,
                        Make = childItem.make,
                        Model = childItem.model,
                        CategoryName = childItem.category_name,
                        Product = childItem.product,
                        Brand = childItem.brand,
                        ItemName = childItem.item_name,
                        ItemCode = childItem.item_code,
                        UnitPrice = childItem.unit_price,
                        Hsn = childItem.hsn,
                        TaxPercentage = childItem.tax_percentage ?? 0,
                        UomName = childItem.uom_code,
                        CatNo = childItem.cat_no,
                        ValuationMethodName = null
                    });
                }
            }
            
            return new PurchaseOrderDetailsDto
            {
                PurchaseOrder = po,
                Items = poItems,
                VendorName = vendorName
            };
        }
    }
}
