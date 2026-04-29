using Microsoft.AspNetCore.Mvc;
using Dapper;
using System.Threading.Tasks;
using System.Linq;
using ERP.API.Services;
using ERP.API.Models;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route("api/purchaseorder")]
    public class PurchaseOrderController : ControllerBase
    {
        private readonly IPurchaseOrderService _purchaseOrderService;
        private readonly ISalesQuotationService _salesQuotationService;

        public PurchaseOrderController(IPurchaseOrderService purchaseOrderService, ISalesQuotationService salesQuotationService)
        {
            _purchaseOrderService = purchaseOrderService;
            _salesQuotationService = salesQuotationService;
        }

        // GET: api/purchaseorder
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var purchaseOrders = await _purchaseOrderService.GetAllAsync();
            var enrichedList = new List<object>();
            foreach (var po in purchaseOrders)
            {
                if (po?.PoId != null)
                {
                    var details = await _purchaseOrderService.GetByPoIdAsync(po.PoId);
                    if (details != null)
                    {
                        object items = new List<object>();
                        int? quoteTitleId = null;
                        int? tcTemplateId = null;
                        object termsAndConditions = null;
                        object leadAddress = null;
                        
                        if (po.QuotationId != null)
                        {
                            // Get items with enhanced BOM structure
                            var itemsResponse = await _salesQuotationService.GetItemsByQuotationIdAsync(po.QuotationId.Value);
                            
                            // Extract items and metadata from the response
                            dynamic itemsData = itemsResponse;
                            items = itemsData?.items ?? new List<object>();
                            quoteTitleId = itemsData?.quoteTitleId;
                            tcTemplateId = itemsData?.tcTemplateId;
                            
                            // Get terms and conditions
                            termsAndConditions = await _salesQuotationService.GetTermsAndConditionsByQuotationIdAsync(po.QuotationId.Value);
                            
                            // Get lead address
                            leadAddress = await _salesQuotationService.GetLeadAddressByQuotationIdAsync(po.QuotationId.Value);
                        }
                        
                        object quotationInfo = null;
                        if (po.QuotationId != null)
                        {
                            quotationInfo = await _salesQuotationService.GetDetailsByQuotationIdAsync(po.QuotationId.Value);
                        }
                        
                        enrichedList.Add(new
                        {
                            purchaseOrder = details.PurchaseOrder,
                            quotationInfo = quotationInfo,
                            items = items,
                            vendorName = details.VendorName,
                            termsAndConditions = termsAndConditions,
                            leadAddress = leadAddress,
                            quoteTitleId = quoteTitleId,
                            tcTemplateId = tcTemplateId
                        });
                    }
                }
            }
            return Ok(enrichedList);
        }

        // GET: api/purchaseorder/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var purchaseOrder = await _purchaseOrderService.GetByIdAsync(id);
            if (purchaseOrder == null)
                return NotFound();
            
            object quotationInfo = null;
            string vendorName = null;
            object items = new List<object>();
            int? quoteTitleId = null;
            int? tcTemplateId = null;
            object termsAndConditions = null;
            object leadAddress = null;
            
            if (purchaseOrder.QuotationId != null)
            {
                quotationInfo = await _salesQuotationService.GetDetailsByQuotationIdAsync(purchaseOrder.QuotationId.Value);
                
                // Get items with enhanced BOM structure (child and accessory items with all fields)
                var itemsResponse = await _salesQuotationService.GetItemsByQuotationIdAsync(purchaseOrder.QuotationId.Value);
                
                // Extract items and metadata from the response
                dynamic itemsData = itemsResponse;
                items = itemsData?.items ?? new List<object>();
                quoteTitleId = itemsData?.quoteTitleId;
                tcTemplateId = itemsData?.tcTemplateId;
                
                var quotationDetails = await _purchaseOrderService.GetByQuotationIdAsync(purchaseOrder.QuotationId.Value);
                vendorName = quotationDetails?.VendorName;
                
                // Get terms and conditions
                termsAndConditions = await _salesQuotationService.GetTermsAndConditionsByQuotationIdAsync(purchaseOrder.QuotationId.Value);
                
                // Get lead address
                leadAddress = await _salesQuotationService.GetLeadAddressByQuotationIdAsync(purchaseOrder.QuotationId.Value);
            }
            
            return Ok(new
            {
                purchaseOrder = purchaseOrder,
                quotationInfo = quotationInfo,
                items = items,
                vendorName = vendorName,
                termsAndConditions = termsAndConditions,
                leadAddress = leadAddress,
                quoteTitleId = quoteTitleId,
                tcTemplateId = tcTemplateId
            });
        }

        // POST: api/purchaseorder
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PurchaseOrderDto purchaseOrder)
        {
            if (purchaseOrder == null)
                return BadRequest();
            var created = await _purchaseOrderService.CreateAsync(purchaseOrder);
            // Fetch enriched PO details for response
            var details = await _purchaseOrderService.GetByPoIdAsync(created.PoId);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, details);
        }

        // PUT: api/purchaseorder/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] PurchaseOrderDto purchaseOrder)
        {
            if (purchaseOrder == null || id != purchaseOrder.Id)
                return BadRequest();
            var updated = await _purchaseOrderService.UpdateAsync(id, purchaseOrder);
            if (updated == null)
                return NotFound();
            return Ok(updated);
        }

        // DELETE: api/purchaseorder/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _purchaseOrderService.DeleteAsync(id);
            if (!deleted)
                return NotFound();
            return NoContent();
        }

            // GET: api/purchaseorder/Dropdown-po
            [HttpGet("Dropdown-po")]
            [ProducesResponseType(typeof(IEnumerable<PurchaseRequisitionDropdownDto>), 200)]
            public async Task<IActionResult> GetPurchaseOrderDropdown()
            {
                var prList = await _purchaseOrderService.GetApprovedPRsForDropdown();
                var dropdownList = new List<PurchaseRequisitionDropdownDto>();
                foreach (var pr in prList)
                {
                    var dto = new PurchaseRequisitionDropdownDto
                    {
                        Id = pr.Id,
                        PurchaseRequisitionId = pr.PurchaseRequisitionId,
                        RequesterName = pr.RequesterName,
                        Description = pr.Description,
                        DeliveryDate = pr.DeliveryDate,
                        BudgetAmount = pr.BudgetAmount,
                        Status = pr.Status,
                        UserCreated = pr.UserCreated,
                        UserUpdated = pr.UserUpdated,
                        SupplierId = pr.SupplierId,
                        VendorName = pr.VendorName,
                        Items = await _purchaseOrderService.GetItemsByRequisitionIdAsync(pr.Id)
                    };
                    dropdownList.Add(dto);
                }
                return Ok(dropdownList);
            }

            // GET: api/purchaseorder/by-quotation/{quotationId}
            [HttpGet("by-quotation/{quotationId}")]
            [ProducesResponseType(typeof(PurchaseOrderDetailsDto), 200)]
            [ProducesResponseType(StatusCodes.Status404NotFound)]
            public async Task<IActionResult> GetByQuotationId(int quotationId)
            {
                var poDetails = await _purchaseOrderService.GetByQuotationIdAsync(quotationId);
                if (poDetails == null)
                    return NotFound(new { message = $"No purchase order found for quotation ID {quotationId}" });
                return Ok(poDetails);
            }

            // POST: api/purchaseorder/create-from-quotation/{quotationId}
            [HttpPost("create-from-quotation/{quotationId}")]
            [ProducesResponseType(typeof(PurchaseOrderDetailsDto), 201)]
            [ProducesResponseType(StatusCodes.Status404NotFound)]
            [ProducesResponseType(StatusCodes.Status400BadRequest)]
            public async Task<IActionResult> CreateFromQuotation(int quotationId)
            {
                try
                {
                    // Get the user ID from claims for audit trail
                    var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                    int? userId = null;
                    if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out var parsedUserId))
                        userId = parsedUserId;

                    // Create purchase order from quotation
                    var createdPO = await _purchaseOrderService.CreatePurchaseOrderFromQuotationAsync(quotationId, userId);
                    if (createdPO == null)
                        return NotFound(new { message = $"Quotation with ID {quotationId} not found" });

                    // Fetch enriched PO details for response
                    var details = await _purchaseOrderService.GetByPoIdAsync(createdPO.PoId);
                    
                    // Get quotation info
                    object quotationInfo = await _salesQuotationService.GetDetailsByQuotationIdAsync(quotationId);
                    
                    // Get items with enhanced BOM structure (child and accessory items with all fields)
                    var itemsResponse = await _salesQuotationService.GetItemsByQuotationIdAsync(quotationId);
                    
                    // Extract items and metadata from the response
                    dynamic itemsData = itemsResponse;
                    object items = itemsData?.items ?? new List<object>();
                    int? quoteTitleId = itemsData?.quoteTitleId;
                    int? tcTemplateId = itemsData?.tcTemplateId;
                    
                    // Get terms and conditions
                    var termsAndConditions = await _salesQuotationService.GetTermsAndConditionsByQuotationIdAsync(quotationId);
                    
                    // Get lead address
                    object leadAddress = await _salesQuotationService.GetLeadAddressByQuotationIdAsync(quotationId);
                    
                    var response = new
                    {
                        purchaseOrder = details?.PurchaseOrder,
                        quotationInfo = quotationInfo,
                        items = items,
                        vendorName = details?.VendorName,
                        termsAndConditions = termsAndConditions,
                        leadAddress = leadAddress,
                        quoteTitleId = quoteTitleId,
                        tcTemplateId = tcTemplateId
                    };
                    
                    return CreatedAtAction(nameof(GetById), new { id = createdPO.Id }, response);
                }
                catch (Exception ex)
                {
                    return BadRequest(new { message = "Failed to create purchase order from quotation", error = ex.Message });
                }
            }

            // GET: api/purchaseorder/compare-po-quotation-items/{id}
            [HttpGet("compare-po-quotation-items/{id}")]
            [ProducesResponseType(200)]
            [ProducesResponseType(StatusCodes.Status404NotFound)]
            public async Task<IActionResult> ComparePOQuotationItems(int id)
            {
                var purchaseOrder = await _purchaseOrderService.GetByIdAsync(id);
                if (purchaseOrder == null)
                    return NotFound(new { message = $"Purchase order with ID {id} not found" });

                if (purchaseOrder.QuotationId == null)
                    return NotFound(new { message = "No quotation associated with this purchase order" });

                // Get PO details with items
                var poDetails = await _purchaseOrderService.GetByPoIdAsync(purchaseOrder.PoId);
                
                // Get quotation info
                var quotationInfo = await _salesQuotationService.GetDetailsByQuotationIdAsync(purchaseOrder.QuotationId.Value);
                
                // Get quotation items with enhanced structure
                var itemsResponse = await _salesQuotationService.GetItemsByQuotationIdAsync(purchaseOrder.QuotationId.Value);
                
                // Extract items and metadata from the response
                dynamic itemsData = itemsResponse;
                object quotationItems = itemsData?.items ?? new List<object>();
                int? quoteTitleId = itemsData?.quoteTitleId;
                int? tcTemplateId = itemsData?.tcTemplateId;
                
                // Get terms and conditions
                var termsAndConditions = await _salesQuotationService.GetTermsAndConditionsByQuotationIdAsync(purchaseOrder.QuotationId.Value);
                
                // Get lead address
                var leadAddress = await _salesQuotationService.GetLeadAddressByQuotationIdAsync(purchaseOrder.QuotationId.Value);
                
                return Ok(new
                {
                    purchaseOrderInfo = purchaseOrder,
                    purchaseOrderItems = quotationItems,
                    quotationInfo = quotationInfo,
                    quotationItems = quotationItems,
                    vendorName = poDetails?.VendorName,
                    termsAndConditions = termsAndConditions,
                    leadAddress = leadAddress,
                    quoteTitleId = quoteTitleId,
                    tcTemplateId = tcTemplateId
                });
            }

            // PATCH: api/purchaseorder/update-status
            [HttpPatch("update-status")]
            [ProducesResponseType(200)]
            [ProducesResponseType(StatusCodes.Status400BadRequest)]
            [ProducesResponseType(StatusCodes.Status404NotFound)]
            public async Task<IActionResult> UpdateStatus([FromBody] UpdatePurchaseOrderStatusRequest request)
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Status))
                    return BadRequest(new { message = "Id and Status are required." });

                var updated = await _purchaseOrderService.UpdateStatusAsync(request.Id, request.Status);
                if (!updated)
                    return NotFound(new { message = $"Purchase order with ID {request.Id} not found." });

                return Ok(new { message = "Status updated successfully." });
            }

            public class UpdatePurchaseOrderStatusRequest
            {
                public int Id { get; set; }
                public string Status { get; set; }
            }
    }
}
