        
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ERP.API.Models;
using ERP.API.Models.DTOs.CompanySetup;
using ERP.API.Services.CompanySetup;

namespace ERP.API.Controllers.CompanySetup
{
    [ApiController]
    [Route("api/[controller]")]
    public class CsSacCodeController : ControllerBase
    {
        private readonly ICsSacCodeService _sacCodeService;

        public CsSacCodeController(ICsSacCodeService sacCodeService)
        {
            _sacCodeService = sacCodeService;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResponse<CsSacCodeDto>>> Search([FromQuery] CsSacCodeSearchDto searchDto)
        {
            try
            {
                var result = await _sacCodeService.SearchAsync(searchDto);
                return Ok(new {
                    message = "SAC codes search completed successfully.",
                    data = result
                });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new {
                    message = "An error occurred while searching SAC codes.",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

     [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _sacCodeService.GetAllAsync();
                return Ok(new {
                    message = "All SAC codes retrieved successfully.",
                    data = result
                });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new {
                    message = "An error occurred while retrieving all SAC codes.",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CsSacCodeDto>> GetById(int id)
        {
            try
            {
                var result = await _sacCodeService.GetByIdAsync(id);
                if (result == null)
                    return NotFound(new { message = $"SAC code with ID {id} not found", data = (object?)null });
                return Ok(new {
                    message = "SAC code retrieved successfully.",
                    data = result
                });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new {
                    message = "An error occurred while retrieving SAC code.",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        [HttpPost]
        public async Task<ActionResult<int>> Create(CsSacCodeDto dto)
        {
            try
            {
                var id = await _sacCodeService.CreateAsync(dto);
                return Ok(new {
                    message = "SAC code created successfully.",
                    data = new { Id = id }
                });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new {
                    message = "An error occurred while creating SAC code.",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        [HttpPut]
        public async Task<ActionResult<bool>> Update(CsSacCodeDto dto)
        {
            try
            {
                var result = await _sacCodeService.UpdateAsync(dto);
                if (!result)
                    return NotFound(new { message = "SAC code not found or update failed.", data = (object?)null });
                return Ok(new {
                    message = "SAC code updated successfully.",
                    data = result
                });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new {
                    message = "An error occurred while updating SAC code.",
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
                var result = await _sacCodeService.DeleteAsync(id);
                if (!result)
                    return NotFound(new { message = "SAC code not found or delete failed.", data = (object?)null });
                return Ok(new {
                    message = "SAC code deleted successfully.",
                    data = result
                });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new {
                    message = "An error occurred while deleting SAC code.",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        [HttpGet("company/{companyId}")]
        public async Task<ActionResult<PagedResponse<CsSacCodeDto>>> GetByCompany(
            int companyId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _sacCodeService.GetByCompanyAsync(companyId, pageNumber, pageSize);
                return Ok(new {
                    message = "SAC codes retrieved successfully by company.",
                    data = result
                });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new {
                    message = "An error occurred while retrieving SAC codes by company.",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }
    }
}
