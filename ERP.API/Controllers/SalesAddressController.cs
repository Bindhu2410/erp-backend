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
{    /// <summary>
    /// API endpoints for managing sales addresses
    /// </summary>    [ApiController]
    [Route("api/[controller]")]
    [EnableCors("AllowAll")]
    [Produces("application/json")]
    [ApiExplorerSettings(GroupName = "SalesAddress")]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Tags("SalesAddress")]
    public class SalesAddressController : ControllerBase
    {
        private readonly SalesAddressService _salesAddressService;
        private readonly SalesSummaryService _summaryService;
        private readonly SalesLeadService _salesLeadService;
        private readonly ILogger<SalesAddressController> _logger;

        public SalesAddressController(
            SalesAddressService salesAddressService,
            SalesSummaryService summaryService,
            SalesLeadService salesLeadService, 
            ILogger<SalesAddressController> logger)
        {
            _salesAddressService = salesAddressService;
            _summaryService = summaryService;
            _salesLeadService = salesLeadService;
            _logger = logger;
        }        private SalesAddress MapToEntity(CreateSalesAddressDto dto)
        {
            return new SalesAddress
            {
                Type = dto.Type?.Trim(),
                Block = dto.Block?.Trim(),
                Department = dto.Department?.Trim(),
                DoorNo = dto.DoorNo?.Trim(),
                Street = dto.Street?.Trim(),
                Landmark = dto.Landmark?.Trim(),
                IsDefault = dto.IsDefault,
                Area = dto.Area,
                City = dto.City?.Trim(),
                State = dto.State?.Trim(),
                Pincode = dto.Pincode,
                OpportunityId = dto.OpportunityId?.ToString(),
                ContactName = dto.ContactName?.Trim(),
                SalesLeadId = dto.SalesLeadId
            };
        }        private SalesAddress MapToEntity(UpdateSalesAddressDto dto, int id)
        {
            return new SalesAddress
            {
                Id = id,
                DateCreated = dto.DateCreated,
                DateUpdated = DateTime.UtcNow,
                Type = dto.Type?.Trim(),
                Block = dto.Block?.Trim(),
                Department = dto.Department?.Trim(),
                DoorNo = dto.DoorNo?.Trim(),
                Street = dto.Street?.Trim(),
                Landmark = dto.Landmark?.Trim(),
                IsDefault = dto.IsDefault,
                Area = dto.Area,
                City = dto.City?.Trim(),
                State = dto.State?.Trim(),
                Pincode = dto.Pincode,
                OpportunityId = dto.OpportunityId?.ToString(),
                ContactName = dto.ContactName?.Trim(),
                SalesLeadId = dto.SalesLeadId,
                IsActive = dto.IsActive ?? false
            };
        }

        private SalesAddressDto MapToDto(SalesAddress entity)
        {
            return new SalesAddressDto
            {
                Id = entity.Id,
                UserCreated = entity.UserCreated,
                DateCreated = entity.DateCreated,
                UserUpdated = entity.UserUpdated,
                DateUpdated = entity.DateUpdated,
                ContactName = entity.ContactName,
                SalesLeadId = entity.SalesLeadId?.ToString(),
                IsActive = entity.IsActive,
                Type = entity.Type,
                Block = entity.Block,
                Department = entity.Department,
                Area = entity.Area,
                City = entity.City,
                State = entity.State,
                Pincode = entity.Pincode,
                OpportunityId = entity.OpportunityId,
                DoorNo = entity.DoorNo,
                Street = entity.Street,
                Landmark = entity.Landmark,
                IsDefault = entity.IsDefault
            };
        }

        /// <summary>
        /// Gets all addresses
        /// </summary>
        /// <returns>List of all addresses</returns>
        /// <response code="200">Returns the list of addresses</response>
        /// <response code="401">If the user is not authorized</response>
        /// <response code="500">If an unexpected error occurs</response>
        [HttpGet(Name = "GetAllAddresses")]
        
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        public async Task<ActionResult<IEnumerable<SalesAddressDto>>> GetAll()
        {
            try
            {
                var addresses = await _salesAddressService.GetAllAsync();
                var result = addresses.Select(MapToDto).ToList();
                _logger.LogInformation("Retrieved {Count} addresses", result.Count);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all addresses");
                return StatusCode(500, new { 
                    message = "An error occurred while retrieving addresses",
                    statusCode = 500,
                    errors = new[] { ex.Message }
                });
            }
        }

        /// <summary>
        /// Gets a specific address by ID
        /// </summary>
        /// <param name="id">The ID of the address to retrieve</param>
        /// <returns>The requested address</returns>
        /// <response code="200">Returns the requested address</response>
        /// <response code="404">If the address is not found</response>
        /// <response code="400">If the ID is invalid</response>        /// <response code="500">If an unexpected error occurs</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        public async Task<ActionResult<SalesAddressDto>> GetById(int id)
        {
            try 
            {
                if (id <= 0)
                {
                    return BadRequest(new { 
                        message = "Invalid ID",
                        statusCode = 400,
                        errors = new[] { "ID must be a positive number" }
                    });
                }

                var address = await _salesAddressService.GetByIdAsync(id);
                if (address == null)
                {
                    return NotFound(new { 
                        message = $"Address with ID {id} not found",
                        statusCode = 404,
                        errors = new[] { $"Address with ID {id} not found" }
                    });
                }

                _logger.LogInformation("Retrieved address with ID {AddressId}", id);
                return Ok(MapToDto(address));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving address {AddressId}", id);
                return StatusCode(500, new { 
                    message = "An error occurred while retrieving the address",
                    statusCode = 500,
                    errors = new[] { ex.Message }
                });
            }
        }

        /// <summary>
        /// Gets all addresses for a specific sales lead
        /// </summary>
        /// <param name="salesLeadId">The ID of the sales lead</param>
        /// <returns>List of addresses associated with the sales lead</returns>
        /// <response code="200">Returns the list of addresses</response>        /// <response code="404">If no addresses are found for the sales lead</response>
        [HttpGet("lead/{salesLeadId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<SalesAddressDto>>> GetBySalesLeadId(int salesLeadId)
        {
            try
            {
                var addresses = await _salesAddressService.GetBySalesLeadIdAsync(salesLeadId);
                if (!addresses.Any())
                {
                    return NotFound(new {
                        message = $"No addresses found for Sales Lead ID {salesLeadId}",
                        statusCode = 404,
                        errors = new[] { $"No addresses exist for Sales Lead {salesLeadId}" }
                    });
                }
                return Ok(addresses.Select(MapToDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving addresses for Sales Lead {SalesLeadId}", salesLeadId);
                return StatusCode(500, new {
                    message = "An error occurred while retrieving addresses",
                    statusCode = 500,
                    errors = new[] { ex.Message }
                });
            }
        }        /// <summary>
        /// Creates a new sales address
        /// </summary>
        /// <param name="dto">The address information</param>
        /// <response code="201">Returns the newly created address ID</response>
        /// <response code="400">If the request data is invalid</response>
        [HttpPost]
        [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<int>> Create([FromBody] CreateSalesAddressDto dto)
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
                if (string.IsNullOrWhiteSpace(dto.Type))
                {
                    validationErrors.Add("Address Type is required");
                }
                if (!dto.SalesLeadId.HasValue)
                {
                    validationErrors.Add("Sales Lead ID is required");
                }
                if (validationErrors.Any())
                {
                    return BadRequest(new {
                        message = "Validation failed",
                        statusCode = 400,
                        errors = validationErrors
                    });
                }
                var address = MapToEntity(dto);
                address.DateCreated = DateTime.UtcNow;
                address.IsActive = true;
                address.UserCreated = 1;
                address.UserUpdated = 1;
                if (dto.SalesLeadId.HasValue)
                {
                    var lead = await _salesLeadService.GetByIdAsync(dto.SalesLeadId.Value);
                    if (lead == null)
                    {
                        return BadRequest(new {
                            message = $"Sales Lead with ID {dto.SalesLeadId} not found",
                            statusCode = 400,
                            errors = new[] { $"Sales Lead with ID {dto.SalesLeadId} not found" }
                        });
                    }
                }
                var id = await _salesAddressService.CreateAsync(address);
                address.Id = id;
                try
                {
                    if (address.SalesLeadId.HasValue)
                    {
                        var lead = await _salesLeadService.GetByIdAsync(address.SalesLeadId.Value);
                        if (lead != null)
                        {
                            var addressType = address.Type ?? "General";
                            var summary = new SalesSummary
                            {
                                Title = $"Address added - {addressType}",
                                Description = $"New {addressType.ToLower()} address added to lead {lead.CustomerName}",
                                DateTime = DateTime.UtcNow,
                                Stage = "lead",
                                StageItemId = lead.Id.ToString(),
                                IsActive = true,
                                Entities = System.Text.Json.JsonSerializer.Serialize(new 
                                { 
                                    LeadId = lead.Id, 
                                    AddressId = id,
                                    AddressType = addressType,
                                    CustomerName = lead.CustomerName 
                                })
                            };
                            await _summaryService.CreateAsync(summary);
                        }
                    }
                }
                catch (Exception summaryEx)
                {
                    _logger.LogWarning(summaryEx, "Failed to create summary for new address {AddressId}", id);
                }
                _logger.LogInformation("Created new address with ID {AddressId}", id);
                return CreatedAtAction(nameof(GetById), new { id }, id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating new address");
                return StatusCode(500, new { 
                    message = "An error occurred while creating the address",
                    statusCode = 500,
                    errors = new[] { ex.Message }
                });
            }
        }        /// <summary>
        /// Updates an existing sales address
        /// </summary>
        /// <param name="id">The ID of the address to update</param>
        /// <param name="dto">The updated address information</param>
        /// <response code="200">Returns the updated address</response>
        /// <response code="400">If the request data is invalid</response>
        /// <response code="404">If the address is not found</response>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(SalesAddressDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<SalesAddressDto>> Update(int id, [FromBody] UpdateSalesAddressDto dto)
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
                if (string.IsNullOrWhiteSpace(dto.Type))
                {
                    validationErrors.Add("Address Type is required");
                }
                if (validationErrors.Any())
                {
                    return BadRequest(new {
                        message = "Validation failed",
                        statusCode = 400,
                        errors = validationErrors
                    });
                }
                if (id <= 0)
                {
                    return BadRequest(new {
                        message = "Invalid ID",
                        statusCode = 400,
                        errors = new[] { "ID must be a positive number" }
                    });
                }
                var existingAddress = await _salesAddressService.GetByIdAsync(id);
                if (existingAddress == null)
                {
                    return NotFound(new {
                        message = $"Address with ID {id} not found",
                        statusCode = 404,
                        errors = new[] { $"Address with ID {id} not found" }
                    });
                }
                if (dto.SalesLeadId.HasValue)
                {
                    var lead = await _salesLeadService.GetByIdAsync(dto.SalesLeadId.Value);
                    if (lead == null)
                    {
                        return BadRequest(new {
                            message = $"Sales Lead with ID {dto.SalesLeadId} not found",
                            statusCode = 400,
                            errors = new[] { $"Sales Lead with ID {dto.SalesLeadId} not found" }
                        });
                    }
                }
                var address = MapToEntity(dto, id);
                address.DateCreated = existingAddress.DateCreated;
                address.UserCreated = existingAddress.UserCreated;
                address.DateUpdated = DateTime.UtcNow;
                address.UserUpdated = 1;
                var updateSuccess = await _salesAddressService.UpdateAsync(address);
                if (!updateSuccess)
                {
                    return StatusCode(500, new {
                        message = "Failed to update address",
                        statusCode = 500,
                        errors = new[] { "Database update operation failed" }
                    });
                }
                var result = MapToDto(address);
                try
                {
                    if (address.SalesLeadId.HasValue)
                    {
                        var lead = await _salesLeadService.GetByIdAsync(address.SalesLeadId.Value);
                        if (lead != null)
                        {
                            var addressType = address.Type ?? "General";
                            var summary = new SalesSummary
                            {
                                Title = $"Address updated - {addressType}",
                                Description = $"{addressType} address updated for lead {lead.CustomerName}",
                                DateTime = DateTime.UtcNow,
                                Stage = "lead",
                                StageItemId = lead.Id.ToString(),
                                IsActive = true,
                                Entities = System.Text.Json.JsonSerializer.Serialize(new 
                                { 
                                    LeadId = lead.Id, 
                                    AddressId = id,
                                    AddressType = addressType,
                                    CustomerName = lead.CustomerName 
                                })
                            };
                            await _summaryService.CreateAsync(summary);
                        }
                    }
                }
                catch (Exception summaryEx)
                {
                    _logger.LogWarning(summaryEx, "Failed to create summary for address update {AddressId}", id);
                }
                _logger.LogInformation("Updated address with ID {AddressId}", id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating address {AddressId}", id);
                return StatusCode(500, new { 
                    message = "An error occurred while updating the address",
                    statusCode = 500,
                    errors = new[] { ex.Message }
                });
            }
        }

        /// <summary>
        /// Deletes an address
        /// </summary>
        /// <param name="id">The ID of the address to delete</param>
        /// <returns>No content</returns>
        /// <response code="204">If the address was successfully deleted</response>
        /// <response code="404">If the address is not found</response>
        /// <response code="401">If the user is not authorized</response>
        /// <response code="500">If an unexpected error occurs</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var address = await _salesAddressService.GetByIdAsync(id);
                if (address == null)
                {
                    return NotFound(new {
                        message = $"Address with ID {id} not found",
                        statusCode = 404,
                        errors = new[] { $"Address with ID {id} not found" }
                    });
                }
                if (address.SalesLeadId.HasValue)
                {
                    var lead = await _salesLeadService.GetByIdAsync(address.SalesLeadId.Value);
                    if (lead != null)
                    {
                        var addressType = address.Type ?? "General";
                        var summary = new SalesSummary
                        {
                            Title = $"Address deleted - {addressType}",
                            Description = $"{addressType} address removed from lead {lead.CustomerName}",
                            DateTime = DateTime.UtcNow,
                            Stage = "lead",
                            StageItemId = lead.Id.ToString(),
                            IsActive = true,
                            Entities = System.Text.Json.JsonSerializer.Serialize(new 
                            { 
                                LeadId = lead.Id, 
                                AddressId = id,
                                AddressType = addressType,
                                CustomerName = lead.CustomerName 
                            })
                        };
                        await _summaryService.CreateAsync(summary);
                    }
                }

                await _salesAddressService.DeleteAsync(id);
                _logger.LogInformation("Deleted address with ID {AddressId}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting address {AddressId}", id);
                return StatusCode(500, new {
                    message = "An error occurred while deleting the address",
                    statusCode = 500, 
                    errors = new[] { ex.Message }
                });
            }
        }
    }
}
