using ERP.API.Models.DTOs.CompanySetup;
using ERP.API.Services.CompanySetup;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ERP.API.Controllers.CompanySetup
{
    [ApiController]
    [Route("api/[controller]")]
    public class FinancialStatementTemplateController : ControllerBase
    {
        private readonly FinancialStatementTemplateService _service;

        public FinancialStatementTemplateController(FinancialStatementTemplateService service)
        {
            _service = service;
        }


        [HttpPost]
        public async Task<IActionResult> Create([FromBody] FinancialStatementTemplateCreateDto dto)
        {
            try
            {
                var id = await _service.CreateAsync(dto);
                return Ok(new { success = true, message = "Created successfully", data = dto });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }


        [HttpPut]
        public async Task<IActionResult> Update([FromBody] FinancialStatementTemplateUpdateDto dto)
        {
            try
            {
                await _service.UpdateAsync(dto);
                return Ok(new { success = true, message = "Updated successfully", data = dto });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }


        [HttpGet("{templateId}")]
        public async Task<IActionResult> GetById(int templateId)
        {
            try
            {
                var result = await _service.GetByIdAsync(templateId);
                if (result == null)
                    return NotFound(new { success = false, message = "Not found" });
                return Ok(new { success = true, message = "Record Fetched successfully for " + result.TemplateId, data = result });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }


        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _service.GetAllAsync();
                return Ok(new { success = true,message = "Record Fetched successfully", data = result });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromBody] FinancialStatementTemplateDeleteDto dto)
        {
            try
            {
                await _service.DeleteAsync(dto);
                return Ok(new { success = true, message = "Deleted successfully", data = dto });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
