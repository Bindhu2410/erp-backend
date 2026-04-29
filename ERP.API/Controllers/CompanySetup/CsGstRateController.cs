using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ERP.API.Models;
using ERP.API.Models.CompanySetup;
using ERP.API.Models.DTOs.CompanySetup;
using ERP.API.Services.CompanySetup;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ERP.API.Controllers.CompanySetup
{
    [Route("api/[controller]")]
    [ApiController]
    public class CsGstRateController : ControllerBase
    {
        private readonly ICsGstRateService _gstRateService;
        private readonly ILogger<CsGstRateController> _logger;

        public CsGstRateController(ICsGstRateService gstRateService, ILogger<CsGstRateController> logger)
        {
            _gstRateService = gstRateService;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByCompanyQuery([FromQuery] int companyId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var searchDto = new CsGstRateSearchDto
                {
                    CompanyId = companyId,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
                var (data, totalRecords, filteredRecords) = await _gstRateService.GetByCompanyAsync(searchDto);
                var dtoList = data.Select(MapToDto).ToList();
                return Ok(new
                {
                    message = "GST Rates retrieved successfully by company",
                    data = dtoList,
                    totalCount = totalRecords,
                    pageSize = searchDto.PageSize,
                    pageNumber = searchDto.PageNumber
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving GST Rates by company");
                return StatusCode(500, new { message = "An error occurred while retrieving GST Rates by company.", error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { message = "Invalid ID. ID must be greater than zero." });
                }
                var gstRate = await _gstRateService.GetByIdAsync(id);
                if (gstRate == null)
                {
                    return NotFound(new { message = $"GST Rate with ID {id} was not found.", data = (object?)null });
                }
                return Ok(new { message = "GST Rate retrieved successfully", data = MapToDto(gstRate) });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving GST Rate by ID");
                return StatusCode(500, new { message = "An error occurred while retrieving GST Rate by ID.", error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpGet("company")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByCompany([FromQuery] CsGstRateSearchDto searchDto)
        {
            try
            {
                var (data, totalRecords, filteredRecords) = await _gstRateService.GetByCompanyAsync(searchDto);
                var dtoList = data.Select(MapToDto).ToList();
                return Ok(new {
                    message = "GST Rates retrieved successfully by company",
                    data = dtoList,
                    totalCount = totalRecords,
                    pageSize = searchDto.PageSize,
                    pageNumber = searchDto.PageNumber
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving GST Rates by company");
                return StatusCode(500, new { message = "An error occurred while retrieving GST Rates by company.", error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpGet("hsnsac")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByHsnSac(
            [FromQuery] int companyId,
            [FromQuery] string hsnSacCode,
            [FromQuery] bool isHsn,
            [FromQuery] DateTime effectiveDate)
        {
            try
            {
                var gstRate = await _gstRateService.GetByHsnSacAsync(companyId, hsnSacCode, isHsn, effectiveDate);
                if (gstRate == null)
                    return NotFound(new { message = $"GST Rate not found for the given criteria.", data = (object?)null });
                return Ok(new { message = "GST Rate retrieved successfully by HSN/SAC", data = MapToDto(gstRate) });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving GST Rate by HSN/SAC");
                return StatusCode(500, new { message = "An error occurred while retrieving GST Rate by HSN/SAC.", error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpGet("all")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var gstRates = await _gstRateService.GetAllAsync();
                var dtos = gstRates.Select(MapToWithCompanyDto);
                return Ok(new {
                    message = "GST Rates retrieved successfully",
                    data = dtos
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all GST rates");
                return StatusCode(500, new { message = "An error occurred while retrieving GST rates", error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CsGstRateDto gstRateDto)
        {
            try
            {
                var gstRate = MapFromDto(gstRateDto);
                var gstRateId = await _gstRateService.CreateAsync(gstRate);
                gstRateDto.GstRateId = gstRateId;
                return CreatedAtAction(nameof(GetById), new { id = gstRateId }, new { message = "GST Rate created successfully", data = gstRateDto });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating GST Rate");
                return StatusCode(500, new { message = "An error occurred while creating GST Rate.", error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] CsGstRateDto gstRateDto)
        {
            try
            {
                if (id != gstRateDto.GstRateId)
                    return BadRequest(new { message = "ID in URL does not match ID in body." });
                var success = await _gstRateService.UpdateAsync(MapFromDto(gstRateDto));
                if (!success)
                    return NotFound(new { message = $"GST Rate with id {id} not found for update.", data = (object?)null });
                return Ok(new { message = "GST Rate updated successfully", data = (object?)null });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating GST Rate");
                return StatusCode(500, new { message = "An error occurred while updating GST Rate.", error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var success = await _gstRateService.DeleteAsync(id);
                if (!success)
                    return NotFound(new { message = $"GST Rate with id {id} not found for deletion.", data = (object?)null });
                return Ok(new { message = "GST Rate deleted successfully", data = (object?)null });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting GST Rate");
                return StatusCode(500, new { message = "An error occurred while deleting GST Rate.", error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        private static CsGstRateDto MapToDto(CsGstRate gstRate)
        {
            return new CsGstRateDto
            {
                GstRateId = gstRate.GstRateId,
                CompanyId = gstRate.CompanyId,
                HsnSacCode = gstRate.HsnSacCode,
                IsHsn = gstRate.IsHsn,
                GstRate = gstRate.GstRate,
                EffectiveDate = gstRate.EffectiveDate,
                CreatedAt = gstRate.CreatedAt,
                UpdatedAt = gstRate.UpdatedAt
            };
        }

        private static CsGstRate MapFromDto(CsGstRateDto dto)
        {
            return new CsGstRate
            {
                GstRateId = dto.GstRateId,
                CompanyId = dto.CompanyId,
                HsnSacCode = dto.HsnSacCode,
                IsHsn = dto.IsHsn,
                GstRate = dto.GstRate,
                EffectiveDate = dto.EffectiveDate
            };
        }

        private static CsGstRateWithCompanyDto MapToWithCompanyDto(CsGstRateWithCompany gstRate)
        {
            return new CsGstRateWithCompanyDto
            {
                GstRateId = gstRate.GstRateId,
                CompanyId = gstRate.CompanyId,
                CompanyName = gstRate.CompanyName,
                HsnSacCode = gstRate.HsnSacCode,
                IsHsn = gstRate.IsHsn,
                GstRate = gstRate.GstRate,
                EffectiveDate = gstRate.EffectiveDate,
                CreatedAt = gstRate.CreatedAt,
                UpdatedAt = gstRate.UpdatedAt
            };
        }
    }
}
