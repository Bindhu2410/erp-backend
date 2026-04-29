using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using ERP.API.Models;
using ERP.API.Models.DTOs;
using ERP.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Cors;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;

namespace ERP.API.Controllers
{    
    [ApiController]
    [Route("api/[controller]")]
    [EnableCors("AllowAll")]
    [Produces("application/json")]
    [ApiExplorerSettings(IgnoreApi = false)]
    public class SalesContactController : ControllerBase
    {
        private readonly SalesContactService _salesContactService;
        private readonly SalesSummaryService _summaryService;
        private readonly SalesLeadService _salesLeadService;
        private readonly ILogger<SalesContactController> _logger;
        private readonly ISalesOpportunityService _opportunityService;
        
        public SalesContactController(
            SalesContactService salesContactService, 
            SalesSummaryService summaryService,
            SalesLeadService salesLeadService,
            ILogger<SalesContactController> logger,
            ISalesOpportunityService opportunityService)
        {
            _salesContactService = salesContactService;
            _summaryService = summaryService;
            _salesLeadService = salesLeadService;
            _logger = logger;
            _opportunityService = opportunityService;
        }
        /// <summary>
        /// Gets contact_name and mobile_no for all contacts by opportunity ID
        /// </summary>
        /// <param name="opportunityId">The opportunity ID</param>
        /// <returns>List of contact_name and mobile_no</returns>
        /// <response code="200">Returns the list of contacts</response>
        /// <response code="404">If no contacts are found for the opportunity</response>
        [HttpGet("opportunity/{opportunityId}/contacts")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Produces("application/json")]
        public async Task<ActionResult<IEnumerable<ERP.API.Models.DTOs.ContactNameMobileDto>>> GetContactsByOpportunityId(string opportunityId)
        {
            if (string.IsNullOrWhiteSpace(opportunityId))
            {
                return BadRequest(new {
                    message = "Opportunity ID is required",
                    statusCode = 400,
                    errors = new[] { "Opportunity ID is required" }
                });
            }
            try
            {
                var contacts = await _salesContactService.GetContactNameMobileByOpportunityIdAsync(opportunityId);
                return Ok(contacts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new {
                    message = "An error occurred while retrieving contacts",
                    statusCode = 500,
                    errors = new[] { ex.Message }
                });
            }
        }

        private SalesContact MapToEntity(SalesContactDto dto)
        {
            return new SalesContact
            {
                Id = dto.Id,
                ContactName = dto.ContactName,
                DepartmentName = dto.DepartmentName,
                Specialist = dto.Specialist,
                Degree = dto.Degree,
                Email = dto.Email,
                MobileNo = dto.MobileNo,
                Website = dto.Website,
                IsActive = dto.IsActive,
                OwnClinic = dto.OwnClinic,
                VisitingHours = dto.VisitingHours,
                ClinicVisitingHours = dto.ClinicVisitingHours,
                LandLineNo = dto.LandLineNo,
                Fax = dto.Fax,
                Salutation = dto.Salutation,
                JobTitle = dto.JobTitle,
                IsDefault = dto.IsDefault,
                SalesLeadId = dto.SalesLeadId,
                DateCreated = dto.DateCreated,
                DateUpdated = dto.DateUpdated,
                UserCreated = dto.UserCreated,
                UserUpdated = dto.UserUpdated
            };
        }

        private SalesContactDto MapToDto(SalesContact entity)
        {
            return new SalesContactDto
            {
                Id = entity.Id,
                ContactName = entity.ContactName,
                DepartmentName = entity.DepartmentName,
                Specialist = entity.Specialist,
                Degree = entity.Degree,
                Email = entity.Email,
                MobileNo = entity.MobileNo,
                Website = entity.Website,
                IsActive = entity.IsActive,
                OwnClinic = entity.OwnClinic,
                VisitingHours = entity.VisitingHours,
                ClinicVisitingHours = entity.ClinicVisitingHours,
                LandLineNo = entity.LandLineNo,
                Fax = entity.Fax,
                Salutation = entity.Salutation,
                JobTitle = entity.JobTitle,
                IsDefault = entity.IsDefault,
                SalesLeadId = entity.SalesLeadId,
                DateCreated = entity.DateCreated,
                DateUpdated = entity.DateUpdated,
                UserCreated = entity.UserCreated,
                UserUpdated = entity.UserUpdated
            };
        }

        /// <summary>
        /// Gets all sales contacts
        /// </summary>
        /// <returns>List of all sales contacts</returns>
        /// <response code="200">Returns the list of contacts</response>
        [HttpGet(Name = "GetAllSalesContacts")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<SalesContactDto>>> GetAll()
        {
            try
            {
                var contacts = await _salesContactService.GetAllAsync();
                var result = contacts.Select(MapToDto).ToList();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    message = "An error occurred while retrieving contacts",
                    statusCode = 500,
                    errors = new[] { ex.Message }
                });
            }
        }

