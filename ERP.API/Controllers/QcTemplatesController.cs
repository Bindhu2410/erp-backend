using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ERP.API.Models;
using ERP.API.Models.DTOs;

namespace ERP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QcTemplatesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public QcTemplatesController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all QC Templates
        /// </summary>
        /// <returns>List of QC Templates</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<QcTemplateResponse>>> GetAll()
        {
            try
            {
                var templates = await _context.QcTemplates.ToListAsync();
                var response = templates.Select(x => MapToResponse(x)).ToList();
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving QC templates", error = ex.Message });
            }
        }

        /// <summary>
        /// Get a QC Template by ID
        /// </summary>
        /// <param name="id">Template ID</param>
        /// <returns>QC Template</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<QcTemplateResponse>> GetById(int id)
        {
            try
            {
                var template = await _context.QcTemplates.FindAsync(id);
                if (template == null)
                    return NotFound(new { message = $"QC Template with ID {id} not found" });

                return Ok(MapToResponse(template));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving QC template", error = ex.Message });
            }
        }

        /// <summary>
        /// Create a new QC Template
        /// </summary>
        /// <param name="request">QC Template request DTO</param>
        /// <returns>Created QC Template</returns>
        [HttpPost]
        public async Task<ActionResult<QcTemplateResponse>> Create([FromBody] QcTemplateRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Check if template name is unique
                var existingTemplate = await _context.QcTemplates
                    .FirstOrDefaultAsync(x => x.TemplateName == request.TemplateName);
                if (existingTemplate != null)
                    return BadRequest(new { message = $"A QC Template with the name '{request.TemplateName}' already exists" });

                // Validate CreatedBy and UpdatedBy reference existing users (to avoid FK violations)
                if (request.CreatedBy.HasValue)
                {
                    var createdUser = await _context.Users.FindAsync(request.CreatedBy.Value);
                    if (createdUser == null)
                        return BadRequest(new { message = $"CreatedBy user with id {request.CreatedBy.Value} does not exist" });
                }

                if (request.UpdatedBy.HasValue)
                {
                    var updatedUser = await _context.Users.FindAsync(request.UpdatedBy.Value);
                    if (updatedUser == null)
                        return BadRequest(new { message = $"UpdatedBy user with id {request.UpdatedBy.Value} does not exist" });
                }

                var template = new QcTemplate
                {
                    TemplateName = request.TemplateName,
                    Description = request.Description,
                    UserCreated = request.CreatedBy,
                    DateCreated = DateTime.UtcNow
                  
                };

                _context.QcTemplates.Add(template);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetById), new { id = template.Id }, MapToResponse(template));
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException?.Message;
                return StatusCode(500, new { message = "Error creating QC template", error = ex.Message, innerError = inner });
            }
        }

        /// <summary>
        /// Update an existing QC Template
        /// </summary>
        /// <param name="id">Template ID</param>
        /// <param name="request">QC Template request DTO</param>
        /// <returns>Updated QC Template</returns>
        [HttpPut("{id}")]
        public async Task<ActionResult<QcTemplateResponse>> Update(int id, [FromBody] QcTemplateRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var template = await _context.QcTemplates.FindAsync(id);
                if (template == null)
                    return NotFound(new { message = $"QC Template with ID {id} not found" });

                // Check if new template name is unique (excluding current template)
                var existingTemplate = await _context.QcTemplates
                    .FirstOrDefaultAsync(x => x.TemplateName == request.TemplateName && x.Id != id);
                if (existingTemplate != null)
                    return BadRequest(new { message = $"A QC Template with the name '{request.TemplateName}' already exists" });

                // Validate UpdatedBy references an existing user (to avoid FK violations)
                if (request.UpdatedBy.HasValue)
                {
                    var updatedUser = await _context.Users.FindAsync(request.UpdatedBy.Value);
                    if (updatedUser == null)
                        return BadRequest(new { message = $"UpdatedBy user with id {request.UpdatedBy.Value} does not exist" });
                }

                template.TemplateName = request.TemplateName;
                template.Description = request.Description;
                template.UserUpdated = request.UpdatedBy;
                template.DateUpdated = DateTime.UtcNow;

                _context.Entry(template).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok(MapToResponse(template));
            }
            catch (DbUpdateConcurrencyException ex)
            {
                var inner = ex.InnerException?.Message;
                return StatusCode(409, new { message = "Concurrency error: Template was modified by another user", error = ex.Message, innerError = inner });
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException?.Message;
                return StatusCode(500, new { message = "Error updating QC template", error = ex.Message, innerError = inner });
            }
        }

        /// <summary>
        /// Delete a QC Template
        /// </summary>
        /// <param name="id">Template ID</param>
        /// <returns>No content if successful</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var template = await _context.QcTemplates.FindAsync(id);
                if (template == null)
                    return NotFound(new { message = $"QC Template with ID {id} not found" });

                _context.QcTemplates.Remove(template);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting QC template", error = ex.Message });
            }
        }

        /// <summary>
        /// Search QC Templates by name
        /// </summary>
        /// <param name="searchTerm">Search term</param>
        /// <returns>List of matching QC Templates</returns>
        [HttpGet("search/{searchTerm}")]
        public async Task<ActionResult<IEnumerable<QcTemplateResponse>>> Search(string searchTerm)
        {
            try
            {
                var templates = await _context.QcTemplates
                    .Where(x => x.TemplateName.Contains(searchTerm) || x.Description.Contains(searchTerm))
                    .ToListAsync();

                var response = templates.Select(x => MapToResponse(x)).ToList();
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error searching QC templates", error = ex.Message });
            }
        }

        /// <summary>
        /// Helper method to map QcTemplate entity to QcTemplateResponse DTO
        /// </summary>
        private QcTemplateResponse MapToResponse(QcTemplate template)
        {
            return new QcTemplateResponse
            {
                Id = template.Id ?? 0,
                TemplateName = template.TemplateName,
                Description = template.Description,
                UserCreated = template.UserCreated,
                DateCreated = template.DateCreated,
                UserUpdated = template.UserUpdated,
                DateUpdated = template.DateUpdated
            };
        }
    }
}
