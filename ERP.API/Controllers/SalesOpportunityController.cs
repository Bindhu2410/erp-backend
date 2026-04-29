using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using ERP.API.Models;
using ERP.API.Models.DTOs;
using ERP.API.Services;
using ERP.API.Helpers;
using Dapper;
using System.IO;
using ClosedXML.Excel;
using System.Data;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Cors;
using System.Security.Claims;
// Removed conflicting local SalesOpportunityWithItemsRequest class
namespace ERP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowAll")]
    [Produces("application/json")]
    [ApiExplorerSettings(IgnoreApi = false)]
    public class SalesOpportunityController : ControllerBase
    {
        private readonly ISalesOpportunityService _opportunityService;
        private readonly ILogger<SalesOpportunityController> _logger;
        private readonly SalesLeadService _salesLeadService;

        public SalesOpportunityController(
            ISalesOpportunityService opportunityService,
            ILogger<SalesOpportunityController> logger,
            SalesLeadService salesLeadService)
        {
            _opportunityService = opportunityService;
            _logger = logger;
            _salesLeadService = salesLeadService;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        /// <summary>
        /// Gets all active sales opportunities
        /// </summary>
        /// <returns>List of all active sales opportunities</returns>
        /// <response code="200">Returns the list of opportunities</response>
        /// <response code="401">If the user is not authorized</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<SalesOpportunity>>> GetOpportunities()
        {
            try
            {
                var opportunities = await _opportunityService.GetOpportunitiesAsync();
                return Ok(opportunities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting opportunities: {Message}", ex.Message);
                return StatusCode(500, $"An error occurred while retrieving opportunities: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets a specific sales opportunity by its opportunity ID
        /// </summary>
        /// <param name="opportunityId">The opportunity ID to retrieve (e.g., OPP00001)</param>
        /// <returns>The requested sales opportunity</returns>
        /// <response code="200">Returns the requested opportunity</response>
        /// <response code="400">If the opportunity ID format is invalid</response>
        /// <response code="404">If the opportunity is not found</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpGet("{opportunityId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<SalesOpportunity>> GetOpportunity(string opportunityId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(opportunityId))
                {
                    _logger.LogWarning("Invalid opportunity ID: {OpportunityId}", opportunityId);
                    return BadRequest("Opportunity ID must be provided");
                }
                var dto = await _opportunityService.GetOpportunityByIdAsync(opportunityId);
                if (dto == null)
                {
                    _logger.LogInformation("Opportunity not found: {OpportunityId}", opportunityId);
                    return NotFound($"Opportunity with ID {opportunityId} not found");
                }
                var model = new SalesOpportunity
                {
                    Id = dto.Id,
                    UserCreated = dto.UserCreated,
                    DateCreated = dto.DateCreated,
                    UserUpdated = dto.UserUpdated,
                    DateUpdated = dto.DateUpdated,
                    Status = dto.Status,
                    ExpectedCompletion = dto.ExpectedCompletion,
                    OpportunityType = dto.OpportunityType,
                    OpportunityFor = dto.OpportunityFor,
                    CustomerId = dto.CustomerId,
                    CustomerName = dto.CustomerName,
                    CustomerType = dto.CustomerType,
                    OpportunityName = dto.OpportunityName,
                    OpportunityId = dto.OpportunityId,
                    Comments = dto.Comments,
                    IsActive = dto.IsActive,
                    LeadId = dto.LeadId,
                    SalesRepresentativeId = dto.SalesRepresentativeId,
                    ContactName = dto.ContactName,
                    ContactMobileNo = dto.ContactMobileNo
                };
                return Ok(new { opportunity = model });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving opportunity {OpportunityId}: {Message}", opportunityId, ex.Message);
                return StatusCode(500, new {
                    message = "Failed to retrieve opportunity",
                    error = ex.Message,
                    statusCode = 500
                });
            }
        }

        /// <summary>
        /// Gets a specific sales opportunity by its numeric database ID
        /// </summary>
        /// <param name="id">The numeric database ID of the opportunity</param>
        /// <returns>The requested sales opportunity</returns>
        /// <response code="200">Returns the requested opportunity</response>
        /// <response code="404">If the opportunity is not found</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpGet("by-id/{opportunityId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<SalesOpportunity>> GetOpportunityById(string opportunityId)
        {
            try
            {
                var opportunity = await _opportunityService.GetByIdAsync(opportunityId);
                if (opportunity == null)
                {
                    _logger.LogInformation("Opportunity not found by ID: {OpportunityId}", opportunityId);
                    return NotFound($"Opportunity with ID {opportunityId} not found");
                }
                _logger.LogInformation("Retrieved opportunity by ID: {OpportunityId}", opportunityId);
                // Map to PascalCase object for response
                var response = new {
                    Id = opportunity.Id,
                    UserCreated = opportunity.UserCreated,
                    DateCreated = opportunity.DateCreated,
                    UserUpdated = opportunity.UserUpdated,
                    DateUpdated = opportunity.DateUpdated,
                    Status = opportunity.Status,
                    ExpectedCompletion = opportunity.ExpectedCompletion,
                    OpportunityType = opportunity.OpportunityType,
                    OpportunityFor = opportunity.OpportunityFor,
                    CustomerId = opportunity.CustomerId,
                    CustomerName = opportunity.CustomerName,
                    CustomerType = opportunity.CustomerType,
                    OpportunityName = opportunity.OpportunityName,
                    OpportunityId = opportunity.OpportunityId,
                    Comments = opportunity.Comments,
                    IsActive = opportunity.IsActive,
                    LeadId = opportunity.LeadId,
                    SalesRepresentativeId = opportunity.SalesRepresentativeId,
                    ContactName = opportunity.ContactName,
                    ContactMobileNo = opportunity.ContactMobileNo
                };
                return Ok(new { opportunity = response });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving opportunity by ID {OpportunityId}: {Message}", opportunityId, ex.Message);
                return StatusCode(500, new
                {
                    message = "Failed to retrieve opportunity by ID",
                    error = ex.Message,
                    statusCode = 500
                });
            }
        }

        /// <summary>
        /// Gets all opportunities for a specific sales lead
        /// </summary>
        /// <param name="leadId">The ID of the sales lead</param>
        /// <returns>List of opportunities associated with the sales lead</returns>
        /// <response code="200">Returns the list of opportunities</response>
        /// <response code="400">If the lead ID is invalid</response>
        /// <response code="404">If no opportunities are found for the sales lead</response>
        /// <response code="401">If the user is not authorized</response>
        /// <response code="500">If there was an internal server error</response>


    // DTO for lead ID request body
    public class LeadIdRequest
    {
        public string? LeadId { get; set; }
    }

   

    /// <summary>
    /// Gets all opportunities for a specific sales lead, with items, contacts, and lead address (like /with-items)
    /// </summary>
    /// <param name="request">The lead ID request</param>
    /// <returns>List of opportunities with items, contacts, and lead address</returns>
    /// <response code="200">Returns the list of opportunities with items</response>
    /// <response code="400">If the lead ID is invalid</response>
    /// <response code="404">If no opportunities are found for the sales lead</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost("lead/by-leadid")]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetOpportunitiesByLeadIdWithItems([FromBody] LeadIdRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.LeadId))
            return BadRequest(new { message = "LeadId is required" });

        try
        {
            var connectionString = (_opportunityService as SalesOpportunityService)?.GetConnectionString();
            if (string.IsNullOrEmpty(connectionString))
                return StatusCode(500, "Could not resolve connection string.");

            using var connection = new Npgsql.NpgsqlConnection(connectionString);
            await connection.OpenAsync();

            // Get the first opportunity for the given LeadId
            var opportunityRaw = await connection.QueryFirstOrDefaultAsync(@"SELECT * FROM sales_opportunities WHERE lead_id = @LeadId LIMIT 1", new { LeadId = request.LeadId });
            if (opportunityRaw == null)
                return NotFound(new { message = $"No opportunities found for LeadId {request.LeadId}" });

            // Fetch the numeric sales_lead.id for the given lead_id
            var leadNumericId = await connection.QueryFirstOrDefaultAsync<int?>(
                "SELECT id FROM sales_lead WHERE lead_id = @LeadId LIMIT 1", new { LeadId = request.LeadId });

            string? contactName = null;
            string? contactMobileNo = null;
            if (leadNumericId.HasValue)
            {
                // Try to fetch contactName and contactMobileNo from sales_lead table first
                var leadContact = await connection.QueryFirstOrDefaultAsync(
                    "SELECT contact_name, contact_mobile_no FROM sales_lead WHERE id = @Id LIMIT 1",
                    new { Id = leadNumericId.Value });
                if (leadContact != null && !string.IsNullOrWhiteSpace(leadContact.contact_name) && !string.IsNullOrWhiteSpace(leadContact.contact_mobile_no))
                {
                    contactName = leadContact.contact_name;
                    contactMobileNo = leadContact.contact_mobile_no;
                }
                else
                {
                    // Fallback to first contact in sales_contacts if not present in sales_lead
                    var contact = await connection.QueryFirstOrDefaultAsync(
                        "SELECT contact_name, mobile_no FROM sales_contacts WHERE sales_lead_id = @SalesLeadId LIMIT 1",
                        new { SalesLeadId = leadNumericId.Value });
                    if (contact != null)
                    {
                        contactName = contact.contact_name;
                        contactMobileNo = contact.mobile_no;
                    }
                }
            }

            // If the opportunity table does not have the correct contact info, update it
            bool needsUpdate = false;
            if (!string.IsNullOrWhiteSpace(contactName) && (!opportunityRaw.contact_name?.Equals(contactName) ?? true))
                needsUpdate = true;
            if (!string.IsNullOrWhiteSpace(contactMobileNo) && (!opportunityRaw.contact_mobile_no?.Equals(contactMobileNo) ?? true))
                needsUpdate = true;

            if (needsUpdate)
            {
                await connection.ExecuteAsync(
                    "UPDATE sales_opportunities SET contact_name = @ContactName, contact_mobile_no = @ContactMobileNo WHERE id = @Id",
                    new { ContactName = contactName, ContactMobileNo = contactMobileNo, Id = opportunityRaw.id });
                // Re-fetch the updated row
                opportunityRaw = await connection.QueryFirstOrDefaultAsync(@"SELECT * FROM sales_opportunities WHERE id = @Id", new { Id = opportunityRaw.id });
            }

            var opportunity = new {
                Id = opportunityRaw.id,
                UserCreated = opportunityRaw.user_created,
                DateCreated = opportunityRaw.date_created,
                UserUpdated = opportunityRaw.user_updated,
                DateUpdated = (opportunityRaw.date_updated == null || opportunityRaw.date_updated is System.DBNull) ? opportunityRaw.date_created : opportunityRaw.date_updated,
                Status = opportunityRaw.status,
                ExpectedCompletion = opportunityRaw.expected_completion,
                OpportunityType = opportunityRaw.opportunity_type,
                OpportunityFor = opportunityRaw.opportunity_for,
                CustomerId = opportunityRaw.customer_id,
                CustomerName = opportunityRaw.customer_name,
                CustomerType = opportunityRaw.customer_type,
                OpportunityName = opportunityRaw.opportunity_name,
                OpportunityId = opportunityRaw.opportunity_id,
                Comments = opportunityRaw.comments,
                IsActive = opportunityRaw.isactive,
                LeadId = opportunityRaw.lead_id,
                SalesRepresentativeId = opportunityRaw.sales_representative_id,
                ContactName = opportunityRaw.contact_name,
                ContactMobileNo = opportunityRaw.contact_mobile_no
            };

            return Ok(new { opportunity });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting opportunities by lead id: {Message}", ex.Message);
            return StatusCode(500, new { message = "An error occurred while retrieving opportunities by lead id", statusCode = 500, errors = new[] { ex.Message } });
        }
    }

        // POST: api/SalesOpportunity
        [HttpPost]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        private string? CleanString(string? value)
        {
            if (string.IsNullOrWhiteSpace(value) || value == "string")
                return null;
            return value;
        }

        [HttpPost]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<object>> CreateOpportunity([FromBody] SalesOpportunity opportunity)
        {
            try
            {
                // Always ignore client-supplied OpportunityId and let backend generate it
                opportunity.OpportunityId = null;

                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        message = "Invalid model state",
                        statusCode = 400,
                        errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                    });
                }

                // Clean string values before saving
                opportunity.OpportunityType = CleanString(opportunity.OpportunityType) ?? opportunity.OpportunityType;
                opportunity.OpportunityFor = CleanString(opportunity.OpportunityFor) ?? opportunity.OpportunityFor;
                opportunity.CustomerId = CleanString(opportunity.CustomerId);
                opportunity.CustomerType = CleanString(opportunity.CustomerType);
                opportunity.OpportunityName = CleanString(opportunity.OpportunityName) ?? opportunity.OpportunityName;
                opportunity.Comments = CleanString(opportunity.Comments);
                opportunity.LeadId = CleanString(opportunity.LeadId);
                opportunity.ContactName = CleanString(opportunity.ContactName);
                opportunity.ContactMobileNo = CleanString(opportunity.ContactMobileNo);

                // Business rule: If LeadId is provided, set Opportunity status to 'Identified' and always update Lead status to 'Converted'
                if (!string.IsNullOrEmpty(opportunity.LeadId))
                {
                    opportunity.Status = "Identified";
                    // Always update the related lead's status to 'Converted'
                    var lead = await _salesLeadService.GetByLeadIdAsync(opportunity.LeadId);
                    if (lead != null)
                    {
                        lead.Status = "Converted";
                        lead.UserUpdated = 1; // Set to a valid user id or get from context
                        await _salesLeadService.UpdateAsync(lead);
                    }
                }

                var dto = new SalesOpportunityDto
                {
                    Id = opportunity.Id ?? 0,
                    UserCreated = opportunity.UserCreated,
                    DateCreated = opportunity.DateCreated,
                    UserUpdated = opportunity.UserUpdated,
                    DateUpdated = opportunity.DateUpdated,
                    Status = opportunity.Status,
                    ExpectedCompletion = opportunity.ExpectedCompletion,
                    OpportunityType = opportunity.OpportunityType,
                    OpportunityFor = opportunity.OpportunityFor,
                    CustomerId = opportunity.CustomerId,
                    CustomerName = opportunity.CustomerName,
                    CustomerType = opportunity.CustomerType,
                    OpportunityName = opportunity.OpportunityName,
                    OpportunityId = opportunity.OpportunityId,
                    Comments = opportunity.Comments,
                    IsActive = opportunity.IsActive,
                    LeadId = opportunity.LeadId,
                    SalesRepresentativeId = opportunity.SalesRepresentativeId,
                    ContactName = opportunity.ContactName,
                    ContactMobileNo = opportunity.ContactMobileNo
                };
                var id = await _opportunityService.CreateOpportunityAsync(dto);
                _logger.LogInformation("Created new opportunity with ID {Id} and OpportunityId {OpportunityId}", id, dto.OpportunityId);
                return Created($"api/SalesOpportunity/{dto.OpportunityId}", new { id, opportunityId = dto.OpportunityId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating opportunity: {Message}", ex.Message);
                if (ex is ArgumentException)
                {
                    return BadRequest(new
                    {
                        message = ex.Message,
                        statusCode = 400
                    });
                }
                return StatusCode(500, new
                {
                    message = "Failed to create opportunity",
                    error = ex.Message,
                    statusCode = 500
                });
            }
        }

        // PUT: api/SalesOpportunity/5
        [HttpPut("{opportunityId}")]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<SalesOpportunity>> UpdateOpportunity([FromRoute] string opportunityId, [FromBody] SalesOpportunity opportunity)
        {
            if (opportunityId != opportunity.OpportunityId)
            {
                return BadRequest(new
                {
                    message = "Opportunity ID mismatch",
                    statusCode = 400
                });
            }
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        message = "Invalid model state",
                        statusCode = 400,
                        errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                    });
                }

                // Clean string values before updating
                opportunity.OpportunityType = CleanString(opportunity.OpportunityType) ?? opportunity.OpportunityType;
                opportunity.OpportunityFor = CleanString(opportunity.OpportunityFor) ?? opportunity.OpportunityFor;
                opportunity.CustomerId = CleanString(opportunity.CustomerId);
                opportunity.CustomerType = CleanString(opportunity.CustomerType);
                opportunity.OpportunityName = CleanString(opportunity.OpportunityName) ?? opportunity.OpportunityName;
                opportunity.Comments = CleanString(opportunity.Comments);
                opportunity.LeadId = CleanString(opportunity.LeadId);
                opportunity.ContactName = CleanString(opportunity.ContactName);
                opportunity.ContactMobileNo = CleanString(opportunity.ContactMobileNo);

                // Business rule: If LeadId is provided, set Opportunity status to 'Identified' and update Lead status to 'Converted' only if lead is Qualified
                if (!string.IsNullOrEmpty(opportunity.LeadId))
                {
                    opportunity.Status = "Identified";
                    // Update the related lead's status to 'Converted' only if currently Qualified
                    var lead = await _salesLeadService.GetByLeadIdAsync(opportunity.LeadId);
                    if (lead != null)
                    {
                        if (lead.Status != null && lead.Status.Equals("Qualified", StringComparison.OrdinalIgnoreCase))
                        {
                            lead.Status = "Converted";
                            await _salesLeadService.UpdateAsync(lead);
                        }
                        else
                        {
                            return BadRequest(new
                            {
                                message = $"Lead status must be 'Qualified' to convert. Current status: {lead.Status}",
                                statusCode = 400
                            });
                        }
                    }
                }

                var dto = new SalesOpportunityDto
                {
                    Id = opportunity.Id ?? 0,
                    UserCreated = opportunity.UserCreated,
                    DateCreated = opportunity.DateCreated,
                    UserUpdated = opportunity.UserUpdated,
                    DateUpdated = opportunity.DateUpdated,
                    Status = opportunity.Status,
                    ExpectedCompletion = opportunity.ExpectedCompletion,
                    OpportunityType = opportunity.OpportunityType,
                    OpportunityFor = opportunity.OpportunityFor,
                    CustomerId = opportunity.CustomerId,
                    CustomerName = opportunity.CustomerName,
                    CustomerType = opportunity.CustomerType,
                    OpportunityName = opportunity.OpportunityName,
                    OpportunityId = opportunity.OpportunityId,
                    Comments = opportunity.Comments,
                    IsActive = opportunity.IsActive,
                    LeadId = opportunity.LeadId,
                    SalesRepresentativeId = opportunity.SalesRepresentativeId,
                    ContactName = opportunity.ContactName,
                    ContactMobileNo = opportunity.ContactMobileNo
                };
                var success = await _opportunityService.UpdateOpportunityAsync(opportunityId, dto);

                if (!success)
                {
                    _logger.LogWarning("Opportunity with ID {OpportunityId} not found for update", opportunityId);
                    return NotFound(new
                    {
                        message = $"Opportunity with ID {opportunityId} not found",
                        statusCode = 404
                    });
                }

                _logger.LogInformation("Updated opportunity with ID {OpportunityId}", opportunityId);
                // Get the updated opportunity and return it
                var updatedDto = await _opportunityService.GetOpportunityByIdAsync(opportunity.OpportunityId);
                if (updatedDto == null)
                    return NotFound();
                var updatedModel = new SalesOpportunity
                {
                    Id = updatedDto.Id,
                    UserCreated = updatedDto.UserCreated,
                    DateCreated = updatedDto.DateCreated,
                    UserUpdated = updatedDto.UserUpdated,
                    DateUpdated = updatedDto.DateUpdated,
                    Status = updatedDto.Status,
                    ExpectedCompletion = updatedDto.ExpectedCompletion,
                    OpportunityType = updatedDto.OpportunityType,
                    OpportunityFor = updatedDto.OpportunityFor,
                    CustomerId = updatedDto.CustomerId,
                    CustomerName = updatedDto.CustomerName,
                    CustomerType = updatedDto.CustomerType,
                    OpportunityName = updatedDto.OpportunityName,
                    OpportunityId = updatedDto.OpportunityId,
                    Comments = updatedDto.Comments,
                    IsActive = updatedDto.IsActive,
                    LeadId = updatedDto.LeadId,
                    SalesRepresentativeId = updatedDto.SalesRepresentativeId,
                    ContactName = updatedDto.ContactName,
                    ContactMobileNo = updatedDto.ContactMobileNo
                };
                return Ok(updatedModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating opportunity {OpportunityId}: {Message}", opportunityId, ex.Message);
                return StatusCode(500, new
                {
                    message = "Failed to update opportunity",
                    error = ex.Message,
                    statusCode = 500
                });
            }
        }

        /// <summary>
        /// Deletes a specific sales opportunity
        /// </summary>
                /// <param name="opportunityId">The ID of the opportunity to delete (e.g., OPP00001)</param>
                /// <returns>No content on success</returns>
                /// <response code="204">If the opportunity was successfully deleted</response>
                /// <response code="400">If the opportunityId format is invalid</response>
                /// <response code="404">If the opportunity is not found</response>
                /// <response code="500">If there was an internal server error</response>
        [HttpDelete("{opportunityId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteOpportunity(string opportunityId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(opportunityId))
                    return BadRequest("Opportunity ID must be provided");
                // Find the numeric ID for the given opportunityId
                var dto = await _opportunityService.GetOpportunityByIdAsync(opportunityId);
                if (dto == null)
                    return NotFound($"Opportunity with ID {opportunityId} not found");
                var success = await _opportunityService.DeleteOpportunityAsync(dto.Id);
                if (!success)
                {
                    _logger.LogWarning("Opportunity with ID {OpportunityId} not found for deletion", opportunityId);
                    return NotFound($"Opportunity with ID {opportunityId} not found");
                }
                _logger.LogInformation("Soft deleted opportunity with ID {OpportunityId}", opportunityId);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting opportunity {OpportunityId}: {Message}", opportunityId, ex.Message);
                return StatusCode(500, $"Failed to delete opportunity {opportunityId}: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets a paginated list of opportunities with filtering and sorting
        /// </summary>
        /// <param name="request">The grid request parameters</param>
        /// <returns>Paginated list of opportunities with total count</returns>
        [HttpPost("grid")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<object>> GetOpportunitiesGrid([FromBody] SalesOpportunityGridRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        message = "Invalid request parameters",
                        statusCode = 400,
                        errors = ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                            .ToList()
                    });
                }

                // Always use CurrentUserId for grid filtering
                // Allow direct userId for testing or integration
                var userId = request.UserCreated ?? GetCurrentUserId();
                if (userId == 0)
                {
                    return Unauthorized(new
                    {
                        message = "User is not authenticated or user ID is missing.",
                        statusCode = 401
                    });
                }

                // If UserCreated is provided, call the SP directly for compatibility
                var requestObj = new {
                    SearchText = request.SearchText,
                    CustomerNames = request.CustomerNames,
                    Statuses = request.Statuses,
                    OpportunityTypes = request.OpportunityTypes,
                    LeadIds = request.LeadIds,
                    PageNumber = request.PageNumber ?? 1,
                    PageSize = request.PageSize ?? 10,
                    OrderBy = request.OrderBy,
                    OrderDirection = request.OrderDirection,
                    UserCreated = userId
                };

                var jsonRequest = Newtonsoft.Json.JsonConvert.SerializeObject(requestObj);
                // Call the fn_get_sales_opportunities_grid_by_user SP
                var result = await _opportunityService.GetOpportunitiesGridByUserSPAsync(jsonRequest);

                var connectionString = (_opportunityService as SalesOpportunityService)?.GetConnectionString();
                using var connection = new Npgsql.NpgsqlConnection(connectionString);

                var gridWithItems = new List<object>();
                foreach (var opp in result.Results)
                {
                    var opportunity = new {
                        Id = opp.Id,
                        UserCreated = opp.UserCreated,
                        DateCreated = opp.DateCreated,
                        UserUpdated = opp.UserUpdated,
                        DateUpdated = opp.DateUpdated,
                        Status = opp.Status,
                        ExpectedCompletion = opp.ExpectedCompletion,
                        OpportunityType = opp.OpportunityType,
                        OpportunityFor = opp.OpportunityFor,
                        CustomerId = opp.CustomerId,
                        CustomerName = opp.CustomerName,
                        CustomerType = opp.CustomerType,
                        OpportunityName = opp.OpportunityName,
                        OpportunityId = opp.OpportunityId,
                        Comments = opp.Comments,
                        IsActive = opp.IsActive,
                        LeadId = opp.LeadId,
                        SalesRepresentativeId = opp.SalesRepresentativeId,
                        ContactName = opp.ContactName,
                        ContactMobileNo = opp.ContactMobileNo
                    };

                    // Fetch related sales items with all fields from both tables, using strongly-typed SalesItemResponse
                    var sql = @"SELECT 
                        si.id, si.user_created, si.date_created, si.user_updated, si.date_updated, si.qty, si.amount, si.is_active, si.item_id, si.stage, si.unit_price, si.stage_item_id, si.bom_id,
                        m.name as make, mo.name as model, p.name as product, im.item_name as ItemName, im.item_code as ItemCode, im.unit_price as UnitPrice, im.hsn, im.tax_percentage as TaxPercentage,
                        c.name as CategoryName
                    FROM public.sales_product si
                    LEFT JOIN item_master im ON si.item_id = im.id
                    LEFT JOIN categories c ON im.category_id = c.id
                    LEFT JOIN make m ON im.make_id = m.id
                    LEFT JOIN model mo ON im.model_id = mo.id
                    LEFT JOIN product p ON im.product_id = p.id
                    WHERE si.stage = 'Opportunity' AND si.stage_item_id = @OpportunityId;";
                    var itemsRaw = (await connection.QueryAsync<ERP.API.Models.SalesItemResponse>(sql, new { OpportunityId = opp.OpportunityId })).ToList();

                    foreach (var item in itemsRaw)
                    {
                        // Fetch child items from sales_product_child_items for explicit child entries
                        item.IncludedChildItems = (await connection.QueryAsync<ERP.API.Models.SalesItemResponse>(
                            "SELECT im.id, m.name as make, mo.name as model, p.name as product, im.item_name as ItemName, im.item_code as ItemCode, im.unit_price as UnitPrice, im.hsn, im.tax_percentage as TaxPercentage, c.name as CategoryName FROM sales_product_child_items spci JOIN item_master im ON spci.child_item_id = im.id LEFT JOIN categories c ON im.category_id = c.id LEFT JOIN make m ON im.make_id = m.id LEFT JOIN model mo ON im.model_id = mo.id LEFT JOIN product p ON im.product_id = p.id WHERE spci.sales_product_id = @SalesProductId",
                            new { SalesProductId = item.Id })).ToList();
                        
                        // If there are no explicit child entries and item has BOM ID, fetch BOM child items
                        if ((item.IncludedChildItems == null || item.IncludedChildItems.Count == 0) && !string.IsNullOrEmpty(item.BomId))
                        {
                            // Fetch BOM child items with full details including rates
                            var bomChildItemsQuery = @"
                                SELECT bci.child_item_id AS ItemId, bci.quantity AS Qty, 
                                       im.item_name AS ItemName, im.item_code AS ItemCode, im.cat_no AS CatNo, 
                                       im.unit_price AS UnitPrice, im.hsn, im.tax_percentage AS TaxPercentage, 
                                       c.name AS CategoryName, u.code AS UomName, 
                                       m.name AS Make, mo.name AS Model, p.name AS Product, 
                                       vm.name AS ValuationMethodName, ime.name AS InventoryMethodName, itt.name AS InventoryTypeName,
                                       rmi.purchase_rate AS PurchaseRate, rmi.sales_rate AS SaleRate, rmi.quotation_rate AS QuoteRate
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
                                WHERE bci.bill_of_material_id = (SELECT id FROM bill_of_materials WHERE bom_id = @BomId)";
                            
                            var bomChildItems = await connection.QueryAsync<dynamic>(bomChildItemsQuery, new { BomId = item.BomId });
                            
                            // Convert to BomChildItemResponse format
                            item.ChildItems = bomChildItems.Select(ci => new ERP.API.Models.BomChildItemResponse
                            {
                                ChildItemId = ci.ItemId ?? 0,
                                Quantity = ci.Qty ?? 0,
                                Make = ci.Make,
                                Model = ci.Model,
                                Product = ci.Product,
                                CategoryName = ci.CategoryName,
                                ValuationMethodName = ci.ValuationMethodName,
                                InventoryMethodName = ci.InventoryMethodName,
                                InventoryTypeName = ci.InventoryTypeName,
                                UnitPrice = ci.UnitPrice,
                                ItemName = ci.ItemName,
                                ItemCode = ci.ItemCode,
                                CatNo = ci.CatNo,
                                UomName = ci.UomName,
                                PurchaseRate = ci.PurchaseRate,
                                SaleRate = ci.SaleRate,
                                QuoteRate = ci.QuoteRate,
                                Hsn = ci.hsn,
                                Tax = ci.TaxPercentage
                            }).ToList();
                            
                            // Also populate IncludedChildItems for backward compatibility
                            item.IncludedChildItems = bomChildItems.Select(ci => new ERP.API.Models.SalesItemResponse
                            {
                                Id = ci.ItemId ?? 0,
                                ItemName = ci.ItemName,
                                ItemCode = ci.ItemCode,
                                Make = ci.Make,
                                Model = ci.Model,
                                Product = ci.Product,
                                CategoryName = ci.CategoryName,
                                UnitPrice = ci.UnitPrice,
                                Hsn = ci.hsn,
                                TaxPercentage = ci.TaxPercentage,
                                Qty = ci.Qty ?? 0
                            }).ToList();
                        }
                        
                        item.AccessoriesItems = (await connection.QueryAsync<ERP.API.Models.SalesItemResponse>(
                            "SELECT im.id, m.name as make, mo.name as model, p.name as product, im.item_name as ItemName, im.item_code as ItemCode, im.unit_price as UnitPrice, im.hsn, im.tax_percentage as TaxPercentage, c.name as CategoryName FROM sales_product_accessories spa JOIN item_master im ON spa.accessories_item_id = im.id LEFT JOIN categories c ON im.category_id = c.id LEFT JOIN make m ON im.make_id = m.id LEFT JOIN model mo ON im.model_id = mo.id LEFT JOIN product p ON im.product_id = p.id WHERE spa.sales_product_id = @SalesProductId",
                            new { SalesProductId = item.Id })).ToList();
                    }

                    gridWithItems.Add(new { opportunity, items = itemsRaw });
                }

                return Ok(new
                {
                    Results = gridWithItems,
                    result.TotalRecords,
                    PageNumber = request.PageNumber ?? 1,
                    PageSize = request.PageSize ?? 10
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting opportunities grid: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    message = "An error occurred while retrieving opportunities",
                    statusCode = 500,
                    errors = new[] { ex.Message }
                });
            }
        }

        /// <summary>
        /// Gets opportunity counts status for Identified, Solution Presentation, Proposal, Negotiation, Closed Won
        /// </summary>
        /// <param name="userId">Optional user ID to filter opportunities by user_created. If not provided, returns data for authenticated user only.</param>
        /// <returns>LeadCardsDto with opportunity counts by status</returns>
        /// <response code="200">Returns the opportunity counts by status</response>
        /// <response code="400">If user ID is invalid</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpGet("cards-status")]
        public async Task<ActionResult<OpportunityCardsDto>> GetOpportunityCardsStatus([FromQuery] int? userId = null)
        {
            try
            {
                // Get current user ID for filtering
                var currentUserId = GetCurrentUserId();
                
                // Determine which user ID to use for filtering
                var targetUserId = userId ?? currentUserId;
                
                // Validate that we have a valid user ID for filtering
                if (targetUserId == 0)
                {
                    return BadRequest(new
                    {
                        message = "User ID is required. Please provide userId parameter or ensure proper authentication.",
                        statusCode = 400
                    });
                }

                var userResult = await _opportunityService.GetOpportunityCardsStatusByUserAsync(targetUserId);
                return Ok(userResult);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "An error occurred while retrieving opportunity cards",
                    statusCode = 500,
                    errors = new[] { ex.Message }
                });
            }
        }

        /// <summary>
        /// Creates a new sales opportunity along with its items in a single request.
        /// </summary>
        /// <param name="request">The sales opportunity and its items</param>
        /// <returns>Result of creation</returns>
        /// <response code="201">Created successfully</response>
        /// <response code="400">Invalid input</response>
        /// <response code="500">Internal server error</response>
   [HttpPost("with-items")]
