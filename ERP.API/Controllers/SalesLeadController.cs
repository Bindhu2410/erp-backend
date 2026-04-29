
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
using System.Security.Claims;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SalesLeadController : ControllerBase
    {
        private readonly SalesLeadService _salesLeadService;
        private readonly SalesSummaryService _summaryService;
        private readonly ILogger<SalesLeadController> _logger;
        private readonly SalesQuotationService _salesQuotationService;
        private readonly IAssignedToDropdownService _assignedToDropdownService;

        public SalesLeadController(
            SalesLeadService salesLeadService,
            SalesSummaryService summaryService,
            ILogger<SalesLeadController> logger,
            SalesQuotationService salesQuotationService,
            IAssignedToDropdownService assignedToDropdownService)
        {
            _salesLeadService = salesLeadService;
            _summaryService = summaryService;
            _logger = logger;
            _salesQuotationService = salesQuotationService;
            _assignedToDropdownService = assignedToDropdownService;
        }
        /// <summary>
        /// Get assigned_to dropdown options based on team hierarchy and role
        /// </summary>
        /// <param name="userId">Current user id</param>
        /// <returns>List of users for assigned_to dropdown</returns>
        [HttpGet("assigned-to-dropdown")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(IEnumerable<AssignedToDropdownDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
public async Task<ActionResult<IEnumerable<AssignedToDropdownDto>>> GetAssignedToDropdown([FromQuery] int userId)
        {
            var result = await _assignedToDropdownService.GetAssignedToDropdownAsync(userId);
            return Ok(result);
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SalesLead>>> GetAll()
        {
            var leads = await _salesLeadService.GetAllAsync();
            // Exclude leads with status 'Converted'
            var filteredLeads = leads?.Where(l => !string.Equals(l.Status, "Converted", StringComparison.OrdinalIgnoreCase)).ToList();
            return Ok(filteredLeads);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SalesLead>> GetById(int id)
        {
            var lead = await _salesLeadService.GetByIdAsync(id);
            if (lead == null)
                return NotFound($"Lead with ID {id} not found");

            // Remove automatic status update to Converted
            return Ok(lead);
        }

        /// <summary>
        /// Creates a new sales lead
        /// </summary>
        /// <param name="lead">The lead information in JSON format</param>
        /// <returns>The ID of the created lead</returns>
        /// <response code="201">Returns the ID of the created lead</response>
        /// <response code="400">If the request data is invalid</response>
        [HttpPost]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] SalesLead lead)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        message = "Invalid model state",
                        statusCode = 400,
                        errors = ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                    });
                }                // Validate required fields
                var validationErrors = new List<string>();

                var website = lead.Website?.Trim();
                if (!string.IsNullOrWhiteSpace(website))
                {
                    if (!website.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && 
                        !website.StartsWith("https://", StringComparison.OrdinalIgnoreCase) && 
                        !website.StartsWith("ftp://", StringComparison.OrdinalIgnoreCase))
                    {
                        website = "http://" + website;
                    }
                    
                    if (!Uri.IsWellFormedUriString(website, UriKind.Absolute))
                    {
                        validationErrors.Add("The Website field must be a valid URL");
                    }
                    else
                    {
                        lead.Website = website;
                    }
                }

                if (validationErrors.Any())
                {
                    return BadRequest(new
                    {
                        message = "Validation failed",
                        statusCode = 400,
                        errors = validationErrors
                    });
                }                // Set default values
                lead.IsActive = lead.IsActive ?? true;
                lead.DateCreated = DateTime.UtcNow;
                lead.DateUpdated = DateTime.UtcNow;
                string CleanString(string? value)
                {
                    if (string.IsNullOrWhiteSpace(value) || value.Trim().ToLower() == "string")
                        return null;
                    return value.Trim();
                }

                // Sanitize input fields and convert empty strings and "string" to null
                lead.CustomerName = CleanString(lead.CustomerName) ?? string.Empty; // Required field
                lead.LeadSource = CleanString(lead.LeadSource);
                lead.ReferralSourceName = CleanString(lead.ReferralSourceName);
                lead.HospitalOfReferral = CleanString(lead.HospitalOfReferral);
                lead.DepartmentOfReferral = CleanString(lead.DepartmentOfReferral);
                lead.SocialMedia = CleanString(lead.SocialMedia);
                lead.EventName = CleanString(lead.EventName);
                lead.Score = CleanString(lead.Score);
                lead.Comments = CleanString(lead.Comments?.Replace("//", ""));
                lead.LeadType = CleanString(lead.LeadType?.Replace("//", ""));
                lead.ContactName = CleanString(lead.ContactName?.Replace("//", ""));
                lead.Salutation = CleanString(lead.Salutation);
                lead.ContactMobileNo = CleanString(lead.ContactMobileNo);
                lead.LandLineNo = CleanString(lead.LandLineNo);
                lead.Email = CleanString(lead.Email);
                lead.Fax = CleanString(lead.Fax);
                lead.DoorNo = CleanString(lead.DoorNo);
                lead.Street = CleanString(lead.Street);
                lead.Landmark = CleanString(lead.Landmark);
                lead.Territory = CleanString(lead.Territory);
                lead.Area = CleanString(lead.Area);
                lead.City = CleanString(lead.City);
                lead.Pincode = CleanString(lead.Pincode);
                lead.District = CleanString(lead.District);
                lead.State = CleanString(lead.State);
                lead.LeadSource = lead.LeadSource?.Trim() ?? string.Empty;
                lead.Comments = lead.Comments?.Replace("//", "")?.Trim();
                lead.LeadType = lead.LeadType?.Replace("//", "")?.Trim();
                lead.ContactName = lead.ContactName?.Replace("//", "")?.Trim();
                // Handle territory as a string value
                lead.Territory = lead.Territory?.Trim();
                // AssignedTo is optional, but sanitize if present
                if (lead.AssignedTo.HasValue && lead.AssignedTo.Value <= 0)
                {
                    lead.AssignedTo = null;
                }

                // Check for duplicate lead by ContactMobileNo or Email
                if (!string.IsNullOrEmpty(lead.ContactMobileNo) || !string.IsNullOrEmpty(lead.Email))
                {
                    // Check for duplicate email
                    if (!string.IsNullOrEmpty(lead.Email))
                    {
                        bool isDuplicateEmail = await _salesLeadService.IsDuplicateLeadAsync(null, lead.Email);
                        if (isDuplicateEmail)
                        {
                            return BadRequest(new
                            {
                                message = "A lead with the same email already exists.",
                                statusCode = 400
                            });
                        }
                    }
                    // Check for duplicate mobile number
                    if (!string.IsNullOrEmpty(lead.ContactMobileNo))
                    {
                        bool isDuplicateMobile = await _salesLeadService.IsDuplicateLeadAsync(lead.ContactMobileNo, null);
                        if (isDuplicateMobile)
                        {
                            return BadRequest(new
                            {
                                message = "A lead with the same mobile number already exists.",
                                statusCode = 400
                            });
                        }
                    }
                }

                // Create the lead
                var id = await _salesLeadService.CreateAsync(lead);

                // Update with generated ID
                string formattedId = $"LD{id:D5}";
                lead.LeadId = formattedId;
                lead.Id = id;
                await _salesLeadService.UpdateAsync(lead);

                // Create summary entry
                var summary = new SalesSummary
                {
                    Title = "Lead created",
                    Description = $"New lead {lead.LeadId} created for {lead.CustomerName}",
                    DateTime = DateTime.UtcNow,
                    Stage = "lead",
                    StageItemId = id.ToString(),
                    IsActive = true,
                    Entities = System.Text.Json.JsonSerializer.Serialize(new { LeadId = id, lead.CustomerName })
                };
                await _summaryService.CreateAsync(summary);

                // Return leadId in the requested format
                return Ok(new { id, leadId = formattedId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "An error occurred while creating the lead",
                    statusCode = 500,
                    errors = new[] { ex.Message }
                });
            }
        }        [HttpPut("{id}")]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<SalesLeadDto>> Update([FromRoute] int id, [FromBody] UpdateSalesLeadDto dto)
        {
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

                // Get existing lead
                var lead = await _salesLeadService.GetByIdAsync(id);
                if (lead == null)
                {
                    return NotFound(new
                    {
                        message = $"Sales Lead with ID {id} not found",
                        statusCode = 404,
                        errors = new[] { $"Sales Lead with ID {id} not found" }
                    });
                }

                // Update database fields with new values if they are provided
                lead.UserCreated = dto.UserCreated ?? lead.UserCreated;
                lead.DateCreated = dto.DateCreated ?? lead.DateCreated;
                lead.UserUpdated = dto.UserUpdated ?? lead.UserUpdated;
                lead.DateUpdated = DateTime.UtcNow;
                // Helper function to clean string values
                string CleanString(string? value, string? existingValue = null, bool keepExisting = false)
                {
                    if (string.IsNullOrWhiteSpace(value) || value.Trim().ToLower() == "string")
                        return keepExisting ? existingValue : null;
                    return value.Trim();
                }

                // Update database fields with new values, converting empty strings and "string" to null
                lead.CustomerName = CleanString(dto.CustomerName, lead.CustomerName) ?? string.Empty; // Required field
                lead.LeadSource = CleanString(dto.LeadSource);
                lead.ReferralSourceName = CleanString(dto.ReferralSourceName);
                lead.HospitalOfReferral = CleanString(dto.HospitalOfReferral);
                lead.DepartmentOfReferral = CleanString(dto.DepartmentOfReferral);
                lead.SocialMedia = CleanString(dto.SocialMedia);
                lead.EventDate = dto.EventDate;
                lead.EventName = CleanString(dto.EventName);
                lead.LeadId = CleanString(dto.LeadId, lead.LeadId, true);  // Keep existing LeadId if empty
                lead.Status = CleanString(dto.Status, lead.Status, true);   // Keep existing Status if empty
                lead.Score = CleanString(dto.Score);
                lead.IsActive = dto.IsActive;
                lead.Comments = CleanString(dto.Comments?.Replace("//", ""));
                lead.LeadType = CleanString(dto.LeadType?.Replace("//", ""));
                lead.ContactName = CleanString(dto.ContactName?.Replace("//", ""));
                lead.Salutation = CleanString(dto.Salutation);
                lead.ContactMobileNo = CleanString(dto.ContactMobileNo);
                lead.LandLineNo = CleanString(dto.LandLineNo);
                lead.Email = CleanString(dto.Email);
                lead.Fax = CleanString(dto.Fax);
                lead.DoorNo = CleanString(dto.DoorNo);
                lead.Street = CleanString(dto.Street);
                lead.Landmark = CleanString(dto.Landmark);
                lead.Website = CleanString(dto.Website);
                lead.Territory = CleanString(dto.Territory);
                lead.Area = CleanString(dto.Area);
                lead.City = CleanString(dto.City);
                lead.Pincode = CleanString(dto.Pincode);
                lead.District = CleanString(dto.District);
                lead.State = CleanString(dto.State);
                // AssignedTo
                lead.AssignedTo = dto.AssignedTo ?? lead.AssignedTo;
                var website = lead.Website?.Trim();
                if (!string.IsNullOrWhiteSpace(website))
                {
                    if (!website.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && 
                        !website.StartsWith("https://", StringComparison.OrdinalIgnoreCase) && 
                        !website.StartsWith("ftp://", StringComparison.OrdinalIgnoreCase))
                    {
                        website = "http://" + website;
                    }
                    
                    if (!Uri.IsWellFormedUriString(website, UriKind.Absolute))
                    {
                        return BadRequest(new
                        {
                            message = "Validation failed",
                            statusCode = 400,
                            errors = new[] { "The Website field must be a valid URL" }
                        });
                    }
                    
                    lead.Website = website;
                }

                // Validate door number
                if (!string.IsNullOrEmpty(lead.DoorNo) && lead.DoorNo.Length > 5)
                {
                    return BadRequest(new
                    {
                        message = "Validation failed",
                        statusCode = 400,
                        errors = new[] { "Door number cannot exceed 5 characters" }
                    });
                }

                // Set status if provided, allow 'New' as a valid value
                var cleanedStatus = CleanString(dto.Status, lead.Status, true);
                lead.Status = string.IsNullOrWhiteSpace(cleanedStatus) || cleanedStatus.ToLower() == "string" ? lead.Status : cleanedStatus;

                // Update the lead
                var success = await _salesLeadService.UpdateAsync(lead);
                if (!success)
                {
                    return StatusCode(500, new
                    {
                        message = $"Failed to update sales lead {id}",
                        statusCode = 500,
                        errors = new[] { "Database update operation failed" }
                    });
                }

                // Create summary entry for update
                var summary = new SalesSummary
                {
                    Title = $"Lead updated",
                    Description = $"Lead information updated for {lead.CustomerName}",
                    DateTime = DateTime.UtcNow,
                    Stage = "lead",
                    StageItemId = id.ToString(),
                    IsActive = true,
                    Entities = System.Text.Json.JsonSerializer.Serialize(new { LeadId = id, lead.CustomerName })
                };
                await _summaryService.CreateAsync(summary);

                // Return updated lead info as DTO
                return Ok(ConvertToDto(lead));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = $"Failed to update sales lead {id}",
                    statusCode = 500,
                    errors = new[] { ex.Message }
                });
            }
        }        private SalesLeadDto ConvertToDto(SalesLead? lead)
        {
            if (lead == null)
            {
                throw new ArgumentNullException(nameof(lead), "Lead entity cannot be null");
            }
            return new SalesLeadDto
            {
                Id = lead.Id ?? 0,
                UserCreated = lead.UserCreated ?? 0,
                DateCreated = lead.DateCreated,
                UserUpdated = lead.UserUpdated ?? 0,
                DateUpdated = lead.DateUpdated,
                CustomerName = lead.CustomerName ?? string.Empty,
                LeadSource = lead.LeadSource ?? string.Empty,
                ReferralSourceName = lead.ReferralSourceName ?? string.Empty,
                HospitalOfReferral = lead.HospitalOfReferral ?? string.Empty,
                DepartmentOfReferral = lead.DepartmentOfReferral ?? string.Empty,
                SocialMedia = lead.SocialMedia ?? string.Empty,
                EventDate = lead.EventDate,
                EventName = lead.EventName ?? string.Empty,
                LeadId = lead.LeadId ?? string.Empty,
                Status = lead.Status ?? string.Empty,
                Score = lead.Score ?? string.Empty,
                IsActive = lead.IsActive ?? true,
                Comments = lead.Comments ?? string.Empty,
                LeadType = lead.LeadType ?? string.Empty,
                Territory = lead.Territory ?? string.Empty,
                Area = lead.Area ?? string.Empty,
                City = lead.City ?? string.Empty,
                District = lead.District ?? string.Empty,
                State = lead.State ?? string.Empty,
                Pincode = lead.Pincode ?? string.Empty,
                AssignedTo = lead.AssignedTo
            };
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var lead = await _salesLeadService.GetByIdAsync(id);
            if (lead == null)
                return NotFound($"Lead with ID {id} not found");

            await _salesLeadService.DeleteAsync(id);

            // Create summary entry for deletion
            var summary = new SalesSummary
            {
                Title = $"Lead deleted - {lead.CustomerName}",
                Description = $"Lead deleted for {lead.CustomerName}",
                DateTime = DateTime.UtcNow,
                Stage = "lead",
                StageItemId = id.ToString(),
                IsActive = true,
                Entities = System.Text.Json.JsonSerializer.Serialize(new { LeadId = id, lead.CustomerName })
            };
            await _summaryService.CreateAsync(summary);

            return NoContent();
        }

        [HttpPost("grid")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<(IEnumerable<SalesLeadGridResult> Results, int TotalRecords)>> GetSalesLeadsGrid([FromBody] SalesLeadGridRequest request)
        {
            // Enforce default sorting: latest leads first by id
            if (string.IsNullOrWhiteSpace(request.OrderBy))
                request.OrderBy = "id";
            if (string.IsNullOrWhiteSpace(request.OrderDirection))
                request.OrderDirection = "DESC";
            else
                request.OrderDirection = request.OrderDirection.ToUpper() == "ASC" ? "ASC" : "DESC";

            _logger.LogInformation($"OrderDirection received: {request.OrderDirection}");
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

                // Get current user ID for filtering
                var currentUserId = GetCurrentUserId();
                
                // Use UserCreated from request body if provided, otherwise fall back to current user
                var targetUserId = request.UserCreated ?? currentUserId;
                
                // If no specific user ID is provided and no authentication, use non-user-specific method
                if (targetUserId == 0)
                {
                    var result = await _salesLeadService.GetSalesLeadsGridAsync(
                        request.SearchText, // searchText
                        null, // zones
                        request.CustomerNames, // customerNames
                        null, // territories
                        request.Statuses, // statuses
                        request.Scores, // scores
                        request.LeadTypes, // leadTypes
                        request.PageNumber, // pageNumber
                        request.PageSize, // pageSize
                        request.OrderBy, // orderBy
                        request.OrderDirection, // orderDirection
                        request.SelectedLeadIds?.ToArray() // selectedLeadIds
                    );

                    // Ensure unique records in the grid API response, preserving order
                    var uniqueResults = result.Results
                        .GroupBy(r => r.Id)
                        .Select(g => g.First())
                        .OrderByDescending(r => r.Id)
                        .ToList();
                    return Ok(new {
                        Results = uniqueResults,
                        result.TotalRecords,
                        request.PageNumber,
                        request.PageSize
                    });
                }
                else
                {
                    var result = await _salesLeadService.GetSalesLeadsGridByUserAsync(
                        targetUserId, // Filter by specified user or current user
                        request.SearchText, // searchText
                        null, // zones
                        request.CustomerNames, // customerNames
                        null, // territories
                        request.Statuses, // statuses
                        request.Scores, // scores
                        request.LeadTypes, // leadTypes
                        request.PageNumber, // pageNumber
                        request.PageSize, // pageSize
                        request.OrderBy, // orderBy
                        request.OrderDirection, // orderDirection
                        request.SelectedLeadIds?.ToArray() // selectedLeadIds
                    );

                    // Ensure unique records in the grid API response, preserving order
                    var uniqueResults = result.Results
                        .GroupBy(r => r.Id)
                        .Select(g => g.First())
                        .OrderByDescending(r => r.Id)
                        .ToList();
                    return Ok(new {
                        Results = uniqueResults,
                        result.TotalRecords,
                        request.PageNumber,
                        request.PageSize
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "An error occurred while retrieving leads",
                    statusCode = 500,
                    errors = new[] { ex.Message }
                });
            }
        }
        [HttpPost("dropdown")]
        public async Task<ActionResult<(IEnumerable<LeadsDropdownResult> Results, int TotalRecords)>> GetLeadsDropdown([FromBody] LeadsDropdownRequest request)
        {
            try
            {
                if (request.PageNumber <= 0)
                {
                    return BadRequest("Page number must be greater than 0");
                }

                if (request.PageSize <= 0)
                {
                    return BadRequest("Page size must be greater than 0");
                }

                var result = await _salesLeadService.GetLeadsDropdownAsync(
                    request.SearchText,
                    request.PageNumber,
                    request.PageSize);

                return Ok(new { result.Results, result.TotalRecords });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        // [HttpGet("details/{id}")]
        // public async Task<ActionResult<SalesLeadDetails>> GetLeadDetailsById(int id)
        // {
        //     var leadDetails = await _salesLeadService.GetLeadDetailsByIdAsync(id);
        //     if (leadDetails == null)
        //         return NotFound($"Lead details with ID {id} not found");

        //     return Ok(leadDetails);
        // }

        [HttpPost("export/excel")]
        public async Task<IActionResult> ExportToExcel([FromBody] SalesLeadGridRequest request)
        {
            try
            {
                var result = await _salesLeadService.GetSalesLeadsGridAsync(
                    request.SearchText, // searchText
                    null, // zones
                    request.CustomerNames, // customerNames
                    null, // territories
                    request.Statuses, // statuses
                    request.Scores, // scores
                    request.LeadTypes, // leadTypes
                    request.PageNumber, // pageNumber
                    request.PageSize, // pageSize
                    request.OrderBy, // orderBy
                    request.OrderDirection, // orderDirection
                    request.SelectedLeadIds?.ToArray() // selectedLeadIds
                );

                // Filter by selected Lead IDs if provided
                if (request.SelectedLeadIds != null && request.SelectedLeadIds.Any())
                {
                    result.Results = result.Results.Where(r => r.LeadId != null && request.SelectedLeadIds.Contains(r.LeadId)).ToList();
                }

                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("SalesLeads");

                // Define headers
                var headers = new[] { "Lead ID", "Customer Name", "Lead Source", "Status", "Score", "Lead Type",
                    "Contact Name", "Contact Mobile", "Email", "Territory", "City", "State", "Date Created" };
                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cell(1, i + 1).Value = headers[i];
                    worksheet.Cell(1, i + 1).Style.Font.Bold = true;
                }

                // Populate data
                int row = 2;
                foreach (var lead in result.Results)
                {
                    worksheet.Cell(row, 1).Value = lead.LeadId ?? "";
                    worksheet.Cell(row, 2).Value = lead.CustomerName ?? "";
                    worksheet.Cell(row, 3).Value = lead.LeadSource ?? "";
                    worksheet.Cell(row, 4).Value = lead.Status ?? "";
                    worksheet.Cell(row, 5).Value = lead.Score ?? "";
                    worksheet.Cell(row, 6).Value = lead.LeadType ?? "";
                    worksheet.Cell(row, 7).Value = lead.ContactName ?? "";
                    worksheet.Cell(row, 8).Value = lead.ContactMobileNo ?? "";
                    worksheet.Cell(row, 9).Value = lead.Email ?? "";
                    worksheet.Cell(row, 10).Value = lead.Territory ?? "";
                    worksheet.Cell(row, 11).Value = lead.City ?? "";
                    worksheet.Cell(row, 12).Value = lead.State ?? "";
                    worksheet.Cell(row, 13).Value = lead.DateCreated?.ToString("yyyy-MM-dd") ?? "";
                    row++;
                }

                // Auto-fit columns
                worksheet.Columns().AdjustToContents();

                // Convert to byte array
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Position = 0;
                    string fileName = $"SalesLeads_{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx";
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error generating Excel file: {ex.Message}");
            }
        }

        /// <summary>
        /// Bulk create sales leads (array input). Rejects leads with duplicate mobile or email (within array or DB).
        /// </summary>
        /// <param name="leads">Array of SalesLead objects</param>
        /// <returns>Array of results for each lead</returns>
        [HttpPost("bulk")]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> BulkCreate([FromBody] List<SalesLead> leads)
        {
            if (leads == null || !leads.Any())
                return BadRequest(new { message = "No leads provided", statusCode = 400 });

            // Track duplicates within the array
            var mobileSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var emailSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var results = new List<object>();

            foreach (var lead in leads)
            {
                var validationErrors = new List<string>();

                // Clean and check website
                var website = lead.Website?.Trim();
                if (!string.IsNullOrWhiteSpace(website))
                {
                    if (!website.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                        !website.StartsWith("https://", StringComparison.OrdinalIgnoreCase) &&
                        !website.StartsWith("ftp://", StringComparison.OrdinalIgnoreCase))
                    {
                        website = "http://" + website;
                    }
                    if (!Uri.IsWellFormedUriString(website, UriKind.Absolute))
                        validationErrors.Add("The Website field must be a valid URL");
                    else
                        lead.Website = website;
                }

                // Clean input fields
                string CleanString(string? value)
                {
                    if (string.IsNullOrWhiteSpace(value) || value.Trim().ToLower() == "string")
                        return null;
                    return value.Trim();
                }
                lead.CustomerName = CleanString(lead.CustomerName) ?? string.Empty;
                lead.LeadSource = CleanString(lead.LeadSource);
                lead.ReferralSourceName = CleanString(lead.ReferralSourceName);
                lead.HospitalOfReferral = CleanString(lead.HospitalOfReferral);
                lead.DepartmentOfReferral = CleanString(lead.DepartmentOfReferral);
                lead.SocialMedia = CleanString(lead.SocialMedia);
                lead.EventName = CleanString(lead.EventName);
                lead.Score = CleanString(lead.Score);
                lead.Comments = CleanString(lead.Comments?.Replace("//", ""));
                lead.LeadType = CleanString(lead.LeadType?.Replace("//", ""));
                lead.ContactName = CleanString(lead.ContactName?.Replace("//", ""));
                lead.Salutation = CleanString(lead.Salutation);
                lead.ContactMobileNo = CleanString(lead.ContactMobileNo);
                lead.LandLineNo = CleanString(lead.LandLineNo);
                lead.Email = CleanString(lead.Email);
                lead.Fax = CleanString(lead.Fax);
                lead.DoorNo = CleanString(lead.DoorNo);
                lead.Street = CleanString(lead.Street);
                lead.Landmark = CleanString(lead.Landmark);
                lead.Territory = CleanString(lead.Territory);
                lead.Area = CleanString(lead.Area);
                lead.City = CleanString(lead.City);
                lead.Pincode = CleanString(lead.Pincode);
                lead.District = CleanString(lead.District);
                lead.State = CleanString(lead.State);

                // Set default values
                lead.IsActive = lead.IsActive ?? true;
                lead.DateCreated = DateTime.UtcNow;
                lead.DateUpdated = DateTime.UtcNow;

                // Check for duplicate mobile/email within the array
                var mobile = lead.ContactMobileNo ?? string.Empty;
                var email = lead.Email ?? string.Empty;
                bool duplicateInArray = false;
                if (!string.IsNullOrEmpty(mobile))
                {
                    if (!mobileSet.Add(mobile))
                    {
                        validationErrors.Add($"Duplicate mobile number in request: {mobile}");
                        duplicateInArray = true;
                    }
                }
                if (!string.IsNullOrEmpty(email))
                {
                    if (!emailSet.Add(email))
                    {
                        validationErrors.Add($"Duplicate email in request: {email}");
                        duplicateInArray = true;
                    }
                }

                // Check for duplicate in DB (only if not already duplicate in array)
                if (!duplicateInArray && (!string.IsNullOrEmpty(mobile) || !string.IsNullOrEmpty(email)))
                {
                    if (!string.IsNullOrEmpty(email))
                    {
                        bool isDuplicateEmail = await _salesLeadService.IsDuplicateLeadAsync(null, email);
                        if (isDuplicateEmail)
                            validationErrors.Add($"A lead with the same email already exists: {email}");
                    }
                    if (!string.IsNullOrEmpty(mobile))
                    {
                        bool isDuplicateMobile = await _salesLeadService.IsDuplicateLeadAsync(mobile, null);
                        if (isDuplicateMobile)
                            validationErrors.Add($"A lead with the same mobile number already exists: {mobile}");
                    }
                }

                if (validationErrors.Any())
                {
                    results.Add(new {
                        lead = lead,
                        status = "rejected",
                        errors = validationErrors
                    });
                    continue;
                }

                // Create the lead
                try
                {
                    var id = await _salesLeadService.CreateAsync(lead);
                    string formattedId = $"LD{id:D5}";
                    lead.LeadId = formattedId;
                    lead.Id = id;
                    await _salesLeadService.UpdateAsync(lead);

                    // Create summary entry
                    var summary = new SalesSummary
                    {
                        Title = "Lead created (bulk)",
                        Description = $"New lead {lead.LeadId} created for {lead.CustomerName}",
                        DateTime = DateTime.UtcNow,
                        Stage = "lead",
                        StageItemId = id.ToString(),
                        IsActive = true,
                        Entities = System.Text.Json.JsonSerializer.Serialize(new { LeadId = id, lead.CustomerName })
                    };
                    await _summaryService.CreateAsync(summary);

                    results.Add(new {
                        leadId = formattedId,
                        id = id,
                        status = "created"
                    });
                }
                catch (Exception ex)
                {
                    // Log full exception server-side for debugging (avoid leaking stack traces to clients)
                    _logger.LogError(ex, "BulkCreate error for lead {@Lead}", lead);
                    results.Add(new {
                        lead = lead,
                        status = "error",
                        errors = new[] { ex.Message }
                    });
                }
            }

            return Ok(results);
        }

//         // New endpoint for PDF export
//         [HttpPost("export/pdf")]
//         public async Task<IActionResult> ExportToPdf([FromBody] SalesLeadGridRequest request)
//         {
//             MemoryStream? stream = null;
//             PdfWriter? writer = null;
//             PdfDocument? pdf = null;
//             Document? document = null;

//             try
//             {
//                 var result = await _salesLeadService.GetSalesLeadsGridAsync(
//                     request.SearchText, // searchText
//                     null, // zones
//                     request.CustomerNames, // customerNames
//                     null, // territories
//                     request.Statuses, // statuses
//                     request.Scores, // scores
//                     request.LeadTypes, // leadTypes
//                     request.PageNumber, // pageNumber
//                     request.PageSize, // pageSize
//                     request.OrderBy, // orderBy
//                     request.OrderDirection, // orderDirection
//                     request.SelectedLeadIds?.ToArray() // selectedLeadIds
//                 );

//                 stream = new MemoryStream();

//                 var writerProperties = new WriterProperties()
//                     .SetPdfVersion(PdfVersion.PDF_2_0);
//                 writer = new PdfWriter(stream, writerProperties);
//                 pdf = new PdfDocument(writer);
//                 document = new Document(pdf, iText.Kernel.Geom.PageSize.A4.Rotate());
//                 document.SetMargins(20, 20, 20, 20);

//                 var normalFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
//                 var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

//                 document.Add(new Paragraph("Sales Leads Report")
//                     .SetFont(boldFont)
//                     .SetFontSize(16)
//                     .SetTextAlignment(TextAlignment.CENTER)
//                     .SetMarginBottom(20));

//                 document.Add(new Paragraph($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}")
//                     .SetFont(normalFont)
//                     .SetFontSize(10)
//                     .SetTextAlignment(TextAlignment.RIGHT)
//                     .SetMarginBottom(20));

//                 var table = new Table(new float[] { 1, 2, 1.5f, 1, 1, 1, 1.5f, 1.5f })
//                     .UseAllAvailableWidth()
//                     .SetFixedLayout();

//                 var headers = new[] { "Lead ID", "Customer Name", "Lead Source", "Status", "Score", "Lead Type", "Contact Name", "Contact Mobile" };
//                 foreach (var header in headers)
//                 {
//                     table.AddHeaderCell(
//                         new Cell()
//                             .Add(new Paragraph(header).SetFont(boldFont))
//                             .SetBackgroundColor(iText.Kernel.Colors.ColorConstants.LIGHT_GRAY)
//                             .SetTextAlignment(TextAlignment.CENTER)
//                             .SetPadding(5)
//                     );
//                 }

//                 foreach (var lead in result.Results)
//                 {
//                     table.AddCell(new Cell().Add(new Paragraph(lead.LeadId ?? "").SetFont(normalFont)).SetPadding(5));
//                     table.AddCell(new Cell().Add(new Paragraph(lead.CustomerName ?? "").SetFont(normalFont)).SetPadding(5));
//                     table.AddCell(new Cell().Add(new Paragraph(lead.LeadSource ?? "").SetFont(normalFont)).SetPadding(5));
//                     table.AddCell(new Cell().Add(new Paragraph(lead.Status ?? "").SetFont(normalFont)).SetPadding(5));
//                     table.AddCell(new Cell().Add(new Paragraph(lead.Score ?? "").SetFont(normalFont)).SetPadding(5));
//                     table.AddCell(new Cell().Add(new Paragraph(lead.LeadType ?? "").SetFont(normalFont)).SetPadding(5));
//                     table.AddCell(new Cell().Add(new Paragraph(lead.ContactName ?? "").SetFont(normalFont)).SetPadding(5));
//                     table.AddCell(new Cell().Add(new Paragraph(lead.ContactMobileNo ?? "").SetFont(normalFont)).SetPadding(5));
//                 }

//                 document.Add(table);

//                 int numberOfPages = pdf.GetNumberOfPages();
//                 for (int i = 1; i <= numberOfPages; i++)
//                 {
//                     document.ShowTextAligned(
//                         new Paragraph(String.Format("Page {0} of {1}", i, numberOfPages)).SetFont(normalFont),
//                         559, 20, i, TextAlignment.RIGHT, VerticalAlignment.BOTTOM, 0);
//                 }

//                 document.Close();
//                 pdf.Close();
//                 writer.Close();

//                 stream.Position = 0;
//                 string fileName = $"SalesLeads_{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";
//                 return File(stream.ToArray(), "application/pdf", fileName);
//             }
//             catch (Exception ex)
//             {
//                 return StatusCode(500, $"Error generating PDF file: {ex.Message}. Stack trace: {ex.StackTrace}");
//             }
//             finally
//             {
//                 try
//                 {
//                     document?.Close();
//                     pdf?.Close();
//                     writer?.Close();
//                     stream?.Dispose();
//                 }
//                 catch
//                 {
//                     // Suppress any errors during cleanup
//                 }
//             }
//         }

//         [HttpPost("export/single-lead-pdf/{id}")]
//         public async Task<IActionResult> ExportSingleLeadToPdf(int id)
//         {
//             MemoryStream? stream = null;
//             PdfWriter? writer = null;
//             PdfDocument? pdf = null;
//             Document? document = null;

//             try
//             {
//                 var lead = await _salesLeadService.GetLeadDetailsByIdAsync(id);
//                 if (lead == null)
//                     return NotFound($"Lead with ID {id} not found");

//                 stream = new MemoryStream();

//                 var writerProperties = new WriterProperties()
//                     .SetPdfVersion(PdfVersion.PDF_2_0);

//                 writer = new PdfWriter(stream, writerProperties);
//                 pdf = new PdfDocument(writer);
//                 document = new Document(pdf, iText.Kernel.Geom.PageSize.A4);
//                 document.SetMargins(20, 20, 20, 20);

//                 document.SetMargins(20, 20, 20, 20);

//                 var normalFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
//                 var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

//                 document.Add(new Paragraph($"Lead Details - {lead.LeadId ?? "N/A"}")
//                     .SetFont(boldFont)
//                     .SetFontSize(16)
//                     .SetTextAlignment(TextAlignment.CENTER)
//                     .SetMarginBottom(20));

//                 document.Add(new Paragraph($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}")
//                     .SetFont(normalFont)
//                     .SetFontSize(10)
//                     .SetTextAlignment(TextAlignment.RIGHT)
//                     .SetMarginBottom(20));

// var detailsTable = new Table(2)
//     .UseAllAvailableWidth()
//     .SetWidth(UnitValue.CreatePercentValue(100));

//                 void AddDetailRow(string label, string? value)
//                 {
//                     detailsTable.AddCell(new Cell().Add(new Paragraph(label).SetFont(boldFont)).SetPadding(5));
//                     detailsTable.AddCell(new Cell().Add(new Paragraph(value ?? "N/A").SetFont(normalFont)).SetPadding(5));
//                 }

//                 AddDetailRow("Lead ID", lead.LeadId);
//                 AddDetailRow("Customer Name", lead.CustomerName);
//                 AddDetailRow("Lead Source", lead.LeadSource);
//                 AddDetailRow("Status", lead.Status);
//                 AddDetailRow("Score", lead.Score);
//                 AddDetailRow("Lead Type", lead.LeadType);
//                 AddDetailRow("Contact Name", lead.ContactName);
//                 AddDetailRow("Contact Mobile", lead.ContactMobileNo);
//                 AddDetailRow("Email", lead.Email);
//                 AddDetailRow("Territory", lead.Territory);
//                 AddDetailRow("City", lead.City);
//                 AddDetailRow("State", lead.State);
//                 AddDetailRow("Date Created", lead.DateCreated?.ToString("yyyy-MM-dd HH:mm:ss"));

//                 document.Add(detailsTable);

//                 if (document != null)
//                 {
//                     document.Close();
//                 }
//                 if (pdf != null)
//                 {
//                     pdf.Close();
//                 }
//                 if (writer != null)
//                 {
//                     writer.Close();
//                 }

//                 stream.Position = 0;
//                 string fileName = $"Lead_{lead.LeadId ?? id.ToString()}_{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";
//                 return File(stream.ToArray(), "application/pdf", fileName);
//             }
//             catch (Exception ex)
//             {
//                 return StatusCode(500, $"Error generating PDF file: {ex.Message}. Stack trace: {ex.StackTrace}");
//             }
//             finally
//             {
//                 try
//                 {
//                     if (document != null)
//                     {
//                         document.Close();
//                     }
//                     if (pdf != null)
//                     {
//                         pdf.Close();
//                     }
//                     if (writer != null)
//                     {
//                         writer.Close();
//                     }
//                     stream?.Dispose();
//                 }
//                 catch
//                 {
//                     // Suppress any errors during cleanup
//                 }
//             }
//         }

//         [HttpPost("export/single-lead-pdf/by-lead-id/{leadId}")]
//         public async Task<IActionResult> ExportSingleLeadToPdfByLeadId(string leadId)
//         {
//             try
//             {
//                 var lead = await _salesLeadService.GetLeadDetailsByLeadIdAsync(leadId);
//                 if (lead == null)
//                     return NotFound($"Lead with ID {leadId} not found");

//                 using (var memoryStream = new MemoryStream())
//                 {
//                     using (var pdfDoc = new PdfDocument(new PdfWriter(memoryStream)))
//                     {
//                         var document = new Document(pdfDoc);
//                         document.SetMargins(20, 20, 20, 20);

//                         var normalFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
//                         var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

//                         // Title - Lead Profile
//                         document.Add(new Paragraph("LEAD PROFILE")
//                             .SetFont(boldFont)
//                             .SetFontSize(20)
//                             .SetFontColor(new DeviceRgb(0, 51, 102))
//                             .SetTextAlignment(TextAlignment.CENTER)
//                             .SetMarginBottom(20));

//                         // Add timestamp
//                         document.Add(new Paragraph($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}")
//                             .SetFont(normalFont)
//                             .SetFontSize(8)
//                             .SetTextAlignment(TextAlignment.RIGHT)
//                             .SetMarginBottom(30));

//                         // Basic Information Section
//                         AddSectionHeader(document, "Basic Information", boldFont);
//                         var basicInfoTable = new Table(2)
//                             .UseAllAvailableWidth()
//                             .SetWidth(UnitValue.CreatePercentValue(100));
                        
//                         AddInfoRow(basicInfoTable, "Lead ID", lead.LeadId ?? "N/A", boldFont, normalFont);
//                         AddInfoRow(basicInfoTable, "Customer Name", lead.CustomerName ?? "N/A", boldFont, normalFont);
//                         AddInfoRow(basicInfoTable, "Lead Source", lead.LeadSource ?? "N/A", boldFont, normalFont);
//                         AddInfoRow(basicInfoTable, "Lead Type", lead.LeadType ?? "N/A", boldFont, normalFont);
//                         AddInfoRow(basicInfoTable, "Status", lead.Status ?? "N/A", boldFont, normalFont);
//                         AddInfoRow(basicInfoTable, "Score", lead.Score ?? "N/A", boldFont, normalFont);
//                         document.Add(basicInfoTable);

//                         // Contact Details Section
//                         AddSectionHeader(document, "Contact Details", boldFont);
//                         var contactTable = new Table(2)
//                             .UseAllAvailableWidth()
//                             .SetWidth(UnitValue.CreatePercentValue(100));

//                         AddInfoRow(contactTable, "Contact Name", lead.ContactName ?? "N/A", boldFont, normalFont);
//                         AddInfoRow(contactTable, "Mobile Number", lead.ContactMobileNo ?? "N/A", boldFont, normalFont);
//                         AddInfoRow(contactTable, "Email", lead.Email ?? "N/A", boldFont, normalFont);
//                         document.Add(contactTable);

//                         // Lead Details Section
//                         AddSectionHeader(document, "Lead Details", boldFont);
//                         var detailsTable = new Table(2)
//     .UseAllAvailableWidth();
//                         AddInfoRow(detailsTable, "Territory", lead.Territory ?? "Not Assigned", boldFont, normalFont);
//                         AddInfoRow(detailsTable, "City", lead.City ?? "Not Assigned", boldFont, normalFont);
//                         AddInfoRow(detailsTable, "State", lead.State ?? "Not Assigned", boldFont, normalFont);
//                         AddInfoRow(detailsTable, "Date Created", lead.DateCreated?.ToString("yyyy-MM-dd HH:mm:ss") ?? "N/A", boldFont, normalFont);
//                         document.Add(detailsTable);

//                         // Notes & Interactions Section
//                         AddSectionHeader(document, "Notes & Interactions", boldFont);
//                         if (lead.BusinessChallenges != null && lead.BusinessChallenges.Any())
//                         {
//                             document.Add(new Paragraph("Business Challenges")
//                                 .SetFont(boldFont)
//                                 .SetFontSize(10)
//                                 .SetMarginTop(10)
//                                 .SetMarginBottom(5));

//                             var challengesTable = new Table(2)
//                                 .UseAllAvailableWidth()
//                                 .SetWidth(UnitValue.CreatePercentValue(100));
                            
//                             foreach (var challenge in lead.BusinessChallenges)
//                             {
//                                 AddInfoRow(challengesTable, "Challenge", challenge.Challenges ?? "N/A", boldFont, normalFont);
//                                 AddInfoRow(challengesTable, "Solution", challenge.Solution ?? "N/A", boldFont, normalFont);
//                             }
//                             document.Add(challengesTable);
//                         }

//                         // Add page numbers
//                         int pages = pdfDoc.GetNumberOfPages();
//                         for (int i = 1; i <= pages; i++)
//                         {
//                             document.ShowTextAligned(
//                                 new Paragraph($"Page {i} of {pages}")
//                                     .SetFont(normalFont)
//                                     .SetFontSize(8),
//                                 PageSize.A4.GetWidth() - 40, 20, i,
//                                 TextAlignment.RIGHT, VerticalAlignment.BOTTOM, 0);
//                         }

//                         document.Close();
//                     }

//                     var pdfBytes = memoryStream.ToArray();
//                     var fileName = $"Lead_{leadId}_{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";
//                     return File(pdfBytes, "application/pdf", fileName);
//                 }
//             }
//             catch (Exception ex)
//             {
//                 return StatusCode(500, $"Error generating PDF file: {ex.Message}. Stack trace: {ex.StackTrace}");
//             }
//         }

        // New endpoint to get a sales lead by quotationId
        // [HttpGet("by-quotation/{quotationId}")]
        // public async Task<ActionResult<SalesLead>> GetByQuotationId(string quotationId)
        // {
        //     var lead = await _salesLeadService.GetByLatestQuotationIdAsync(quotationId);
        //     if (lead == null)
        //         return NotFound($"Lead with Quotation ID {quotationId} not found");
        //     return Ok(lead);
        // }

        // New endpoint to get sales lead cards summary (New, Qualified, Unqualified)

        /// <summary>
        /// Gets sales lead cards summary (New, Contacted, Qualified, Converted, Lost) for a user
        /// </summary>
        /// <param name="request">Request body with UserId</param>
        /// <returns>LeadCardsDto</returns>
        [HttpGet("cards")]
        public async Task<ActionResult<LeadCardsDto>> GetLeadCardsByUser([FromQuery] int? userId = null)
        {
            try
            {
                int resolvedUserId = (userId == null || userId <= 0) ? GetCurrentUserId() : userId.Value;
                _logger.LogInformation($"[GetLeadCardsByUser] Resolved userId: {resolvedUserId} (input: {userId})");
                if (resolvedUserId <= 0)
                {
                    _logger.LogWarning("[GetLeadCardsByUser] No valid userId provided or found in claims.");
                    return BadRequest(new { message = "UserId is required (either as query parameter or via authentication)" });
                }

                var userResult = await _salesLeadService.GetLeadCardsByUserAsync(resolvedUserId);
                if (userResult == null)
                {
                    _logger.LogWarning($"[GetLeadCardsByUser] No result returned for userId: {resolvedUserId}");
                    return NotFound(new { message = $"No lead cards found for userId {resolvedUserId}" });
                }
                return Ok(userResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[GetLeadCardsByUser] Exception for userId: {userId}");
                return StatusCode(500, new
                {
                    message = "An error occurred while retrieving lead cards",
                    statusCode = 500,
                    errors = new[] { ex.Message, ex.StackTrace }
                });
            }
        }

        // (Optional) Keep the old GET for backward compatibility
        // [HttpGet("cards")]
        // public async Task<ActionResult<LeadCardsDto>> GetLeadCards()
        // {
        //     try
        //     {
        //         var result = await _salesLeadService.GetLeadCardsAsync();
        //         return Ok(result);
        //     }
        //     catch (Exception ex)
        //     {
        //         return StatusCode(500, new
        //         {
        //             message = "An error occurred while retrieving lead cards",
        //             statusCode = 500,
        //             errors = new[] { ex.Message }
        //         });
        //     }
        // }

        /// <summary>
        /// Retrieves all sales quotations with their items for a given Sales Lead
        /// </summary>
        /// <param name="leadId">The Sales Lead ID (can be internal ID like '16' or external ID like 'LD00016')</param>
        /// <returns>List of quotations with their items</returns>
        [HttpGet("{leadId}/quotations-with-items")]
        [ProducesResponseType(typeof(IEnumerable<SalesQuotationWithItemsResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetQuotationsWithItemsByLeadId([FromRoute] string leadId)
        {
            if (string.IsNullOrWhiteSpace(leadId))
                return BadRequest(new { message = "leadId is required" });
            try
            {
                _logger.LogInformation($"Fetching quotations for leadId: '{leadId}'");
                
                // Convert internal ID to external ID format if needed
                string externalLeadId = leadId.Trim();
                
                // Check if leadId is numeric (internal ID)
                if (int.TryParse(leadId.Trim(), out int internalId))
                {
                    // Convert internal ID to external ID format (LD + 5-digit padded number)
                    externalLeadId = $"LD{internalId:D5}";
                    _logger.LogInformation($"Converted internal leadId '{leadId}' to external format '{externalLeadId}'");
                }
                
                // Use the service method that returns quotations without trying to fetch items
                var result = await _salesQuotationService.GetQuotationsWithItemsAsync();
                
                // Filter by lead ID and return only quotations that match
                var filteredResult = result.Where(q => q.Quotation?.LeadId == externalLeadId).ToList();
                
                _logger.LogInformation($"Quotations found: {filteredResult.Count}");
                
                return Ok(filteredResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching quotations with items for lead {leadId}");
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        // New endpoint to fetch the latest quotation with items for a given leadId
        // [HttpGet("{leadId}/latest-quotation-with-items")]
        // [ProducesResponseType(typeof(SalesQuotationWithItemsResponse), StatusCodes.Status200OK)]
        // public async Task<IActionResult> GetLatestQuotationWithItemsByLeadId([FromRoute] string leadId)
        // {
        //     if (string.IsNullOrWhiteSpace(leadId))
        //         return BadRequest(new { message = "leadId is required" });
        //     try
        //     {
        //         var quotation = await _salesQuotationService.GetLatestQuotationWithItemsByLeadIdAsync(leadId.Trim());
        //         if (quotation == null)
        //             return NotFound(new { message = $"No quotations found for leadId {leadId}" });
        //         return Ok(quotation);
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError(ex, $"Error fetching latest quotation with items for lead {leadId}");
        //         return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        //     }
        // }

        // Helper method to add a section header to the PDF document
        // private void AddSectionHeader(Document document, string header, iText.Kernel.Font.PdfFont boldFont)
        // {
        //     document.Add(new Paragraph(header)
        //         .SetFont(boldFont)
        //         .SetFontSize(12)
        //         .SetFontColor(new iText.Kernel.Colors.DeviceRgb(0, 51, 102))
        //         .SetMarginTop(20)
        //         .SetMarginBottom(10));
        // }

        // // Helper method to add a row to a table in the PDF document
        // private void AddInfoRow(Table table, string label, string value, iText.Kernel.Font.PdfFont boldFont, iText.Kernel.Font.PdfFont normalFont)
        // {
        //     table.AddCell(new Cell().Add(new Paragraph(label).SetFont(boldFont).SetFontSize(10)));
        //     table.AddCell(new Cell().Add(new Paragraph(value).SetFont(normalFont).SetFontSize(10)));
        // }
    }
}