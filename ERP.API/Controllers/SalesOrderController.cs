using System;
using Dapper;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ERP.API.Models;
using ERP.API.Models.DTOs;
using ERP.API.Services;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SalesOrderController : ControllerBase
    {
        private readonly ISalesOrderService _salesOrderService;
        private readonly IPurchaseOrderService _purchaseOrderService;
        private readonly IConfiguration _configuration;

        public SalesOrderController(ISalesOrderService salesOrderService, IPurchaseOrderService purchaseOrderService, IConfiguration configuration)
        {
            _salesOrderService = salesOrderService;
            _purchaseOrderService = purchaseOrderService;
            _configuration = configuration;
        }

        /// <summary>
        /// Gets SalesOrder, Quotation, and Items by Purchase Order internal ID.
        /// </summary>
        [HttpGet("by-po/{purchaseOrderId}")]
        public async Task<ActionResult<SalesOrderWithQuotationAndItemsDto>> GetByPurchaseOrderId(int purchaseOrderId)
        {
            if (purchaseOrderId <= 0)
                return BadRequest(new { message = "purchaseOrderId is required." });

            // Get PO details to find the po_id string
            var po = await _purchaseOrderService.GetByIdAsync(purchaseOrderId);
            if (po == null)
                return NotFound(new { message = $"Purchase Order with ID {purchaseOrderId} not found" });

            var result = await _salesOrderService.GetSalesOrderWithQuotationAndItemsByPoIdAsync(po.PoId);
            if (result == null)
                return NotFound(new { message = $"No sales order found for Purchase Order ID {purchaseOrderId}" });

            return Ok(result);
        }
        /// <summary>
        /// Creates a new Sales Order from a Purchase Order ID.
        /// </summary>
        [HttpPost("from-purchase-order")]
        public async Task<IActionResult> CreateSalesOrderFromPurchaseOrder([FromBody] CreateSalesOrderFromPoRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.PurchaseOrderId))
                return BadRequest(new { message = "PurchaseOrderId is required." });

            var poDetails = await _purchaseOrderService.GetByPoIdAsync(request.PurchaseOrderId);
            if (poDetails == null || poDetails.PurchaseOrder == null)
                return NotFound(new { message = $"Purchase Order with ID '{request.PurchaseOrderId}' not found." });
            var po = poDetails.PurchaseOrder;

            // 1.5. Restrict: Only one sales order per purchase order
            var existingOrder = await _salesOrderService.GetSalesOrderByPoIdAsync(po.PoId);
            if (existingOrder != null && existingOrder.Id > 0)
            {
                return BadRequest(new { message = $"A sales order has already been created for this purchase order (PO ID: {po.PoId})." });
            }

            var newOrder = new SalesOrder
            {
                OrderDate = DateTimeOffset.UtcNow,
                Status = "Created",
                QuotationId = po.QuotationId,
                PoId = po.PoId,
                AcceptanceDate = null,
                TotalAmount = 0,
                TaxAmount = 0,
                GrandTotal = 0,
                Notes = null,
                UserCreated = 1,
                DateCreated = DateTimeOffset.UtcNow
            };

            // 3. Create the SalesOrder in DB
            var createdOrder = await _salesOrderService.CreateSalesOrderAsync(newOrder);
            if (createdOrder == null)
                return StatusCode(500, new { message = "Failed to create sales order." });

            // 4. Optionally copy quotation items as order items (if needed)
            // Removed: CopyQuotationItemsToOrder, since Quotation is not available

            // 5. Update the purchase order's sales_order_id to the new sales order's OrderId (string)
            if (!string.IsNullOrEmpty(createdOrder.OrderId))
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                using (var connection = new Npgsql.NpgsqlConnection(connectionString))
                {
                    // Use po_id for the WHERE clause to match the table's primary key
                    await connection.ExecuteAsync("UPDATE purchase_order SET sales_order_id = @SalesOrderId WHERE po_id = @PoId", new { SalesOrderId = createdOrder.OrderId, PoId = po.PoId });
                }
            }

            // 6. Update the purchase order status to 'Confirmed'
            // await _purchaseOrderService.UpdatePurchaseOrderStatusAsync(po.PoId, "Confirmed");

            // 7. Fetch and return PO details with items
            var poDetailsWithItems = await _purchaseOrderService.GetByPoIdAsync(request.PurchaseOrderId);
            void EnsureItemChildren(List<ERP.API.Models.SalesItemResponseDto> itemList)
            {
                if (itemList == null) return;
                // If you have child items or accessories in SalesItemResponseDto, handle them here
            }
            EnsureItemChildren(poDetailsWithItems?.Items);
            return Ok(new {
                SalesOrder = createdOrder,
                Items = poDetailsWithItems?.Items ?? new List<ERP.API.Models.SalesItemResponseDto>()
            });
        }

        public class CreateSalesOrderFromPoRequest
        {
            public string PurchaseOrderId { get; set; }
        }

        /// <summary>
        /// Updates the po_id for a sales order after PO creation.
        /// </summary>
        [HttpPost("update-po-id")]
        public async Task<IActionResult> UpdateSalesOrderPoId([FromBody] UpdateSalesOrderPoIdRequest request)
        {
            if (request == null || request.SalesOrderId <= 0 || request.PoId <= 0)
                return BadRequest(new { message = "Invalid sales order id or PO id." });

            var updated = await _salesOrderService.UpdateSalesOrderPoIdAsync(request.SalesOrderId, request.PoId);
            if (!updated)
                return NotFound(new { message = "Sales order not found or update failed." });

            return Ok(new { message = "PO ID updated successfully." });
        }

        /// <summary>
        /// DTO for updating po_id in sales_orders
        /// </summary>
        public class UpdateSalesOrderPoIdRequest
        {
            public int SalesOrderId { get; set; }
            public int PoId { get; set; }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SalesOrderGrid>>> GetAll()
        {
            var result = await _salesOrderService.GetAllSalesOrdersAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var details = await _salesOrderService.GetSalesOrderDetailAsync(id);
            if (details == null)
                return NotFound(new { message = $"Sales order with ID {id} not found." });
            return Ok(details);
        }

        [HttpPost]
        public async Task<ActionResult<SalesOrder>> Create([FromBody] SalesOrder salesOrder)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _salesOrderService.CreateSalesOrderAsync(salesOrder);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] SalesOrder salesOrder)
        {
            if (id != salesOrder.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _salesOrderService.UpdateSalesOrderAsync(salesOrder);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _salesOrderService.DeleteSalesOrderAsync(id);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }        [HttpGet("quotation/{id}")]
        public async Task<ActionResult<QuotationWithOrderResponse>> GetQuotationById(int id)
        {
            try
            {
                var quotation = await _salesOrderService.GetQuotationByIdAsync(id);
                if (quotation == null)
                {
                    return NotFound(new
                    {
                        message = $"Quotation with ID {id} not found",
                        statusCode = 404
                    });
                }
                return Ok(quotation);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Failed to retrieve quotation",
                    error = ex.Message,
                    statusCode = 500
                });
            }
        }
        [HttpGet("by-quotation/{quotationId}")]
        public async Task<IActionResult> CreateOrderFromQuotation(int quotationId)
        {
            var quotationWithOrder = await _salesOrderService.GetQuotationByIdAsync(quotationId);
            if (quotationWithOrder == null || quotationWithOrder.Quotation == null)
                return NotFound(new { message = $"Quotation with ID {quotationId} not found" });

            // Prevent creating another sales order for the same quotation
            if (quotationWithOrder.SalesOrder != null && quotationWithOrder.SalesOrder.Id > 0)
                return BadRequest(new { message = "A sales order has already been created for this quotation." });

            // Double check in DB if any sales order exists for this quotationId
            var existingOrder = await _salesOrderService.GetSalesOrderByQuotationIdAsync(quotationId);
            if (existingOrder != null && existingOrder.Id > 0)
                return BadRequest(new { message = "A sales order has already been created for this quotation (DB check)." });

            var quotation = quotationWithOrder.Quotation;
            // Debug log for status value
            Console.WriteLine($"[DEBUG] Quotation status for ID {quotationId}: '{quotation.Status}'");
            if (quotation.Status == null)
                return BadRequest(new { message = $"Quotation status is missing for ID: {quotationId}" });
            if (!string.Equals(quotation.Status.Trim(), "Approved", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { message = $"Quotation is not approved. Actual status: '{quotation.Status}'" });

            // 2. Prepare SalesOrder object
            var newOrder = new SalesOrder
            {
                CustomerId = quotation.CustomerId,
                OrderDate = DateTimeOffset.UtcNow,
                Status = "Draft", // Set status to Draft as required
                QuotationId = quotation.Id,
                PoId = null,
                AcceptanceDate = null,
                TotalAmount = quotation.TotalAmount ?? 0,
                TaxAmount = quotation.TaxAmount ?? 0,
                GrandTotal = quotation.GrandTotal ?? 0,
                Notes = quotation.Comments,
                UserCreated = 1, // System user
                DateCreated = DateTimeOffset.UtcNow
            };

            // 3. Create the SalesOrder in DB
            var createdOrder = await _salesOrderService.CreateSalesOrderAsync(newOrder);
            if (createdOrder == null)
                return StatusCode(500, new { message = "Failed to create sales order." });

            // 4. Copy quotation items as order items
            await _salesOrderService.CopyQuotationItemsToOrder(quotationId, createdOrder.Id);

            // 5. Fetch quotation info and items
            var quotationInfo = quotation; // Already fetched above
            var items = await _salesOrderService.GetQuotationItemsAsync(quotationId);

            // 6. Return the created order, quotation info, and items
            return Ok(new {
                SalesOrder = createdOrder,
                Quotation = quotationInfo,
                Items = items
            });
        }
        [HttpPost("from-approved-quotation/{quotationId}")]
        public async Task<IActionResult> CreateOrderFromApprovedQuotation(int quotationId)
        {
            // 1. Fetch the quotation and its items
            var quotationWithOrder = await _salesOrderService.GetQuotationByIdAsync(quotationId);
            if (quotationWithOrder == null || quotationWithOrder.Quotation == null)
                return NotFound(new { message = $"Quotation with ID {quotationId} not found" });

            var quotation = quotationWithOrder.Quotation;
            if (!string.Equals(quotation.Status, "Approved", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { message = $"Quotation is not approved. Actual status: '{quotation.Status}'" });

            // 2. Prepare SalesOrder object
            var newOrder = new SalesOrder
            {
                CustomerId = quotation.CustomerId,
                OrderDate = DateTimeOffset.UtcNow,
                Status = "Created",
                QuotationId = quotation.Id,
                PoId = null,
                AcceptanceDate = null,
                TotalAmount = quotation.TotalAmount ?? 0,
                TaxAmount = quotation.TaxAmount ?? 0,
                GrandTotal = quotation.GrandTotal ?? 0,
                Notes = quotation.Comments,
                UserCreated = 1, // System user
                DateCreated = DateTimeOffset.UtcNow
            };

            // 3. Create the SalesOrder in DB
            var createdOrder = await _salesOrderService.CreateSalesOrderAsync(newOrder);
            if (createdOrder == null)
                return StatusCode(500, new { message = "Failed to create sales order." });

            // 4. Copy quotation items as order items
            await _salesOrderService.CopyQuotationItemsToOrder(quotationId, createdOrder.Id);

            // 5. Return the created order
            return Ok(createdOrder);
        }

        [HttpGet("details/{id}")]
        public async Task<ActionResult<SalesOrderDetailsDto>> GetDetailsById(int id)
        {
            var details = await _salesOrderService.GetSalesOrderDetailsByIdAsync(id);
            if (details == null)
            {
                return NotFound();
            }
            // Fetch lead address if possible
            object leadAddress = null;
            if (details?.Quotation != null && !string.IsNullOrEmpty(details.Quotation.OpportunityId))
            {
                try
                {
                    var connectionString = _configuration.GetConnectionString("DefaultConnection");
                    using (var oppConn = new Npgsql.NpgsqlConnection(connectionString))
                    {
                        await oppConn.OpenAsync();
                        var opp = await oppConn.QueryFirstOrDefaultAsync<dynamic>(
                            "SELECT lead_id FROM sales_opportunities WHERE opportunity_id = @OpportunityId LIMIT 1",
                            new { OpportunityId = details.Quotation.OpportunityId });
                        string leadId = null;
                        if (opp != null && opp.lead_id != null && !string.IsNullOrEmpty((string)opp.lead_id))
                        {
                            leadId = (string)opp.lead_id;
                            leadAddress = await oppConn.QueryFirstOrDefaultAsync(
                                "SELECT pincode, area, state, district, city, door_no, street, landmark FROM sales_lead WHERE lead_id = @LeadId LIMIT 1",
                                new { LeadId = leadId });
                        }
                    }
                }
                catch { }
            }
            // Recursively ensure child and accessories lists are never null and are always lists
            void EnsureItemChildren(List<SalesProduct> itemList)
            {
                if (itemList == null) return;
                foreach (var item in itemList)
                {
                    if (item.IncludedChildItems == null)
                        item.IncludedChildItems = new List<SalesProduct>();
                    if (item.AccessoriesItems == null)
                        item.AccessoriesItems = new List<SalesProduct>();
                    EnsureItemChildren(item.IncludedChildItems);
                    EnsureItemChildren(item.AccessoriesItems);
                }
            }
            EnsureItemChildren(details.Items);
            return Ok(new {
                details.SalesOrder,
                details.Quotation,
                Items = details.Items ?? new List<SalesProduct>(),
                details.CustomerName,
                LeadAddress = leadAddress
            });
        }

        [HttpPost("quotation-grid/search")]
        public async Task<ActionResult<IEnumerable<QuotationGridDto>>> SearchQuotationGrid([FromBody] QuotationGridSearchRequest request)
        {
            var quotations = await _salesOrderService.GetQuotationGridAsync(request);
            return Ok(quotations);
        }
    }
}