[Consumes("application/json")]
[Produces("application/json")]
[ProducesResponseType(StatusCodes.Status201Created)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status500InternalServerError)]
public async Task<IActionResult> CreateOpportunityWithItems([FromBody] SalesOpportunityWithItemsRequest request)
{
    // New logic: Accepts Items array. Auto-fills Stage and StageItemId.
    if (request == null || request.Items == null || request.Items.Count == 0)
        return BadRequest("Items are required");

    var firstItem = request.Items[0];
    if (string.IsNullOrWhiteSpace(firstItem.BomId) || firstItem.Quantity <= 0)
        return BadRequest("BomId and Quantity are required");

    var connectionString = (_opportunityService as SalesOpportunityService)?.GetConnectionString();
    if (string.IsNullOrEmpty(connectionString))
        return StatusCode(500, "Could not resolve connection string.");

    using var connection = new Npgsql.NpgsqlConnection(connectionString);
    await connection.OpenAsync();
    using var transaction = await connection.BeginTransactionAsync();

    try
    {
        // Generate OpportunityId (StageItemId)
        var generatedOpportunityId = await ERP.API.Helpers.IdGenerator.GenerateOpportunityId(connection);

        // Defensive cleanup of existing items
        var salesProductIdsToDelete = await connection.QueryAsync<int>(
            "SELECT id FROM sales_product WHERE stage = 'Opportunity' AND stage_item_id = @StageItemId;",
            new { StageItemId = generatedOpportunityId }, transaction);

        if (salesProductIdsToDelete.Any())
        {
            // First delete accessories referencing these products
            await connection.ExecuteAsync(
                "DELETE FROM sales_product_accessories WHERE sales_product_id = ANY(@Ids);",
                new { Ids = salesProductIdsToDelete.ToArray() }, transaction);

            await connection.ExecuteAsync(
                "DELETE FROM sales_product_child_items WHERE sales_product_id = ANY(@Ids);",
                new { Ids = salesProductIdsToDelete.ToArray() }, transaction);

            await connection.ExecuteAsync(
                "DELETE FROM sales_product WHERE id = ANY(@Ids);",
                new { Ids = salesProductIdsToDelete.ToArray() }, transaction);
        }

        // Fetch BOM details
        var mainItem = request.Items[0];
        var bomDetails = await connection.QueryFirstOrDefaultAsync<dynamic>(
            @"SELECT id, bom_id, bom_name 
              FROM bill_of_materials 
              WHERE bom_id = @BomId LIMIT 1",
            new { BomId = mainItem.BomId }, transaction);

        if (bomDetails == null)
        {
            await transaction.RollbackAsync();
            return BadRequest("Invalid BomId or BOM not found");
        }

        // Insert Opportunity with all fields from request.Opportunity
        var oppDto = request.Opportunity;
        var opportunity = new SalesOpportunity
        {
            OpportunityId = generatedOpportunityId,
            Status = !string.IsNullOrEmpty(oppDto.Status) ? oppDto.Status : "Opportunity",
            OpportunityName = !string.IsNullOrEmpty(oppDto.OpportunityName) ? oppDto.OpportunityName : bomDetails.bom_name,
            CustomerId = oppDto.CustomerId,
            CustomerName = oppDto.CustomerName,
            CustomerType = oppDto.CustomerType,
            OpportunityType = oppDto.OpportunityType,
            OpportunityFor = oppDto.OpportunityFor,
            Comments = oppDto.Comments,
            IsActive = true,
            DateCreated = DateTime.UtcNow,
            ExpectedCompletion = oppDto.ExpectedCompletion,
            LeadId = oppDto.LeadId,
            SalesRepresentativeId = oppDto.SalesRepresentativeId,
            ContactName = oppDto.ContactName,
            ContactMobileNo = oppDto.ContactMobileNo,
            UserCreated = oppDto.UserCreated,
            UserUpdated = oppDto.UserUpdated,
            DateUpdated = oppDto.DateUpdated
        };

        const string sqlOpp = @"
            INSERT INTO sales_opportunities (
                status, opportunity_name, opportunity_id, isactive, date_created, customer_id, customer_name, customer_type, opportunity_type, opportunity_for, comments, expected_completion, lead_id, sales_representative_id, contact_name, contact_mobile_no, user_created, user_updated, date_updated)
            VALUES (
                @Status, @OpportunityName, @OpportunityId, @IsActive, @DateCreated, @CustomerId, @CustomerName, @CustomerType, @OpportunityType, @OpportunityFor, @Comments, @ExpectedCompletion, @LeadId, @SalesRepresentativeId, @ContactName, @ContactMobileNo, @UserCreated, @UserUpdated, @DateUpdated)
            RETURNING id";

        var opportunityDbId = await connection.ExecuteScalarAsync<int>(sqlOpp, opportunity, transaction);

        // Insert each item into sales_product
        foreach (var item in request.Items)
        {
            var bomAccessoryItemIdsJson = (item.AccessoryItemIds != null && item.AccessoryItemIds.Count > 0)
                ? Newtonsoft.Json.JsonConvert.SerializeObject(item.AccessoryItemIds)
                : null;

            var bomChildItems = await connection.QueryAsync<dynamic>(
                @"SELECT child_item_id 
                  FROM bill_of_material_child_items 
                  WHERE bill_of_material_id = (SELECT id FROM bill_of_materials WHERE bom_id = @BomId)",
                new { BomId = item.BomId }, transaction);

            var bomChildItemIds = bomChildItems.Select(ci => (int)ci.child_item_id).ToList();
            var bomChildItemIdsJson = bomChildItemIds.Count > 0
                ? Newtonsoft.Json.JsonConvert.SerializeObject(bomChildItemIds)
                : null;

            const string sql = @"
                INSERT INTO sales_product 
                (bom_id, qty, is_active, date_created, stage, stage_item_id, bom_child_item_ids, bom_accessory_item_ids) 
                VALUES 
                (@BomId, @Quantity, @IsActive, @DateCreated, @Stage, @StageItemId, @BomChildItemIds::jsonb, @BomAccessoryItemIds::jsonb) 
                RETURNING id";

            var salesProductId = await connection.ExecuteScalarAsync<int>(sql, new
            {
                BomId = item.BomId,
                Quantity = item.Quantity,
                IsActive = true,
                DateCreated = DateTime.UtcNow,
                Stage = "Opportunity",
                StageItemId = opportunityDbId,
                BomChildItemIds = bomChildItemIdsJson,
                BomAccessoryItemIds = bomAccessoryItemIdsJson
            }, transaction);

            // Insert accessory items into sales_product_accessories
            if (item.AccessoryItems != null && item.AccessoryItems.Count > 0)
            {
                foreach (var acc in item.AccessoryItems)
                {
                    await connection.ExecuteAsync(
                        "INSERT INTO sales_product_accessories (sales_product_id, accessories_item_id, quantity, isactive, date_created, parent_child_item_id) VALUES (@SalesProductId, @AccessoryItemId, @Quantity, @IsActive, @DateCreated, @ParentChildItemId)",
                        new {
                            SalesProductId = salesProductId,
                            AccessoryItemId = acc.AccessoryDetailId,
                            Quantity = acc.Qty,
                            IsActive = true,
                            DateCreated = DateTime.UtcNow,
                            ParentChildItemId = acc.ParentChildItemId
                        }, transaction);
                }
            }
            else if (item.AccessoryItemIds != null && item.AccessoryItemIds.Count > 0)
            {
                foreach (var accessoryId in item.AccessoryItemIds)
                {
                    await connection.ExecuteAsync(
                        "INSERT INTO sales_product_accessories (sales_product_id, accessories_item_id, quantity, isactive, date_created) VALUES (@SalesProductId, @AccessoryItemId, @Quantity, @IsActive, @DateCreated)",
                        new {
                            SalesProductId = salesProductId,
                            AccessoryItemId = accessoryId,
                            Quantity = item.Quantity,
                            IsActive = true,
                            DateCreated = DateTime.UtcNow
                        }, transaction);
                }
            }
        }

        // When a LeadId is provided, mark the related lead as Converted within the same transaction
        if (!string.IsNullOrWhiteSpace(opportunity.LeadId))
        {
                await connection.ExecuteAsync(
                    "UPDATE sales_lead SET status = @Status, date_updated = CURRENT_TIMESTAMP, user_updated = CASE WHEN @UserUpdated > 0 THEN @UserUpdated ELSE user_updated END WHERE lead_id = @LeadId",
                    new { Status = "Converted", UserUpdated = GetCurrentUserId(), LeadId = opportunity.LeadId },
                    transaction);
        }

        // Prepare items array with full details
        var itemsArray = new List<object>();
        foreach (var item in request.Items)
        {
            // Fetch accessory item details (multiple)
            List<dynamic> accessoryItemDetails = new List<dynamic>();
            if (item.AccessoryItemIds != null && item.AccessoryItemIds.Count > 0)
            {
                var accessoryQuery = @"SELECT im.*, c.name AS category_name, m.name as make, mo.name as model, p.name as product 
                                       FROM item_master im
                                       LEFT JOIN categories c ON im.category_id = c.id
                                       LEFT JOIN make m ON im.make_id = m.id
                                       LEFT JOIN model mo ON im.model_id = mo.id
                                       LEFT JOIN product p ON im.product_id = p.id
                                       WHERE im.id = ANY(@AccessoryItemIds)";
                var rawAccessoryItems = await connection.QueryAsync<dynamic>(
                    accessoryQuery,
                    new { AccessoryItemIds = item.AccessoryItemIds },
                    transaction);

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
                @"SELECT bom_id, bom_name, bom_type 
                  FROM bill_of_materials 
                  WHERE bom_id = @BomId LIMIT 1",
                new { BomId = item.BomId }, transaction);

            // Fetch BOM child items with BOM-level detail
            var bomChildItems = await connection.QueryAsync<dynamic>(
                @"SELECT bci.child_item_id AS childItemId, bci.quantity, im.item_name, im.item_code, im.cat_no, im.unit_price, im.hsn, COALESCE(NULLIF(im.tax_percentage, -1), 0) AS tax, m.name as make, mo.name as model, p.name as product, c.name AS category_name, vm.name AS valuation_method_name, ime.name AS inventory_method_name, itt.name AS inventory_type_name, u.code AS uom_name, rmi.purchase_rate AS purchase_rate, rmi.sales_rate AS sale_rate, rmi.quotation_rate AS quote_rate, bom.quote_title_id, bom.tc_template_id
                  FROM bill_of_material_child_items bci
                  JOIN item_master im ON bci.child_item_id = im.id
                  LEFT JOIN categories c ON im.category_id = c.id
                  LEFT JOIN make m ON im.make_id = m.id
                  LEFT JOIN model mo ON im.model_id = mo.id
                  LEFT JOIN product p ON im.product_id = p.id
                  LEFT JOIN valuation_method vm ON im.valuation_method_id = vm.id
                  LEFT JOIN inventory_method ime ON im.inventory_method_id = ime.id
                  LEFT JOIN inventory_types itt ON im.group_id = itt.id
                  LEFT JOIN uom u ON im.uom_id = u.id
                  LEFT JOIN rate_master_items rmi ON im.id = rmi.item_id
                  LEFT JOIN bill_of_materials bom ON bci.bill_of_material_id = bom.id
                  WHERE bci.bill_of_material_id = 
                        (SELECT id FROM bill_of_materials WHERE bom_id = @BomId)",
                new { BomId = item.BomId }, transaction);

            var bomChildItemList = bomChildItems.Select(ci => new {
                childItemId = ci.childItemId,
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
                tax = ci.tax,
                quoteTitleId = ci.quote_title_id,
                tcTemplateId = ci.tc_template_id
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

        await transaction.CommitAsync();

        // Build response
        var response = new
        {
            Opportunity = new
            {
                Id = opportunityDbId,
                UserCreated = opportunity.UserCreated,
                DateCreated = opportunity.DateCreated,
                UserUpdated = opportunity.UserUpdated,
                DateUpdated = opportunity.DateUpdated,
                Status = opportunity.Status,
                ExpectedCompletion = opportunity.ExpectedCompletion,
                OpportunityType = opportunity.OpportunityType,
                OpportunityFor = opportunity.OpportunityFor,
                CustomerId = opportunity.CustomerId,
                CustomerName = opportunity.CustomerName,
                CustomerType = opportunity.CustomerType,
                OpportunityName = opportunity.OpportunityName,
                OpportunityId = opportunity.OpportunityId,
                Comments = opportunity.Comments,
                IsActive = opportunity.IsActive,
                LeadId = opportunity.LeadId,
                SalesRepresentativeId = opportunity.SalesRepresentativeId,
                ContactName = opportunity.ContactName,
                ContactMobileNo = opportunity.ContactMobileNo
            },
            Items = itemsArray,
            Stage = "Opportunity",
            StageItemId = opportunityDbId
        };

        return Created($"api/SalesOpportunity/{opportunityDbId}", response);
    }
    catch (Exception ex)
    {
        await transaction.RollbackAsync();
        _logger.LogError(ex, "Error creating opportunity with items: {Message}", ex.Message);
        return StatusCode(500, new { message = "Failed to create opportunity with items", error = ex.Message });
    }
}


        [HttpGet("with-items")]
        public async Task<IActionResult> GetAllOpportunitiesWithItems()
        {
            try
            {
                var connectionString = (_opportunityService as SalesOpportunityService)?.GetConnectionString();
                if (string.IsNullOrEmpty(connectionString))
                    return StatusCode(500, "Could not resolve connection string.");

                using var connection = new Npgsql.NpgsqlConnection(connectionString);
                // Get all opportunities in descending order by id, excluding those already used for a quotation
                var opportunities = (await connection.QueryAsync(@"
                    SELECT * FROM sales_opportunities 
                    WHERE opportunity_id IS NOT NULL AND opportunity_id NOT IN (SELECT opportunity_id FROM sales_quotations WHERE opportunity_id IS NOT NULL)
                    ORDER BY id DESC")).ToList();

                var results = new List<object>();
                foreach (var opp in opportunities)
                {
                // Fetch all contacts for this opportunity's lead (if present)
                List<object>? fetchedContacts = null;
                if (opp.lead_id != null)
                {
                    var leadNumericId = await connection.QueryFirstOrDefaultAsync<int?>(
                        "SELECT id FROM sales_lead WHERE lead_id = @LeadId", new { LeadId = opp.lead_id });
                    if (leadNumericId.HasValue)
                    {
                        var contacts = (await connection.QueryAsync(
                            "SELECT contact_name, mobile_no FROM sales_contacts WHERE sales_lead_id = @SalesLeadId",
                            new { SalesLeadId = leadNumericId.Value })).ToList();
                        if (contacts.Count > 0)
                        {
                            fetchedContacts = contacts.Select(c => new { ContactName = c.contact_name, MobileNo = c.mobile_no }).ToList<object>();
                        }
                    }
                }
                        // Fetch related sales items - handle both direct items and BOM-based items
                        var sql = @"SELECT 
                            si.id, si.user_created, si.date_created, si.user_updated, si.date_updated, si.qty, si.amount, si.is_active, si.item_id, si.stage, si.unit_price, si.stage_item_id, si.bom_id,
                            si.bom_child_item_ids, si.bom_accessory_item_ids,
                            -- Direct item details
                            m.name as make, mo.name as model, p.name as product, im.item_name as ItemName, im.item_code as ItemCode, im.unit_price as UnitPrice, im.hsn, im.tax_percentage as TaxPercentage,
                            c.name as CategoryName,
                            -- BOM details
                            bom.bom_name as BomName, bom.bom_type as BomType
                        FROM public.sales_product si
                        LEFT JOIN item_master im ON si.item_id = im.id
                        LEFT JOIN categories c ON im.category_id = c.id
                        LEFT JOIN make m ON im.make_id = m.id
                        LEFT JOIN model mo ON im.model_id = mo.id
                        LEFT JOIN product p ON im.product_id = p.id
                        LEFT JOIN bill_of_materials bom ON si.bom_id = bom.bom_id
                        WHERE si.stage = 'Opportunity' AND si.stage_item_id = @StageItemId;";
                        var itemsRaw = (await connection.QueryAsync<dynamic>(sql, new { StageItemId = opp.id })).ToList();

                        // Convert to SalesItemResponse and populate BOM details when item_id is null
                        var salesItems = new List<ERP.API.Models.SalesItemResponse>();
                        foreach (var item in itemsRaw)
                        {
                            var salesItem = new ERP.API.Models.SalesItemResponse
                            {
                                Id = item.id,
                                UserCreated = item.user_created,
                                DateCreated = item.date_created,
                                UserUpdated = item.user_updated,
                                DateUpdated = item.date_updated,
                                Qty = item.qty,
                                Amount = item.amount,
                                IsActive = item.is_active,
                                ItemId = item.item_id,
                                Stage = item.stage,
                                UnitPrice = item.unit_price,
                                StageItemId = item.stage_item_id?.ToString(),
                                BomId = item.bom_id,
                                Make = item.make,
                                Model = item.model,
                                Product = item.product,
                                CategoryName = item.CategoryName,
                                Hsn = item.hsn,
                                TaxPercentage = item.TaxPercentage
                            };

                            // If item_id is null but bom_id exists, use BOM details
                            if (item.item_id == null && !string.IsNullOrEmpty(item.bom_id))
                            {
                                // Use BOM name from the join, or fallback to direct query
                                var bomName = item.BomName;
                                if (string.IsNullOrEmpty(bomName))
                                {
                                    var bomDetails = await connection.QueryFirstOrDefaultAsync<dynamic>(
                                        "SELECT bom_name FROM bill_of_materials WHERE bom_id = @BomId",
                                        new { BomId = item.bom_id });
                                    bomName = bomDetails?.bom_name;
                                }
                                
                                salesItem.ItemName = bomName ?? item.bom_id;
                                salesItem.ItemCode = item.bom_id;
                                
                                // Fetch BOM details and rates
                                var bomFullDetails = await connection.QueryFirstOrDefaultAsync<dynamic>(
                                    @"SELECT bom_name, bom_type, effective_from, effective_to, quote_title_id, tc_template_id, make
                                      FROM bill_of_materials 
                                      WHERE bom_id = @BomId",
                                    new { BomId = item.bom_id });
                                
                                if (bomFullDetails != null)
                                {
                                    salesItem.BomName = bomFullDetails.bom_name;
                                    salesItem.BomType = bomFullDetails.bom_type;
                                    salesItem.EffectiveFrom = bomFullDetails.effective_from;
                                    salesItem.EffectiveTo = bomFullDetails.effective_to;
                                    salesItem.QuoteTitleId = bomFullDetails.quote_title_id;
                                    salesItem.TcTemplateId = bomFullDetails.tc_template_id;
                                    salesItem.Make = bomFullDetails.make;
                                }
                                
                                // Calculate total rates from child items
                                var bomRates = await connection.QueryFirstOrDefaultAsync<dynamic>(
                                    @"SELECT 
                                        SUM(rmi.purchase_rate * bci.quantity) as total_purchase_rate,
                                        SUM(rmi.sales_rate * bci.quantity) as total_sale_rate,
                                        SUM(rmi.quotation_rate * bci.quantity) as total_quote_rate,
                                        AVG(rmi.tax) as avg_tax
                                      FROM bill_of_material_child_items bci
                                      JOIN item_master im ON bci.child_item_id = im.id
                                      LEFT JOIN rate_master_items rmi ON im.id = rmi.item_id
                                      WHERE bci.bill_of_material_id = (SELECT id FROM bill_of_materials WHERE bom_id = @BomId)",
                                    new { BomId = item.bom_id });
                                
                                if (bomRates != null)
                                {
                                    salesItem.UnitPrice = bomRates.total_quote_rate;
                                    salesItem.TaxPercentage = bomRates.avg_tax;
                                    salesItem.PurchaseRate = bomRates.total_purchase_rate;
                                    salesItem.SaleRate = bomRates.total_sale_rate;
                                    salesItem.QuoteRate = bomRates.total_quote_rate;
                                }
                            }
                            else
                            {
                                salesItem.ItemName = item.ItemName;
                                salesItem.ItemCode = item.ItemCode;
                                
                                // Fetch rates for regular items
                                if (item.item_id != null)
                                {
                                    var itemRates = await connection.QueryFirstOrDefaultAsync<dynamic>(
                                        @"SELECT purchase_rate, sales_rate, quotation_rate, tax
                                          FROM rate_master_items 
                                          WHERE item_id = @ItemId",
                                        new { ItemId = item.item_id });
                                    
                                    if (itemRates != null)
                                    {
                                        salesItem.PurchaseRate = itemRates.purchase_rate;
                                        salesItem.SaleRate = itemRates.sales_rate;
                                        salesItem.QuoteRate = itemRates.quotation_rate;
                                        if (salesItem.TaxPercentage == null)
                                            salesItem.TaxPercentage = itemRates.tax;
                                    }
                                }
                            }

                            salesItems.Add(salesItem);
                        }

                        foreach (var item in salesItems)
                        {
                            // Fetch child items from sales_product_child_items for explicit child entries
                            item.IncludedChildItems = (await connection.QueryAsync<ERP.API.Models.SalesItemResponse>(
                                "SELECT spci.child_item_id AS ItemId, spci.quantity AS Qty, im.item_name AS ItemName, im.item_code AS ItemCode, im.cat_no AS CatNo, im.unit_price AS UnitPrice, im.hsn, im.tax_percentage AS TaxPercentage, c.name AS CategoryName, u.code AS UomName, m.name AS Make, mo.name AS Model, p.name AS Product, vm.name AS ValuationMethodName, ime.name AS InventoryMethodName, itt.name AS InventoryTypeName FROM sales_product_child_items spci JOIN item_master im ON spci.child_item_id = im.id LEFT JOIN categories c ON im.category_id = c.id LEFT JOIN uom u ON im.uom_id = u.id LEFT JOIN make m ON im.make_id = m.id LEFT JOIN model mo ON im.model_id = mo.id LEFT JOIN product p ON im.product_id = p.id LEFT JOIN valuation_method vm ON im.valuation_method_id = vm.id LEFT JOIN inventory_method ime ON im.inventory_method_id = ime.id LEFT JOIN inventory_types itt ON im.group_id = itt.id WHERE spci.sales_product_id = @SalesProductId",
                                new { SalesProductId = item.Id })).ToList();

                            // If there are no explicit child entries, fetch BOM child items for BOM-based entry
                            if ((item.IncludedChildItems == null || item.IncludedChildItems.Count == 0) && !string.IsNullOrEmpty(item.BomId))
                            {
                                // Fetch BOM child items with full details including rates
                                var bomChildItemsQuery = @"
                                    SELECT bci.child_item_id AS ItemId, bci.quantity AS Qty, 
                                           im.item_name AS ItemName, im.item_code AS ItemCode, im.cat_no AS CatNo, 
                                           im.unit_price AS UnitPrice, im.hsn, im.tax_percentage AS TaxPercentage, 
                                           c.name AS CategoryName, u.code AS UomName, 
                                           m.name AS Make, mo.name AS Model, p.name AS Product, 
                                           vm.name AS ValuationMethodName, ime.name AS InventoryMethodName, itt.name AS InventoryTypeName,
                                           rmi.purchase_rate AS PurchaseRate, rmi.sales_rate AS SaleRate, rmi.quotation_rate AS QuoteRate
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
                                    WHERE bci.bill_of_material_id = (SELECT id FROM bill_of_materials WHERE bom_id = @BomId)";
                                
                                var bomChildItems = await connection.QueryAsync<dynamic>(bomChildItemsQuery, new { BomId = item.BomId });
                                
                                // Convert to BomChildItemResponse format
                                item.ChildItems = bomChildItems.Select(ci => new ERP.API.Models.BomChildItemResponse
                                {
                                    ChildItemId = ci.ItemId ?? 0,
                                    Quantity = ci.Qty ?? 0,
                                    Make = ci.Make,
                                    Model = ci.Model,
                                    Product = ci.Product,
                                    CategoryName = ci.CategoryName,
                                    ValuationMethodName = ci.ValuationMethodName,
                                    InventoryMethodName = ci.InventoryMethodName,
                                    InventoryTypeName = ci.InventoryTypeName,
                                    UnitPrice = ci.UnitPrice,
                                    ItemName = ci.ItemName,
                                    ItemCode = ci.ItemCode,
                                    CatNo = ci.CatNo,
                                    UomName = ci.UomName,
                                    PurchaseRate = ci.PurchaseRate,
                                    SaleRate = ci.SaleRate,
                                    QuoteRate = ci.QuoteRate,
                                    Hsn = ci.hsn,
                                    Tax = ci.TaxPercentage
                                }).ToList();
                                
                                // Also populate IncludedChildItems for backward compatibility
                                item.IncludedChildItems = (await connection.QueryAsync<ERP.API.Models.SalesItemResponse>(
                                    @"SELECT bci.child_item_id AS ItemId, bci.quantity AS Qty, im.item_name AS ItemName, im.item_code AS ItemCode, im.cat_no AS CatNo, im.unit_price AS UnitPrice, im.hsn, im.tax_percentage AS TaxPercentage, c.name AS CategoryName, u.code AS UomName, m.name AS Make, mo.name AS Model, p.name AS Product, vm.name AS ValuationMethodName, ime.name AS InventoryMethodName, itt.name AS InventoryTypeName
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
                                      WHERE bci.bill_of_material_id = (SELECT id FROM bill_of_materials WHERE bom_id = @BomId)",
                                    new { BomId = item.BomId })).ToList();
                            }
                            
                            // Populate BOM details if available
                            if (!string.IsNullOrEmpty(item.BomId))
                            {
                                var bomDetails = await connection.QueryFirstOrDefaultAsync<dynamic>(
                                    @"SELECT bom_name, bom_type, effective_from, effective_to, quote_title_id, tc_template_id, make
                                      FROM bill_of_materials 
                                      WHERE bom_id = @BomId",
                                    new { BomId = item.BomId });
                                
                                if (bomDetails != null)
                                {
                                    item.BomName = bomDetails.bom_name;
                                    item.BomType = bomDetails.bom_type;
                                    item.EffectiveFrom = bomDetails.effective_from;
                                    item.EffectiveTo = bomDetails.effective_to;
                                    item.QuoteTitleId = bomDetails.quote_title_id;
                                    item.TcTemplateId = bomDetails.tc_template_id;
                                }
                            }

                            // Fetch accessory items from sales_product_accessories table
                            item.AccessoriesItems = (await connection.QueryAsync<ERP.API.Models.SalesItemResponse>(
                                "SELECT spa.accessories_item_id AS ItemId, spa.quantity AS Qty, im.item_name as ItemName, im.item_code as ItemCode, im.unit_price as UnitPrice, im.hsn, im.tax_percentage as TaxPercentage, c.name as CategoryName, m.name as Make, mo.name as Model, p.name as Product FROM sales_product_accessories spa JOIN item_master im ON spa.accessories_item_id = im.id LEFT JOIN categories c ON im.category_id = c.id LEFT JOIN make m ON im.make_id = m.id LEFT JOIN model mo ON im.model_id = mo.id LEFT JOIN product p ON im.product_id = p.id WHERE spa.sales_product_id = @SalesProductId",
                                new { SalesProductId = item.Id })).ToList();

                            // If no accessory items found in tables, try to get them from BOM JSON columns
                            if (item.AccessoriesItems.Count == 0)
                            {
                                var rawItem = itemsRaw.FirstOrDefault(r => r.id == item.Id);
                                if (rawItem?.bom_accessory_item_ids != null)
                                {
                                    try
                                    {
                                        var accessoryItemIds = Newtonsoft.Json.JsonConvert.DeserializeObject<List<int>>(rawItem.bom_accessory_item_ids.ToString());
                                        if (accessoryItemIds?.Count > 0)
                                        {
                                            item.AccessoriesItems = (await connection.QueryAsync<ERP.API.Models.SalesItemResponse>(
                                                "SELECT im.id, m.name as make, mo.name as model, p.name as product, im.item_name as ItemName, im.item_code as ItemCode, im.unit_price as UnitPrice, im.hsn, im.tax_percentage as TaxPercentage, c.name as CategoryName FROM item_master im LEFT JOIN categories c ON im.category_id = c.id LEFT JOIN make m ON im.make_id = m.id LEFT JOIN model mo ON im.model_id = mo.id LEFT JOIN product p ON im.product_id = p.id WHERE im.id = ANY(@ItemIds)",
                                                new { ItemIds = accessoryItemIds })).ToList();
                                        }
                                    }
                                    catch { /* Ignore JSON parsing errors */ }
                                }
                            }
                        }

                // Map to PascalCase C# object for response
                var opportunity = new {
                    Id = opp.id,
                    UserCreated = opp.user_created,
                    DateCreated = opp.date_created,
                    UserUpdated = opp.user_updated,
                    DateUpdated = opp.date_updated ?? opp.date_created, // Ensure DateUpdated is included
                    Status = opp.status,
                    ExpectedCompletion = opp.expected_completion,
                    OpportunityType = opp.opportunity_type,
                    OpportunityFor = opp.opportunity_for,
                    CustomerId = opp.customer_id,
                    CustomerName = opp.customer_name,
                    CustomerType = opp.customer_type,
                    OpportunityName = opp.opportunity_name,
                    OpportunityId = opp.opportunity_id,
                    Comments = opp.comments,
                    IsActive = opp.isactive,
                    LeadId = opp.lead_id,
                    SalesRepresentativeId = opp.sales_representative_id,
                    ContactName = opp.contact_name,
                    ContactMobileNo = opp.contact_mobile_no,
                    Contacts = fetchedContacts,
                    LeadAddress = opp.lead_id != null
                        ? await connection.QueryFirstOrDefaultAsync(
                            @"SELECT pincode, area, state, district, city, door_no, street, landmark FROM sales_lead WHERE lead_id = @LeadId LIMIT 1",
                            new { LeadId = opp.lead_id })
                        : null,
                    Items = salesItems
                };

                        results.Add(new { opportunity, items = salesItems });
                }

                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all opportunities with items: {Message}", ex.Message);
                return StatusCode(500, new { message = "Failed to retrieve opportunities with items", error = ex.Message });
            }
        }

        /// <summary>
        /// Updates a sales opportunity and its items by numeric database ID only
        /// </summary>
        /// <param name="id">The numeric database ID of the opportunity to update</param>
        /// <param name="request">The updated opportunity and items</param>
        /// <returns>The updated opportunity and items</returns>
        /// <response code="200">Updated successfully</response>
        /// <response code="400">Invalid input</response>
        /// <response code="404">Opportunity not found</response>
        /// <response code="500">Internal server error</response>
        [HttpPut("with-items/{id:int}")]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateOpportunityWithItemsById([FromRoute] int id, [FromBody] SalesOpportunityWithItemsRequest request)
        {
            var firstItem = request.Items != null && request.Items.Count > 0 ? request.Items[0] : null;
            if (request == null || firstItem == null || string.IsNullOrWhiteSpace(firstItem.BomId) || firstItem.Quantity <= 0)
                return BadRequest("BomId and Quantity are required");

            var connectionString = (_opportunityService as SalesOpportunityService)?.GetConnectionString();
            if (string.IsNullOrEmpty(connectionString))
                return StatusCode(500, "Could not resolve connection string.");

            using var connection = new Npgsql.NpgsqlConnection(connectionString);
            await connection.OpenAsync();
            using var transaction = await connection.BeginTransactionAsync();
            try
            {
                // Get opportunity_id from numeric id (id is varchar, so cast int to varchar)
                var opportunityId = await connection.QueryFirstOrDefaultAsync<string>(
                    "SELECT opportunity_id FROM sales_opportunities WHERE id = CAST(@Id AS varchar)",
                    new { Id = id }, transaction);
                if (string.IsNullOrEmpty(opportunityId))
                {
                    await transaction.RollbackAsync();
                    return NotFound(new { message = $"Opportunity with ID {id} not found", statusCode = 404 });
                }

                // Update all opportunity info fields from request
                var bomDetails = await connection.QueryFirstOrDefaultAsync<dynamic>(@"SELECT id, bom_id, bom_name FROM bill_of_materials WHERE bom_id = @BomId LIMIT 1", new { BomId = firstItem.BomId }, transaction);
                if (bomDetails == null)
                {
                    await transaction.RollbackAsync();
                    return BadRequest("Invalid BomId or BOM not found");
                }
                var opp = request.Opportunity;
                var sqlOpp = @"
                    UPDATE sales_opportunities SET
                        opportunity_name = @OpportunityName,
                        status = @Status,
                        customer_id = @CustomerId,
                        customer_name = @CustomerName,
                        customer_type = @CustomerType,
                        expected_completion = @ExpectedCompletion,
                        opportunity_type = @OpportunityType,
                        opportunity_for = @OpportunityFor,
                        comments = @Comments,
                        isactive = @IsActive,
                        lead_id = @LeadId,
                        sales_representative_id = @SalesRepresentativeId,
                        contact_name = @ContactName,
                        contact_mobile_no = @ContactMobileNo,
                        date_updated = CURRENT_TIMESTAMP
                    WHERE opportunity_id = @OpportunityId";
                await connection.ExecuteAsync(sqlOpp, new
                {
                    OpportunityName = bomDetails.bom_name,
                    Status = opp.Status,
                    CustomerId = opp.CustomerId,
                    CustomerName = opp.CustomerName,
                    CustomerType = opp.CustomerType,
                    ExpectedCompletion = opp.ExpectedCompletion,
                    OpportunityType = opp.OpportunityType,
                    OpportunityFor = opp.OpportunityFor,
                    Comments = opp.Comments,
                    IsActive = opp.IsActive,
                    LeadId = opp.LeadId,
                    SalesRepresentativeId = opp.SalesRepresentativeId,
                    ContactName = opp.ContactName,
                    ContactMobileNo = opp.ContactMobileNo,
                    OpportunityId = opportunityId
                }, transaction);

                // If the updated opportunity has a LeadId, ensure the related lead is marked Converted
                if (!string.IsNullOrWhiteSpace(opp.LeadId))
                {
                    await connection.ExecuteAsync(
                        "UPDATE sales_lead SET status = @Status, date_updated = CURRENT_TIMESTAMP, user_updated = CASE WHEN @UserUpdated > 0 THEN @UserUpdated ELSE user_updated END WHERE lead_id = @LeadId",
                        new { Status = "Converted", UserUpdated = GetCurrentUserId(), LeadId = opp.LeadId },
                        transaction);
                }

            // Delete all sales_product for this opportunity using numeric id
            // First get the sales_product IDs to delete
            var salesProductIds = await connection.QueryAsync<int>(
                "SELECT id FROM sales_product WHERE stage = 'Opportunity' AND stage_item_id = CAST(@StageItemId AS varchar)",
                new { StageItemId = id }, transaction);
            
            if (salesProductIds.Any())
            {
                // Delete accessories first to avoid foreign key constraint violation
                await connection.ExecuteAsync(
                    "DELETE FROM sales_product_accessories WHERE sales_product_id = ANY(@Ids)",
                    new { Ids = salesProductIds.ToArray() }, transaction);
                
                // Delete child items
                await connection.ExecuteAsync(
                    "DELETE FROM sales_product_child_items WHERE sales_product_id = ANY(@Ids)",
                    new { Ids = salesProductIds.ToArray() }, transaction);
                
                // Now delete the sales_product records
                await connection.ExecuteAsync(
                    "DELETE FROM sales_product WHERE id = ANY(@Ids)",
                    new { Ids = salesProductIds.ToArray() }, transaction);
            }

            // Fetch BOM child items
            var childItems = await connection.QueryAsync<dynamic>(@"SELECT ci.child_item_id, i.item_name, ci.quantity, i.unit_price FROM bill_of_material_child_items ci JOIN item_master i ON ci.child_item_id = i.id WHERE ci.bill_of_material_id = @BillOfMaterialId", new { BillOfMaterialId = bomDetails.id }, transaction);

            // Insert BOM as single sales_product row with accessory JSON
            var bomAccessoryItemIdsJson = (firstItem.AccessoryItems != null && firstItem.AccessoryItems.Count > 0)
                ? Newtonsoft.Json.JsonConvert.SerializeObject(firstItem.AccessoryItems.Select(a => a.AccessoryDetailId).ToList())
                : (firstItem.AccessoryItemIds != null && firstItem.AccessoryItemIds.Count > 0)
                ? Newtonsoft.Json.JsonConvert.SerializeObject(firstItem.AccessoryItemIds)
                : null;

            var bomChildItemIdsForJson = childItems.Select(ci => (int)ci.child_item_id).ToArray();
            var bomChildItemIdsJson = bomChildItemIdsForJson.Length > 0 ? Newtonsoft.Json.JsonConvert.SerializeObject(bomChildItemIdsForJson) : null;

            var sqlInsert = @"INSERT INTO sales_product (bom_id, qty, is_active, date_created, stage, stage_item_id, bom_child_item_ids, bom_accessory_item_ids) VALUES (@BomId, @Quantity, @IsActive, @DateCreated, @Stage, @StageItemId, @BomChildItemIds::jsonb, @BomAccessoryItemIds::jsonb) RETURNING id";
            var newSalesProductId = await connection.ExecuteScalarAsync<int>(sqlInsert, new
            {
                BomId = firstItem.BomId,
                Quantity = firstItem.Quantity,
                IsActive = true,
                DateCreated = DateTime.UtcNow,
                Stage = "Opportunity",
                StageItemId = id,
                BomChildItemIds = bomChildItemIdsJson,
                BomAccessoryItemIds = bomAccessoryItemIdsJson
            }, transaction);

            // Insert accessories with parentChildItemId
            if (firstItem.AccessoryItems != null && firstItem.AccessoryItems.Count > 0)
            {
                foreach (var acc in firstItem.AccessoryItems)
                {
                    await connection.ExecuteAsync(
                        "INSERT INTO sales_product_accessories (sales_product_id, accessories_item_id, quantity, isactive, date_created, parent_child_item_id) VALUES (@SalesProductId, @AccessoryItemId, @Quantity, @IsActive, @DateCreated, @ParentChildItemId)",
                        new {
                            SalesProductId = newSalesProductId,
                            AccessoryItemId = acc.AccessoryDetailId,
                            Quantity = acc.Qty,
                            IsActive = true,
                            DateCreated = DateTime.UtcNow,
                            ParentChildItemId = acc.ParentChildItemId
                        }, transaction);
                }
            }
            else if (firstItem.AccessoryItemIds != null && firstItem.AccessoryItemIds.Count > 0)
            {
                foreach (var accessoryId in firstItem.AccessoryItemIds)
                {
                    await connection.ExecuteAsync(
                        "INSERT INTO sales_product_accessories (sales_product_id, accessories_item_id, quantity, isactive, date_created) VALUES (@SalesProductId, @AccessoryItemId, @Quantity, @IsActive, @DateCreated)",
                        new {
                            SalesProductId = newSalesProductId,
                            AccessoryItemId = accessoryId,
                            Quantity = firstItem.Quantity,
                            IsActive = true,
                            DateCreated = DateTime.UtcNow
                        }, transaction);
                }
            }

            await transaction.CommitAsync();
            // Build response
            var itemsArray = new List<object>();
            itemsArray.Add(new {
                BomId = firstItem.BomId,
                AccessoryItemIds = firstItem.AccessoryItemIds,
                Quantity = firstItem.Quantity
            });
            var response = new {
                Id = id,
                Items = itemsArray,
                Stage = "Opportunity",
                StageItemId = opportunityId
            };
            return Ok(response);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error updating opportunity with items by id: {Message}", ex.Message);
            return StatusCode(500, new { message = "Failed to update opportunity with items by id", error = ex.Message });
        }
    }
 /// <summary>
    /// Deletes a specific sales opportunity by its numeric database ID (id)
    /// </summary>
    /// <param name="id">The numeric database ID of the opportunity to delete</param>
    /// <returns>No content on success</returns>
    /// <response code="204">If the opportunity was successfully deleted</response>
    /// <response code="400">If the id is invalid</response>
    /// <response code="404">If the opportunity is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpDelete("by-id/{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteOpportunityById([FromRoute] int id)
    {
        try
        {
            if (id <= 0)
                return BadRequest("A valid numeric ID must be provided");

            var connectionString = (_opportunityService as SalesOpportunityService)?.GetConnectionString();
            if (string.IsNullOrEmpty(connectionString))
                return StatusCode(500, "Could not resolve connection string.");

            using var connection = new Npgsql.NpgsqlConnection(connectionString);
            // Check if the opportunity exists
            var opportunity = await connection.QueryFirstOrDefaultAsync("SELECT id FROM sales_opportunities WHERE id::int = @Id", new { Id = id });
            if (opportunity == null)
                return NotFound($"Opportunity with ID {id} not found");

            // Soft delete: set isactive = false
            var rows = await connection.ExecuteAsync("UPDATE sales_opportunities SET isactive = false WHERE id::int = @Id", new { Id = id });
            if (rows == 0)
            {
                _logger.LogWarning("Opportunity with ID {Id} not found for deletion", id);
                return NotFound($"Opportunity with ID {id} not found");
            }
            _logger.LogInformation("Soft deleted opportunity with numeric ID {Id}", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting opportunity by numeric ID {Id}: {Message}", id, ex.Message);
            return StatusCode(500, $"Failed to delete opportunity {id}: {ex.Message}");
        }
    }

        /// <summary>
        /// Gets a specific sales opportunity by its numeric database ID, including related sales items, child items, accessory items, and quotation information if available
        /// </summary>
        /// <param name="id">The numeric database ID of the opportunity</param>
        /// <returns>The requested sales opportunity, its items, and quotation information if a quotation was created from this opportunity</returns>
        /// <response code="200">Returns the requested opportunity, items, and quotation information</response>
        /// <response code="404">If the opportunity is not found</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpGet("with-items/by-id/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetOpportunityWithItemsById([FromRoute] int id)
                // Debug: Log BOM ID and BOM accessory item IDs
        {
            var connectionString = (_opportunityService as SalesOpportunityService)?.GetConnectionString();
            if (string.IsNullOrEmpty(connectionString))
                return StatusCode(500, "Could not resolve connection string.");

            using var connection = new Npgsql.NpgsqlConnection(connectionString);
            await connection.OpenAsync();
            try
            {
                // Get opportunity_id from numeric id (id is varchar, so cast int to varchar)
                var opportunityId = await connection.QueryFirstOrDefaultAsync<string>(
                    "SELECT opportunity_id FROM sales_opportunities WHERE id = CAST(@Id AS varchar)",
                    new { Id = id });
                if (string.IsNullOrEmpty(opportunityId))
                {
                    return NotFound(new { message = $"Opportunity with ID {id} not found", statusCode = 404 });
                }

                // Fetch the full opportunity record (all fields) to return, then map to PascalCase object
                var opportunityRaw = await connection.QueryFirstOrDefaultAsync(@"SELECT * FROM sales_opportunities WHERE opportunity_id = @OpportunityId", new { OpportunityId = opportunityId });
                if (opportunityRaw == null)
                {
                    return NotFound(new { message = $"Opportunity with ID {id} not found", statusCode = 404 });
                }

                // Build opportunity info
                var opportunity = new
                {
                    Id = opportunityRaw.id,
                    OpportunityId = opportunityRaw.opportunity_id,
                    OpportunityName = opportunityRaw.opportunity_name,
                    Status = opportunityRaw.status,
                    CustomerId = opportunityRaw.customer_id,
                    CustomerName = opportunityRaw.customer_name,
                    CustomerType = opportunityRaw.customer_type,
                    DateCreated = opportunityRaw.date_created,
                    IsActive = opportunityRaw.isactive
                };

                // Fetch BOM child items and map to ItemDropdownDto
                // ...existing code...
                var childItemsRaw = await connection.QueryAsync<dynamic>(@"SELECT * FROM bill_of_material_child_items WHERE bill_of_material_id = (SELECT id FROM bill_of_materials WHERE bom_id = @BomId)", new { BomId = opportunityRaw.bom_id });
                var childItemIds = childItemsRaw.Select(ci => (int)ci.child_item_id).ToList();
                var items = childItemIds.Count > 0 ? (await connection.QueryAsync<dynamic>(@"SELECT * FROM item_master WHERE id = ANY(@ItemIds)", new { ItemIds = childItemIds })).ToList() : new List<dynamic>();
                var categories = await connection.QueryAsync<dynamic>(@"SELECT * FROM categories");
                var uoms = await connection.QueryAsync<dynamic>(@"SELECT * FROM uom");
                var valuationMethods = await connection.QueryAsync<dynamic>(@"SELECT * FROM valuation_method");
                var inventoryMethods = await connection.QueryAsync<dynamic>(@"SELECT * FROM inventory_method");
                var inventoryTypes = await connection.QueryAsync<dynamic>(@"SELECT * FROM inventory_types");
                var rates = childItemIds.Count > 0 ? (await connection.QueryAsync<dynamic>(@"SELECT * FROM rate_master_items WHERE item_id = ANY(@ItemIds)", new { ItemIds = childItemIds })).ToList() : new List<dynamic>();

                var childItems = childItemsRaw.Select(ci => {
                    var item = items.FirstOrDefault(i => i.id == ci.child_item_id);
                    var categoryName = item?.category_id != null ? categories.FirstOrDefault(c => c.id == item.category_id)?.name : null;
                    var uomName = item?.uom_id != null ? uoms.FirstOrDefault(u => u.id == item.uom_id)?.code : null;
                    var valuationMethodName = item?.valuation_method_id != null ? valuationMethods.FirstOrDefault(vm => vm.id == item.valuation_method_id)?.name : null;
                    var inventoryMethodName = item?.inventory_method_id != null ? inventoryMethods.FirstOrDefault(im => im.id == item.inventory_method_id)?.name : null;
                    var inventoryTypeName = item?.group_id != null ? inventoryTypes.FirstOrDefault(it => it.id == item.group_id)?.name : null;
                    var rate = rates.FirstOrDefault(r => r.item_id == ci.child_item_id);
                    return new ERP.API.Models.DTOs.ItemDropdownDto {
                        ItemId = ci.child_item_id,
                        Quantity = ci.quantity,
                        Make = null,
                        Model = null,
                        Product = null,
                        CategoryName = categoryName,
                        ValuationMethodName = valuationMethodName,
                        InventoryMethodName = inventoryMethodName,
                        InventoryTypeName = inventoryTypeName,
                        UnitPrice = item?.unit_price,
                        ItemName = item?.item_name,
                        ItemCode = item?.item_code,
                        CatNo = item?.cat_no,
                        UomName = uomName,
                        PurchaseRate = rate?.purchase_rate,
                        SaleRate = rate?.sale_rate,
                        QuoteRate = rate?.quote_rate,
                        HSN = rate?.hsn_code
                    };
                }).ToList();

                // Fetch accessory items and map to ItemDropdownDto
                List<ERP.API.Models.DTOs.ItemDropdownDto> accessoryItems = new List<ERP.API.Models.DTOs.ItemDropdownDto>();
                if (opportunityRaw.bom_accessory_item_ids != null)
                {
                    try
                    {
                        var ids = Newtonsoft.Json.JsonConvert.DeserializeObject<List<int>>(opportunityRaw.bom_accessory_item_ids.ToString());
                        _logger.LogInformation($"Debug Accessory IDs: {string.Join(",", ids)}");
                        if (ids.Count > 0)
                        {
                            var accessoryQuery = @"SELECT i.id as ItemId, c.name as CategoryName, ig.name as GroupName, vm.name as ValuationMethodName, m.name as Make, mo.name as Model, p.name as Product, i.item_name as ItemName, i.item_code as ItemCode FROM item_master i LEFT JOIN categories c ON i.category_id = c.id LEFT JOIN inventory_group ig ON i.group_id = ig.id LEFT JOIN valuation_method vm ON i.valuation_method_id = vm.id LEFT JOIN make m ON i.make_id = m.id LEFT JOIN model mo ON i.model_id = mo.id LEFT JOIN product p ON i.product_id = p.id WHERE i.id = ANY(@AccessoryItemIds)";
                            var rawAccessoryItems = await connection.QueryAsync<dynamic>(accessoryQuery, new { AccessoryItemIds = ids });
                            _logger.LogInformation($"Debug Accessory items count: {rawAccessoryItems.Count()}");
                            accessoryItems = rawAccessoryItems.Select(item => new ERP.API.Models.DTOs.ItemDropdownDto {
                                ItemId = item.ItemId,
                                CategoryName = item.CategoryName,
                                GroupName = item.GroupName,
                                ValuationMethodName = item.ValuationMethodName,
                                Make = item.Make,
                                Model = item.Model,
                                Product = item.Product,
                                ItemName = item.ItemName,
                                ItemCode = item.ItemCode
                            }).ToList();
                        }
                    }
                    catch (Exception ex) { _logger.LogError(ex, "Accessory item debug error"); }
                }

                // Fetch full opportunity info from sales_opportunities
                var opportunityInfo = await connection.QueryFirstOrDefaultAsync<dynamic>(
                    "SELECT * FROM sales_opportunities WHERE id = CAST(@Id AS varchar)", new { Id = id });

                // Fetch child items with full details (use unique variable names)
                List<ERP.API.Models.DTOs.ItemDropdownDto> childItems2 = new List<ERP.API.Models.DTOs.ItemDropdownDto>();
                if (!string.IsNullOrEmpty(Convert.ToString(opportunityRaw.bom_id))) {
                    var childItemsRaw2 = await connection.QueryAsync<dynamic>(@"SELECT * FROM bill_of_material_child_items WHERE bill_of_material_id = (SELECT id FROM bill_of_materials WHERE bom_id = @BomId)", new { BomId = opportunityRaw.bom_id });
                    var childItemIds2 = childItemsRaw2.Select(ci => (int)ci.child_item_id).ToList();
                    var childItemsData2 = childItemIds2.Count > 0 ? (await connection.QueryAsync<dynamic>(@"SELECT im.*, c.name as category_name, ig.name as group_name, vm.name as valuation_method_name, it.name as inventory_type_name, u.code as uom_name, rmi.quotation_rate AS quote_rate, rmi.sales_rate AS sale_rate, rmi.purchase_rate AS purchase_rate, COALESCE(NULLIF(rmi.tax, -1), 0) AS tax_percentage, m.name as make, mo.name as model, p.name as product, bom.quote_title_id, bom.tc_template_id FROM item_master im LEFT JOIN categories c ON im.category_id = c.id LEFT JOIN inventory_group ig ON im.group_id = ig.id LEFT JOIN valuation_method vm ON im.valuation_method_id = vm.id LEFT JOIN inventory_types it ON im.group_id = it.id LEFT JOIN uom u ON im.uom_id = u.id LEFT JOIN rate_master_items rmi ON im.id = rmi.item_id LEFT JOIN make m ON im.make_id = m.id LEFT JOIN model mo ON im.model_id = mo.id LEFT JOIN product p ON im.product_id = p.id LEFT JOIN bill_of_materials bom ON im.id = ANY(SELECT unnest(string_to_array(trim(both '[]' from bom.bom_child_item_ids::text), ','))::int[]) WHERE bom.bom_id = @BomId) WHERE im.id = ANY(@ItemIds)", new { ItemIds = childItemIds2, BomId = opportunityRaw.bom_id })).ToList() : new List<dynamic>();
                    childItems2 = childItemsRaw2.Select(ci => {
                        var item = childItemsData2.FirstOrDefault(i => i.id == ci.child_item_id);
                        return new ERP.API.Models.DTOs.ItemDropdownDto {
                            ItemId = ci.child_item_id,
                            Quantity = ci.quantity,
                            Make = item?.make,
                            Model = item?.model,
                            Product = item?.product,
                            CategoryName = item?.category_name,
                            ValuationMethodName = item?.valuation_method_name,
                            InventoryMethodName = item?.inventory_method_name,
                            InventoryTypeName = item?.inventory_type_name,
                            UnitPrice = item?.unit_price,
                            ItemName = item?.item_name,
                            ItemCode = item?.item_code,
                            CatNo = item?.cat_no,
                            UomName = item?.uom_name,
                            PurchaseRate = item?.purchase_rate,
                            SaleRate = item?.sale_rate,
                            QuoteRate = item?.quote_rate,
                            GroupName = item?.group_name,
                            HSN = item?.hsn,
                            TaxPercentage = item?.tax_percentage,
                            QuoteTitleId = item?.quote_title_id,
                            TcTemplateId = item?.tc_template_id
                        };
                    }).ToList();
                }
                // If BOM is missing, fetch items directly from sales_product for this opportunity
                if (childItems2.Count == 0) {
                    var directItemsRaw = await connection.QueryAsync<dynamic>(@"SELECT * FROM sales_product WHERE stage_item_id = @OpportunityId", new { OpportunityId = opportunityRaw.opportunity_id });
                    var directItemIds = directItemsRaw.Select(i => (int?)i.item_id).Where(i => i.HasValue).Select(i => i.Value).ToList();
                    var directItemsData = directItemIds.Count > 0 ? (await connection.QueryAsync<dynamic>(@"SELECT im.*, c.name as category_name, ig.name as group_name, vm.name as valuation_method_name, it.name as inventory_type_name, u.code as uom_name, rmi.quotation_rate AS quote_rate, rmi.sales_rate AS sale_rate, rmi.purchase_rate AS purchase_rate, COALESCE(NULLIF(rmi.tax, -1), 0) AS tax_percentage, m.name as make, mo.name as model, p.name as product FROM item_master im LEFT JOIN categories c ON im.category_id = c.id LEFT JOIN inventory_group ig ON im.group_id = ig.id LEFT JOIN valuation_method vm ON im.valuation_method_id = vm.id LEFT JOIN inventory_types it ON im.group_id = it.id LEFT JOIN uom u ON im.uom_id = u.id LEFT JOIN rate_master_items rmi ON im.id = rmi.item_id LEFT JOIN make m ON im.make_id = m.id LEFT JOIN model mo ON im.model_id = mo.id LEFT JOIN product p ON im.product_id = p.id WHERE im.id = ANY(@ItemIds)", new { ItemIds = directItemIds })).ToList() : new List<dynamic>();
                    childItems2 = directItemsRaw.Select(i => {
                        var item = directItemsData.FirstOrDefault(d => d.id == i.item_id);
                        return new ERP.API.Models.DTOs.ItemDropdownDto {
                            ItemId = i.item_id,
                            Quantity = i.qty,
                            Make = item?.make,
                            Model = item?.model,
                            Product = item?.product,
                            CategoryName = item?.category_name,
                            ValuationMethodName = item?.valuation_method_name,
                            InventoryMethodName = item?.inventory_method_name,
                            InventoryTypeName = item?.inventory_type_name,
                            UnitPrice = item?.unit_price,
                            ItemName = item?.item_name,
                            ItemCode = item?.item_code,
                            CatNo = item?.cat_no,
                            UomName = item?.uom_name,
                            PurchaseRate = item?.purchase_rate,
                            SaleRate = item?.sale_rate,
                            QuoteRate = item?.quote_rate,
                            GroupName = item?.group_name,
                            HSN = item?.hsn,
                            TaxPercentage = item?.tax_percentage
                        };
                    }).ToList();
                }

                // Fetch accessory items with full details (use unique variable names)
                List<ERP.API.Models.DTOs.ItemDropdownDto> accessoryItems2 = new List<ERP.API.Models.DTOs.ItemDropdownDto>();
                if (opportunityRaw.bom_accessory_item_ids != null)
                {
                    try
                    {
                        var ids2 = Newtonsoft.Json.JsonConvert.DeserializeObject<List<int>>(opportunityRaw.bom_accessory_item_ids.ToString());
                        if (ids2.Count > 0)
                                {
                                    var accessoryData2 = await connection.QueryAsync<dynamic>(@"SELECT im.*, c.name as category_name, ig.name as group_name, vm.name as valuation_method_name, it.name as inventory_type_name, u.code as uom_name, rmi.quotation_rate AS quote_rate, rmi.sales_rate AS sale_rate, rmi.purchase_rate AS purchase_rate, COALESCE(NULLIF(rmi.tax, -1), 0) AS tax_percentage, m.name as make, mo.name as model, p.name as product, bom.quote_title_id, bom.tc_template_id FROM item_master im LEFT JOIN categories c ON im.category_id = c.id LEFT JOIN inventory_group ig ON im.group_id = ig.id LEFT JOIN valuation_method vm ON im.valuation_method_id = vm.id LEFT JOIN inventory_types it ON im.group_id = it.id LEFT JOIN uom u ON im.uom_id = u.id LEFT JOIN rate_master_items rmi ON im.id = rmi.item_id LEFT JOIN make m ON im.make_id = m.id LEFT JOIN model mo ON im.model_id = mo.id LEFT JOIN product p ON im.product_id = p.id LEFT JOIN bill_of_materials bom ON im.id = ANY(SELECT unnest(string_to_array(trim(both '[]' from bom.bom_accessory_item_ids::text), ','))::int[]) WHERE bom.bom_id = @BomId) WHERE im.id = ANY(@AccessoryItemIds)", new { AccessoryItemIds = ids2, BomId = opportunityRaw.bom_id });
                                    accessoryItems2 = accessoryData2.Select(item => new ERP.API.Models.DTOs.ItemDropdownDto {
                                        ItemId = item.id,
                                        CategoryName = item?.category_name,
                                        GroupName = item?.group_name,
                                        ValuationMethodName = item?.valuation_method_name,
                                        Make = item?.make,
                                        Model = item?.model,
                                        Product = item?.product,
                                        ItemName = item?.item_name,
                                        ItemCode = item?.item_code,
                                        CatNo = item?.cat_no,
                                        UomName = item?.uom_name,
                                        UnitPrice = item?.unit_price,
                                        SaleRate = item?.sale_rate,
                                        QuoteRate = item?.quote_rate,
                                        PurchaseRate = item?.purchase_rate,
                                        InventoryTypeName = item?.inventory_type_name,
                                        HSN = item?.hsn,
                                        TaxPercentage = item?.tax_percentage,
                                        QuoteTitleId = item?.quote_title_id,
                                        TcTemplateId = item?.tc_template_id
                                    }).ToList();
                                }
                    }
                    catch (Exception ex) { _logger.LogError(ex, "Accessory item fetch error"); }
                }
                // If BOM is missing, fetch accessory items directly from sales_product for this opportunity
                if (accessoryItems2.Count == 0) {
                    var directAccessoryRaw = await connection.QueryAsync<dynamic>(@"SELECT * FROM sales_product WHERE stage_item_id = @OpportunityId AND bom_accessory_item_ids IS NOT NULL", new { OpportunityId = opportunityRaw.opportunity_id });
                    foreach (var sp in directAccessoryRaw) {
                        if (sp.bom_accessory_item_ids != null) {
                            try {
                                var ids3 = Newtonsoft.Json.JsonConvert.DeserializeObject<List<int>>(sp.bom_accessory_item_ids.ToString());
                                if (ids3.Count > 0) {
                                    var accessoryData3 = await connection.QueryAsync<dynamic>(@"SELECT im.*, c.name as category_name, ig.name as group_name, vm.name as valuation_method_name, it.name as inventory_type_name, u.code as uom_name, rmi.quotation_rate AS quote_rate, rmi.sales_rate AS sale_rate, rmi.purchase_rate AS purchase_rate, COALESCE(NULLIF(rmi.tax, -1), 0) AS tax_percentage, m.name as make, mo.name as model, p.name as product FROM item_master im LEFT JOIN categories c ON im.category_id = c.id LEFT JOIN inventory_group ig ON im.group_id = ig.id LEFT JOIN valuation_method vm ON im.valuation_method_id = vm.id LEFT JOIN inventory_types it ON im.group_id = it.id LEFT JOIN uom u ON im.uom_id = u.id LEFT JOIN rate_master_items rmi ON im.id = rmi.item_id LEFT JOIN make m ON im.make_id = m.id LEFT JOIN model mo ON im.model_id = mo.id LEFT JOIN product p ON im.product_id = p.id WHERE im.id = ANY(@AccessoryItemIds)", new { AccessoryItemIds = ids3 });
                                    var mapped = accessoryData3.Select(item => new ERP.API.Models.DTOs.ItemDropdownDto {
                                        ItemId = item.id,
                                        CategoryName = item?.category_name,
                                        GroupName = item?.group_name,
                                        ValuationMethodName = item?.valuation_method_name,
                                        Make = item?.make,
                                        Model = item?.model,
                                        Product = item?.product,
                                        ItemName = item?.item_name,
                                        ItemCode = item?.item_code,
                                        CatNo = item?.cat_no,
                                        UomName = item?.uom_name,
                                        UnitPrice = item?.unit_price,
                                        SaleRate = item?.sale_rate,
                                        QuoteRate = item?.quote_rate,
                                        PurchaseRate = item?.purchase_rate,
                                        InventoryTypeName = item?.inventory_type_name,
                                        HSN = item?.hsn,
                                        TaxPercentage = item?.tax_percentage,
                                        QuoteTitleId = item?.quote_title_id,
                                        TcTemplateId = item?.tc_template_id
                                    }).ToList();
                                    accessoryItems2.AddRange(mapped);
                                }
                            } catch (Exception ex) { _logger.LogError(ex, "Direct accessory item fetch error"); }
                        }
                    }
                }

                // Fetch BOM details for bomName and bomType
                dynamic bomDetails = null;
                if (!string.IsNullOrEmpty(Convert.ToString(opportunityRaw.bom_id))) {
                    bomDetails = await connection.QueryFirstOrDefaultAsync(@"SELECT bom_id, bom_name, bom_type FROM bill_of_materials WHERE bom_id = @BomId", new { BomId = opportunityRaw.bom_id });
                }
                // Fetch all items from sales_product for this opportunity using numeric id
                var salesProductRows = await connection.QueryAsync<dynamic>(@"SELECT id, bom_id, item_id, qty, bom_accessory_item_ids FROM sales_product WHERE stage_item_id = CAST(@StageItemId AS varchar) AND bom_id IS NOT NULL AND bom_id ~ '^[A-Za-z]'", new { StageItemId = id });
                var itemsArray = new List<object>();
                foreach (var row in salesProductRows)
                {
                    List<int> accessoryIds = new List<int>();
                    if (row.bom_accessory_item_ids != null)
                    {
                        try
                        {
                            accessoryIds = Newtonsoft.Json.JsonConvert.DeserializeObject<List<int>>(row.bom_accessory_item_ids.ToString());
                        }
                        catch { }
                    }
                    itemsArray.Add(new {
                        BomId = row.bom_id,
                        ItemId = row.item_id,
                        AccessoryItemIds = accessoryIds,
                        Quantity = row.qty ?? 0
                    });
                }
                    // Build response to match POST /api/SalesOpportunity/with-items
                    // Build items array in the same format as POST response
                    var itemsList = new List<object>();
                    foreach (var row in salesProductRows)
                    {
                        List<int> accessoryIds = new List<int>();
                        if (row.bom_accessory_item_ids != null)
                        {
                            try
                            {
                                accessoryIds = Newtonsoft.Json.JsonConvert.DeserializeObject<List<int>>(row.bom_accessory_item_ids.ToString());
                            }
                            catch { }
                        }
                        // Fetch accessory item details — always query sales_product_accessories by sales_product_id
                        List<dynamic> accessoryItemDetails = new List<dynamic>();
                        if (row.id != null)
                        {
                            var accessoryQuery = @"SELECT spa.accessories_item_id AS id, spa.quantity, spa.parent_child_item_id,
                                ad.accessories_name AS item_name, ad.item_type AS category_name, ad.qty AS default_qty
                                FROM sales_product_accessories spa
                                JOIN accessories_details ad ON spa.accessories_item_id = ad.id
                                WHERE spa.sales_product_id = @SalesProductId";
                            var rawAccessoryItems = await connection.QueryAsync<dynamic>(accessoryQuery, new { SalesProductId = (int)row.id });
                            accessoryItemDetails = rawAccessoryItems.Select(ai => (dynamic)new {
                                id = (int)ai.id,
                                itemId = (int)ai.id,
                                itemName = (string)(ai.item_name ?? ""),
                                itemCode = "",
                                unitPrice = 0m,
                                hsn = "",
                                taxPercentage = 0m,
                                categoryName = (string)(ai.category_name ?? ""),
                                make = "",
                                model = "",
                                product = "",
                                quantity = (int)(ai.quantity ?? ai.default_qty ?? 1),
                                parentChildItemId = ai.parent_child_item_id != null ? (int?)ai.parent_child_item_id : null
                            }).ToList<dynamic>();
                        }
                        // Fetch BOM details
                        var bomDetailsFull = await connection.QueryFirstOrDefaultAsync(@"SELECT bom_id, bom_name, bom_type FROM bill_of_materials WHERE bom_id = @BomId LIMIT 1", new { BomId = row.bom_id });
                        // Fetch BOM child items with full detail fields
                        var bomChildItems = await connection.QueryAsync<dynamic>(@"SELECT bci.child_item_id, bci.quantity, im.item_name, im.item_code, im.cat_no, im.unit_price, im.hsn, im.tax_percentage, m.name as make, mo.name as model, p.name as product, c.name as category_name, vm.name as valuation_method_name, ime.name as inventory_method_name, itt.name as inventory_type_name, u.code as uom_name, rmi.purchase_rate AS purchase_rate, rmi.sales_rate AS sale_rate, rmi.quotation_rate AS quote_rate, bom.quote_title_id, bom.tc_template_id FROM bill_of_material_child_items bci JOIN item_master im ON bci.child_item_id = im.id LEFT JOIN make m ON im.make_id = m.id LEFT JOIN model mo ON im.model_id = mo.id LEFT JOIN product p ON im.product_id = p.id LEFT JOIN categories c ON im.category_id = c.id LEFT JOIN valuation_method vm ON im.valuation_method_id = vm.id LEFT JOIN inventory_method ime ON im.inventory_method_id = ime.id LEFT JOIN inventory_types itt ON im.group_id = itt.id LEFT JOIN uom u ON im.uom_id = u.id LEFT JOIN rate_master_items rmi ON im.id = rmi.item_id LEFT JOIN bill_of_materials bom ON bci.bill_of_material_id = bom.id WHERE bci.bill_of_material_id = (SELECT id FROM bill_of_materials WHERE bom_id = @BomId)", new { BomId = row.bom_id });
                        // Extract quoteTitleId and tcTemplateId from first child item
                        var firstChildItem = bomChildItems.FirstOrDefault();
                        var quoteTitleId = firstChildItem?.quote_title_id;
                        var tcTemplateId = firstChildItem?.tc_template_id;
                        
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
                        if (row.bom_id != null)
                        {
                            itemsList.Add(new {
                                bomId = row.bom_id,
                                bomName = bomDetailsFull?.bom_name,
                                bomType = bomDetailsFull?.bom_type,
                                bomChildItems = bomChildItemList,
                                accessoryItemIds = accessoryItemDetails.Select(a => (int)a.id).ToList(),
                                accessoryItems = accessoryItemDetails,
                                quantity = row.qty ?? 0
                            });
                        }
                        else if (row.item_id != null)
                        {
                            // Fetch direct item details from item_master
                            var itemDetail = await connection.QueryFirstOrDefaultAsync<dynamic>(@"SELECT im.*, m.name as make, mo.name as model, p.name as product, c.name as category_name, u.code as uom_name FROM item_master im LEFT JOIN make m ON im.make_id = m.id LEFT JOIN model mo ON im.model_id = mo.id LEFT JOIN product p ON im.product_id = p.id LEFT JOIN categories c ON im.category_id = c.id LEFT JOIN uom u ON im.uom_id = u.id WHERE im.id = @ItemId", new { ItemId = row.item_id });
                            
                            if (itemDetail != null)
                            {
                                itemsList.Add(new {
                                    itemId = itemDetail.id,
                                    itemName = itemDetail.item_name,
                                    itemCode = itemDetail.item_code,
                                    make = itemDetail.make,
                                    model = itemDetail.model,
                                    product = itemDetail.product,
                                    categoryName = itemDetail.category_name,
                                    uomName = itemDetail.uom_name,
                                    unitPrice = itemDetail.unit_price,
                                    quantity = row.qty ?? 0,
                                    childItems = new List<object>() // Direct item has no child items
                                });
                            }
                        }
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
                    
                    // Check if this opportunity has created a quotation
                    dynamic quotationInfo = null;
                    if (opportunityInfo?.opportunity_id != null)
                    {
                        quotationInfo = await connection.QueryFirstOrDefaultAsync(
                            @"SELECT id, quotation_id, status, quotation_date, customer_name, comments, 
                                     valid_till, quotation_for, order_type, delivery_within, delivery_after,
                                     freight_charge, discount, taxes, delivery, payment, warranty,
                                     user_created, date_created, user_updated, date_updated
                              FROM sales_quotations 
                              WHERE opportunity_id = @OpportunityId AND is_active = true
                              ORDER BY date_created DESC LIMIT 1",
                            new { OpportunityId = opportunityInfo.opportunity_id });
                    }

                    var response = new
                    {
                        Opportunity = new
                        {
                            Id = opportunityInfo?.id,
                            UserCreated = opportunityInfo?.user_created,
                            DateCreated = opportunityInfo?.date_created,
                            UserUpdated = opportunityInfo?.user_updated,
                            DateUpdated = opportunityInfo?.date_updated,
                            Status = opportunityInfo?.status,
                            ExpectedCompletion = opportunityInfo?.expected_completion,
                            OpportunityType = opportunityInfo?.opportunity_type,
                            OpportunityFor = opportunityInfo?.opportunity_for,
                            CustomerId = opportunityInfo?.customer_id,
                            CustomerName = opportunityInfo?.customer_name,
                            CustomerType = opportunityInfo?.customer_type,
                            OpportunityName = opportunityInfo?.opportunity_name,
                            OpportunityId = opportunityInfo?.opportunity_id,
                            Comments = opportunityInfo?.comments,
                            IsActive = opportunityInfo?.isactive,
                            LeadId = opportunityInfo?.lead_id,
                            SalesRepresentativeId = opportunityInfo?.sales_representative_id,
                            ContactName = opportunityInfo?.contact_name,
                            ContactMobileNo = opportunityInfo?.contact_mobile_no
                        },
                        Items = itemsList,
                        Stage = "Opportunity",
                        StageItemId = opportunityInfo?.id,
                        QuoteTitleId = responseQuoteTitleId,
                        TcTemplateId = responseTcTemplateId,
                        Quotation = quotationInfo != null ? new
                        {
                            Id = quotationInfo.id,
                            QuotationId = quotationInfo.quotation_id,
                            Status = quotationInfo.status,
                            QuotationDate = quotationInfo.quotation_date,
                            CustomerName = quotationInfo.customer_name,
                            Comments = quotationInfo.comments,
                            ValidTill = quotationInfo.valid_till,
                            QuotationFor = quotationInfo.quotation_for,
                            OrderType = quotationInfo.order_type,
                            DeliveryWithin = quotationInfo.delivery_within,
                            DeliveryAfter = quotationInfo.delivery_after,
                            FreightCharge = quotationInfo.freight_charge,
                            Discount = quotationInfo.discount,
                            Taxes = quotationInfo.taxes,
                            Delivery = quotationInfo.delivery,
                            Payment = quotationInfo.payment,
                            Warranty = quotationInfo.warranty,
                            UserCreated = quotationInfo.user_created,
                            DateCreated = quotationInfo.date_created,
                            UserUpdated = quotationInfo.user_updated,
                            DateUpdated = quotationInfo.date_updated
                        } : null
                    };
                    return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving opportunity with items by id {Id}: {Message}", id, ex.Message);
                return StatusCode(500, new {
                    message = "Failed to retrieve opportunity with items by id",
                    error = ex.Message,
                    statusCode = 500
                });
            }
        }

        /// <summary>
        /// Converts a lead to an opportunity or creates a new opportunity if LeadId is null
        /// </summary>
        /// <param name="dto">Sales opportunity data</param>
        /// <returns>Result of conversion or creation</returns>
        // removed stray closing brace to keep methods inside the class
        /// <summary>
        /// Gets all sales opportunities with their items, contacts, and lead address
        /// </summary>
        /// <returns>List of opportunities with items, contacts, and lead address</returns>
        /// <response code="200">Returns the list of opportunities with items</response>
        /// <response code="500">If there was an internal server error</response>
    }
}
