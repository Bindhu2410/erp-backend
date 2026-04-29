using ERP.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeAllowanceController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EmployeeAllowanceController(AppDbContext context)
        {
            _context = context;
        }
    

        /// <summary>
        /// Get all allowances for an employee
        /// </summary>
        [HttpGet("employee/{employeeId}")]
        public async Task<ActionResult<IEnumerable<EmployeeAllowanceDto>>> GetByEmployeeId(int employeeId)
        {
            var allowances = await _context.EmployeeAllowances
                .Where(a => a.EmployeeId == employeeId && a.Active == true)
                .ToListAsync();

            if (!allowances.Any())
                return NotFound(new { message = $"No allowances found for employee ID {employeeId}." });

            var dtos = allowances.Select(a => new EmployeeAllowanceDto
            {
                Id = a.Id,
                EmployeeId = a.EmployeeId,
                AllowanceType = a.AllowanceType,
                AllowanceAmount = a.AllowanceAmount,
                AllowEffFrom = a.AllowEffFrom,
                AllowEffTo = a.AllowEffTo
            });

            return Ok(dtos);
        }

        /// <summary>
        /// Get a specific allowance by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<EmployeeAllowanceDto>> GetById(int id)
        {
            var allowance = await _context.EmployeeAllowances
                .FirstOrDefaultAsync(a => a.Id == id);

            if (allowance == null)
                return NotFound(new { message = $"Allowance with ID {id} not found." });

            var dto = new EmployeeAllowanceDto
            {
                Id = allowance.Id,
                EmployeeId = allowance.EmployeeId,
                AllowanceType = allowance.AllowanceType,
                AllowanceAmount = allowance.AllowanceAmount,
                AllowEffFrom = allowance.AllowEffFrom,
                AllowEffTo = allowance.AllowEffTo
            };

            return Ok(dto);
        }

        /// <summary>
        /// Create a new allowance for an employee
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<EmployeeAllowanceDto>> Create([FromBody] EmployeeAllowanceCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Verify employee exists
            var employee = await _context.SalesEmployees.FindAsync(dto.EmployeeId);
            if (employee == null)
                return NotFound(new { message = $"Employee with ID {dto.EmployeeId} not found." });

            var allowance = new EmployeeAllowance
            {
                EmployeeId = dto.EmployeeId,
                AllowanceType = dto.AllowanceType,
                AllowanceAmount = dto.AllowanceAmount,
                AllowEffFrom = dto.AllowEffFrom,
                AllowEffTo = dto.AllowEffTo,
                UserCreated = dto.UserCreated,
                DateCreated = DateTime.UtcNow,
                UserUpdated = dto.UserCreated,
                DateUpdated = DateTime.UtcNow,
                Active = true
            };

            _context.EmployeeAllowances.Add(allowance);
            await _context.SaveChangesAsync();

            var resultDto = new EmployeeAllowanceDto
            {
                Id = allowance.Id,
                EmployeeId = allowance.EmployeeId,
                AllowanceType = allowance.AllowanceType,
                AllowanceAmount = allowance.AllowanceAmount,
                AllowEffFrom = allowance.AllowEffFrom,
                AllowEffTo = allowance.AllowEffTo
            };

            return CreatedAtAction(nameof(GetById), new { id = allowance.Id }, resultDto);
        }

        /// <summary>
        /// Update an allowance
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] EmployeeAllowanceUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var allowance = await _context.EmployeeAllowances.FindAsync(id);
            if (allowance == null)
                return NotFound(new { message = $"Allowance with ID {id} not found." });

            if (dto.AllowanceType != null) allowance.AllowanceType = dto.AllowanceType;
            if (dto.AllowanceAmount.HasValue) allowance.AllowanceAmount = dto.AllowanceAmount;
            if (dto.AllowEffFrom.HasValue) allowance.AllowEffFrom = dto.AllowEffFrom;
            if (dto.AllowEffTo.HasValue) allowance.AllowEffTo = dto.AllowEffTo;

            allowance.UserUpdated = dto.UserUpdated;
            allowance.DateUpdated = DateTime.UtcNow;

            _context.EmployeeAllowances.Update(allowance);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Allowance updated successfully.", id = allowance.Id });
        }

        /// <summary>
        /// Delete an allowance (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, [FromQuery] int userUpdated)
        {
            var allowance = await _context.EmployeeAllowances.FindAsync(id);
            if (allowance == null)
                return NotFound(new { message = $"Allowance with ID {id} not found." });

            allowance.Active = false;
            allowance.UserUpdated = userUpdated;
            allowance.DateUpdated = DateTime.UtcNow;

            _context.EmployeeAllowances.Update(allowance);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Allowance deleted successfully." });
        }
    }
}
