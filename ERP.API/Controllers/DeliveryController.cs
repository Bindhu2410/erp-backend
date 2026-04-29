using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using ERP.API.Models;
using ERP.API.Services;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
public class DeliveryController : ControllerBase
{
    private readonly IDeliveryService _deliveryService;
    private readonly IPurchaseOrderService _purchaseOrderService;
    private readonly ISalesQuotationService _salesQuotationService;

    public DeliveryController(IDeliveryService deliveryService, IPurchaseOrderService purchaseOrderService, ISalesQuotationService salesQuotationService)
    {
        _deliveryService = deliveryService;
        _purchaseOrderService = purchaseOrderService;
        _salesQuotationService = salesQuotationService;
    }

        // ...existing code...

        [HttpGet("by-purchaseorder/{purchaseOrderId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetByPurchaseOrderId(string purchaseOrderId)
        {
            // Fetch deliveries for the given purchase order
            var deliveries = await _deliveryService.GetByPurchaseOrderIdAsync(purchaseOrderId);
            if (deliveries == null || !deliveries.Any()) return NotFound();

            // Fetch PO details
            var poDetails = await _purchaseOrderService.GetByPoIdAsync(purchaseOrderId);
            // Fetch Quotation info if available
            object quotationInfo = null;
            if (poDetails?.PurchaseOrder?.QuotationId != null)
            {
                quotationInfo = await _salesQuotationService.GetDetailsByQuotationIdAsync(poDetails.PurchaseOrder.QuotationId.Value);
            }

            // For each delivery, include items (already enriched), child items, and accessory items
            var result = new List<object>();
            foreach (var delivery in deliveries)
            {
                var items = delivery.Items ?? new List<SalesItemResponse>();
                var itemsWithChildren = new List<object>();
                foreach (var item in items)
                {
                    itemsWithChildren.Add(new {
                        Item = item,
                        ChildItems = item.IncludedChildItems,
                        AccessoryItems = item.AccessoriesItems
                    });
                }
                result.Add(new {
                    Delivery = delivery,
                    PurchaseOrder = poDetails,
                    Quotation = quotationInfo,
                    Items = itemsWithChildren
                });
            }
            return Ok(result);
        }
        
        [HttpPost("grid")]
        public async Task<ActionResult<DeliveryGridResponse>> SearchDeliveryGrid([FromBody] DeliveryGridRequest request)
        {
            (IEnumerable<Delivery> data, int totalRecords) = await _deliveryService.GetDeliveryGridAsync(request);
            var response = new DeliveryGridResponse { Data = data, TotalRecords = totalRecords };
            return Ok(response);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Delivery>>> GetAll()
        {
            var deliveries = await _deliveryService.GetAllAsync();
            return Ok(deliveries);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Delivery>> GetById(int id)
        {
            var delivery = await _deliveryService.GetByIdAsync(id);
            if (delivery == null) return NotFound();
            return Ok(delivery);
        }

        [HttpPost]
        /// <summary>
        /// Create a new delivery.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/delivery
        ///     {
        ///       "userCreated": 0,
        ///       "dateCreated": "2025-07-24T07:53:29.497Z",
        ///       "userUpdated": 0,
        ///       "dateUpdated": "2025-07-24T07:53:29.497Z",
        ///       "salesOrderId": "string",
        ///       "poId": "string",
        ///       "deliveryId": "string",
        ///       "deliveryDate": "2025-07-24T07:53:29.497Z",
        ///       "deliveryStatus": "string",
        ///       "dispatchAddress": "string",
        ///       "priority": "string",
        ///       "transporterName": "string",
        ///       "items": [
        ///         {
        ///           "itemId": 0,
        ///           "userCreated": 0,
        ///           "dateCreated": "2025-07-24T07:54:02.064Z",
        ///           "userUpdated": 0,
        ///           "dateUpdated": "2025-07-24T07:54:02.064Z",
        ///           "qty": 0,
        ///           "amount": 0,
        ///           "isActive": true,
        ///           "unitPrice": 0,
        ///           "includedChildItemIds": [0],
        ///           "accessoriesIds": [0]
        ///         }
        ///       ]
        ///     }
        /// </remarks>
        public async Task<ActionResult<Delivery>> Create([FromBody] DeliveryRequest deliveryRequest)
        {
            // Map DeliveryRequest to Delivery
            var delivery = new Delivery
            {
                UserCreated = deliveryRequest.UserCreated,
                DateCreated = deliveryRequest.DateCreated,
                UserUpdated = deliveryRequest.UserUpdated,
                DateUpdated = deliveryRequest.DateUpdated,
                SalesOrderId = deliveryRequest.SalesOrderId,
                PoId = deliveryRequest.PoId,
                DeliveryId = deliveryRequest.DeliveryId,
                DeliveryDate = deliveryRequest.DeliveryDate,
                DeliveryStatus = deliveryRequest.DeliveryStatus,
                DispatchAddress = deliveryRequest.DispatchAddress,
                Priority = deliveryRequest.Priority,
                TransporterName = deliveryRequest.TransporterName,
                VehicleNo = deliveryRequest.VehicleNo,
                DriverName = deliveryRequest.DriverName,
                DriverContact = deliveryRequest.DriverContact,
                ModeOfDelivery = deliveryRequest.ModeOfDelivery,
                InvoiceId = deliveryRequest.InvoiceId,
                Items = deliveryRequest.Items?.ConvertAll(item => new SalesItemResponse
                {
                    ItemId = item.ItemId,
                    UserCreated = item.UserCreated,
                    DateCreated = item.DateCreated,
                    UserUpdated = item.UserUpdated,
                    DateUpdated = item.DateUpdated,
                    Qty = item.Qty,
                    Amount = (double)item.Amount,
                    IsActive = item.IsActive,
                    UnitPrice = item.UnitPrice
                })
            };
            var created = await _deliveryService.CreateAsync(delivery) as ERP.API.Models.DeliveryResponse;
            // No need to fetch enriched again, as CreateAsync now returns all info
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Delivery>> Update(int id, [FromBody] DeliveryRequest deliveryRequest)
        {
            // Map DeliveryRequest to Delivery
            var delivery = new Delivery
            {
                UserCreated = deliveryRequest.UserCreated,
                DateCreated = deliveryRequest.DateCreated,
                UserUpdated = deliveryRequest.UserUpdated,
                DateUpdated = deliveryRequest.DateUpdated,
                SalesOrderId = deliveryRequest.SalesOrderId,
                PoId = deliveryRequest.PoId,
                DeliveryId = deliveryRequest.DeliveryId,
                DeliveryDate = deliveryRequest.DeliveryDate,
                DeliveryStatus = deliveryRequest.DeliveryStatus,
                DispatchAddress = deliveryRequest.DispatchAddress,
                Priority = deliveryRequest.Priority,
                TransporterName = deliveryRequest.TransporterName,
                VehicleNo = deliveryRequest.VehicleNo,
                DriverName = deliveryRequest.DriverName,
                DriverContact = deliveryRequest.DriverContact,
                ModeOfDelivery = deliveryRequest.ModeOfDelivery,
                InvoiceId = deliveryRequest.InvoiceId,
                Items = deliveryRequest.Items?.ConvertAll(item => new SalesItemResponse
                {
                    ItemId = item.ItemId,
                    UserCreated = item.UserCreated,
                    DateCreated = item.DateCreated,
                    UserUpdated = item.UserUpdated,
                    DateUpdated = item.DateUpdated,
                    Qty = item.Qty,
                    Amount = (double)item.Amount,
                    IsActive = item.IsActive,
                    UnitPrice = item.UnitPrice
                })
            };
            var updated = await _deliveryService.UpdateAsync(id, delivery);
            if (updated == null) return NotFound();
            // Fetch enriched delivery (with full includedChildItems/accessoriesItems) for response
            var enriched = await _deliveryService.GetByIdAsync(updated.Id);
            return Ok(enriched);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _deliveryService.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}
