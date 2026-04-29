using System;
using System.Linq;
using System.Threading.Tasks;
using ERP.API.Models;
using ERP.API.Models.CompanySetup;
using ERP.API.Models.DTOs.CompanySetup;
using ERP.API.Services.CompanySetup;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers.CompanySetup
{
    [Route("api/[controller]")]
    [ApiController]
    public class CsHsnCodeController : ControllerBase
    {
        private readonly ICsHsnCodeService _hsnCodeService;

        public CsHsnCodeController(ICsHsnCodeService hsnCodeService)
        {
            _hsnCodeService = hsnCodeService;
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CsHsnCodeDto>> GetById(int id)
        {
            try
            {
                var hsnCode = await _hsnCodeService.GetByIdAsync(id);
                if (hsnCode == null)
                    return NotFound(new { message = $"HSN code with ID {id} not found" });
                return Ok(new {
                    message = "HSN code retrieved successfully",
                    data = MapToDto(hsnCode)
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetById: {ex.Message}");
                return StatusCode(500, new {
                    message = "An error occurred while retrieving HSN code",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        [HttpGet("company")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PagedResult<CsHsnCodeDto>>> GetByCompany([FromQuery] CsHsnCodeSearchDto searchDto)
        {
            try
            {
                var (data, totalRecords, filteredRecords) = await _hsnCodeService.GetByCompanyAsync(searchDto);
                var dtoList = data.Select(MapToDto).ToList();
                return Ok(new {
                    message = "HSN codes retrieved successfully",
                    data = dtoList,
                    totalRecords,
                    filteredRecords
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetByCompany: {ex.Message}");
                return StatusCode(500, new {
                    message = "An error occurred while retrieving HSN codes by company",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CsHsnCodeDto>> Create(CsHsnCodeDto hsnCodeDto)
        {
            try
            {
                Console.WriteLine($"Received HSN Code: CompanyId={hsnCodeDto.CompanyId}, Code={hsnCodeDto.Code}, Description={hsnCodeDto.Description}, DefaultGstRate={hsnCodeDto.DefaultGstRate}");
                if (string.IsNullOrEmpty(hsnCodeDto.Code))
                {
                    ModelState.AddModelError(nameof(hsnCodeDto.Code), "The Code field is required.");
                }
                if (string.IsNullOrEmpty(hsnCodeDto.Description))
                {
                    ModelState.AddModelError(nameof(hsnCodeDto.Description), "The Description field is required.");
                }
                if (!ModelState.IsValid)
                {
                    return BadRequest(new 
                    {
                        message = "Invalid model state",
                        statusCode = 400,
                        errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                    });
                }
                var hsnCode = MapFromDto(hsnCodeDto);
                var hsnCodeId = await _hsnCodeService.CreateAsync(hsnCode);
                hsnCodeDto.HsnCodeId = hsnCodeId;
                return CreatedAtAction(nameof(GetById), new { id = hsnCodeId }, new {
                    message = "HSN code created successfully",
                    data = hsnCodeDto
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Create: {ex.Message}");
                return StatusCode(500, new {
                    message = "An error occurred while creating HSN code",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int id, CsHsnCodeDto hsnCodeDto)
        {
            try
            {
                if (id != hsnCodeDto.HsnCodeId)
                    return BadRequest(new { message = "ID in URL does not match ID in body." });
                var success = await _hsnCodeService.UpdateAsync(MapFromDto(hsnCodeDto));
                if (!success)
                    return NotFound(new { message = $"HSN code with ID {id} not found or update failed" });
                return Ok(new {
                    message = "HSN code updated successfully",
                    data = hsnCodeDto
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Update: {ex.Message}");
                return StatusCode(500, new {
                    message = "An error occurred while updating HSN code",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            Console.WriteLine($"Received DELETE request for HSN code ID: {id}");
            if (id <= 0)
            {
                Console.WriteLine($"Invalid ID provided for deletion: {id}");
                return BadRequest(new { message = $"Invalid ID: {id}" });
            }
            try
            {
                var existingRecord = await _hsnCodeService.GetByIdAsync(id);
                if (existingRecord == null)
                {
                    Console.WriteLine($"HSN code with ID {id} not found in controller before deletion");
                    return NotFound(new { message = $"HSN code with ID {id} not found" });
                }
                Console.WriteLine($"Found HSN code with ID {id}: {existingRecord.Code} - {existingRecord.Description}");
                Console.WriteLine($"Attempting to delete HSN code with ID {id} in controller");
                var success = await _hsnCodeService.DeleteAsync(id);
                if (!success)
                {
                    Console.WriteLine($"Delete operation for HSN code ID {id} returned false");
                    return StatusCode(500, new { message = $"Delete operation for HSN code with ID {id} failed on the server" });
                }
                var checkRecord = await _hsnCodeService.GetByIdAsync(id);
                if (checkRecord != null)
                {
                    Console.WriteLine($"WARNING: Record with ID {id} still exists after reported successful deletion");
                    return StatusCode(500, new { message = "Delete operation reported success but the record still exists" });
                }
                Console.WriteLine($"HSN code with ID {id} deleted successfully");
                return Ok(new {
                    message = "HSN code deleted successfully",
                    data = existingRecord
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in controller while deleting HSN code ID {id}: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                return StatusCode(500, new {
                    message = $"An error occurred while deleting HSN code",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        private static CsHsnCodeDto MapToDto(CsHsnCode hsnCode)
        {
            return new CsHsnCodeDto
            {
                HsnCodeId = hsnCode.HsnCodeId,
                CompanyId = hsnCode.CompanyId,
                Code = hsnCode.Code,
                Description = hsnCode.Description,
                IsActive = hsnCode.IsActive,
                DefaultGstRate = hsnCode.DefaultGstRate
            };
        }

        private static CsHsnCode MapFromDto(CsHsnCodeDto dto)
        {
            return new CsHsnCode
            {
                HsnCodeId = dto.HsnCodeId,
                CompanyId = dto.CompanyId,
                Code = dto.Code,
                Description = dto.Description,
                IsActive = dto.IsActive,
                DefaultGstRate = dto.DefaultGstRate
            };
        }
    }
}
