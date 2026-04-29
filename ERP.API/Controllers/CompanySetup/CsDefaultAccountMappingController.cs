using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ERP.API.Models.CompanySetup;
using ERP.API.Services.CompanySetup;
using Swashbuckle.AspNetCore.Annotations;

namespace ERP.API.Controllers.CompanySetup
{
    [ApiController]
    [Route("api/[controller]")]
    public class CsDefaultAccountMappingController : ControllerBase
    {
        private readonly ICsDefaultAccountMappingService _defaultAccountMappingService;
        private readonly ILogger<CsDefaultAccountMappingController> _logger;

        public CsDefaultAccountMappingController(
            ICsDefaultAccountMappingService defaultAccountMappingService,
            ILogger<CsDefaultAccountMappingController> logger)
        {
            _defaultAccountMappingService = defaultAccountMappingService;
            _logger = logger;
        }

        [HttpPost]
        [SwaggerOperation(Summary = "Creates a new default account mapping")]
        [ProducesResponseType(typeof(CsDefaultAccountMapping), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateDefaultAccountMapping([FromBody] CsDefaultAccountMappingRequest request)
        {
            try
            {
                var result = await _defaultAccountMappingService.CreateDefaultAccountMappingAsync(request);
                return CreatedAtAction(
                    nameof(GetDefaultAccountMappingById),
                    new { mappingId = result.MappingId },
                    new { message = "Default account mapping created successfully", data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating default account mapping");
                return StatusCode(500, new { message = "An error occurred while creating default account mapping", error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpPut("{mappingId}")]
        [SwaggerOperation(Summary = "Updates an existing default account mapping")]
        [ProducesResponseType(typeof(CsDefaultAccountMapping), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateDefaultAccountMapping(int mappingId, [FromBody] CsDefaultAccountMappingRequest request)
        {
            try
            {
                var result = await _defaultAccountMappingService.UpdateDefaultAccountMappingAsync(mappingId, request);
                if (result == null)
                    return NotFound(new { message = $"Default account mapping with id {mappingId} not found for update.", data = (object?)null });
                return Ok(new { message = "Default account mapping updated successfully", data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating default account mapping");
                return StatusCode(500, new { message = "An error occurred while updating default account mapping", error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpDelete("{mappingId}")]
        [SwaggerOperation(Summary = "Deletes a default account mapping")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteDefaultAccountMapping(int mappingId)
        {
            try
            {
                var result = await _defaultAccountMappingService.DeleteDefaultAccountMappingAsync(mappingId);
                if (!result)
                    return NotFound(new { message = $"Default account mapping with id {mappingId} not found for deletion.", data = (object?)null });
                return Ok(new { message = "Default account mapping deleted successfully", data = (object?)null });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting default account mapping");
                return StatusCode(500, new { message = "An error occurred while deleting default account mapping", error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpGet("{mappingId}")]
        [SwaggerOperation(Summary = "Gets a default account mapping by ID")]
        [ProducesResponseType(typeof(CsDefaultAccountMapping), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetDefaultAccountMappingById(int mappingId)
        {
            try
            {
                var result = await _defaultAccountMappingService.GetDefaultAccountMappingByIdAsync(mappingId);
                if (result == null)
                    return NotFound(new { message = $"Default account mapping with id {mappingId} not found.", data = (object?)null });
                return Ok(new { message = "Default account mapping retrieved successfully", data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving default account mapping by id");
                return StatusCode(500, new { message = "An error occurred while retrieving default account mapping by id", error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpGet("company/{companyId}")]
        [SwaggerOperation(Summary = "Gets default account mappings by company")]
        [ProducesResponseType(typeof(CsDefaultAccountMappingResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDefaultAccountMappingsByCompany(
            int companyId,
            [FromQuery] string? searchText,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var request = new CsDefaultAccountMappingSearchRequest
                {
                    CompanyId = companyId,
                    SearchText = searchText,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
                var result = await _defaultAccountMappingService.GetDefaultAccountMappingsByCompanyAsync(request);
                return Ok(new { message = "Default account mappings retrieved successfully by company", data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving default account mappings by company");
                return StatusCode(500, new { message = "An error occurred while retrieving default account mappings by company", error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpGet("all")]
        [SwaggerOperation(Summary = "Gets all default account mappings across all companies")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllDefaultAccountMappings()
        {
            try
            {
                var result = await _defaultAccountMappingService.GetAllDefaultAccountMappingsAsync();
                return Ok(new { message = "Default account mappings retrieved successfully", data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAllDefaultAccountMappings endpoint");
                return StatusCode(500, new { message = "An error occurred while retrieving default account mappings", error = ex.Message, stackTrace = ex.StackTrace });
            }
        }
    }
}
