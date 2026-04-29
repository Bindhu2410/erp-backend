using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using ERP.API.Models;
using ERP.API.Services;
using System.Text.Json;
using System.Text.Json.Serialization;
using ERP.API.JsonConverters;
using System.Linq;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]    [Consumes("application/json")]
    [Produces("application/json")]
    public class SalesActivityMeetingController : ControllerBase
    {
        private readonly SalesActivityMeetingService _activityMeetingService;
        private readonly SalesSummaryService _summaryService;
        private readonly JsonSerializerOptions _jsonOptions;

        public SalesActivityMeetingController(
            SalesActivityMeetingService activityMeetingService,
            SalesSummaryService summaryService)
        {
            _activityMeetingService = activityMeetingService;
            _summaryService = summaryService;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new TimeSpanJsonConverter() }
            };
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SalesActivityMeeting>>> GetAll()
        {
            var meetings = await _activityMeetingService.GetAllAsync();
            return Ok(meetings);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SalesActivityMeeting>> GetById(int id)
        {
            var meeting = await _activityMeetingService.GetByIdAsync(id);
            if (meeting == null)
                return NotFound($"Meeting with ID {id} not found");

            return Ok(meeting);
        }

        [HttpGet("stage/{stage}/{stageItemId}")]
        public async Task<ActionResult<IEnumerable<SalesActivityMeeting>>> GetByStage(string stage, string stageItemId)
        {
            if (!SalesStage.IsValid(stage))
                return BadRequest($"Invalid stage. Valid stages are: {string.Join(", ", SalesStage.ValidStages)}");

            if (stage.Equals(SalesStage.Lead, StringComparison.OrdinalIgnoreCase))
            {
                if (!int.TryParse(stageItemId, out int leadId))
                    return BadRequest("For Lead stage, stageItemId must be a valid sales_leads table ID");
            }

            var meetings = await _activityMeetingService.GetByStageAsync(stage, stageItemId);
            if (!meetings.Any())
                return NotFound($"No meetings found for stage {stage} and item ID {stageItemId}");

            return Ok(meetings);
        }
        [HttpPost]
        [Consumes("application/json")]
        public async Task<ActionResult<int>> Create([FromBody] SalesActivityMeeting meeting)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { errors = ModelState });

            // MeetingTitle is now optional

            if (string.IsNullOrEmpty(meeting.CustomerName))
                return BadRequest("Customer Name is required");

            if (meeting.MeetingDateTime == default)
                return BadRequest("Meeting Date Time is required");

            if (string.IsNullOrEmpty(meeting.Stage))
                return BadRequest("Stage is required");

            if (!SalesStage.IsValid(meeting.Stage))
                return BadRequest($"Invalid stage. Valid stages are: {string.Join(", ", SalesStage.ValidStages)}");

            if (string.IsNullOrEmpty(meeting.StageItemId))
                return BadRequest("Stage Item ID is required");

            // For Lead stage, validate that StageItemId is a valid integer
            if (meeting.Stage.Equals(SalesStage.Lead, StringComparison.OrdinalIgnoreCase))
            {
                if (!int.TryParse(meeting.StageItemId, out int leadId))
                    return BadRequest("For Lead stage, StageItemId must be a valid sales_leads table ID");
            }            // Set default values
            meeting.Description ??= string.Empty;
            meeting.Comments ??= string.Empty;
            meeting.Address ??= string.Empty;
            meeting.Status ??= "Scheduled";
            meeting.MeetingType ??= string.Empty;
            meeting.City ??= string.Empty;
            meeting.Area ??= string.Empty;
            meeting.ParentMeeting ??= string.Empty;
            meeting.Participant ??= string.Empty;
            meeting.FileUrl ??= string.Empty;
            meeting.CustomerId ??= string.Empty;
            meeting.Duration = meeting.Duration == default ? TimeSpan.Zero : meeting.Duration; // Ensure Duration is set to zero if not provided
            meeting.DateCreated = DateTime.UtcNow;

            var id = await _activityMeetingService.CreateAsync(meeting);

            // Create a summary entry
            var summary = new SalesSummary
            {
                IconUrl = "/icons/meeting.png",
                Title = $"meeting status updated - {meeting.Status}",
                Description = $"Meeting with {meeting.CustomerName}",
                DateTime = meeting.MeetingDateTime,
                IsActive = true,
                Stage = meeting.Stage,
                StageItemId = meeting.StageItemId,
                UserCreated = meeting.UserCreated,
                DateCreated = meeting.DateCreated,
                Entities = JsonSerializer.Serialize(new { MeetingId = id })
            };

            await _summaryService.CreateAsync(summary);

            return CreatedAtAction(nameof(GetById), new { id }, id);
        }        [HttpPut("{id}")]
        [Consumes("application/json")]
        public async Task<IActionResult> Update(int id, [FromBody] SalesActivityMeeting meeting)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { errors = ModelState });

            if (id != meeting.Id)
                return BadRequest("ID mismatch");

            // Check if comments have been updated
            var existingMeeting = await _activityMeetingService.GetByIdAsync(id);
            bool commentsChanged = existingMeeting != null && existingMeeting.Comments != meeting.Comments;

            // Set default values for nullable fields
            meeting.Comments ??= string.Empty;
            meeting.Status ??= "Scheduled";
            meeting.Participant ??= string.Empty;
            meeting.FileUrl ??= string.Empty;
            meeting.Description ??= string.Empty;
            meeting.MeetingType ??= string.Empty;
            meeting.City ??= string.Empty;
            meeting.Area ??= string.Empty;
            meeting.Address ??= string.Empty;
            //meeting.AssignedTo ??= string.Empty;

            // Ensure Duration is set to zero if not provided
            meeting.Duration = meeting.Duration == default ? TimeSpan.Zero : meeting.Duration;
            meeting.DateUpdated = DateTime.UtcNow;

            var success = await _activityMeetingService.UpdateAsync(meeting);
            if (!success)
                return NotFound($"Meeting with ID {id} not found");

            // Create summary entry
            var summary = new SalesSummary
            {
                IconUrl = "/icons/meeting.png",
                Title = $"Meeting updated - {meeting.Status}",
                Description = meeting.MeetingTitle,
                DateTime = meeting.MeetingDateTime,
                IsActive = true,
                Stage = meeting.Stage,
                StageItemId = meeting.StageItemId,
                UserCreated = meeting.UserUpdated ?? meeting.UserCreated,
                DateCreated = DateTime.UtcNow,
                Entities = JsonSerializer.Serialize(new { MeetingId = id })
            };

            await _summaryService.CreateAsync(summary);

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            // Get the meeting before deleting
            var meeting = await _activityMeetingService.GetByIdAsync(id);
            if (meeting == null)
                return NotFound($"Meeting with ID {id} not found");

            var success = await _activityMeetingService.DeleteAsync(id);
            if (!success)
                return NotFound($"Meeting with ID {id} not found");

            // Create a summary entry for the deletion
            var summary = new SalesSummary
            {
                IconUrl = "/icons/meeting.png",
                Title = "meeting deleted",
                Description = $"Meeting with {meeting.CustomerName}",
                DateTime = DateTime.UtcNow,
                IsActive = true,
                Stage = meeting.Stage,
                StageItemId = meeting.StageItemId,
                UserCreated = meeting.UserUpdated ?? meeting.UserCreated,
                DateCreated = DateTime.UtcNow,
                Entities = JsonSerializer.Serialize(new { MeetingId = id })
            };

            await _summaryService.CreateAsync(summary);

            return NoContent();
        }

        private void AddError(Dictionary<string, string[]> errors, string key, string message)
        {
            errors[key] = new[] { message };
        }
    }
}