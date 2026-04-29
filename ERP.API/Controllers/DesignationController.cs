using ERP.API.Models;
using ERP.API.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DesignationController : ControllerBase
    {
        private readonly DesignationService _designationService;

        public DesignationController(DesignationService designationService)
        {
            _designationService = designationService;
        }

        [HttpPost]
        public async Task<ActionResult<int>> Create([FromBody] Designation designation)
        {
            var userId = 1; // Get from JWT token or session
            var id = await _designationService.CreateAsync(designation, userId);
            return Ok(id);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Designation>>> GetAll()
        {
            var designations = await _designationService.GetAllAsync();
            return Ok(designations);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Designation>> GetById(int id)
        {
            var designation = await _designationService.GetByIdAsync(id);
            if (designation == null) return NotFound();
            return Ok(designation);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Designation designation)
        {
            var userId = 1; // Get from JWT token or session
            var success = await _designationService.UpdateAsync(id, designation, userId);
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _designationService.DeleteAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}