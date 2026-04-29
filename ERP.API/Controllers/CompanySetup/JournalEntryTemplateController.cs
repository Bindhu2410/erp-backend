using System.Threading.Tasks;
using ERP.API.Models.DTOs;
using ERP.API.Services.CompanySetup;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers.CompanySetup
{
    [ApiController]
    [Route("api/[controller]")]
    public class JournalEntryTemplateController : ControllerBase
    {
        private readonly JournalEntryTemplateService _service;

        public JournalEntryTemplateController(JournalEntryTemplateService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] JournalEntryTemplateCreateDto dto)
        {
            try
            {
                var id = await _service.CreateAsync(dto);
                var created = await _service.GetByIdAsync(id);
                return Ok(new { success = true, message = "Record created successfully", data = created });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message, data = (object?)null });
            }
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] JournalEntryTemplateUpdateDto dto)
        {
            try
            {
                await _service.UpdateAsync(dto);
                var updated = await _service.GetByIdAsync(dto.TemplateId);
                return Ok(new { success = true, message = "Record updated successfully", data = updated });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message, data = (object?)null });
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
                return Ok(new { success = true, message = "Record fetched successfully", data = result });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message, data = (object?)null });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _service.GetAllAsync();
                return Ok(new { success = true, message = "Records fetched successfully", data = result });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message, data = (object?)null });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromBody] JournalEntryTemplateDeleteDto dto)
        {
            try
            {
                await _service.DeleteAsync(dto);
                return Ok(new { success = true, message = "Record deleted successfully", data = dto });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message, data = (object?)null });
            }
        }
    }
}
