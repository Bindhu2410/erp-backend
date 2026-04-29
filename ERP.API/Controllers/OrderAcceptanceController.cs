using Dapper;
using Npgsql;
using Microsoft.Extensions.Configuration;
using ERP.API.Models;
using ERP.API.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderAcceptanceController : ControllerBase
    {
        private readonly IOrderAcceptanceService _orderAcceptanceService;
        private readonly ISalesQuotationService _salesQuotationService;
        private readonly SalesTermsAndConditionsService _termsService;
        private readonly SalesAddressService _addressService;
        private readonly IPurchaseOrderService _purchaseOrderService;
        private readonly ISalesOpportunityService _opportunityService;
        private readonly IConfiguration _configuration;
        public OrderAcceptanceController(
            IOrderAcceptanceService orderAcceptanceService,
            ISalesQuotationService salesQuotationService,
            SalesTermsAndConditionsService termsService,
            SalesAddressService addressService,
            IPurchaseOrderService purchaseOrderService,
            ISalesOpportunityService opportunityService,
            IConfiguration configuration)
        {
            _orderAcceptanceService = orderAcceptanceService;
            _salesQuotationService = salesQuotationService;
            _termsService = termsService;
            _addressService = addressService;
            _purchaseOrderService = purchaseOrderService;
            _opportunityService = opportunityService;
            _configuration = configuration;
        }
        // POST: api/orderacceptance/create-from-po/{purchaseOrderId}
        [HttpPost("create-from-po/{purchaseOrderId}")]
        [ProducesResponseType(201)]
        [ProducesResponseType(200)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateFromPurchaseOrder(int purchaseOrderId)
        {
            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                int? userId = null;
                if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out var parsedUserId))
                    userId = parsedUserId;

                // Get PO details first
                var purchaseOrder = await _purchaseOrderService.GetByIdAsync(purchaseOrderId);
                if (purchaseOrder == null)
                    return NotFound(new { message = $"Purchase Order with ID {purchaseOrderId} not found" });

                // Check if Order Acceptance already exists
                var existingOA = await _orderAcceptanceService.GetOrderAcceptanceByPOAsync(purchaseOrder.PoId);
                OrderAcceptance orderAcceptance;
                bool isNewlyCreated = false;

                if (existingOA != null)
                {
                    orderAcceptance = existingOA;
                }
                else
                {
                    orderAcceptance = await _orderAcceptanceService.CreateOrderAcceptanceFromPOAsync(purchaseOrderId, userId);
                    isNewlyCreated = true;
                }
                
                var poDetails = await _purchaseOrderService.GetByPoIdAsync(purchaseOrder.PoId);
                
                // Get items from quotation
                var items = new List<object>();
                object termsAndConditions = null;
                object leadAddress = null;
                int? quoteTitleId = null;
                int? tcTemplateId = null;
                
                if (purchaseOrder.QuotationId != null)
                {
                    var itemsResponse = await _salesQuotationService.GetItemsByQuotationIdAsync(purchaseOrder.QuotationId.Value);
                    
                    // Extract items and metadata from the response (same as PurchaseOrder GET)
                    dynamic itemsData = itemsResponse;
                    items = itemsData?.items ?? new List<object>();
                    quoteTitleId = itemsData?.quoteTitleId;
                    tcTemplateId = itemsData?.tcTemplateId;
                    
                    // Get terms and conditions
                    termsAndConditions = await _salesQuotationService.GetTermsAndConditionsByQuotationIdAsync(purchaseOrder.QuotationId.Value);
                    
                    // Get lead address
                    leadAddress = await _salesQuotationService.GetLeadAddressByQuotationIdAsync(purchaseOrder.QuotationId.Value);
                }
                
                var response = new
                {
                    orderAcceptance = orderAcceptance,
                    purchaseOrder = purchaseOrder,
                    items = items,
                    vendorName = poDetails?.VendorName,
                    termsAndConditions = termsAndConditions,
                    leadAddress = leadAddress,
                    quoteTitleId = quoteTitleId,
                    tcTemplateId = tcTemplateId
                };

                if (isNewlyCreated)
                {
                    return CreatedAtAction(nameof(CreateFromPurchaseOrder), new { purchaseOrderId = purchaseOrderId }, response);
                }
                else
                {
                    return Ok(response);
                }
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to create order acceptance", error = ex.Message });
            }
        }
    }
}
