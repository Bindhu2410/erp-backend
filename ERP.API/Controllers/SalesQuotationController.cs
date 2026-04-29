using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using ERP.API.Models;
using ERP.API.Models.DTOs;
using ERP.API.Services;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Dapper;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route("api/sales-quotations")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public class SalesQuotationController : ControllerBase
    {
        private readonly SalesQuotationService _salesQuotationService;
        private readonly SalesSummaryService _summaryService;
        private readonly SalesTermsAndConditionsService _termsService;
        private readonly SalesLeadService _salesLeadService;
        private readonly SalesAddressService _salesAddressService;
        private readonly IPurchaseOrderService _purchaseOrderService;
        private readonly ILogger<SalesQuotationController> _logger;

        public SalesQuotationController(
            SalesQuotationService salesQuotationService,
            SalesSummaryService summaryService,
            SalesTermsAndConditionsService termsService,
            SalesLeadService salesLeadService,
            SalesAddressService salesAddressService,
            IPurchaseOrderService purchaseOrderService,
            ILogger<SalesQuotationController> logger)
        {
            _salesQuotationService = salesQuotationService;
            _summaryService = summaryService;
            _termsService = termsService;
            _salesLeadService = salesLeadService;
            _salesAddressService = salesAddressService;
            _purchaseOrderService = purchaseOrderService;
            _logger = logger;
        }

        /// <summary>
        /// Patch (partial update) only status and comments fields of a sales quotation
        /// </summary>
        /// <param name="id">The ID of the quotation to patch</param>
        /// <param name="request">The patch data for status and comments</param>
        /// <returns>Patched status and comments</returns>
        [HttpPatch("{id}/with-items")]
        [ProducesResponseType(typeof(StatusCommentsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PatchQuotationWithItems(int id, [FromBody] StatusCommentsRequest request)
        {
            if (request == null)
                return BadRequest(new { message = "Invalid request body" });

            try
            {
                var connectionString = _salesQuotationService.GetConnectionString();
                using var connection = new Npgsql.NpgsqlConnection(connectionString);
                await connection.OpenAsync();
                using var transaction = await connection.BeginTransactionAsync();

                // Check if quotation exists and get current status
                var quotation = await connection.QueryFirstOrDefaultAsync<dynamic>(
                    "SELECT id, status, quotation_id, version FROM sales_quotations WHERE id = @Id", new { Id = id }, transaction);
                if (quotation == null)
                    return NotFound(new { message = $"Quotation with ID {id} not found" });

                string currentStatus = quotation.status?.ToString()?.Trim();
                string newStatus = request.Status?.Trim();
                
                // Check if this is a Draft status update - only update in place if BOTH current and new status are Draft
                bool isDraftUpdate = string.Equals(newStatus, "Draft", StringComparison.OrdinalIgnoreCase) &&
                                   string.Equals(currentStatus, "Draft", StringComparison.OrdinalIgnoreCase);
                
                int quotationIdToReturn = id;
                
                if (isDraftUpdate)
                {
                    // Update in place for Draft status
                    await connection.ExecuteAsync(
                        "UPDATE sales_quotations SET status = @Status, comments = @Comments, assigned_to = @AssignedTo WHERE id = @Id",
                        new { Status = request.Status, Comments = request.Comments, AssignedTo = request.AssignedTo, Id = id }, transaction);
                }
                else if (!string.IsNullOrEmpty(newStatus) && !string.Equals(newStatus, currentStatus, StringComparison.OrdinalIgnoreCase))
                {
                    // Create new version for non-Draft status changes
                    string newQuotationId = IncrementQuotationVersion(quotation.quotation_id?.ToString());
                    string[] parts = newQuotationId.Split('/');
                    string newVersion = parts.Length > 1 ? parts[1] : "1.2";
                    
                    // If changing FROM Draft status, ensure the original Draft record remains visible in history
                    if (string.Equals(currentStatus, "Draft", StringComparison.OrdinalIgnoreCase))
                    {
                        // Keep the original Draft record as-is for version history
                        // The new version will be created below
                    }
                    
                    // Get existing quotation data
                    var existingData = await connection.QueryFirstOrDefaultAsync<dynamic>(
                        "SELECT * FROM sales_quotations WHERE id = @Id", new { Id = id }, transaction);
                    
                    // Create new quotation record with incremented version
                    const string insertSql = @"
                        INSERT INTO sales_quotations (
                            user_created, date_created, version, terms, valid_till, quotation_for, status, lost_reason, customer_id, quotation_type, quotation_date, order_type, comments, delivery_within, delivery_after, is_active, opportunity_id, lead_id, customer_name, taxes, delivery, payment, warranty, freight_charge, is_current, parent_sales_quotations_id, quotation_id, contact_name, contact_mobile_no, assigned_to
                        ) VALUES (
                            @UserCreated, @DateCreated, @Version, @Terms, @ValidTill, @QuotationFor, @Status, @LostReason, @CustomerId, @QuotationType, @QuotationDate, @OrderType, @Comments, @DeliveryWithin, @DeliveryAfter, @IsActive, @OpportunityId, @LeadId, @CustomerName, @Taxes, @Delivery, @Payment, @Warranty, @FreightCharge, @IsCurrent, @ParentSalesQuotationsId, @QuotationId, @ContactName, @ContactMobileNo, @AssignedTo
                        ) RETURNING id";
                    
                    quotationIdToReturn = await connection.ExecuteScalarAsync<int>(insertSql, new
                    {
                        UserCreated = existingData.user_created,
                        DateCreated = DateTime.UtcNow,
                        Version = newVersion,
                        Terms = existingData.terms,
                        ValidTill = existingData.valid_till,
                        QuotationFor = existingData.quotation_for,
                        Status = request.Status,
                        LostReason = existingData.lost_reason,
                        CustomerId = existingData.customer_id,
                        QuotationType = existingData.quotation_type,
                        QuotationDate = existingData.quotation_date,
                        OrderType = existingData.order_type,
                        Comments = request.Comments,
                        DeliveryWithin = existingData.delivery_within,
                        DeliveryAfter = existingData.delivery_after,
                        IsActive = existingData.is_active,
                        OpportunityId = existingData.opportunity_id,
                        LeadId = existingData.lead_id,
                        CustomerName = existingData.customer_name,
                        Taxes = existingData.taxes,
                        Delivery = existingData.delivery,
                        Payment = existingData.payment,
                        Warranty = existingData.warranty,
                        FreightCharge = existingData.freight_charge,
                        IsCurrent = existingData.is_current,
                        ParentSalesQuotationsId = existingData.parent_sales_quotations_id,
                        QuotationId = newQuotationId,
                        ContactName = existingData.contact_name,
                        ContactMobileNo = existingData.contact_mobile_no,
                        AssignedTo = request.AssignedTo
                    }, transaction);
                    
                    // Copy items from original quotation to new version
                    var existingItems = await connection.QueryAsync<dynamic>(
                        "SELECT * FROM sales_product WHERE stage = 'Quotation' AND stage_item_id = @StageItemId",
                        new { StageItemId = id.ToString() }, transaction);
                    
                    foreach (var item in existingItems)
                    {
                        await connection.ExecuteAsync(@"
                            INSERT INTO sales_product (bom_id, qty, bom_child_item_ids, bom_accessory_item_ids, stage, stage_item_id, user_created, date_created)
                            VALUES (@BomId, @Qty, @BomChildItemIds, @BomAccessoryItemIds, @Stage, @StageItemId, @UserCreated, @DateCreated)",
                            new {
                                BomId = item.bom_id,
                                Qty = item.qty,
                                BomChildItemIds = item.bom_child_item_ids,
                                BomAccessoryItemIds = item.bom_accessory_item_ids,
                                Stage = "Quotation",
                                StageItemId = quotationIdToReturn.ToString(),
                                UserCreated = existingData.user_created,
                                DateCreated = DateTime.UtcNow
                            }, transaction);
                    }
                    
                    // Copy terms and conditions if they exist
                    var existingTerms = await connection.QueryFirstOrDefaultAsync<dynamic>(
                        "SELECT * FROM sales_terms_and_conditions WHERE quotation_id = @QuotationId ORDER BY id DESC LIMIT 1",
                        new { QuotationId = id }, transaction);
                    
                    if (existingTerms != null)
                    {
                        await connection.ExecuteAsync(@"
                            INSERT INTO sales_terms_and_conditions (user_created, date_created, taxes, freight_charges, delivery, payment, warranty, template_name, is_default, is_active, quotation_id)
                            VALUES (@UserCreated, @DateCreated, @Taxes, @FreightCharges, @Delivery, @Payment, @Warranty, @TemplateName, @IsDefault, @IsActive, @QuotationId)",
                            new {
                                UserCreated = existingTerms.user_created,
                                DateCreated = DateTime.UtcNow,
                                Taxes = existingTerms.taxes,
                                FreightCharges = existingTerms.freight_charges,
                                Delivery = existingTerms.delivery,
                                Payment = existingTerms.payment,
                                Warranty = existingTerms.warranty,
                                TemplateName = existingTerms.template_name,
                                IsDefault = existingTerms.is_default,
                                IsActive = existingTerms.is_active,
                                QuotationId = quotationIdToReturn
                            }, transaction);
                    }
                }
                else
                {
                    // Just update comments and assigned_to without status change
                    await connection.ExecuteAsync(
                        "UPDATE sales_quotations SET comments = @Comments, assigned_to = @AssignedTo WHERE id = @Id",
                        new { Comments = request.Comments, AssignedTo = request.AssignedTo, Id = id }, transaction);
                }

                // If status is 'Final Quotation', create a purchase order
                if (!string.IsNullOrEmpty(request.Status) &&
                    request.Status.Trim().Equals("Final Quotation", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        // Get the user ID from claims for audit trail
                        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                        int? userId = null;
                        if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out var parsedUserId))
                            userId = parsedUserId;

                        // Create purchase order from quotation
                        var createdPO = await _purchaseOrderService.CreatePurchaseOrderFromQuotationAsync(quotationIdToReturn, userId);
                        if (createdPO != null)
                        {
                            _logger.LogInformation($"Purchase Order {createdPO.PoId} created from Quotation ID {quotationIdToReturn}");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error creating purchase order from quotation {quotationIdToReturn}");
                        // Don't fail the quotation update if PO creation fails, just log it
                    }
                }

                // Update opportunity status to "Negotiation" when quotation status is "Negotiation"
                if (!string.IsNullOrEmpty(request.Status) &&
                    request.Status.Trim().Equals("Negotiation", StringComparison.OrdinalIgnoreCase))
                {
                    // Get opportunity_id from quotation
                    var quotationData = await connection.QueryFirstOrDefaultAsync<dynamic>(
                        "SELECT opportunity_id FROM sales_quotations WHERE id = @Id", new { Id = quotationIdToReturn }, transaction);
                    string opportunityId = quotationData?.opportunity_id;
                    
                    _logger.LogInformation($"Quotation {quotationIdToReturn} status changed to Negotiation. OpportunityId: {opportunityId}");
                    
                    if (!string.IsNullOrEmpty(opportunityId))
                    {
                        var rowsUpdated = await connection.ExecuteAsync(
                            "UPDATE sales_opportunities SET status = @Status, date_updated = @DateUpdated WHERE opportunity_id = @OpportunityId",
                            new { Status = "Negotiation", DateUpdated = DateTime.UtcNow, OpportunityId = opportunityId },
                            transaction);
                        
                        _logger.LogInformation($"Updated {rowsUpdated} opportunity records to Negotiation status for OpportunityId: {opportunityId}");
                    }
                    else
                    {
                        _logger.LogWarning($"No opportunity_id found for quotation {quotationIdToReturn}");
                    }
                }

                // If status is 'Negotiation', create a task
                if (!string.IsNullOrEmpty(request.Status) &&
                    request.Status.Trim().Equals("Negotiation", StringComparison.OrdinalIgnoreCase))
                {
                    // Get QuotationId for description
                    var qRow = await connection.QueryFirstOrDefaultAsync<dynamic>(
                        "SELECT quotation_id, user_created FROM sales_quotations WHERE id = @Id", new { Id = quotationIdToReturn }, transaction);
                    string quotationIdStr = qRow?.quotation_id?.ToString() ?? quotationIdToReturn.ToString();
                    int? ownerId = qRow?.user_created;
                    
                    // Build task description with comments if provided
                    string taskDescription = $"Negotiation required for quotation {quotationIdStr}";
                    if (!string.IsNullOrWhiteSpace(request.Comments))
                    {
                        taskDescription += $"\nComments for Quotation : {request.Comments}";
                    }
                    // Insert into task table
                    const string sqlTask = @"INSERT INTO task (task_name, description, status, task_type, owner_id, assignee_id, stage, stage_item_id) VALUES (@TaskName, @Description, @Status, @TaskType, @OwnerId, @AssigneeId, @Stage, @StageItemId)";
                    await connection.ExecuteAsync(sqlTask, new
                    {
                        TaskName = $"Negotiation for Quotation",
                        Description = taskDescription,
                        Status = "Open",
                        TaskType = "Main",
                        OwnerId = ownerId,
                        AssigneeId = request.AssignedTo, // Can be null
                        Stage = "Quotation",
                        StageItemId = quotationIdToReturn.ToString()
                    }, transaction);
                }

                await transaction.CommitAsync();

                // Return the updated fields from the correct quotation ID
                var updated = await connection.QueryFirstOrDefaultAsync<dynamic>(
                    "SELECT status, comments, assigned_to, quotation_id FROM sales_quotations WHERE id = @Id", new { Id = quotationIdToReturn });

                return Ok(new StatusCommentsResponse
                {
                    Status = updated?.status,
                    Comments = updated?.comments,
                    AssignedTo = updated?.assigned_to
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid patch request for quotation {QuotationId}", id);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error patching quotation status/comments/assigned_to for ID {QuotationId}", id);
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        private async Task<string> GenerateNextQuotationIdAsync(Npgsql.NpgsqlConnection connection, Npgsql.NpgsqlTransaction transaction = null)
        {
            // Get current financial year (April to March)
            var currentDate = DateTime.Now;
            var financialYearStart = currentDate.Month >= 4 ? currentDate.Year : currentDate.Year - 1;
            var financialYearEnd = financialYearStart + 1;
            var financialYearSuffix = $"{financialYearStart.ToString().Substring(2)}-{financialYearEnd.ToString().Substring(2)}";
            
            var lastQuotationId = await connection.QueryFirstOrDefaultAsync<string>(
                "SELECT quotation_id FROM sales_quotations WHERE quotation_id IS NOT NULL AND quotation_id LIKE @Pattern ORDER BY id DESC LIMIT 1", 
                new { Pattern = $"QTN/{financialYearSuffix}-%" }, transaction: transaction);
            
            int nextNumber = 1;
            if (!string.IsNullOrEmpty(lastQuotationId))
            {
                // Extract number from QTN/26-27-001/1.1 format
                var parts = lastQuotationId.Split('/');
                if (parts.Length >= 2)
                {
                    var secondPart = parts[1]; // 26-27-001
                    var lastDashIndex = secondPart.LastIndexOf('-');
                    if (lastDashIndex > 0 && int.TryParse(secondPart.Substring(lastDashIndex + 1), out int lastNumber))
                    {
                        nextNumber = lastNumber + 1;
                    }
                }
            }
            
            string newQuotationNumber = $"QTN/{financialYearSuffix}-{nextNumber.ToString("D3")}";
            return $"{newQuotationNumber}/1.1";
        }

        private string IncrementQuotationVersion(string currentQuotationId)
        {
            if (string.IsNullOrEmpty(currentQuotationId))
            {
                // Get current financial year for fallback
                var currentDate = DateTime.Now;
                var financialYearStart = currentDate.Month >= 4 ? currentDate.Year : currentDate.Year - 1;
                var financialYearEnd = financialYearStart + 1;
                var financialYearSuffix = $"{financialYearStart.ToString().Substring(2)}-{financialYearEnd.ToString().Substring(2)}";
                return $"QTN/{financialYearSuffix}-001/1.2";
            }
                
            string[] parts = currentQuotationId.Split('/');
            if (parts.Length < 3)
            {
                // Invalid format, return default
                var currentDate = DateTime.Now;
                var financialYearStart = currentDate.Month >= 4 ? currentDate.Year : currentDate.Year - 1;
                var financialYearEnd = financialYearStart + 1;
                var financialYearSuffix = $"{financialYearStart.ToString().Substring(2)}-{financialYearEnd.ToString().Substring(2)}";
                return $"QTN/{financialYearSuffix}-001/1.2";
            }
            
            string baseQuotationNumber = $"{parts[0]}/{parts[1]}";
            string oldVersion = parts[2];
            string[] versionParts = oldVersion.Split('.');
            int major = 1, minor = 1;
            if (versionParts.Length == 2 && int.TryParse(versionParts[0], out major) && int.TryParse(versionParts[1], out minor))
            {
                minor += 1;
            }
            else
            {
                major = 1; minor = 2;
            }
            string newVersion = $"{major}.{minor}";
            return $"{baseQuotationNumber}/{newVersion}";
        }
        /// <summary>
        /// Test endpoint to manually update opportunity status based on quotation
        /// </summary>
        [HttpPost("test-opportunity-update/{quotationId}")]
        public async Task<IActionResult> TestOpportunityUpdate(int quotationId)
        {
            var connectionString = _salesQuotationService.GetConnectionString();
            using var connection = new Npgsql.NpgsqlConnection(connectionString);
            await connection.OpenAsync();

            try
            {
                // Get quotation details
                var quotation = await connection.QueryFirstOrDefaultAsync<dynamic>(
                    "SELECT id, quotation_id, status, opportunity_id FROM sales_quotations WHERE id = @Id",
                    new { Id = quotationId });

                if (quotation == null)
                    return NotFound($"Quotation {quotationId} not found");

                string opportunityId = quotation.opportunity_id;
                string quotationStatus = quotation.status;

                if (string.IsNullOrEmpty(opportunityId))
                    return BadRequest($"Quotation {quotationId} has no opportunity_id");

                // Check current opportunity status
                var opportunity = await connection.QueryFirstOrDefaultAsync<dynamic>(
                    "SELECT opportunity_id, status FROM sales_opportunities WHERE opportunity_id = @OpportunityId",
                    new { OpportunityId = opportunityId });

                if (opportunity == null)
                    return NotFound($"Opportunity {opportunityId} not found");

                // Update opportunity status based on quotation status
                string newOpportunityStatus = quotationStatus?.Trim().Equals("Negotiation", StringComparison.OrdinalIgnoreCase) == true 
                    ? "Negotiation" : "Proposal";

                var rowsUpdated = await connection.ExecuteAsync(
                    "UPDATE sales_opportunities SET status = @Status, date_updated = @DateUpdated WHERE opportunity_id = @OpportunityId",
                    new { Status = newOpportunityStatus, DateUpdated = DateTime.UtcNow, OpportunityId = opportunityId });

                return Ok(new {
                    QuotationId = quotationId,
                    QuotationStatus = quotationStatus,
                    OpportunityId = opportunityId,
                    OldOpportunityStatus = opportunity.status,
                    NewOpportunityStatus = newOpportunityStatus,
                    RowsUpdated = rowsUpdated
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in test opportunity update");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Test endpoint to verify quotation versioning logic
        /// </summary>
        [HttpGet("test-versioning/{baseQuotationId}")]
        public async Task<IActionResult> TestVersioning(string baseQuotationId)
        {
            var connectionString = _salesQuotationService.GetConnectionString();
            using var connection = new Npgsql.NpgsqlConnection(connectionString);
            await connection.OpenAsync();

            // Get all versions of this quotation
            var versions = await connection.QueryAsync<dynamic>(
                @"SELECT id, quotation_id, version, status, date_created, customer_name 
                  FROM sales_quotations 
                  WHERE quotation_id LIKE @Pattern 
                  ORDER BY date_created ASC",
                new { Pattern = baseQuotationId + "/%" });

            return Ok(new
            {
                BaseQuotationId = baseQuotationId,
                Versions = versions.Select(v => new
                {
                    Id = v.id,
                    QuotationId = v.quotation_id,
                    Version = v.version,
                    Status = v.status,
                    DateCreated = v.date_created,
                    CustomerName = v.customer_name
                })
            });
        }

        private QuotationResponseDto MapToResponseDto(SalesQuotation quotation)
        {
            // Map only existing properties in QuotationResponseDto
            return new QuotationResponseDto
            {
                Id = quotation.Id ?? 0, // Fix: handle nullable int
                QuotationId = quotation.QuotationId,
                Version = quotation.Version,
                CustomerName = quotation.CustomerName,
                Status = quotation.Status,
                ValidTill = quotation.ValidTill,
                ContactName = quotation.ContactName,
                ContactMobileNo = quotation.ContactMobileNo,
                // Add other fields as needed, but only those present in QuotationResponseDto
            };
        }

        /// <summary>
        /// Get quotations by lead ID
        /// </summary>
        /// <param name="leadId">The ID of the lead</param>
        /// <returns>List of quotations for the lead</returns>
        /// <response code="200">Returns the list of quotations</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpGet("lead/{leadId}")]
        [ProducesResponseType(typeof(IEnumerable<QuotationResponseDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<QuotationResponseDto>>> GetByLeadId(string leadId)
        {
            try
            {
                var quotations = await _salesQuotationService.GetQuotationsByLeadIdAsync(leadId);
                return Ok(quotations.Select(q => MapToResponseDto(q)));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid lead ID format {LeadId}", leadId);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting quotations for lead {LeadId}", leadId);
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        /// <summary>
        /// Get sales quotation cards (count by status)
        /// </summary>
        /// <returns>Object with status counts</returns>
        [HttpGet("cards")]
        public async Task<ActionResult<SalesQuotationCardsDto>> GetSalesQuotationCards([FromQuery] int? userId = null)
        {
            var connectionString = _salesQuotationService.GetConnectionString();
            using var connection = new Npgsql.NpgsqlConnection(connectionString);
            await connection.OpenAsync();

            int targetUserId = 0;
            if (userId.HasValue)
            {
                targetUserId = userId.Value;
            }
            else
            {
                // Try to get from claims (like OpportunityController)
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out var parsedId))
                    targetUserId = parsedId;
            }

            SalesQuotationCardsDto dto;
            try
            {
                if (targetUserId > 0)
                {
                    dto = await _salesQuotationService.GetSalesQuotationCardsByUserAsync(targetUserId);
                }
                else
                {
                    dto = await _salesQuotationService.GetSalesQuotationCardsAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching quotation cards");
                return StatusCode(500, new { error = "Failed to fetch quotation cards", details = ex.Message });
            }
            return Ok(dto);
        }

        /// <summary>
        /// Gets sales quotation counts status for Draft, Pending Approval, Approved, Rejected, Sent
        /// </summary>
        /// <returns>SalesQuotationCardsDto with quotation counts by status</returns>
        /// <response code="200">Returns the quotation counts by status</response>
        /// <response code="500">If there was an internal server error</response>


        /// <summary>
        /// Create a new sales quotation with items
        /// </summary>
        [HttpPost("with-items")]
        [ProducesResponseType(typeof(SalesQuotationWithItemsResponse), StatusCodes.Status201Created)]
        public async Task<IActionResult> CreateQuotationWithItems([FromBody] SalesQuotationWithItemsRequest request)
        {
            QuotationResponseDto createdQuotation = null;
            List<SalesItemResponse> createdItems = null;
            ERP.API.Models.DTOs.SalesAddressDto customerAddress = null;
            object leadAddress = null;
            if (request == null || request.Quotation == null || request.Items == null)
                return BadRequest("Invalid request body");

            // Do not nullify TermsAndConditions; allow it to be processed and returned if present

            // Sync top-level CustomerName to nested Quotation
            if (!string.IsNullOrEmpty(request.CustomerName))
                request.Quotation.CustomerName = request.CustomerName;
            else if (!string.IsNullOrEmpty(request.Quotation.CustomerName))
                request.CustomerName = request.Quotation.CustomerName;
            // Sync contact fields
            if (!string.IsNullOrEmpty(request.ContactName))
                request.Quotation.ContactName = request.ContactName;
            else if (!string.IsNullOrEmpty(request.Quotation.ContactName))
                request.ContactName = request.Quotation.ContactName;
            if (!string.IsNullOrEmpty(request.ContactMobileNo))
                request.Quotation.ContactMobileNo = request.ContactMobileNo;
            else if (!string.IsNullOrEmpty(request.Quotation.ContactMobileNo))
                request.ContactMobileNo = request.Quotation.ContactMobileNo;


            var connectionString = _salesQuotationService.GetConnectionString();
            if (string.IsNullOrEmpty(connectionString))
                return StatusCode(500, "Could not resolve connection string.");

            using var connection = new Npgsql.NpgsqlConnection(connectionString);
            await connection.OpenAsync();
            using var transaction = await connection.BeginTransactionAsync();
            try
            {
                // Always set version to 1.1 for new quotations
                request.Quotation.Version = "1.1";
                
                // Ensure new quotations start with Draft status
                if (string.IsNullOrEmpty(request.Quotation.Status))
                {
                    request.Quotation.Status = "Draft";
                }

                // Generate next QuotationId using helper method
                string fullQuotationId = await GenerateNextQuotationIdAsync(connection, transaction);
                request.Quotation.QuotationId = fullQuotationId;

                // Insert quotation
                const string sqlQuotation = @"
                    INSERT INTO sales_quotations (
                        user_created, date_created, version, terms, valid_till, quotation_for, status, lost_reason, customer_id, quotation_type, quotation_date, order_type, comments, delivery_within, delivery_after, is_active, opportunity_id, lead_id, customer_name, taxes, delivery, payment, warranty, freight_charge, is_current, parent_sales_quotations_id, quotation_id, contact_name, contact_mobile_no, assigned_to
                    ) VALUES (
                        @UserCreated, @DateCreated, @Version, @Terms, @ValidTill, @QuotationFor, @Status, @LostReason, @CustomerId, @QuotationType, @QuotationDate, @OrderType, @Comments, @DeliveryWithin, @DeliveryAfter, @IsActive, @OpportunityId, @LeadId, @CustomerName, @Taxes, @Delivery, @Payment, @Warranty, @FreightCharge, @IsCurrent, @ParentSalesQuotationsId, @QuotationId, @ContactName, @ContactMobileNo, @AssignedTo
                    ) RETURNING id";
                var quotationId = await connection.ExecuteScalarAsync<int>(sqlQuotation, new
                {
                    request.Quotation.UserCreated,
                    DateCreated = DateTime.UtcNow,
                    request.Quotation.Version,
                    request.Quotation.Terms,
                    request.Quotation.ValidTill,
                    request.Quotation.QuotationFor,
                    request.Quotation.Status,
                    request.Quotation.LostReason,
                    request.Quotation.CustomerId,
                    request.Quotation.QuotationType,
                    request.Quotation.QuotationDate,
                    request.Quotation.OrderType,
                    request.Quotation.Comments,
                    request.Quotation.DeliveryWithin,
                    request.Quotation.DeliveryAfter,
                    IsActive = request.Quotation.IsActive,
                    request.Quotation.OpportunityId,
                    LeadId = string.IsNullOrEmpty(request.Quotation.LeadId) ? null : request.Quotation.LeadId,
                    request.Quotation.CustomerName,
                    request.Quotation.Taxes,
                    request.Quotation.Delivery,
                    request.Quotation.Payment,
                    request.Quotation.Warranty,
                    request.Quotation.FreightCharge,
                    IsCurrent = request.Quotation.IsCurrent,
                    ParentSalesQuotationsId = (request.Quotation.ParentSalesQuotationsId == 0 ? (int?)null : request.Quotation.ParentSalesQuotationsId),
                    QuotationId = request.Quotation.QuotationId,
                    ContactName = request.Quotation.ContactName,
                    ContactMobileNo = request.Quotation.ContactMobileNo,
                    AssignedTo = request.Quotation.AssignedTo
                }, transaction);

                // Update opportunity status to "Proposal" when quotation is created using opportunity ID
                if (!string.IsNullOrEmpty(request.Quotation.OpportunityId))
                {
                    await connection.ExecuteAsync(
                        "UPDATE sales_opportunities SET status = @Status, date_updated = @DateUpdated WHERE opportunity_id = @OpportunityId",
                        new { Status = "Proposal", DateUpdated = DateTime.UtcNow, OpportunityId = request.Quotation.OpportunityId },
                        transaction);
                }

                // --- Create a negotiation task if assigned_to is present ---
                if (request.Quotation.AssignedTo != null && request.Quotation.UserCreated != null)
                {
                    // Insert into task table with correct columns, including stage and stage_item_id
                    const string sqlTask = @"
                        INSERT INTO task (
                            task_name, description, status, task_type, owner_id, assignee_id, stage, stage_item_id
                        ) VALUES (
                            @TaskName, @Description, @Status, @TaskType, @OwnerId, @AssigneeId, @Stage, @StageItemId
                        )";
                    await connection.ExecuteAsync(sqlTask, new
                    {
                        TaskName = "Negotiation for Quotation",
                        Description = $"Negotiation required for quotation {request.Quotation.QuotationId}",
                        Status = "Open",
                        TaskType = "Main",
                        OwnerId = request.Quotation.UserCreated,
                        AssigneeId = request.Quotation.AssignedTo,
                        Stage = "Quotation",
                        StageItemId = quotationId.ToString()
                    }, transaction);
                }


                // Insert TermsAndConditions if provided
                SalesTermsAndConditions createdTerms = null;
                if (request.TermsAndConditions != null)
                {
                    var terms = request.TermsAndConditions;
                    var sqlTerms = @"INSERT INTO sales_terms_and_conditions (
                        user_created, date_created, user_updated, date_updated, taxes, freight_charges, delivery, payment, warranty, template_name, is_default, is_active, quotation_id
                    ) VALUES (
                        @UserCreated, @DateCreated, @UserUpdated, @DateUpdated, @Taxes, @FreightCharges, @Delivery, @Payment, @Warranty, @TemplateName, @IsDefault, @IsActive, @QuotationId
                    ) RETURNING id;";
                    var termsId = await connection.ExecuteScalarAsync<int>(sqlTerms, new
                    {
                        terms.UserCreated,
                        DateCreated = DateTime.UtcNow,
                        UserUpdated = terms.UserUpdated,
                        DateUpdated = (DateTime?)null,
                        terms.Taxes,
                        terms.FreightCharges,
                        terms.Delivery,
                        terms.Payment,
                        terms.Warranty,
                        terms.TemplateName,
                        IsDefault = terms.IsDefault,
                        IsActive = terms.IsActive,
                        QuotationId = quotationId
                    }, transaction);
                    // Use the just-inserted terms for the response
                    createdTerms = new SalesTermsAndConditions
                    {
                        Id = termsId,
                        UserCreated = terms.UserCreated,
                        DateCreated = DateTime.UtcNow,
                        UserUpdated = terms.UserUpdated,
                        DateUpdated = DateTime.UtcNow, // Fix: cannot assign null to non-nullable DateTime
                        Taxes = terms.Taxes,
                        FreightCharges = terms.FreightCharges,
                        Delivery = terms.Delivery,
                        Payment = terms.Payment,
                        Warranty = terms.Warranty,
                        TemplateName = terms.TemplateName,
                        IsDefault = terms.IsDefault,
                        IsActive = terms.IsActive,
                        QuotationId = quotationId
                    };

                    // Insert quotation items into sales_product after terms
                    foreach (var item in request.Items)
                    {
                        // Fetch BOM child item IDs for this BOM
                        var bomChildItems = await connection.QueryAsync<int>(
                            @"SELECT child_item_id FROM bill_of_material_child_items WHERE bill_of_material_id = (SELECT id FROM bill_of_materials WHERE bom_id = @BomId)",
                            new { BomId = item.BomId }, transaction);
                        var bomChildItemIdsJson = bomChildItems.Any() ? Newtonsoft.Json.JsonConvert.SerializeObject(bomChildItems.ToList()) : null;

                        var sqlProduct = @"INSERT INTO sales_product (
                            user_created, date_created, user_updated, date_updated, qty, is_active, stage, stage_item_id, bom_id, bom_child_item_ids, bom_accessory_item_ids
                        ) VALUES (
                            @UserCreated, @DateCreated, @UserUpdated, @DateUpdated, @Qty, @IsActive, @Stage, @StageItemId, @BomId, CAST(@BomChildItemIds AS jsonb), CAST(@BomAccessoryItemIds AS jsonb)
                        );";
                        await connection.ExecuteAsync(sqlProduct, new
                        {
                            UserCreated = request.Quotation.UserCreated,
                            DateCreated = DateTime.UtcNow,
                            UserUpdated = (int?)null,
                            DateUpdated = (DateTime?)null,
                            Qty = item.Quantity,
                            IsActive = true,
                            Stage = "Quotation",
                            StageItemId = quotationId.ToString(),
                            BomId = item.BomId,
                            BomChildItemIds = bomChildItemIdsJson,
                            BomAccessoryItemIds = item.AccessoryItemIds != null ? Newtonsoft.Json.JsonConvert.SerializeObject(item.AccessoryItemIds) : null
                        }, transaction);
                    }
                }

                // Prepare items array with full details for response
                var itemsArray = new List<object>();
                foreach (var item in request.Items)
                {
                    // Fetch accessory item details
                    List<dynamic> accessoryItemDetails = new List<dynamic>();
                    if (item.AccessoryItemIds != null && item.AccessoryItemIds.Count > 0)
                    {
                        var accessoryQuery = @"SELECT im.*, c.name AS category_name, m.name as make, mo.name as model, p.name as product FROM item_master im LEFT JOIN categories c ON im.category_id = c.id LEFT JOIN make m ON im.make_id = m.id LEFT JOIN model mo ON im.model_id = mo.id LEFT JOIN product p ON im.product_id = p.id WHERE im.id = ANY(@AccessoryItemIds)";
                        var rawAccessoryItems = await connection.QueryAsync<dynamic>(accessoryQuery, new { AccessoryItemIds = item.AccessoryItemIds }, transaction);
                        accessoryItemDetails = rawAccessoryItems.Select(ai => new {
                            id = ai.id,
                            make = ai.make,
                            model = ai.model,
                            product = ai.product,
                            itemName = ai.item_name,
                            itemCode = ai.item_code,
                            unitPrice = ai.unit_price,
                            hsn = ai.hsn,
                            taxPercentage = ai.tax_percentage,
                            categoryName = ai.category_name
                        }).ToList<dynamic>();
                    }

                    // Fetch BOM details
                    var bomDetailsFull = await connection.QueryFirstOrDefaultAsync(
                        @"SELECT bom_id, bom_name, bom_type FROM bill_of_materials WHERE bom_id = @BomId LIMIT 1",
                        new { BomId = item.BomId }, transaction);

                    // Fetch BOM child items
                    var bomChildItems = await connection.QueryAsync<dynamic>(
                        @"SELECT im.id, m.name as make, mo.name as model, p.name as product, im.item_name, im.item_code, im.unit_price, im.hsn, im.tax_percentage, c.name AS category_name FROM bill_of_material_child_items bci JOIN item_master im ON bci.child_item_id = im.id LEFT JOIN categories c ON im.category_id = c.id LEFT JOIN make m ON im.make_id = m.id LEFT JOIN model mo ON im.model_id = mo.id LEFT JOIN product p ON im.product_id = p.id WHERE bci.bill_of_material_id = (SELECT id FROM bill_of_materials WHERE bom_id = @BomId)",
                        new { BomId = item.BomId }, transaction);
                    var bomChildItemList = bomChildItems.Select(ci => new {
                        id = ci.id,
                        make = ci.make,
                        model = ci.model,
                        product = ci.product,
                        itemName = ci.item_name,
                        itemCode = ci.item_code,
                        unitPrice = ci.unit_price,
                        hsn = ci.hsn,
                        taxPercentage = ci.tax_percentage,
                        categoryName = ci.category_name
                    }).ToList();

                    itemsArray.Add(new {
                        bomId = item.BomId,
                        bomName = bomDetailsFull?.bom_name,
                        bomType = bomDetailsFull?.bom_type,
                        bomChildItems = bomChildItemList,
                        accessoryItemIds = item.AccessoryItemIds,
                        accessoryItems = accessoryItemDetails,
                        quantity = item.Quantity
                    });
                }


                var response = new SalesQuotationWithItemsResponse
                {
                    Quotation = createdQuotation ?? new QuotationResponseDto {
                        UserCreated = request.Quotation.UserCreated,
                        DateCreated = DateTime.UtcNow,
                        UserUpdated = request.Quotation.UserUpdated,
                        DateUpdated = DateTime.UtcNow,
                        Id = 0,
                        QuotationType = request.Quotation.QuotationType,
                        OrderType = request.Quotation.OrderType,
                        QuotationDate = request.Quotation.QuotationDate,
                        Status = request.Quotation.Status,
                        Version = request.Quotation.Version,
                        Terms = request.Quotation.Terms,
                        ValidTill = request.Quotation.ValidTill,
                        QuotationFor = request.Quotation.QuotationFor,
                        LostReason = request.Quotation.LostReason,
                        CustomerId = request.Quotation.CustomerId,
                        Comments = request.Quotation.Comments,
                        DeliveryWithin = request.Quotation.DeliveryWithin,
                        DeliveryAfter = request.Quotation.DeliveryAfter,
                        IsActive = request.Quotation.IsActive,
                        QuotationId = request.Quotation.QuotationId,
                        OpportunityId = request.Quotation.OpportunityId,
                        LeadId = request.Quotation.LeadId,
                        Taxes = request.Quotation.Taxes,
                        Delivery = request.Quotation.Delivery,
                        Payment = request.Quotation.Payment,
                        Warranty = request.Quotation.Warranty,
                        FreightCharge = request.Quotation.FreightCharge,
                        IsCurrent = request.Quotation.IsCurrent,
                        ParentSalesQuotationsId = request.Quotation.ParentSalesQuotationsId,
                        SalesOrderId = null,
                        OrderId = null,
                        OrderDate = null,
                        ExpectedDeliveryDate = null,
                        OrderStatus = null,
                        PoId = null,
                        AcceptanceDate = null,
                        TotalAmount = null,
                        TaxAmount = null,
                        GrandTotal = null,
                        Notes = null,
                        CustomerName = request.Quotation.CustomerName,
                        CustomerAddress = null,
                        CustomerMobile = null,
                        CustomerEmail = null,
                        Discount = null,
                        MobileNum = null,
                        ContactName = request.Quotation.ContactName,
                        ContactMobileNo = request.Quotation.ContactMobileNo,
                        AssignedTo = request.Quotation.AssignedTo
                    },
                    Items = itemsArray,
                    CustomerName = createdQuotation?.CustomerName ?? request.Quotation.CustomerName,
                    CustomerAddress = customerAddress,
                    TermsAndConditions = createdTerms,
                    LeadAddress = leadAddress,
                    ContactName = createdQuotation?.ContactName ?? request.Quotation.ContactName,
                    ContactMobileNo = createdQuotation?.ContactMobileNo ?? request.Quotation.ContactMobileNo
                };
                await transaction.CommitAsync();
                return Created($"api/sales-quotations/{quotationId}", response);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating quotation with items: {Message}", ex.Message);
                return StatusCode(500, new { message = "Failed to create quotation with items", error = ex.Message });
            }

        }


        /// <summary>
        /// Gets all sales quotations with their items.
        /// </summary>
        [HttpGet("with-items")]
        public async Task<ActionResult<IEnumerable<SalesQuotationWithItemsResponse>>> GetQuotationsWithItems()
        {
            var connectionString = _salesQuotationService.GetConnectionString();
            using var connection = new Npgsql.NpgsqlConnection(connectionString);
            await connection.OpenAsync();
         var quotations = (await connection.QueryAsync<QuotationResponseDto>(
          @"SELECT user_created, date_created, user_updated, date_updated, id, quotation_type, order_type, 
              quotation_date, status, version, terms, valid_till, quotation_for, lost_reason, customer_id, 
              comments, delivery_within, delivery_after, is_active, quotation_id, opportunity_id, lead_id, 
              CASE WHEN taxes ~ '^[0-9]+$' THEN taxes::int ELSE NULL END as taxes, 
              is_current, parent_sales_quotations_id, 
              customer_name, contact_name, contact_mobile_no
          FROM sales_quotations 
          WHERE is_active = true 
          ORDER BY date_created DESC" )).ToList();
            var result = new List<SalesQuotationWithItemsResponse>();
            foreach (var quotation in quotations)
            {
                var items = (await connection.QueryAsync<SalesItemResponse>(
                    @"SELECT si.*, m.name as make, mo.name as model, p.name as product, im.item_name as ItemName, im.item_code as ItemCode, im.unit_price as UnitPrice, im.hsn, im.tax_percentage as TaxPercentage 
                    FROM sales_product si 
                    JOIN item_master im ON si.item_id = im.id 
                    LEFT JOIN make m ON im.make_id = m.id LEFT JOIN model mo ON im.model_id = mo.id LEFT JOIN product p ON im.product_id = p.id
                    WHERE si.stage = 'Quotation' AND si.stage_item_id = @QuotationId",
                    new { QuotationId = quotation.Id.ToString() })).ToList();

                // Map items to required format
                var formattedItems = items.Select(item => new {
                    childItemId = item.Id,
                    quantity = item.Qty, // Use Qty property from SalesItemResponse
                    make = item.Make,
                    model = item.Model,
                    product = item.Product,
                    categoryName = item.Category, // Use Category property from SalesItemResponse
                    valuationMethodName = "", // Not available in SalesItemResponse, set blank or fetch if needed
                    inventoryMethodName = "", // Not available in SalesItemResponse, set blank or fetch if needed
                    inventoryTypeName = "", // Not available in SalesItemResponse, set blank or fetch if needed
                    unitPrice = item.UnitPrice,
                    itemName = item.ItemName,
                    itemCode = item.ItemCode,
                    catNo = "", // CatNo not available, set blank
                    uomName = item.Uom, // Use Uom property from SalesItemResponse
                    purchaseRate = "", // Not available, set blank
                    saleRate = "", // Not available, set blank
                    quoteRate = "", // Not available, set blank
                    hsn = item.Hsn,
                    tax = item.TaxPercentage
                }).ToList();

                // Fetch TermsAndConditions for this quotation
                SalesTermsAndConditions termsAndConditions = null;
                var terms = await connection.QueryFirstOrDefaultAsync<SalesTermsAndConditions>(
                    "SELECT * FROM sales_terms_and_conditions WHERE quotation_id = @QuotationId ORDER BY id DESC LIMIT 1",
                    new { QuotationId = quotation.Id });
                if (terms != null)
                {
                    termsAndConditions = terms;
                }

                // If warranty missing on the quotation, try a fallback: use terms from other versions (same base quotation number)
                try
                {
                    if (string.IsNullOrEmpty(termsAndConditions?.Warranty))
                    {
                        string baseQuotationNumber = null;
                        if (!string.IsNullOrEmpty(quotation.QuotationId) && quotation.QuotationId.Contains('/'))
                            baseQuotationNumber = quotation.QuotationId.Split('/')[0];
                        else if (!string.IsNullOrEmpty(quotation.QuotationId))
                            baseQuotationNumber = quotation.QuotationId;

                        if (!string.IsNullOrEmpty(baseQuotationNumber))
                        {
                            var fallback = await connection.QueryFirstOrDefaultAsync<SalesTermsAndConditions>(
                                @"SELECT stc.* FROM sales_terms_and_conditions stc
                                  JOIN sales_quotations sq ON stc.quotation_id = sq.id
                                  WHERE sq.quotation_id LIKE @Pattern
                                  ORDER BY stc.id DESC LIMIT 1",
                                new { Pattern = baseQuotationNumber + "/%" });
                            if (fallback != null)
                                termsAndConditions = fallback;
                        }
                    }

                    if (string.IsNullOrEmpty(quotation.Warranty) && termsAndConditions != null && !string.IsNullOrEmpty(termsAndConditions.Warranty))
                        quotation.Warranty = termsAndConditions.Warranty;
                }
                catch { }

                // Fetch customer address if needed
                ERP.API.Models.DTOs.SalesAddressDto customerAddress = null;
                string leadIdToUse = quotation.LeadId;
                if (!string.IsNullOrEmpty(quotation.OpportunityId))
                {
                    var opp = await connection.QueryFirstOrDefaultAsync<dynamic>(
                        "SELECT lead_id FROM sales_opportunities WHERE opportunity_id = @OpportunityId LIMIT 1",
                        new { OpportunityId = quotation.OpportunityId });
                    if (opp != null && opp.lead_id != null && !string.IsNullOrEmpty((string)opp.lead_id))
                    {
                        leadIdToUse = (string)opp.lead_id;
                    }
                }
                if (!string.IsNullOrEmpty(leadIdToUse))
                {
                    var addresses = await _salesAddressService.GetBySalesLeadIdAsync(int.TryParse(leadIdToUse, out var parsedLeadId) ? parsedLeadId : (int?)null);
                    var address = addresses?.FirstOrDefault(a => a.IsDefault == true) ?? addresses?.FirstOrDefault();
                    if (address != null)
                    {
                        customerAddress = new ERP.API.Models.DTOs.SalesAddressDto
                        {
                            Id = address.Id,
                            DoorNo = address.DoorNo,
                            Street = address.Street,
                            Area = address.Area,
                            Block = address.Block,
                            City = address.City,
                            State = address.State,
                            Pincode = address.Pincode,
                            Landmark = address.Landmark,
                            IsDefault = address.IsDefault,
                            Type = address.Type,
                            Department = address.Department,
                            OpportunityId = address.OpportunityId
                        };
                    }
                }

                // --- Fetch lead address fields from sales_lead if OpportunityId is present ---
                object leadAddress = null;
                if (!string.IsNullOrEmpty(quotation.OpportunityId))
                {
                    var leadIdObj = await connection.QueryFirstOrDefaultAsync<dynamic>(
                        "SELECT lead_id FROM sales_opportunities WHERE opportunity_id = @OpportunityId LIMIT 1",
                        new { OpportunityId = quotation.OpportunityId });
                    string leadId = null;
                    if (leadIdObj != null && leadIdObj.lead_id != null)
                        leadId = leadIdObj.lead_id.ToString();
                    if (!string.IsNullOrEmpty(leadId))
                    {
                        leadAddress = await connection.QueryFirstOrDefaultAsync(
                            @"SELECT pincode, area, state, district, city, door_no, street, landmark FROM sales_lead WHERE lead_id = @LeadId LIMIT 1",
                            new { LeadId = leadId });
                    }
                }
                result.Add(new SalesQuotationWithItemsResponse
                {
                    Quotation = quotation,
                    Items = formattedItems.Cast<object>().ToList(),
                    CustomerName = quotation.CustomerName,
                    TermsAndConditions = termsAndConditions,
                    CustomerAddress = customerAddress,
                    LeadAddress = leadAddress,
                    ContactName = quotation.ContactName,
                    ContactMobileNo = quotation.ContactMobileNo
                });
            }
            return Ok(result);
        }

        /// <summary>
        /// Gets a sales quotation with its items by ID.
        /// </summary>
        [HttpGet("{id}/with-items")]
        public async Task<ActionResult<SalesQuotationWithItemsResponse>> GetQuotationWithItemsById(int id)
        {
            var connectionString = _salesQuotationService.GetConnectionString();
            using var connection = new Npgsql.NpgsqlConnection(connectionString);
            await connection.OpenAsync();
            var quotation = await connection.QueryFirstOrDefaultAsync<QuotationResponseDto>(
                @"SELECT user_created, date_created, user_updated, date_updated, id, quotation_type, order_type, 
                    quotation_date, status, version, terms, valid_till, quotation_for, lost_reason, customer_id, 
                    comments, delivery_within, delivery_after, is_active, quotation_id, opportunity_id, lead_id, 
                    CASE WHEN taxes ~ '^[0-9]+$' THEN taxes::int ELSE NULL END as taxes, 
                    is_current, parent_sales_quotations_id, 
                    customer_name, contact_name, contact_mobile_no
                FROM sales_quotations WHERE id = @Id AND is_active = true",
                new { Id = id });
            if (quotation == null)
                return NotFound(new { message = $"Quotation with ID {id} not found" });

            // Use the standardized method to get items with proper BOM structure
            var items = await _salesQuotationService.GetItemsByQuotationIdAsync(quotation.Id);

            SalesTermsAndConditions termsAndConditions = null;
            var terms = await connection.QueryFirstOrDefaultAsync<SalesTermsAndConditions>(
                "SELECT * FROM sales_terms_and_conditions WHERE quotation_id = @QuotationId ORDER BY id DESC LIMIT 1",
                new { QuotationId = quotation.Id });
            if (terms != null)
                termsAndConditions = terms;

            // If warranty still missing, try to find terms from other versions of same quotation (base number)
            try
            {
                if ((termsAndConditions == null || string.IsNullOrEmpty(termsAndConditions.Warranty)))
                {
                    string baseQuotationNumber = null;
                    if (!string.IsNullOrEmpty(quotation.QuotationId) && quotation.QuotationId.Contains('/'))
                        baseQuotationNumber = quotation.QuotationId.Split('/')[0];
                    else if (!string.IsNullOrEmpty(quotation.QuotationId))
                        baseQuotationNumber = quotation.QuotationId;

                    if (!string.IsNullOrEmpty(baseQuotationNumber))
                    {
                        var fallbackTerms = await connection.QueryFirstOrDefaultAsync<SalesTermsAndConditions>(
                            @"SELECT stc.* FROM sales_terms_and_conditions stc
                               JOIN sales_quotations sq ON stc.quotation_id = sq.id
                               WHERE sq.quotation_id LIKE @Pattern
                               ORDER BY stc.id DESC LIMIT 1",
                            new { Pattern = baseQuotationNumber + "/%" });
                        if (fallbackTerms != null)
                        {
                            termsAndConditions = fallbackTerms;
                        }
                    }
                }

                // Warranty is now only available in terms and conditions
            }
            catch { /* ignore fallback failures */ }

            ERP.API.Models.DTOs.SalesAddressDto customerAddress = null;
            string leadIdToUse = quotation.LeadId;
            if (!string.IsNullOrEmpty(quotation.OpportunityId))
            {
                var opp = await connection.QueryFirstOrDefaultAsync<dynamic>(
                    "SELECT lead_id FROM sales_opportunities WHERE opportunity_id = @OpportunityId LIMIT 1",
                    new { OpportunityId = quotation.OpportunityId });
                if (opp != null && opp.lead_id != null && !string.IsNullOrEmpty((string)opp.lead_id))
                    leadIdToUse = (string)opp.lead_id;
            }
            // Build version history for this quotation (all versions that share the same base quotation number)
            List<object> versionHistory = new List<object>();
            try
            {
                string baseQuotationNumber = null;
                if (!string.IsNullOrEmpty(quotation.QuotationId) && quotation.QuotationId.Contains('/'))
                {
                    // Extract base number: QTN/25-26-001/1.1 -> QTN/25-26-001
                    var parts = quotation.QuotationId.Split('/');
                    if (parts.Length >= 2)
                        baseQuotationNumber = $"{parts[0]}/{parts[1]}";
                }
                else if (!string.IsNullOrEmpty(quotation.QuotationId))
                    baseQuotationNumber = quotation.QuotationId;

                if (!string.IsNullOrEmpty(baseQuotationNumber))
                {
                    var versions = await connection.QueryAsync<dynamic>(
                        @"SELECT id, quotation_id, version, status, date_created, user_created, customer_name 
                          FROM sales_quotations 
                          WHERE quotation_id LIKE @Pattern 
                          ORDER BY date_created DESC",
                        new { Pattern = baseQuotationNumber + "/_%" });
                    versionHistory = versions.Select(v => new {
                        quotationVersionId = v.quotation_id,
                        customerName = v.customer_name,
                        status = v.status
                    }).ToList<object>();
                }
            }
            catch { /* non-fatal; versionHistory will remain empty */ }

            object leadAddress = null;
            if (!string.IsNullOrEmpty(quotation.OpportunityId))
            {
                var leadIdObj = await connection.QueryFirstOrDefaultAsync<dynamic>(
                    "SELECT lead_id FROM sales_opportunities WHERE opportunity_id = @OpportunityId LIMIT 1",
                    new { OpportunityId = quotation.OpportunityId });
                string leadId = null;
                if (leadIdObj != null && leadIdObj.lead_id != null)
                    leadId = leadIdObj.lead_id.ToString();
                if (!string.IsNullOrEmpty(leadId))
                {
                    leadAddress = await connection.QueryFirstOrDefaultAsync(
                        "SELECT pincode, area, state, district, city, door_no, street, landmark FROM sales_lead WHERE lead_id = @LeadId LIMIT 1",
                        new { LeadId = leadId });
                }
            }

            // Build items array to match opportunity response format exactly
            var itemsList = new List<object>();
            var salesProductRows = await connection.QueryAsync<dynamic>(@"SELECT id, bom_id, qty, bom_accessory_item_ids, bom_child_item_ids FROM sales_product WHERE stage = 'Quotation' AND stage_item_id = @StageItemId", new { StageItemId = id.ToString() });
            foreach (var row in salesProductRows)
            {
                // Skip items with null or empty bom_id
                if (row.bom_id == null || string.IsNullOrWhiteSpace(row.bom_id.ToString()))
                    continue;

                List<int> accessoryIds = new List<int>();
                List<dynamic> accessoryItemDetails = new List<dynamic>();
                
                // Parse accessory item IDs from JSON field
                if (row.bom_accessory_item_ids != null)
                {
                    try
                    {
                        accessoryIds = Newtonsoft.Json.JsonConvert.DeserializeObject<List<int>>(row.bom_accessory_item_ids.ToString()) ?? new List<int>();
                    }
                    catch { }
                }
                
                // Fetch accessory item details
                if (accessoryIds.Count > 0)
                {
                    var accessoryQuery = @"SELECT im.id, im.item_name, im.item_code, im.unit_price, im.hsn, im.tax_percentage, c.name AS category_name, m.name as make, mo.name as model, p.name as product 
                                          FROM item_master im 
                                          LEFT JOIN categories c ON im.category_id = c.id 
                                          LEFT JOIN make m ON im.make_id = m.id 
                                          LEFT JOIN model mo ON im.model_id = mo.id 
                                          LEFT JOIN product p ON im.product_id = p.id 
                                          WHERE im.id = ANY(@AccessoryIds)";
                    var rawAccessoryItems = await connection.QueryAsync<dynamic>(accessoryQuery, new { AccessoryIds = accessoryIds.ToArray() });
                    accessoryItemDetails = rawAccessoryItems.Select(ai => new {
                        id = ai.id,
                        itemId = ai.id,
                        make = ai.make,
                        model = ai.model,
                        product = ai.product,
                        itemName = ai.item_name,
                        itemCode = ai.item_code,
                        unitPrice = ai.unit_price,
                        hsn = ai.hsn,
                        taxPercentage = ai.tax_percentage,
                        categoryName = ai.category_name,
                        quantity = 1
                    }).ToList<dynamic>();
                }
                
                // Fetch BOM details
                var bomDetailsFull = await connection.QueryFirstOrDefaultAsync(@"SELECT bom_id, bom_name, bom_type FROM bill_of_materials WHERE bom_id = @BomId LIMIT 1", new { BomId = row.bom_id });
                
                // Skip items where BOM details are not found or have null/empty bom_name
                if (bomDetailsFull == null || 
                    (string.IsNullOrWhiteSpace(bomDetailsFull.bom_name?.ToString()) && 
                     string.IsNullOrWhiteSpace(bomDetailsFull.bom_type?.ToString())))
                    continue;
                
                // Fetch BOM child items with full detail fields
                var bomChildItems = await connection.QueryAsync<dynamic>(@"SELECT bci.child_item_id, bci.quantity, im.item_name, im.item_code, im.cat_no, im.unit_price, im.hsn, im.tax_percentage, m.name as make, mo.name as model, p.name as product, c.name as category_name, vm.name as valuation_method_name, ime.name as inventory_method_name, itt.name as inventory_type_name, u.code as uom_name, rmi.purchase_rate AS purchase_rate, rmi.sales_rate AS sale_rate, rmi.quotation_rate AS quote_rate, bom.quote_title_id, bom.tc_template_id FROM bill_of_material_child_items bci JOIN item_master im ON bci.child_item_id = im.id LEFT JOIN make m ON im.make_id = m.id LEFT JOIN model mo ON im.model_id = mo.id LEFT JOIN product p ON im.product_id = p.id LEFT JOIN categories c ON im.category_id = c.id LEFT JOIN valuation_method vm ON im.valuation_method_id = vm.id LEFT JOIN inventory_method ime ON im.inventory_method_id = ime.id LEFT JOIN inventory_types itt ON im.group_id = itt.id LEFT JOIN uom u ON im.uom_id = u.id LEFT JOIN rate_master_items rmi ON im.id = rmi.item_id LEFT JOIN bill_of_materials bom ON bci.bill_of_material_id = bom.id WHERE bci.bill_of_material_id = (SELECT id FROM bill_of_materials WHERE bom_id = @BomId)", new { BomId = row.bom_id });
                
                var bomChildItemList = bomChildItems.Select(ci => new {
                    childItemId = ci.child_item_id,
                    quantity = ci.quantity,
                    make = ci.make,
                    model = ci.model,
                    product = ci.product,
                    categoryName = ci.category_name,
                    valuationMethodName = ci.valuation_method_name,
                    inventoryMethodName = ci.inventory_method_name,
                    inventoryTypeName = ci.inventory_type_name,
                    unitPrice = ci.unit_price,
                    itemName = ci.item_name,
                    itemCode = ci.item_code,
                    catNo = ci.cat_no,
                    uomName = ci.uom_name,
                    purchaseRate = ci.purchase_rate,
                    saleRate = ci.sale_rate,
                    quoteRate = ci.quote_rate,
                    hsn = ci.hsn,
                    tax = ci.tax_percentage == -1 ? 0 : ci.tax_percentage
                }).ToList();

                // Fetch accessories: first check quotation-level accessories, then fall back to opportunity-level
                var allAccessoryItems = new List<dynamic>();

                // Step 1: Read from the quotation's own sales_product_accessories row
                var quotAccQuery = @"SELECT spa.accessories_item_id AS id, spa.quantity, spa.parent_child_item_id,
                    ad.accessories_name AS item_name, ad.item_type AS category_name
                    FROM sales_product_accessories spa
                    JOIN accessories_details ad ON spa.accessories_item_id = ad.id
                    WHERE spa.sales_product_id = @SalesProductId";
                var quotAcc = await connection.QueryAsync<dynamic>(quotAccQuery, new { SalesProductId = (int)row.id });
                if (quotAcc.Any())
                {
                    // Quotation has its own accessories saved (from the Edit popup) - use these
                    allAccessoryItems = quotAcc.Select(ai => (dynamic)new {
                        id = (int)ai.id,
                        itemId = (int)ai.id,
                        itemName = (string)(ai.item_name ?? ""),
                        categoryName = (string)(ai.category_name ?? ""),
                        quantity = ai.quantity != null ? (int)ai.quantity : 1,
                        hsn = "",
                        unitPrice = 0,
                        taxPercentage = 0,
                        parentChildItemId = ai.parent_child_item_id != null ? (int?)ai.parent_child_item_id : null
                    }).ToList<dynamic>();
                }
                else
                {
                    // Step 2: Fall back to linked opportunity's accessories
                    var oppRow = await connection.QueryFirstOrDefaultAsync<dynamic>(
                        "SELECT id FROM sales_opportunities WHERE opportunity_id = @OpportunityId LIMIT 1",
                        new { OpportunityId = quotation.OpportunityId });
                    if (oppRow != null)
                    {
                        var oppSalesProduct = await connection.QueryFirstOrDefaultAsync<dynamic>(
                            "SELECT id FROM sales_product WHERE stage = 'Opportunity' AND stage_item_id = CAST(@StageItemId AS varchar) AND bom_id = @BomId LIMIT 1",
                            new { StageItemId = Convert.ToString(oppRow.id), BomId = (string)row.bom_id });
                        if (oppSalesProduct != null)
                        {
                            var accQuery = @"SELECT spa.accessories_item_id AS id, spa.quantity, spa.parent_child_item_id,
                                ad.accessories_name AS item_name, ad.item_type AS category_name, ad.qty AS default_qty
                                FROM sales_product_accessories spa
                                JOIN accessories_details ad ON spa.accessories_item_id = ad.id
                                WHERE spa.sales_product_id = @SalesProductId";
                            var rawAcc = await connection.QueryAsync<dynamic>(accQuery, new { SalesProductId = Convert.ToInt32(oppSalesProduct.id) });
                            allAccessoryItems = rawAcc.Select(ai => (dynamic)new {
                                id = (int)ai.id,
                                itemId = (int)ai.id,
                                itemName = (string)(ai.item_name ?? ""),
                                categoryName = (string)(ai.category_name ?? ""),
                                quantity = ai.quantity != null ? (int)ai.quantity : (ai.default_qty != null ? (int)(decimal)ai.default_qty : 1),
                                hsn = "",
                                unitPrice = 0,
                                taxPercentage = 0,
                                parentChildItemId = ai.parent_child_item_id != null ? (int?)ai.parent_child_item_id : null
                            }).ToList<dynamic>();
                        }
                    }
                }

                
                itemsList.Add(new {
                    bomId = row.bom_id,
                    bomName = bomDetailsFull?.bom_name,
                    bomType = bomDetailsFull?.bom_type,
                    childItems = bomChildItemList,
                    accessoryItems = allAccessoryItems,
                    accessoryItemIds = accessoryIds,
                    quantity = row.qty ?? 0
                });
            }
            // Extract quoteTitleId and tcTemplateId from first item's BOM child items
            int? responseQuoteTitleId = null;
            int? responseTcTemplateId = null;
            
            foreach (var row in salesProductRows)
            {
                if (row.bom_id != null)
                {
                    var bomChildItems = await connection.QueryAsync<dynamic>(
                        @"SELECT bom.quote_title_id, bom.tc_template_id 
                          FROM bill_of_material_child_items bci 
                          LEFT JOIN bill_of_materials bom ON bci.bill_of_material_id = bom.id 
                          WHERE bci.bill_of_material_id = (SELECT id FROM bill_of_materials WHERE bom_id = @BomId) 
                          LIMIT 1", 
                        new { BomId = row.bom_id });
                    
                    var firstBomChild = bomChildItems.FirstOrDefault();
                    if (firstBomChild != null)
                    {
                        responseQuoteTitleId = firstBomChild.quote_title_id;
                        responseTcTemplateId = firstBomChild.tc_template_id;
                        break; // Use values from first BOM found
                    }
                }
            }

            var response = new
            {
                Quotation = new
                {
                    Id = quotation?.Id,
                    UserCreated = quotation?.UserCreated,
                    DateCreated = quotation?.DateCreated,
                    UserUpdated = quotation?.UserUpdated,
                    DateUpdated = quotation?.DateUpdated,
                    Status = quotation?.Status,
                    Version = quotation?.Version,
                    QuotationType = quotation?.QuotationType,
                    OrderType = quotation?.OrderType,
                    QuotationDate = quotation?.QuotationDate,
                    ValidTill = quotation?.ValidTill,
                    QuotationFor = quotation?.QuotationFor,
                    CustomerId = quotation?.CustomerId,
                    CustomerName = quotation?.CustomerName,
                    Comments = quotation?.Comments,
                    IsActive = quotation?.IsActive,
                    LeadId = quotation?.LeadId,
                    OpportunityId = quotation?.OpportunityId,
                    QuotationId = quotation?.QuotationId,
                    ContactName = quotation?.ContactName,
                    ContactMobileNo = quotation?.ContactMobileNo
                },
                Items = itemsList,
                Stage = "Quotation",
                StageItemId = quotation?.Id,
                QuoteTitleId = responseQuoteTitleId,
                TcTemplateId = responseTcTemplateId,
                LeadAddress = leadAddress,
                VersionHistory = versionHistory
            };
            return Ok(response);
        }
            
        


        /// <summary>
        /// Delete a sales quotation with its items and related data
        /// </summary>
        /// <param name="id">The ID of the quotation to delete</param>
        /// <returns>Status of the delete operation</returns>
        [HttpDelete("{id}/with-items")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteQuotationWithItems(int id)
        {
            var connectionString = _salesQuotationService.GetConnectionString();
            using var connection = new Npgsql.NpgsqlConnection(connectionString);
            await connection.OpenAsync();
            using var transaction = await connection.BeginTransactionAsync();
            try
            {

            // Check if quotation exists
            var quotation = await connection.QueryFirstOrDefaultAsync<QuotationResponseDto>(@"
                SELECT user_created, date_created, user_updated, date_updated, id, quotation_type, order_type, 
                       quotation_date, status, version, terms, valid_till, quotation_for, lost_reason, customer_id, 
                       comments, delivery_within, delivery_after, is_active, quotation_id, opportunity_id, lead_id, 
                       CASE WHEN taxes ~ '^[0-9]+$' THEN taxes::int ELSE NULL END as taxes, 
                       delivery, payment, warranty, 
                       CASE WHEN freight_charge ~ '^[0-9]+\.?[0-9]*$' THEN freight_charge::decimal ELSE NULL END as freight_charge, 
                       is_current, parent_sales_quotations_id, 
                       customer_name, contact_name, contact_mobile_no
                FROM sales_quotations WHERE id = @Id", new { Id = id }, transaction);
            if (quotation == null)
            {
                await transaction.RollbackAsync();
                return NotFound(new { message = $"Quotation with ID {id} not found" });
            }


            // Delete related purchase orders referencing this quotation (to avoid FK violation)
            await connection.ExecuteAsync(
                "DELETE FROM purchase_order WHERE quotation_id = @QuotationId",
                new { QuotationId = id }, transaction);

            // Delete related sales invoices referencing this quotation (to avoid FK violation)
            await connection.ExecuteAsync(
                "DELETE FROM sales_invoices WHERE quotation_id = @QuotationId",
                new { QuotationId = id }, transaction);

            // Get all sales_product ids for this quotation
            var productIds = (await connection.QueryAsync<int>(
                "SELECT id FROM sales_product WHERE stage = 'Quotation' AND stage_item_id = @QuotationId",
                new { QuotationId = id.ToString() }, transaction)).ToList();

            if (productIds.Count > 0)
            {
                // Delete child items
                await connection.ExecuteAsync(
                    "DELETE FROM sales_product_child_items WHERE sales_product_id = ANY(@Ids)",
                    new { Ids = productIds.ToArray() }, transaction);
                // Delete accessories
                await connection.ExecuteAsync(
                    "DELETE FROM sales_product_accessories WHERE sales_product_id = ANY(@Ids)",
                    new { Ids = productIds.ToArray() }, transaction);
            }
            // Delete items
            await connection.ExecuteAsync(
                "DELETE FROM sales_product WHERE stage = 'Quotation' AND stage_item_id = @QuotationId",
                new { QuotationId = id.ToString() }, transaction);

            // Delete terms and conditions
            await connection.ExecuteAsync(
                "DELETE FROM sales_terms_and_conditions WHERE quotation_id = @QuotationId",
                new { QuotationId = id }, transaction);

            // Delete the quotation itself
            await connection.ExecuteAsync(
                "DELETE FROM sales_quotations WHERE id = @Id",
                new { Id = id }, transaction);

                await transaction.CommitAsync();
                return Ok(new { message = $"Quotation with ID {id} and its related items were deleted successfully." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error deleting quotation with items: {Message}", ex.Message);
                return StatusCode(500, new { message = "Failed to delete quotation with items", error = ex.Message });
            }
        }
   [HttpPut("{id}/consolidated")]
        [ProducesResponseType(typeof(SalesQuotationWithItemsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateQuotationConsolidated(int id, [FromBody] SalesQuotationWithItemsRequest request)
        {
            if (request == null || request.Quotation == null)
                return BadRequest(new { message = "Invalid request body" });

            // Map termsAndConditions fields to quotation fields if present
            if (request.TermsAndConditions != null)
            {
                if (!string.IsNullOrEmpty(request.TermsAndConditions.Taxes))
                {
                    if (int.TryParse(request.TermsAndConditions.Taxes, out var taxesInt))
                        request.Quotation.Taxes = taxesInt;
                }
                if (!string.IsNullOrEmpty(request.TermsAndConditions.FreightCharges))
                {
                    if (int.TryParse(request.TermsAndConditions.FreightCharges, out var freightInt))
                        request.Quotation.FreightCharge = freightInt;
                }
                if (!string.IsNullOrEmpty(request.TermsAndConditions.Delivery))
                    request.Quotation.Delivery = request.TermsAndConditions.Delivery;
                if (!string.IsNullOrEmpty(request.TermsAndConditions.Payment))
                    request.Quotation.Payment = request.TermsAndConditions.Payment;
                if (!string.IsNullOrEmpty(request.TermsAndConditions.Warranty))
                    request.Quotation.Warranty = request.TermsAndConditions.Warranty;
            }
            // If status is 'Final Quotation', set to 'Final Quotation' instead
            if (request.Quotation.Status != null && request.Quotation.Status.Trim().Equals("Final Quotation", StringComparison.OrdinalIgnoreCase))
            {
                request.Quotation.Status = "Final Quotation";
            }

            var connectionString = _salesQuotationService.GetConnectionString();
            using var connection = new Npgsql.NpgsqlConnection(connectionString);
            await connection.OpenAsync();
            using var transaction = await connection.BeginTransactionAsync();
            try
            {
                // Fetch the existing quotation to get the current quotationId and version
                var existingQuotation = await connection.QueryFirstOrDefaultAsync<QuotationResponseDto>(@"
                    SELECT user_created, date_created, user_updated, date_updated, id, quotation_type, order_type, 
                           quotation_date, status, version, terms, valid_till, quotation_for, lost_reason, customer_id, 
                           comments, delivery_within, delivery_after, is_active, quotation_id, opportunity_id, lead_id, 
                           CASE WHEN taxes ~ '^[0-9]+$' THEN taxes::int ELSE NULL END as taxes, 
                           delivery, payment, warranty, 
                           CASE WHEN freight_charge ~ '^[0-9]+\.?[0-9]*$' THEN freight_charge::decimal ELSE NULL END as freight_charge, 
                           is_current, parent_sales_quotations_id, 
                           customer_name, contact_name, contact_mobile_no
                    FROM sales_quotations WHERE id = @Id", new { Id = id }, transaction);
                if (existingQuotation == null)
                {
                    await transaction.RollbackAsync();
                    return NotFound(new { message = $"Quotation with ID {id} not found" });
                }

                string status = request.Quotation.Status?.Trim();
                string currentStatus = existingQuotation.Status?.Trim();
                
                // Check if this is a Draft status update - only update in place if BOTH current and new status are Draft
                bool isDraftUpdate = string.Equals(status, "Draft", StringComparison.OrdinalIgnoreCase) &&
                                   string.Equals(currentStatus, "Draft", StringComparison.OrdinalIgnoreCase);
                
                if (isDraftUpdate)
                {
                    // Update in place (same id, same quotationId) for Draft status
                    const string updateQuotationSql = @"UPDATE sales_quotations SET
                        user_updated = @UserUpdated,
                        date_updated = @DateUpdated,
                        version = @Version,
                        terms = @Terms,
                        valid_till = @ValidTill,
                        quotation_for = @QuotationFor,
                        status = @Status,
                        lost_reason = @LostReason,
                        customer_id = @CustomerId,
                        quotation_type = @QuotationType,
                        quotation_date = @QuotationDate,
                        order_type = @OrderType,
                        comments = @Comments,
                        delivery_within = @DeliveryWithin,
                        delivery_after = @DeliveryAfter,
                        is_active = @IsActive,
                        opportunity_id = @OpportunityId,
                        lead_id = @LeadId,
                        customer_name = @CustomerName,
                        taxes = @Taxes,
                        delivery = @Delivery,
                        payment = @Payment,
                        warranty = @Warranty,
                        freight_charge = @FreightCharge,
                        is_current = @IsCurrent,
                        parent_sales_quotations_id = @ParentSalesQuotationsId
                    WHERE id = @Id;";
                    await connection.ExecuteAsync(updateQuotationSql, new {
                        Id = id,
                        request.Quotation.UserUpdated,
                        DateUpdated = DateTime.UtcNow,
                        request.Quotation.Version,
                        request.Quotation.Terms,
                        request.Quotation.ValidTill,
                        request.Quotation.QuotationFor,
                        request.Quotation.Status,
                        request.Quotation.LostReason,
                        request.Quotation.CustomerId,
                        request.Quotation.QuotationType,
                        request.Quotation.QuotationDate,
                        request.Quotation.OrderType,
                        request.Quotation.Comments,
                        request.Quotation.DeliveryWithin,
                        request.Quotation.DeliveryAfter,
                        IsActive = request.Quotation.IsActive,
                        request.Quotation.OpportunityId,
                        request.Quotation.LeadId,
                        request.Quotation.CustomerName,
                        request.Quotation.Taxes,
                        request.Quotation.Delivery,
                        request.Quotation.Payment,
                        request.Quotation.Warranty,
                        request.Quotation.FreightCharge,
                        IsCurrent = request.Quotation.IsCurrent,
                        ParentSalesQuotationsId = (request.Quotation.ParentSalesQuotationsId == 0 ? null : request.Quotation.ParentSalesQuotationsId)
                    }, transaction);

                    // If status is 'Final Quotation', create a purchase order
                    if (string.Equals(status, "Final Quotation", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            // Get the user ID from claims for audit trail
                            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                            int? userId = null;
                            if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out var parsedUserId))
                                userId = parsedUserId;

                            // Create purchase order from quotation
                            var createdPO = await _purchaseOrderService.CreatePurchaseOrderFromQuotationAsync(id, userId);
                            if (createdPO != null)
                            {
                                _logger.LogInformation($"Purchase Order {createdPO.PoId} created from Quotation ID {id}");
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Error creating purchase order from quotation {id}");
                            // Don't fail the quotation update if PO creation fails, just log it
                        }
                    }

                    // Update opportunity status to "Negotiation" when quotation status is "Negotiation"
                    if (string.Equals(status, "Negotiation", StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogInformation($"Quotation {id} status changed to Negotiation in consolidated update. ExistingQuotation OpportunityId: {existingQuotation.OpportunityId}");
                        
                        if (!string.IsNullOrEmpty(existingQuotation.OpportunityId))
                        {
                            var rowsUpdated = await connection.ExecuteAsync(
                                "UPDATE sales_opportunities SET status = @Status, date_updated = @DateUpdated WHERE opportunity_id = @OpportunityId",
                                new { Status = "Negotiation", DateUpdated = DateTime.UtcNow, OpportunityId = existingQuotation.OpportunityId },
                                transaction);
                            
                            _logger.LogInformation($"Updated {rowsUpdated} opportunity records to Negotiation status for OpportunityId: {existingQuotation.OpportunityId}");
                        }
                        else
                        {
                            _logger.LogWarning($"No opportunity_id found for quotation {id} in consolidated update");
                        }
                    }

                    // Optionally update items if provided
                    if (request.Items != null)
                    {
                        // Delete existing child/accessory links first (to avoid orphaned rows)
                        var existingProductIds = (await connection.QueryAsync<int>(
                            "SELECT id FROM sales_product WHERE stage = 'Quotation' AND stage_item_id = @QuotationId",
                            new { QuotationId = id.ToString() }, transaction)).ToList();
                        if (existingProductIds.Count > 0)
                        {
                            await connection.ExecuteAsync(
                                "DELETE FROM sales_product_child_items WHERE sales_product_id = ANY(@Ids)",
                                new { Ids = existingProductIds.ToArray() }, transaction);
                            await connection.ExecuteAsync(
                                "DELETE FROM sales_product_accessories WHERE sales_product_id = ANY(@Ids)",
                                new { Ids = existingProductIds.ToArray() }, transaction);
                        }
                        // Delete existing items for this quotation
                        await connection.ExecuteAsync(
                            "DELETE FROM sales_product WHERE stage = 'Quotation' AND stage_item_id = @QuotationId",
                            new { QuotationId = id.ToString() }, transaction);

                        // Insert new items and always get the new ID for child/accessory inserts
                        foreach (var item in request.Items)
                        {
                            // First fetch BOM child item IDs for this BOM
                            var bomChildItems = await connection.QueryAsync<int>(
                                @"SELECT child_item_id FROM bill_of_material_child_items WHERE bill_of_material_id = (SELECT id FROM bill_of_materials WHERE bom_id = @BomId)",
                                new { BomId = item.BomId }, transaction);
                            var bomChildItemIdsJson = bomChildItems.Any() ? System.Text.Json.JsonSerializer.Serialize(bomChildItems.ToList()) : null;

                            // Now insert the sales_product with both BOM child items and accessory items
                            var sql = @"INSERT INTO public.sales_product (bom_id, qty, bom_child_item_ids, bom_accessory_item_ids, stage, stage_item_id) 
                                      VALUES (@BomId, @Qty, CAST(@BomChildItemIds AS jsonb), CAST(@AccessoryItemIds AS jsonb), @Stage, @StageItemId) 
                                      RETURNING id;";
                            var salesProductId = await connection.ExecuteScalarAsync<int>(sql, new {
                                BomId = item.BomId,
                                Qty = item.Quantity,
                                BomChildItemIds = bomChildItemIdsJson,
                                AccessoryItemIds = item.AccessoryItemIds != null ? System.Text.Json.JsonSerializer.Serialize(item.AccessoryItemIds) : null,
                                Stage = "Quotation",
                                StageItemId = id.ToString()
                            }, transaction);

                            // Insert accessories into sales_product_accessories (prefer AccessoryItems for parent_child_item_id)
                            if (item.AccessoryItems != null && item.AccessoryItems.Count > 0)
                            {
                                foreach (var acc in item.AccessoryItems)
                                {
                                    await connection.ExecuteAsync(
                                        @"INSERT INTO sales_product_accessories (sales_product_id, accessories_item_id, quantity, isactive, date_created, parent_child_item_id)
                                          VALUES (@SalesProductId, @AccessoryItemId, @Qty, true, @DateCreated, @ParentChildItemId)",
                                        new {
                                            SalesProductId = salesProductId,
                                            AccessoryItemId = acc.AccessoryDetailId,
                                            Qty = acc.Qty > 0 ? acc.Qty : 1,
                                            DateCreated = DateTime.UtcNow,
                                            ParentChildItemId = acc.ParentChildItemId
                                        },
                                        transaction);
                                }
                            }
                            else if (item.AccessoryItemIds != null && item.AccessoryItemIds.Count > 0)
                            {
                                // Fallback: insert without parent_child_item_id
                                foreach (var accId in item.AccessoryItemIds)
                                {
                                    await connection.ExecuteAsync(
                                        @"INSERT INTO sales_product_accessories (sales_product_id, accessories_item_id, quantity, isactive, date_created)
                                          VALUES (@SalesProductId, @AccessoryItemId, 1, true, @DateCreated)",
                                        new { SalesProductId = salesProductId, AccessoryItemId = accId, DateCreated = DateTime.UtcNow },
                                        transaction);
                                }
                            }
                        }
                    }

                    // Upsert TermsAndConditions for this quotation if provided
                    if (request.TermsAndConditions != null)
                    {
                        var t = request.TermsAndConditions;
                        var existingTerms = await connection.QueryFirstOrDefaultAsync<SalesTermsAndConditions>(
                            "SELECT * FROM sales_terms_and_conditions WHERE quotation_id = @QuotationId ORDER BY id DESC LIMIT 1",
                            new { QuotationId = id }, transaction);
                        if (existingTerms != null)
                        {
                            var updateSql = @"UPDATE sales_terms_and_conditions SET user_updated = @UserUpdated, date_updated = @DateUpdated, taxes = @Taxes, freight_charges = @FreightCharges, delivery = @Delivery, payment = @Payment, warranty = @Warranty, template_name = @TemplateName, is_default = @IsDefault, is_active = @IsActive WHERE id = @Id";
                            await connection.ExecuteAsync(updateSql, new
                            {
                                UserUpdated = t.UserUpdated != 0 ? t.UserUpdated : (request.Quotation.UserUpdated == 0 ? existingTerms.UserUpdated : request.Quotation.UserUpdated),
                                DateUpdated = DateTime.UtcNow,
                                Taxes = t.Taxes,
                                FreightCharges = t.FreightCharges,
                                Delivery = t.Delivery,
                                Payment = t.Payment,
                                Warranty = t.Warranty,
                                TemplateName = t.TemplateName,
                                IsDefault = t.IsDefault,
                                IsActive = t.IsActive,
                                Id = existingTerms.Id
                            }, transaction);
                        }
                        else
                        {
                            var insertSql = @"INSERT INTO sales_terms_and_conditions (user_created, date_created, user_updated, date_updated, taxes, freight_charges, delivery, payment, warranty, template_name, is_default, is_active, quotation_id) VALUES (@UserCreated, @DateCreated, @UserUpdated, @DateUpdated, @Taxes, @FreightCharges, @Delivery, @Payment, @Warranty, @TemplateName, @IsDefault, @IsActive, @QuotationId);";
                            await connection.ExecuteAsync(insertSql, new
                            {
                                UserCreated = t.UserCreated != 0 ? t.UserCreated : (request.Quotation.UserCreated == 0 ? 0 : request.Quotation.UserCreated),
                                DateCreated = DateTime.UtcNow,
                                UserUpdated = t.UserUpdated,
                                DateUpdated = DateTime.UtcNow,
                                Taxes = t.Taxes,
                                FreightCharges = t.FreightCharges,
                                Delivery = t.Delivery,
                                Payment = t.Payment,
                                Warranty = t.Warranty,
                                TemplateName = t.TemplateName,
                                IsDefault = t.IsDefault,
                                IsActive = t.IsActive,
                                QuotationId = id
                            }, transaction);
                        }
                    }

                    // Fetch updated quotation and items for response
                    var quotation = await connection.QueryFirstOrDefaultAsync<QuotationResponseDto>(@"
                        SELECT user_created, date_created, user_updated, date_updated, id, quotation_type, order_type, 
                               quotation_date, status, version, terms, valid_till, quotation_for, lost_reason, customer_id, 
                               comments, delivery_within, delivery_after, is_active, quotation_id, opportunity_id, lead_id, 
                               CASE WHEN taxes ~ '^[0-9]+$' THEN taxes::int ELSE NULL END as taxes, 
                               delivery, payment, warranty, 
                               CASE WHEN freight_charge ~ '^[0-9]+\.?[0-9]*$' THEN freight_charge::decimal ELSE NULL END as freight_charge, 
                               is_current, parent_sales_quotations_id, 
                               customer_name, contact_name, contact_mobile_no
                        FROM sales_quotations WHERE id = @Id", new { Id = id }, transaction);
                    // Fetch updated items using BOM-based approach (same as GET endpoint)
                    var updatedSalesProductRows = await connection.QueryAsync<dynamic>(
                        @"SELECT id, bom_id, qty, bom_accessory_item_ids FROM sales_product WHERE stage = 'Quotation' AND stage_item_id = @StageItemId",
                        new { StageItemId = id.ToString() }, transaction);

                    var itemsList = new List<object>();
                    foreach (var row in updatedSalesProductRows)
                    {
                        if (row.bom_id == null || string.IsNullOrWhiteSpace(row.bom_id.ToString())) continue;

                        List<int> accessoryIds = new List<int>();
                        if (row.bom_accessory_item_ids != null)
                        {
                            try { accessoryIds = Newtonsoft.Json.JsonConvert.DeserializeObject<List<int>>(row.bom_accessory_item_ids.ToString()) ?? new List<int>(); } catch { }
                        }

                        var bomDetailsFull = await connection.QueryFirstOrDefaultAsync<dynamic>(
                            @"SELECT bom_id, bom_name, bom_type FROM bill_of_materials WHERE bom_id = @BomId LIMIT 1",
                            new { BomId = row.bom_id }, transaction);

                        // Fetch accessories from sales_product_accessories joined with accessories_details
                        var accDetails = await connection.QueryAsync<dynamic>(
                            @"SELECT spa.accessories_item_id AS id, spa.quantity, spa.parent_child_item_id,
                              ad.accessories_name AS item_name, ad.item_type AS category_name
                              FROM sales_product_accessories spa
                              JOIN accessories_details ad ON spa.accessories_item_id = ad.id
                              WHERE spa.sales_product_id = @SalesProductId",
                            new { SalesProductId = (int)row.id }, transaction);

                        var accessoryItemsList = accDetails.Select(ai => (object)new {
                            id = (int)ai.id,
                            itemId = (int)ai.id,
                            itemName = (string)(ai.item_name ?? ""),
                            categoryName = (string)(ai.category_name ?? ""),
                            quantity = ai.quantity != null ? (int)ai.quantity : 1,
                            parentChildItemId = ai.parent_child_item_id != null ? (int?)ai.parent_child_item_id : null
                        }).ToList();

                        itemsList.Add(new {
                            bomId = row.bom_id,
                            bomName = bomDetailsFull?.bom_name,
                            bomType = bomDetailsFull?.bom_type,
                            accessoryItems = accessoryItemsList,
                            accessoryItemIds = accessoryIds,
                            quantity = row.qty ?? 0
                        });
                    }

                    // Fetch TermsAndConditions for this quotation
                    SalesTermsAndConditions termsAndConditions = null;
                    var terms = await connection.QueryFirstOrDefaultAsync<SalesTermsAndConditions>(
                        "SELECT * FROM sales_terms_and_conditions WHERE quotation_id = @QuotationId ORDER BY id DESC LIMIT 1",
                        new { QuotationId = id }, transaction);
                    if (terms != null)
                    {
                        termsAndConditions = terms;
                    }
                    // Ensure the quotation has a warranty value: use terms warranty when quotation.warranty is null
                    try
                    {
                        if (string.IsNullOrEmpty(quotation.Warranty) && termsAndConditions != null && !string.IsNullOrEmpty(termsAndConditions.Warranty))
                        {
                            quotation.Warranty = termsAndConditions.Warranty;
                        }
                    }
                    catch { }

                    // Fetch address based on opportunityId -> leadId -> address
                    ERP.API.Models.DTOs.SalesAddressDto customerAddress = null;
                    string leadIdToUse = quotation.LeadId;
                    if (!string.IsNullOrEmpty(quotation.OpportunityId))
                    {
                        var opp = await connection.QueryFirstOrDefaultAsync<dynamic>(
                            "SELECT lead_id FROM sales_opportunities WHERE opportunity_id = @OpportunityId LIMIT 1",
                            new { OpportunityId = quotation.OpportunityId }, transaction);
                        if (opp != null && opp.lead_id != null && !string.IsNullOrEmpty((string)opp.lead_id))
                        {
                            leadIdToUse = (string)opp.lead_id;
                        }
                    }
                    if (!string.IsNullOrEmpty(leadIdToUse))
                    {
                        var addresses = await _salesAddressService.GetBySalesLeadIdAsync(int.TryParse(leadIdToUse, out var parsedLeadId) ? parsedLeadId : (int?)null);
                        var address = addresses?.FirstOrDefault(a => a.IsDefault == true) ?? addresses?.FirstOrDefault();
                        if (address != null)
                        {
                            customerAddress = new ERP.API.Models.DTOs.SalesAddressDto
                            {
                                Id = address.Id,
                                DoorNo = address.DoorNo,
                                Street = address.Street,
                                Area = address.Area,
                                Block = address.Block,
                                City = address.City,
                                State = address.State,
                                Pincode = address.Pincode,
                                Landmark = address.Landmark,
                                IsDefault = address.IsDefault,
                                Type = address.Type,
                                Department = address.Department,
                                OpportunityId = address.OpportunityId
                            };
                        }
                    }

                    await transaction.CommitAsync();
                    return Ok(new SalesQuotationWithItemsResponse
                    {
                        Quotation = quotation,
                        Items = itemsList,
                        CustomerName = quotation.CustomerName,
                        TermsAndConditions = termsAndConditions,
                        CustomerAddress = customerAddress
                    });
                }
                else
                {
                    // Create a new record with incremented version for any non-Draft status change
                    string newQuotationId = IncrementQuotationVersion(existingQuotation.QuotationId);
                    string[] parts = newQuotationId.Split('/');
                    string newVersion = parts.Length > 1 ? parts[1] : "1.2";
                    
                    request.Quotation.Version = newVersion;
                    request.Quotation.QuotationId = newQuotationId;
                    
                    // If changing FROM Draft status, ensure the original Draft record remains visible in history
                    if (string.Equals(currentStatus, "Draft", StringComparison.OrdinalIgnoreCase))
                    {
                        // Keep the original Draft record as-is for version history
                        // The new version will be created below
                    }

                    // Insert new quotation
                    const string insertQuotationSql = @"
                        INSERT INTO sales_quotations (
                            user_created, date_created, version, terms, valid_till, quotation_for, status, lost_reason, customer_id, quotation_type, quotation_date, order_type, comments, delivery_within, delivery_after, is_active, opportunity_id, lead_id, customer_name, taxes, delivery, payment, warranty, freight_charge, is_current, parent_sales_quotations_id, quotation_id
                        ) VALUES (
                            @UserCreated, @DateCreated, @Version, @Terms, @ValidTill, @QuotationFor, @Status, @LostReason, @CustomerId, @QuotationType, @QuotationDate, @OrderType, @Comments, @DeliveryWithin, @DeliveryAfter, @IsActive, @OpportunityId, @LeadId, @CustomerName, @Taxes, @Delivery, @Payment, @Warranty, @FreightCharge, @IsCurrent, @ParentSalesQuotationsId, @QuotationId
                        ) RETURNING id";
                    var newId = await connection.ExecuteScalarAsync<int>(insertQuotationSql, new {
                        request.Quotation.UserCreated,
                        DateCreated = DateTime.UtcNow,
                        request.Quotation.Version,
                        request.Quotation.Terms,
                        request.Quotation.ValidTill,
                        request.Quotation.QuotationFor,
                        request.Quotation.Status,
                        request.Quotation.LostReason,
                        request.Quotation.CustomerId,
                        request.Quotation.QuotationType,
                        request.Quotation.QuotationDate,
                        request.Quotation.OrderType,
                        request.Quotation.Comments,
                        request.Quotation.DeliveryWithin,
                        request.Quotation.DeliveryAfter,
                        IsActive = request.Quotation.IsActive,
                        request.Quotation.OpportunityId,
                        LeadId = string.IsNullOrEmpty(request.Quotation.LeadId) ? null : request.Quotation.LeadId,
                        request.Quotation.CustomerName,
                        request.Quotation.Taxes,
                        request.Quotation.Delivery,
                        request.Quotation.Payment,
                        request.Quotation.Warranty,
                        request.Quotation.FreightCharge,
                        IsCurrent = request.Quotation.IsCurrent,
                        ParentSalesQuotationsId = (request.Quotation.ParentSalesQuotationsId == 0 ? null : request.Quotation.ParentSalesQuotationsId),
                        QuotationId = request.Quotation.QuotationId
                    }, transaction);

                    // Update opportunity status to "Negotiation" when quotation status is "Negotiation" (new version path)
                    if (string.Equals(status, "Negotiation", StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogInformation($"New quotation version {newId} status is Negotiation. ExistingQuotation OpportunityId: {existingQuotation.OpportunityId}");
                        
                        if (!string.IsNullOrEmpty(existingQuotation.OpportunityId))
                        {
                            var rowsUpdated = await connection.ExecuteAsync(
                                "UPDATE sales_opportunities SET status = @Status, date_updated = @DateUpdated WHERE opportunity_id = @OpportunityId",
                                new { Status = "Negotiation", DateUpdated = DateTime.UtcNow, OpportunityId = existingQuotation.OpportunityId },
                                transaction);
                            
                            _logger.LogInformation($"Updated {rowsUpdated} opportunity records to Negotiation status for OpportunityId: {existingQuotation.OpportunityId} (new version)");
                        }
                        else
                        {
                            _logger.LogWarning($"No opportunity_id found for quotation {newId} in new version path");
                        }
                    }

                    // Insert items for new quotation
                    if (request.Items != null)
                    {
                        foreach (var item in request.Items)
                        {
                            // First fetch BOM child item IDs for this BOM
                            var bomChildItems = await connection.QueryAsync<int>(
                                @"SELECT child_item_id FROM bill_of_material_child_items WHERE bill_of_material_id = (SELECT id FROM bill_of_materials WHERE bom_id = @BomId)",
                                new { BomId = item.BomId }, transaction);
                            var bomChildItemIdsJson = bomChildItems.Any() ? System.Text.Json.JsonSerializer.Serialize(bomChildItems.ToList()) : null;

                            // Now insert the sales_product with both BOM child items and accessory items
                            var sql = @"INSERT INTO public.sales_product (bom_id, qty, bom_child_item_ids, bom_accessory_item_ids, stage, stage_item_id) 
                                      VALUES (@BomId, @Qty, CAST(@BomChildItemIds AS jsonb), CAST(@AccessoryItemIds AS jsonb), @Stage, @StageItemId) 
                                      RETURNING id;";
                            var salesProductId = await connection.ExecuteScalarAsync<int>(sql, new {
                                BomId = item.BomId,
                                Qty = item.Quantity,
                                BomChildItemIds = bomChildItemIdsJson,
                                AccessoryItemIds = item.AccessoryItemIds != null ? System.Text.Json.JsonSerializer.Serialize(item.AccessoryItemIds) : null,
                                Stage = "Quotation",
                                StageItemId = newId.ToString()
                            }, transaction);

                            // Insert accessories with parent_child_item_id (prefer AccessoryItems for scoped display)
                            if (item.AccessoryItems != null && item.AccessoryItems.Count > 0)
                            {
                                foreach (var acc in item.AccessoryItems)
                                {
                                    await connection.ExecuteAsync(
                                        @"INSERT INTO sales_product_accessories (sales_product_id, accessories_item_id, quantity, isactive, date_created, parent_child_item_id)
                                          VALUES (@SalesProductId, @AccessoryItemId, @Qty, true, @DateCreated, @ParentChildItemId)",
                                        new {
                                            SalesProductId = salesProductId,
                                            AccessoryItemId = acc.AccessoryDetailId,
                                            Qty = acc.Qty > 0 ? acc.Qty : 1,
                                            DateCreated = DateTime.UtcNow,
                                            ParentChildItemId = acc.ParentChildItemId
                                        },
                                        transaction);
                                }
                            }
                            else if (item.AccessoryItemIds != null && item.AccessoryItemIds.Count > 0)
                            {
                                foreach (var accId in item.AccessoryItemIds)
                                {
                                    await connection.ExecuteAsync(
                                        @"INSERT INTO sales_product_accessories (sales_product_id, accessories_item_id, quantity, isactive, date_created)
                                          VALUES (@SalesProductId, @AccessoryItemId, 1, true, @DateCreated)",
                                        new { SalesProductId = salesProductId, AccessoryItemId = accId, DateCreated = DateTime.UtcNow },
                                        transaction);
                                }
                            }
                        }
                    }

                    // Upsert TermsAndConditions for the new quotation if provided
                    if (request.TermsAndConditions != null)
                    {
                        var t = request.TermsAndConditions;
                        var existingTermsNew = await connection.QueryFirstOrDefaultAsync<SalesTermsAndConditions>(
                            "SELECT * FROM sales_terms_and_conditions WHERE quotation_id = @QuotationId ORDER BY id DESC LIMIT 1",
                            new { QuotationId = newId }, transaction);
                        if (existingTermsNew != null)
                        {
                            var updateSql = @"UPDATE sales_terms_and_conditions SET user_updated = @UserUpdated, date_updated = @DateUpdated, taxes = @Taxes, freight_charges = @FreightCharges, delivery = @Delivery, payment = @Payment, warranty = @Warranty, template_name = @TemplateName, is_default = @IsDefault, is_active = @IsActive WHERE id = @Id";
                            await connection.ExecuteAsync(updateSql, new
                            {
                                UserUpdated = t.UserUpdated != 0 ? t.UserUpdated : (request.Quotation.UserUpdated == 0 ? existingTermsNew.UserUpdated : request.Quotation.UserUpdated),
                                DateUpdated = DateTime.UtcNow,
                                Taxes = t.Taxes,
                                FreightCharges = t.FreightCharges,
                                Delivery = t.Delivery,
                                Payment = t.Payment,
                                Warranty = t.Warranty,
                                TemplateName = t.TemplateName,
                                IsDefault = t.IsDefault,
                                IsActive = t.IsActive,
                                Id = existingTermsNew.Id
                            }, transaction);
                        }
                        else
                        {
                            var insertSql = @"INSERT INTO sales_terms_and_conditions (user_created, date_created, user_updated, date_updated, taxes, freight_charges, delivery, payment, warranty, template_name, is_default, is_active, quotation_id) VALUES (@UserCreated, @DateCreated, @UserUpdated, @DateUpdated, @Taxes, @FreightCharges, @Delivery, @Payment, @Warranty, @TemplateName, @IsDefault, @IsActive, @QuotationId);";
                            await connection.ExecuteAsync(insertSql, new
                            {
                                UserCreated = t.UserCreated != 0 ? t.UserCreated : (request.Quotation.UserCreated == 0 ? 0 : request.Quotation.UserCreated),
                                DateCreated = DateTime.UtcNow,
                                UserUpdated = t.UserUpdated,
                                DateUpdated = DateTime.UtcNow,
                                Taxes = t.Taxes,
                                FreightCharges = t.FreightCharges,
                                Delivery = t.Delivery,
                                Payment = t.Payment,
                                Warranty = t.Warranty,
                                TemplateName = t.TemplateName,
                                IsDefault = t.IsDefault,
                                IsActive = t.IsActive,
                                QuotationId = newId
                            }, transaction);
                        }
                    }

                    // Fetch new quotation and items for response
                    var quotation = await connection.QueryFirstOrDefaultAsync<QuotationResponseDto>(@"
                        SELECT user_created, date_created, user_updated, date_updated, id, quotation_type, order_type, 
                               quotation_date, status, version, terms, valid_till, quotation_for, lost_reason, customer_id, 
                               comments, delivery_within, delivery_after, is_active, quotation_id, opportunity_id, lead_id, 
                               CASE WHEN taxes ~ '^[0-9]+$' THEN taxes::int ELSE NULL END as taxes, 
                               delivery, payment, warranty, 
                               CASE WHEN freight_charge ~ '^[0-9]+\.?[0-9]*$' THEN freight_charge::decimal ELSE NULL END as freight_charge, 
                               is_current, parent_sales_quotations_id, 
                               customer_name, contact_name, contact_mobile_no
                        FROM sales_quotations WHERE id = @Id", new { Id = newId }, transaction);
                    var items = (await connection.QueryAsync<dynamic>(
                        @"SELECT 
                            sp.id,
                            sp.qty as quantity,
                            sp.bom_id as bomId,
                            sp.bom_child_item_ids,
                            sp.bom_accessory_item_ids,
                            bom.bom_name as bomName,
                            bom.bom_type as bomType
                        FROM sales_product sp
                        LEFT JOIN bill_of_materials bom ON sp.bom_id = bom.bom_id
                        WHERE sp.stage = 'Quotation' AND sp.stage_item_id = @QuotationId",
                        new { QuotationId = newId.ToString() }, transaction)).ToList();

                    var itemsResponse = new List<object>();
                    foreach (var item in items)
                    {
                        // Parse bom_child_item_ids and bom_accessory_item_ids from JSONB
                        int[] bomChildItemIds = null;
                        int[] bomAccessoryItemIds = null;
                        try 
                        { 
                            bomChildItemIds = item.bom_child_item_ids != null ? 
                                System.Text.Json.JsonSerializer.Deserialize<int[]>(item.bom_child_item_ids.ToString()) : 
                                Array.Empty<int>(); 
                        } 
                        catch 
                        { 
                            bomChildItemIds = Array.Empty<int>(); 
                        }
                        
                        try 
                        { 
                            bomAccessoryItemIds = item.bom_accessory_item_ids != null ? 
                                System.Text.Json.JsonSerializer.Deserialize<int[]>(item.bom_accessory_item_ids.ToString()) : 
                                Array.Empty<int>(); 
                        } 
                        catch 
                        { 
                            bomAccessoryItemIds = Array.Empty<int>(); 
                        }

                        // Fetch BOM child items with full details
                        var bomChildItemList = new List<dynamic>();
                        if (bomChildItemIds.Length > 0)
                        {
                            var childQuery = @"
                                SELECT 
                                    im.id,
                                    m.name as make,
                                    mo.name as model,
                                    p.name as product,
                                    im.item_name,
                                    im.item_code,
                                    im.unit_price,
                                    im.hsn,
                                    im.tax_percentage,
                                    c.name as category_name
                                FROM item_master im 
                                LEFT JOIN categories c ON im.category_id = c.id
                                LEFT JOIN make m ON im.make_id = m.id 
                                LEFT JOIN model mo ON im.model_id = mo.id 
                                LEFT JOIN product p ON im.product_id = p.id
                                WHERE im.id = ANY(@Ids)";
                            var rawChildItems = await connection.QueryAsync<dynamic>(childQuery, new { Ids = bomChildItemIds }, transaction);
                            bomChildItemList = rawChildItems.Select(ci => new
                            {
                                id = ci.id,
                                make = ci.make,
                                model = ci.model,
                                product = ci.product,
                                itemName = ci.item_name,
                                itemCode = ci.item_code,
                                unitPrice = ci.unit_price,
                                hsn = ci.hsn,
                                taxPercentage = ci.tax_percentage,
                                categoryName = ci.category_name
                            }).ToList<dynamic>();
                        }

                        // Fetch accessory items with full details
                        var accessoryItemList = new List<dynamic>();
                        if (bomAccessoryItemIds.Length > 0)
                        {
                            var accessoryQuery = @"
                                SELECT 
                                    im.id,
                                    m.name as make,
                                    mo.name as model,
                                    p.name as product,
                                    im.item_name,
                                    im.item_code,
                                    im.unit_price,
                                    im.hsn,
                                    im.tax_percentage,
                                    c.name as category_name
                                FROM item_master im 
                                LEFT JOIN categories c ON im.category_id = c.id
                                LEFT JOIN make m ON im.make_id = m.id 
                                LEFT JOIN model mo ON im.model_id = mo.id 
                                LEFT JOIN product p ON im.product_id = p.id
                                WHERE im.id = ANY(@Ids)";
                            var rawAccessoryItems = await connection.QueryAsync<dynamic>(accessoryQuery, new { Ids = bomAccessoryItemIds }, transaction);
                            accessoryItemList = rawAccessoryItems.Select(ai => new
                            {
                                id = ai.id,
                                make = ai.make,
                                model = ai.model,
                                product = ai.product,
                                itemName = ai.item_name,
                                itemCode = ai.item_code,
                                unitPrice = ai.unit_price,
                                hsn = ai.hsn,
                                taxPercentage = ai.tax_percentage,
                                categoryName = ai.category_name
                            }).ToList<dynamic>();
                        }

                        itemsResponse.Add(new
                        {
                            bomId = item.bomId,
                            bomName = item.bomName,
                            bomType = item.bomType,
                            bomChildItems = bomChildItemList,
                            accessoryItemIds = bomAccessoryItemIds,
                            accessoryItems = accessoryItemList,
                            quantity = item.quantity
                        });
                    }

                    // Fetch TermsAndConditions for this quotation
                    SalesTermsAndConditions termsAndConditions = null;
                    var terms = await connection.QueryFirstOrDefaultAsync<SalesTermsAndConditions>(
                        "SELECT * FROM sales_terms_and_conditions WHERE quotation_id = @QuotationId ORDER BY id DESC LIMIT 1",
                        new { QuotationId = newId }, transaction);
                    if (terms != null)
                    {
                        termsAndConditions = terms;
                    }

                    // Fetch address based on opportunityId -> leadId -> address
                    ERP.API.Models.DTOs.SalesAddressDto customerAddress = null;
                    string leadIdToUse = quotation.LeadId;
                    if (!string.IsNullOrEmpty(quotation.OpportunityId))
                    {
                        var opp = await connection.QueryFirstOrDefaultAsync<dynamic>(
                            "SELECT lead_id FROM sales_opportunities WHERE opportunity_id = @OpportunityId LIMIT 1",
                            new { OpportunityId = quotation.OpportunityId }, transaction);
                        if (opp != null && opp.lead_id != null && !string.IsNullOrEmpty((string)opp.lead_id))
                        {
                            leadIdToUse = (string)opp.lead_id;
                        }
                    }
                    if (!string.IsNullOrEmpty(leadIdToUse))
                    {
                        var addresses = await _salesAddressService.GetBySalesLeadIdAsync(int.TryParse(leadIdToUse, out var parsedLeadId) ? parsedLeadId : (int?)null);
                        var address = addresses?.FirstOrDefault(a => a.IsDefault == true) ?? addresses?.FirstOrDefault();
                        if (address != null)
                        {
                            customerAddress = new ERP.API.Models.DTOs.SalesAddressDto
                            {
                                Id = address.Id,
                                DoorNo = address.DoorNo,
                                Street = address.Street,
                                Area = address.Area,
                                Block = address.Block,
                                City = address.City,
                                State = address.State,
                                Pincode = address.Pincode,
                                Landmark = address.Landmark,
                                IsDefault = address.IsDefault,
                                Type = address.Type,
                                Department = address.Department,
                                OpportunityId = address.OpportunityId
                            };
                        }
                    }

                    await transaction.CommitAsync();
                    return Ok(new SalesQuotationWithItemsResponse
                    {
                        Quotation = quotation,
                        Items = itemsResponse.Cast<object>().ToList(),
                        CustomerName = quotation.CustomerName,
                        TermsAndConditions = termsAndConditions,
                        CustomerAddress = customerAddress,
                        ContactName = quotation.ContactName,
                        ContactMobileNo = quotation.ContactMobileNo
                    });
                }

                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error updating quotation with items: {Message}", ex.Message);
                    return StatusCode(500, new { message = "Failed to update quotation with items", error = ex.Message });
                }
            }

        }

    }