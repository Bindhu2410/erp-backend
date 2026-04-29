using System.Collections.Generic;
using System.Threading.Tasks;
using ERP.API.Models.DTOs;
using ERP.API.Services.CompanySetup;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers.CompanySetup
{
    [ApiController]
    [Route("api/[controller]")]
    public class CurrencyExchangeRateController : ControllerBase
    {
        private readonly CurrencyExchangeRateService _service;

        public CurrencyExchangeRateController(CurrencyExchangeRateService service)
        {
            _service = service;
        }


        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CurrencyExchangeRateCreateDto dto)
        {
            try
            {
                await _service.CreateAsync(dto);
                return Ok(new {
                    success = true,
                    message = "Record created successfully",
                    data = dto
                });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new {
                    success = false,
                    message = ex.Message,
                    data = (object?)null
                });
            }
        }


        [HttpPut]
        public async Task<IActionResult> Update([FromBody] CurrencyExchangeRateUpdateDto dto)
        {
            try
            {
                await _service.UpdateAsync(dto);
                return Ok(new {
                    success = true,
                    message = "Record updated successfully",
                    data = dto
                });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new {
                    success = false,
                    message = ex.Message,
                    data = (object?)null
                });
            }
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _service.GetByIdAsync(id);
                if (result == null)
                    return NotFound(new { success = false, message = "Record not found", data = (object?)null });
                return Ok(new {
                    success = true,
                    message = "Record fetched successfully",
                    data = result
                });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new {
                    success = false,
                    message = ex.Message,
                    data = (object?)null
                });
            }
        }


        [HttpGet("company/{companyId}")]
        public async Task<IActionResult> GetByCompanyId(int companyId)
        {
            try
            {
                var result = await _service.GetByCompanyIdAsync(companyId);
                if (result == null || !result.Any())
                    return NotFound(new { success = false, message = "Record not found", data = (object?)null });
                return Ok(new {
                    success = true,
                    message = "Record fetched successfully",
                    data = result
                });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new {
                    success = false,
                    message = ex.Message,
                    data = (object?)null
                });
            }
        }


        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] bool onlyActive = false)
        {
            try
            {
                var result = await _service.GetAllAsync(onlyActive);
                return Ok(new {
                    success = true,
                    message = "Records fetched successfully",
                    data = result
                });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new {
                    success = false,
                    message = ex.Message,
                    data = (object?)null
                });
            }
        }


        [HttpDelete]
        public async Task<IActionResult> Delete([FromBody] CurrencyExchangeRateDeleteDto dto)
        {
            try
            {
                await _service.DeleteAsync(dto);
                return Ok(new {
                    success = true,
                    message = "Record deleted successfully",
                    data = (object?)null
                });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new {
                    success = false,
                    message = ex.Message,
                    data = (object?)null
                });
            }
        }
    }
}
