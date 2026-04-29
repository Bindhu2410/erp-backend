using ERP.API.Models;
using ERP.API.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DepartmentController : ControllerBase
    {
        private readonly DepartmentService _departmentService;

        public DepartmentController(DepartmentService departmentService)
        {
            _departmentService = departmentService;
        }

        [HttpPost]
        public async Task<ActionResult<int>> Create([FromBody] Department department)
        {
            var userId = 1; // Get from JWT token or session
            var id = await _departmentService.CreateAsync(department, userId);
            return Ok(id);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Department>>> GetAll()
        {
            var departments = await _departmentService.GetAllAsync();
            return Ok(departments);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Department>> GetById(int id)
        {
            var department = await _departmentService.GetByIdAsync(id);
            if (department == null) return NotFound();
            return Ok(department);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Department department)
        {
            var userId = 1; // Get from JWT token or session
            var success = await _departmentService.UpdateAsync(id, department, userId);
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _departmentService.DeleteAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}