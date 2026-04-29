using System;
using System.Threading.Tasks;
using ERP.API.UserManagement.DTOs;
using ERP.API.UserManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ERP.API.UserManagement.Controllers
{
    [Route("api/UmTeamHierarchy")]
    [ApiController]
    public class TeamHierarchyController : ControllerBase
    {
        private readonly ITeamHierarchyService _service;
        private readonly ILogger<TeamHierarchyController> _logger;

        public TeamHierarchyController(ITeamHierarchyService service, ILogger<TeamHierarchyController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpPost("add-or-update")]
        public async Task<IActionResult> AddOrUpdate([FromBody] AddOrUpdateTeamHierarchyDto dto)
        {
            var message = await _service.AddOrUpdateTeamHierarchyAsync(dto);
            return Ok(new { status = true, message });
        }

        [HttpDelete("{userId}")]
        public async Task<IActionResult> Delete(int userId)
        {
            var message = await _service.DeleteTeamHierarchyAsync(userId);
            return Ok(new { status = true, message });
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _service.GetTeamHierarchyAsync();
            return Ok(new { status = true, message = "Team hierarchy retrieved successfully", data });
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetByUserId(int userId)
        {
            var data = await _service.GetTeamHierarchyByUserIdAsync(userId);
            if (data == null)
                return NotFound(new { status = false, message = "Team hierarchy not found for user." });
            return Ok(new { status = true, message = "Team hierarchy retrieved successfully", data });
        }
    }
}