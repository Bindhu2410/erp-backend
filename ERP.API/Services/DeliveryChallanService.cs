using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using ERP.API.Models;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace ERP.API.Services
{
    public class DeliveryChallanService : IDeliveryChallanService
    {
        private readonly string _connectionString;

        public DeliveryChallanService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<IEnumerable<DeliveryChallanResponse>> GetAllAsync()
        {
            using var connection = new NpgsqlConnection(_connectionString);
            var sql = @"
                SELECT dc.*, 
                       so.order_id as SalesOrderNo, 
                       e.first_name || ' ' || e.last_name as SalesmanName, 
                       sl.customer_name as PartyName
                FROM delivery_challans dc
                LEFT JOIN sales_orders so ON dc.sales_order_id = so.id
                LEFT JOIN employees e ON dc.salesman_id = e.id
                LEFT JOIN sales_lead sl ON dc.party_id = sl.id
                ORDER BY dc.date_created DESC";
            
            return await connection.QueryAsync<DeliveryChallanResponse>(sql);
        }

        public async Task<DeliveryChallanResponse?> GetByIdAsync(int id)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            var sql = @"
                SELECT dc.*, 
                       so.order_id as SalesOrderNo, 
                       e.first_name || ' ' || e.last_name as SalesmanName, 
                       sl.customer_name as PartyName
                FROM delivery_challans dc
                LEFT JOIN sales_orders so ON dc.sales_order_id = so.id
                LEFT JOIN employees e ON dc.salesman_id = e.id
                LEFT JOIN sales_lead sl ON dc.party_id = sl.id
                WHERE dc.id = @Id";

            var response = await connection.QueryFirstOrDefaultAsync<DeliveryChallanResponse>(sql, new { Id = id });
            if (response != null)
            {
                response.ItemDetails = (await GetItemsByChallanId(connection, response.Id)).ToList();
            }
            return response;
        }

        public async Task<DeliveryChallanResponse> CreateAsync(DeliveryChallanRequest request)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();

            try
            {
                // Auto-generate delivery_challan_id if not provided
                string challanId = request.DeliveryChallanId;
                if (string.IsNullOrEmpty(challanId))
                {
                    var now = DateTime.UtcNow;
                    var yearShort = now.ToString("yy"); // last two digits of year
                    var countSql = "SELECT COUNT(*) FROM delivery_challans WHERE SUBSTRING(delivery_challan_id, 4, 2) = @YearShort";
                    var count = await connection.ExecuteScalarAsync<int>(countSql, new { YearShort = yearShort }, transaction);
                    var nextNumber = count + 1;
                    challanId = $"DC-{yearShort}-{nextNumber:D3}";
                }

                var sql = @"
                    INSERT INTO delivery_challans (
                        delivery_challan_id, delivery_date, sales_order_id, salesman_id, party_id, 
                        delivery_status, dispatch_address, priority, transporter_name, vehicle_no, 
                        driver_name, driver_contact, mode_of_delivery, notes,
                        location, form_20_sno, form_20_no, ref_no, ref_date, dispatched_by, delivered_by,
                        goods_consign_from, goods_consign_to, booking_address, booking_qty, app_value,
                        delivery_at, delivery_add1, delivery_add2, document_through, invoice_no, invoice_date,
                        gross_amount, net_amount, total_qty, amount_in_words, delivery_to, remarks,
                        prepared_by, authorized_by, received_by, user_created
                    ) VALUES (
                        @DeliveryChallanId, @DeliveryDate, @SalesOrderId, @SalesmanId, @PartyId, 
                        @DeliveryStatus, @DispatchAddress, @Priority, @TransporterName, @VehicleNo, 
                        @DriverName, @DriverContact, @ModeOfDelivery, @Notes,
                        @Location, @Form20SNo, @Form20No, @RefNo, @RefDate, @DispatchedBy, @DeliveredBy,
                        @GoodsConsignFrom, @GoodsConsignTo, @BookingAddress, @BookingQty, @AppValue,
                        @DeliveryAt, @DeliveryAdd1, @DeliveryAdd2, @DocumentThrough, @InvoiceNo, @InvoiceDate,
                        @GrossAmount, @NetAmount, @TotalQty, @AmountInWords, @DeliveryTo, @Remarks,
                        @PreparedBy, @AuthorizedBy, @ReceivedBy, @UserCreated
                    ) RETURNING id";

                var id = await connection.ExecuteScalarAsync<int>(sql, new
                {
                    DeliveryChallanId = challanId,
                    request.DeliveryDate,
                    request.SalesOrderId,
                    request.SalesmanId,
                    request.PartyId,
                    request.DeliveryStatus,
                    request.DispatchAddress,
                    request.Priority,
                    request.TransporterName,
                    request.VehicleNo,
                    request.DriverName,
                    request.DriverContact,
                    request.ModeOfDelivery,
                    request.Notes,
                    request.Location,
                    request.Form20SNo,
                    request.Form20No,
                    request.RefNo,
                    request.RefDate,
                    request.DispatchedBy,
                    request.DeliveredBy,
                    request.GoodsConsignFrom,
                    request.GoodsConsignTo,
                    request.BookingAddress,
                    request.BookingQty,
                    request.AppValue,
                    request.DeliveryAt,
                    request.DeliveryAdd1,
                    request.DeliveryAdd2,
                    request.DocumentThrough,
                    request.InvoiceNo,
                    request.InvoiceDate,
                    request.GrossAmount,
                    request.NetAmount,
                    request.TotalQty,
                    request.AmountInWords,
                    request.DeliveryTo,
                    request.Remarks,
                    request.PreparedBy,
                    request.AuthorizedBy,
                    request.ReceivedBy,
                    request.UserCreated
                }, transaction);

                if (request.Items != null && request.Items.Any())
                {
                    var itemSql = @"
                        INSERT INTO delivery_challan_items (
                            delivery_challan_id, item_id, qty, unit_price, amount,
                            so_no, make, category, product, model, visual_item_id,
                            equl_ins, match_no, ord_qty, current_stock, unit, user_created
                        ) VALUES (
                            @DeliveryChallanId, @ItemId, @Qty, @UnitPrice, @Amount,
                            @SoNo, @Make, @Category, @Product, @Model, @VisualItemId,
                            @EqulIns, @MatchNo, @OrdQty, @CurrentStock, @Unit, @UserCreated
                        )";

                    foreach (var item in request.Items)
                    {
                        await connection.ExecuteAsync(itemSql, new
                        {
                            DeliveryChallanId = id,
                            item.ItemId,
                            item.Qty,
                            item.UnitPrice,
                            item.Amount,
                            item.SoNo,
                            item.Make,
                            item.Category,
                            item.Product,
                            item.Model,
                            item.VisualItemId,
                            item.EqulIns,
                            item.MatchNo,
                            item.OrdQty,
                            item.CurrentStock,
                            item.Unit,
                            request.UserCreated
                        }, transaction);
                    }
                }

                transaction.Commit();
                return await GetByIdAsync(id) ?? throw new Exception("Failed to retrieve created record.");
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<DeliveryChallanResponse?> UpdateAsync(int id, DeliveryChallanRequest request)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();

            try
            {
                var sql = @"
                    UPDATE delivery_challans SET
                        delivery_date = @DeliveryDate,
                        sales_order_id = @SalesOrderId,
                        salesman_id = @SalesmanId,
                        party_id = @PartyId,
                        delivery_status = @DeliveryStatus,
                        dispatch_address = @DispatchAddress,
                        priority = @Priority,
                        transporter_name = @TransporterName,
                        vehicle_no = @VehicleNo,
                        driver_name = @DriverName,
                        driver_contact = @DriverContact,
                        mode_of_delivery = @ModeOfDelivery,
                        notes = @Notes,
                        location = @Location,
                        form_20_sno = @Form20SNo,
                        form_20_no = @Form20No,
                        ref_no = @RefNo,
                        ref_date = @RefDate,
                        dispatched_by = @DispatchedBy,
                        delivered_by = @DeliveredBy,
                        goods_consign_from = @GoodsConsignFrom,
                        goods_consign_to = @GoodsConsignTo,
                        booking_address = @BookingAddress,
                        booking_qty = @BookingQty,
                        app_value = @AppValue,
                        delivery_at = @DeliveryAt,
                        delivery_add1 = @DeliveryAdd1,
                        delivery_add2 = @DeliveryAdd2,
                        document_through = @DocumentThrough,
                        invoice_no = @InvoiceNo,
                        invoice_date = @InvoiceDate,
                        gross_amount = @GrossAmount,
                        net_amount = @NetAmount,
                        total_qty = @TotalQty,
                        amount_in_words = @AmountInWords,
                        delivery_to = @DeliveryTo,
                        remarks = @Remarks,
                        prepared_by = @PreparedBy,
                        authorized_by = @AuthorizedBy,
                        received_by = @ReceivedBy,
                        user_updated = @UserUpdated,
                        date_updated = CURRENT_TIMESTAMP
                    WHERE id = @Id";

                var affected = await connection.ExecuteAsync(sql, new
                {
                    Id = id,
                    request.DeliveryDate,
                    request.SalesOrderId,
                    request.SalesmanId,
                    request.PartyId,
                    request.DeliveryStatus,
                    request.DispatchAddress,
                    request.Priority,
                    request.TransporterName,
                    request.VehicleNo,
                    request.DriverName,
                    request.DriverContact,
                    request.ModeOfDelivery,
                    request.Notes,
                    request.Location,
                    request.Form20SNo,
                    request.Form20No,
                    request.RefNo,
                    request.RefDate,
                    request.DispatchedBy,
                    request.DeliveredBy,
                    request.GoodsConsignFrom,
                    request.GoodsConsignTo,
                    request.BookingAddress,
                    request.BookingQty,
                    request.AppValue,
                    request.DeliveryAt,
                    request.DeliveryAdd1,
                    request.DeliveryAdd2,
                    request.DocumentThrough,
                    request.InvoiceNo,
                    request.InvoiceDate,
                    request.GrossAmount,
                    request.NetAmount,
                    request.TotalQty,
                    request.AmountInWords,
                    request.DeliveryTo,
                    request.Remarks,
                    request.PreparedBy,
                    request.AuthorizedBy,
                    request.ReceivedBy,
                    request.UserUpdated
                }, transaction);

                if (affected == 0) return null;

                // Simple update for items: delete and re-insert
                await connection.ExecuteAsync("DELETE FROM delivery_challan_items WHERE delivery_challan_id = @Id", new { Id = id }, transaction);

                if (request.Items != null && request.Items.Any())
                {
                    var itemSql = @"
                        INSERT INTO delivery_challan_items (
                            delivery_challan_id, item_id, qty, unit_price, amount,
                            so_no, make, category, product, model, visual_item_id,
                            equl_ins, match_no, ord_qty, current_stock, unit, user_created
                        ) VALUES (
                            @DeliveryChallanId, @ItemId, @Qty, @UnitPrice, @Amount,
                            @SoNo, @Make, @Category, @Product, @Model, @VisualItemId,
                            @EqulIns, @MatchNo, @OrdQty, @CurrentStock, @Unit, @UserCreated
                        )";

                    foreach (var item in request.Items)
                    {
                        await connection.ExecuteAsync(itemSql, new
                        {
                            DeliveryChallanId = id,
                            item.ItemId,
                            item.Qty,
                            item.UnitPrice,
                            item.Amount,
                            item.SoNo,
                            item.Make,
                            item.Category,
                            item.Product,
                            item.Model,
                            item.VisualItemId,
                            item.EqulIns,
                            item.MatchNo,
                            item.OrdQty,
                            item.CurrentStock,
                            item.Unit,
                            UserCreated = request.UserUpdated
                        }, transaction);
                    }
                }

                transaction.Commit();
                return await GetByIdAsync(id);
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            var sql = "DELETE FROM delivery_challans WHERE id = @Id";
            var result = await connection.ExecuteAsync(sql, new { Id = id });
            return result > 0;
        }

        public async Task<DeliveryChallanGridResponse> GetGridAsync(DeliveryChallanGridRequest request)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            var offset = (request.Page - 1) * request.PageSize;
            
            var baseSql = @"
                FROM delivery_challans dc
                LEFT JOIN sales_orders so ON dc.sales_order_id = so.id
                LEFT JOIN employees e ON dc.salesman_id = e.id
                LEFT JOIN sales_lead sl ON dc.party_id = sl.id
                WHERE (1=1)";

            if (!string.IsNullOrEmpty(request.SearchText))
            {
                baseSql += " AND (dc.delivery_challan_id ILIKE @Search OR so.order_id ILIKE @Search OR sl.customer_name ILIKE @Search)";
            }

            if (!string.IsNullOrEmpty(request.Status))
            {
                baseSql += " AND dc.delivery_status = @Status";
            }

            var countSql = "SELECT COUNT(*) " + baseSql;
            var totalRecords = await connection.ExecuteScalarAsync<int>(countSql, new { Search = $"%{request.SearchText}%", Status = request.Status });

            var dataSql = @"
                SELECT dc.*, 
                       so.order_id as SalesOrderNo, 
                       e.first_name || ' ' || e.last_name as SalesmanName, 
                       sl.customer_name as PartyName " + 
                baseSql + 
                " ORDER BY dc.id DESC OFFSET @Offset LIMIT @Limit";

            var data = await connection.QueryAsync<DeliveryChallanResponse>(dataSql, new 
            { 
                Search = $"%{request.SearchText}%", 
                Status = request.Status,
                Offset = offset,
                Limit = request.PageSize
            });

            return new DeliveryChallanGridResponse
            {
                Data = data,
                TotalRecords = totalRecords
            };
        }

        private async Task<IEnumerable<DeliveryChallanItemResponse>> GetItemsByChallanId(NpgsqlConnection connection, int challanId)
        {
            var sql = @"
                SELECT dci.*, 
                       im.item_name as ItemName, 
                       im.item_code as ItemCode
                FROM delivery_challan_items dci
                JOIN item_master im ON dci.item_id = im.id
                WHERE dci.delivery_challan_id = @ChallanId";

            return await connection.QueryAsync<DeliveryChallanItemResponse>(sql, new { ChallanId = challanId });
        }
    }
}
