using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ERP.API.Models;
using ERP.API.Models.DTOs.CompanySetup;
using ERP.API.Services.CompanySetup;

namespace ERP.API.Controllers.CompanySetup
{
    [ApiController]
    [Route("api/CsTdsRates")] // Using explicit route for clarity
    public class CsTdsRatesController : ControllerBase
    {
        private readonly ICsTdsRateService _tdsRateService;

        public CsTdsRatesController(ICsTdsRateService tdsRateService)
        {
            _tdsRateService = tdsRateService;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResponse<CsTdsRateDto>>> Search([FromQuery] CsTdsRateSearchDto searchDto)
        {
            try 
            {
                if (!searchDto.CompanyId.HasValue)
                {
                    return BadRequest(new { message = "CompanyId is required for search" });
                }
                var result = await _tdsRateService.SearchAsync(searchDto);
                return Ok(new {
                    message = "TDS Rates search completed successfully.",
                    data = result
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in CsTdsRatesController.Search: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                return StatusCode(500, new {
                    message = "An error occurred while searching TDS Rates",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CsTdsRateDto>> GetById(int id)
        {
            try
            {
                var result = await _tdsRateService.GetByIdAsync(id);
                if (result == null)
                    return NotFound(new { message = $"TDS Rate with ID {id} not found", data = (object?)null });
                return Ok(new {
                    message = "TDS Rate retrieved successfully.",
                    data = result
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in CsTdsRatesController.GetById: {ex.Message}");
                return StatusCode(500, new {
                    message = "An error occurred while retrieving the TDS Rate",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        [HttpPost]
        public async Task<ActionResult<int>> Create(CsTdsRateDto dto)
        {
            try
            {
                var id = await _tdsRateService.CreateAsync(dto);
                return Ok(new {
                    message = "TDS Rate created successfully.",
                    data = new { Id = id }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new {
                    message = "An error occurred while creating TDS Rate.",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        [HttpPut]
        public async Task<ActionResult<bool>> Update(CsTdsRateDto dto)
        {
            try
            {
                var result = await _tdsRateService.UpdateAsync(dto);
                if (!result)
                    return NotFound(new { message = "TDS Rate not found or update failed.", data = (object?)null });
                return Ok(new {
                    message = "TDS Rate updated successfully.",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new {
                    message = "An error occurred while updating TDS Rate.",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<bool>> Delete(int id)
        {
            try
            {
                var result = await _tdsRateService.DeleteAsync(id);
                if (!result)
                    return NotFound(new { message = "TDS Rate not found or delete failed.", data = (object?)null });
                return Ok(new {
                    message = "TDS Rate deleted successfully.",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new {
                    message = "An error occurred while deleting TDS Rate.",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        [HttpGet("company/{companyId}")]
        public async Task<ActionResult<PagedResponse<CsTdsRateDto>>> GetByCompany(
            int companyId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _tdsRateService.GetByCompanyAsync(companyId, pageNumber, pageSize);
                return Ok(new {
                    message = "TDS Rates retrieved successfully by company.",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new {
                    message = "An error occurred while retrieving TDS Rates by company.",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        [HttpGet("all")]
        public async Task<ActionResult<ApiResponse<List<CsTdsRateDto>>>> GetAllItems()
        {
            try
            {
                var result = await _tdsRateService.GetAllItemsAsync();
                return Ok(new {
                    message = "TDS Rates retrieved successfully.",
                    data = result
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in CsTdsRatesController.GetAllItems: {ex.Message}");
                return StatusCode(500, new {
                    message = "An error occurred while retrieving all TDS Rates.",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }
    }
}
