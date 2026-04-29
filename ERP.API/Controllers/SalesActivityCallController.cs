using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using ERP.API.Models;
using ERP.API.Services;
using System.Linq;
using System.Text.Json;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Consumes("application/json")]
    [Produces("application/json")]
    public class SalesActivityCallController : ControllerBase
    {
        private readonly SalesActivityCallService _activityCallService;
        private readonly SalesSummaryService _summaryService;
        private readonly SalesExternalCommentService _externalCommentService;

        public SalesActivityCallController(
            SalesActivityCallService activityCallService,
            SalesSummaryService summaryService,
            SalesExternalCommentService externalCommentService)
        {
            _activityCallService = activityCallService;
            _summaryService = summaryService;
            _externalCommentService = externalCommentService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SalesActivityCall>>> GetAll()
        {
            var calls = await _activityCallService.GetAllAsync();
            return Ok(calls);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SalesActivityCall>> GetById(int id)
        {
            var call = await _activityCallService.GetByIdAsync(id);
            if (call == null)
                return NotFound($"Call with ID {id} not found");

            return Ok(call);
        }

        [HttpGet("stage/{stage}/{stageItemId}")]
        public async Task<ActionResult<IEnumerable<SalesActivityCall>>> GetByStage(string stage, string stageItemId)
        {
            var calls = await _activityCallService.GetByStageAsync(stage, stageItemId);
            return Ok(calls);
        }

        [HttpGet("upcoming")]
        public async Task<ActionResult<IEnumerable<SalesActivityCall>>> GetUpcomingCalls()
        {
            var calls = await _activityCallService.GetUpcomingCallsAsync();
            return Ok(calls);
        }
        [HttpPost]
        [Consumes("application/json")]
        [Produces("application/json")]
        public async Task<ActionResult<int>> Create([FromBody] SalesActivityCall call)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // CallTitle is now optional

            call.IsActive = true;
            call.Comments ??= string.Empty;
            call.Status ??= "Scheduled";
            call.FileUrl ??= string.Empty;
            call.DateCreated = DateTime.UtcNow;

            var id = await _activityCallService.CreateAsync(call);

            // If there are comments, create an external comment
            if (!string.IsNullOrWhiteSpace(call.Comments))
            {
                var externalComment = new SalesExternalComment
                {
                    Title = "New Call Comment",
                    Description = call.Comments,
                    DateTime = DateTime.UtcNow,
                    Stage = call.Stage,
                    StageItemId = call.StageItemId,
                    IsActive = true,
                    ActivityId = $"Call-{id}",
                    UserCreated = call.UserCreated,
                    DateCreated = DateTime.UtcNow
                };

                await _externalCommentService.CreateAsync(externalComment);
            }

            // Create summary entry
            var summary = new SalesSummary
            {
                IconUrl = "/icons/call.png",
                Title = $"Call status updated - {call.Status}",
                Description = call.CallTitle,
                DateTime = call.CallDateTime,
                IsActive = true,
                Stage = call.Stage,
                StageItemId = call.StageItemId,
                UserCreated = call.UserCreated,
                DateCreated = call.DateCreated,
                Entities = JsonSerializer.Serialize(new { CallId = id })
            };

            await _summaryService.CreateAsync(summary);

            return CreatedAtAction(nameof(GetById), new { id }, id);
        }

        [HttpPut("{id}")]
        [Consumes("application/json")]
        [Produces("application/json")]
        public async Task<IActionResult> Update(int id, [FromBody] SalesActivityCall call)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != call.Id)
                return BadRequest("ID mismatch");

            // Check if comments have been updated
            var existingCall = await _activityCallService.GetByIdAsync(id);
            bool commentsChanged = existingCall != null && existingCall.Comments != call.Comments;

            call.Comments ??= string.Empty;
            call.Status ??= "Scheduled";
            call.FileUrl ??= string.Empty;
            call.DateUpdated = DateTime.UtcNow;

            var success = await _activityCallService.UpdateAsync(call);
            if (!success)
                return NotFound($"Call with ID {id} not found");

            // If comments were updated, create an external comment
            if (commentsChanged && !string.IsNullOrWhiteSpace(call.Comments))
            {
                var externalComment = new SalesExternalComment
                {
                    Title = "Call Comment Updated",
                    Description = call.Comments,
                    DateTime = DateTime.UtcNow,
                    Stage = call.Stage,
                    StageItemId = call.StageItemId,
                    IsActive = true,
                    ActivityId = $"Call-{id}",
                    UserCreated = call.UserUpdated ?? call.UserCreated,
                    DateCreated = DateTime.UtcNow
                };

                await _externalCommentService.CreateAsync(externalComment);
            }

            // Create summary entry
            var summary = new SalesSummary
            {
                IconUrl = "/icons/call.png",
                Title = $"Call updated - {call.Status}",
                Description = call.CallTitle,
                DateTime = call.CallDateTime,
                IsActive = true,
                Stage = call.Stage,
                StageItemId = call.StageItemId,
                UserCreated = call.UserUpdated ?? call.UserCreated,
                DateCreated = DateTime.UtcNow,
                Entities = JsonSerializer.Serialize(new { CallId = id })
            };

            await _summaryService.CreateAsync(summary);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            // Get the call before deleting
            var call = await _activityCallService.GetByIdAsync(id);
            if (call == null)
                return NotFound($"Call with ID {id} not found");

            var success = await _activityCallService.DeleteAsync(id);
            if (!success)
                return NotFound($"Call with ID {id} not found");

            // Create a summary entry for the deletion
            var summary = new SalesSummary
            {
                IconUrl = "/icons/call.png",
                Title = "call deleted",
                Description = $"Call with {call.CallWith}",
                DateTime = DateTime.UtcNow,
                IsActive = true,
                Stage = call.Stage,
                StageItemId = call.StageItemId,
                UserCreated = call.UserUpdated ?? call.UserCreated,
                DateCreated = DateTime.UtcNow,
                Entities = JsonSerializer.Serialize(new { CallId = id })
            };

            await _summaryService.CreateAsync(summary);

            return NoContent();
        }
    }
}