        /// <summary>
        /// Gets a specific sales contact by ID
        /// </summary>
        /// <param name="id">The ID of the contact to retrieve</param>
        /// <returns>The requested sales contact</returns>
        /// <response code="200">Returns the requested contact</response>
        /// <response code="404">If the contact is not found</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Produces("application/json")]
        public async Task<ActionResult<SalesContactDto>> GetById(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { 
                    message = "Invalid ID",
                    statusCode = 400,
                    errors = new[] { "ID must be a positive number" }
                });
            }

            try 
            {
                var contact = await _salesContactService.GetByIdAsync(id);
                if (contact == null)
                {
                    return NotFound(new { 
                        message = $"Contact with ID {id} not found",
                        statusCode = 404,
                        errors = new[] { $"Contact with ID {id} not found" }
                    });
                }
                return Ok(MapToDto(contact));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    message = "An error occurred while retrieving the contact",
                    statusCode = 500,
                    errors = new[] { ex.Message }
                });
            }
        }

        /// <summary>
        /// Gets all contacts for a specific sales lead
        /// </summary>
        /// <param name="salesLeadId">The ID of the sales lead</param>
        /// <returns>List of contacts associated with the sales lead</returns>
        /// <response code="200">Returns the list of contacts</response>
        /// <response code="404">If no contacts are found for the sales lead</response>
        [HttpGet("lead/{salesLeadId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Produces("application/json")]
        public async Task<ActionResult<IEnumerable<SalesContactDto>>> GetBySalesLeadId(int salesLeadId)
        {
            try
            {
                var contacts = await _salesContactService.GetBySalesLeadIdAsync(salesLeadId);
                if (!contacts.Any())
                {
                    return NotFound(new {
                        message = $"No contacts found for Sales Lead ID {salesLeadId}",
                        statusCode = 404,
                        errors = new[] { $"No contacts exist for Sales Lead {salesLeadId}" }
                    });
                }
                return Ok(contacts.Select(MapToDto));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    message = "An error occurred while retrieving contacts",
                    statusCode = 500,
                    errors = new[] { ex.Message }
                });
            }
        }

        /// <summary>
        /// Creates a new sales contact
        /// </summary>
        /// <param name="dto">The contact information to create</param>
        /// <returns>The ID of the created contact</returns>
        /// <response code="201">Returns the ID of the created contact</response>
        /// <response code="400">If the model state is invalid</response>
        [HttpPost]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<int>> Create([FromBody] SalesContactDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { 
                        message = "Invalid model state",
                        statusCode = 400,
                        errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                    });
                }

                var validationErrors = new List<string>();
                if (string.IsNullOrWhiteSpace(dto.ContactName))
                {
                    validationErrors.Add("Contact Name is required");
                }
                if (!string.IsNullOrWhiteSpace(dto.Email) && !new EmailAddressAttribute().IsValid(dto.Email))
                {
                    validationErrors.Add("Invalid email address format");
                }
                if (validationErrors.Any())
                {
                    return BadRequest(new {
                        message = "Validation failed",
                        statusCode = 400,
                        errors = validationErrors
                    });
                }

                var contact = MapToEntity(dto);
                contact.DateCreated = DateTime.UtcNow;
                contact.UserCreated = 1; // Default user ID since we're removing auth

                var id = await _salesContactService.CreateAsync(contact);

                // Create summary if contact is associated with a lead
                if (contact.SalesLeadId.HasValue)
                {
                    var lead = await _salesLeadService.GetByIdAsync(contact.SalesLeadId.Value);
                    if (lead != null)
                    {
                        var summary = new SalesSummary
                        {
                            Title = $"Contact added - {contact.ContactName}",
                            Description = $"New contact {contact.ContactName} added to lead {lead.CustomerName}",
                            DateTime = DateTime.UtcNow,
                            Stage = "lead",
                            StageItemId = lead.Id.ToString(),
                            IsActive = true,
                            Entities = System.Text.Json.JsonSerializer.Serialize(new 
                            { 
                                LeadId = lead.Id, 
                                ContactId = id,
                                ContactName = contact.ContactName,
                                CustomerName = lead.CustomerName 
                            })
                        };
                        await _summaryService.CreateAsync(summary);
                    }
                }
                return Created($"api/SalesContact/{id}", id);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    message = "An error occurred while creating the contact",
                    statusCode = 500,
                    errors = new[] { ex.Message }
                });
            }
        }

        /// <summary>
        /// Updates an existing sales contact
        /// </summary>
        /// <param name="id">The ID of the contact to update</param>
        /// <param name="dto">The updated contact information</param>
        /// <returns>The updated contact data</returns>
        /// <response code="200">Returns the updated contact</response>
        /// <response code="404">If the contact is not found</response>
        /// <response code="400">If the model state is invalid</response>
        [HttpPut("{id}")]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<SalesContactDto>> Update(int id, [FromBody] SalesContactDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { 
                        message = "Invalid model state",
                        statusCode = 400,
                        errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                    });
                }

                var existingContact = await _salesContactService.GetByIdAsync(id);
                if (existingContact == null)
                {
                    return NotFound(new {
                        message = $"Contact with ID {id} not found",
                        statusCode = 404,
                        errors = new[] { $"Contact with ID {id} not found" }
                    });
                }

                dto.Id = id;
                if (dto.SalesLeadId.HasValue)
                {
                    var lead = await _salesLeadService.GetByIdAsync(dto.SalesLeadId.Value);
                    if (lead == null)
                    {
                        return BadRequest($"Sales Lead with ID {dto.SalesLeadId} not found");
                    }
                }

                var contact = MapToEntity(dto);
                contact.Id = id;
                contact.DateUpdated = DateTime.UtcNow;
                contact.UserUpdated = 1; // Default user ID since we're removing auth

                var updateSuccess = await _salesContactService.UpdateAsync(contact);
                if (!updateSuccess)
                {
                    return StatusCode(500, "Failed to update contact");
                }

                contact = await _salesContactService.GetByIdAsync(id);
                if (contact == null)
                {
                    return StatusCode(500, new {
                        message = "Contact was updated but could not be retrieved",
                        statusCode = 500,
                        errors = new[] { "Error retrieving updated contact" }
                    });
                }

                var result = MapToDto(contact);

                try
                {
                    if (contact.SalesLeadId.HasValue)
                    {
                        var lead = await _salesLeadService.GetByIdAsync(contact.SalesLeadId.Value);
                        if (lead != null)
                        {
                            var summary = new SalesSummary
                            {
                                Title = $"Contact updated - {contact.ContactName}",
                                Description = $"Contact {contact.ContactName} updated for lead {lead.CustomerName}",
                                DateTime = DateTime.UtcNow,
                                Stage = "lead",
                                StageItemId = lead.Id.ToString(),
                                IsActive = true,
                                Entities = System.Text.Json.JsonSerializer.Serialize(new 
                                { 
                                    LeadId = lead.Id, 
                                    ContactId = id,
                                    ContactName = contact.ContactName,
                                    CustomerName = lead.CustomerName 
                                })
                            };
                            await _summaryService.CreateAsync(summary);
                        }
                    }
                }
                catch (Exception summaryEx)
                {
                    _logger.LogWarning(summaryEx, "Failed to create summary for contact update {ContactId}", id);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    message = "An error occurred while updating the contact",
                    statusCode = 500,
                    errors = new[] { ex.Message }
                });
            }
        }

        /// <summary>
        /// Deletes a sales contact
        /// </summary>
        /// <param name="id">The ID of the contact to delete</param>
        /// <returns>No content if successful</returns>
        /// <response code="204">If the contact was successfully deleted</response>
        /// <response code="404">If the contact is not found</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                // Get the contact before deleting to create summary
                var contact = await _salesContactService.GetByIdAsync(id);
                if (contact == null)
                {
                    return NotFound(new { 
                        message = $"Contact with ID {id} not found",
                        statusCode = 404,
                        errors = new[] { $"Contact with ID {id} not found" }
                    });
                }

                var success = await _salesContactService.DeleteAsync(id);
                if (!success)
                {
                    return StatusCode(500, new { 
                        message = "Failed to delete contact",
                        statusCode = 500,
                        errors = new[] { "Error occurred while deleting the contact" }
                    });
                }

                // Create summary if contact is associated with a lead
                if (contact.SalesLeadId.HasValue)
                {
                    var lead = await _salesLeadService.GetByIdAsync(contact.SalesLeadId.Value);
                    if (lead != null)
                    {
                        var summary = new SalesSummary
                        {
                            Title = $"Contact deleted - {contact.ContactName}",
                            Description = $"Contact {contact.ContactName} deleted from lead {lead.CustomerName}",
                            DateTime = DateTime.UtcNow,
                            Stage = "lead",
                            StageItemId = lead.Id.ToString(),
                            IsActive = true,
                            Entities = System.Text.Json.JsonSerializer.Serialize(new 
                            { 
                                LeadId = lead.Id, 
                                ContactId = id,
                                ContactName = contact.ContactName,
                                CustomerName = lead.CustomerName 
                            })
                        };
                        await _summaryService.CreateAsync(summary);
                    }
                }

                _logger.LogInformation("Deleted contact with ID {ContactId}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting contact {Id}: {Message}", id, ex.Message);
                return StatusCode(500, new { 
                    message = "An error occurred while deleting the contact",
                    statusCode = 500,
                    errors = new[] { ex.Message }
                });
            }
        }
    }
}