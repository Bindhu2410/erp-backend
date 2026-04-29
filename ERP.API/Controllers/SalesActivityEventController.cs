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
    [Route("api/[controller]")]
    [Consumes("application/json")]
    [Produces("application/json")]
    public class SalesActivityEventController : ControllerBase
    {
        private readonly SalesActivityEventService _activityEventService;
        private readonly SalesSummaryService _summaryService;
        private readonly SalesExternalCommentService _externalCommentService;
        private readonly JsonSerializerOptions _jsonOptions;

        public SalesActivityEventController(
            SalesActivityEventService activityEventService,
            SalesSummaryService summaryService,
            SalesExternalCommentService externalCommentService)
        {
            _activityEventService = activityEventService;
            _summaryService = summaryService;
            _externalCommentService = externalCommentService;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new TimeSpanJsonConverter() }
            };
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SalesActivityEvent>>> GetAll()
        {
            var events = await _activityEventService.GetAllAsync();
            return Ok(events);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SalesActivityEvent>> GetById(int id)
        {
            var evt = await _activityEventService.GetByIdAsync(id);
            if (evt == null)
                return NotFound($"Event with ID {id} not found");

            return Ok(evt);
        }

        [HttpGet("stage/{stage}/{stageItemId}")]
        public async Task<ActionResult<IEnumerable<SalesActivityEvent>>> GetByStage(string stage, string stageItemId)
        {
            if (!SalesStage.IsValid(stage))
                return BadRequest($"Invalid stage. Valid stages are: {string.Join(", ", SalesStage.ValidStages)}");

            if (stage.Equals(SalesStage.Lead, StringComparison.OrdinalIgnoreCase))
            {
                if (!int.TryParse(stageItemId, out int leadId))
                    return BadRequest("For Lead stage, stageItemId must be a valid sales_leads table ID");
            }

            var events = await _activityEventService.GetByStageAsync(stage, stageItemId);
            if (!events.Any())
                return NotFound($"No events found for stage {stage} and item ID {stageItemId}");

            return Ok(events);
        }

        [HttpPost]
        [Consumes("application/json")]
        [Produces("application/json")]
        public async Task<ActionResult<int>> Create([FromBody] SalesActivityEvent evt)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // EventTitle is now optional

            if (string.IsNullOrEmpty(evt.Stage))
                return BadRequest("Stage is required");

            if (!SalesStage.IsValid(evt.Stage))
                return BadRequest($"Invalid stage. Valid stages are: {string.Join(", ", SalesStage.ValidStages)}");

            if (string.IsNullOrEmpty(evt.StageItemId))
                return BadRequest("Stage Item ID is required");

            // For Lead stage, validate that StageItemId is a valid integer
            if (evt.Stage.Equals(SalesStage.Lead, StringComparison.OrdinalIgnoreCase))
            {
                if (!int.TryParse(evt.StageItemId, out int leadId))
                    return BadRequest("For Lead stage, StageItemId must be a valid sales_leads table ID");
            }

            // Validate dates
            if (evt.EndDate < evt.StartDate)
                return BadRequest("End Date cannot be earlier than Start Date");

            if (evt.EndDate == evt.StartDate && evt.EndTime <= evt.StartTime)
                return BadRequest("End Time must be later than Start Time when Start Date and End Date are the same");

            // Set default values
            evt.IsActive = true;
            evt.Description ??= string.Empty;
            evt.Comments ??= string.Empty;
            evt.Status ??= "Scheduled";
            evt.Priority ??= "Normal";
            evt.EventLocation ??= string.Empty;
            evt.Participant ??= string.Empty;
            evt.FileUrl ??= string.Empty;
            evt.EventId ??= string.Empty;
            evt.StartDate = evt.StartDate.Date.Add(evt.StartTime);
            evt.EndDate = evt.EndDate.Date.Add(evt.EndTime);
            evt.StartTime = evt.StartTime;
            evt.EndTime = evt.EndTime;
            evt.DateCreated = DateTime.UtcNow;

            var id = await _activityEventService.CreateAsync(evt);

            // If there are comments, create an external comment
            if (!string.IsNullOrWhiteSpace(evt.Comments))
            {
                var externalComment = new SalesExternalComment
                {
                    Title = "New Event Comment",
                    Description = evt.Comments,
                    DateTime = DateTime.UtcNow,
                    Stage = evt.Stage,
                    StageItemId = evt.StageItemId,
                    IsActive = true,
                    ActivityId = $"Event-{id}",
                    UserCreated = evt.UserCreated,
                    DateCreated = DateTime.UtcNow
                };

                await _externalCommentService.CreateAsync(externalComment);
            }

            // Create a summary entry
            var summary = new SalesSummary
            {
                IconUrl = "/icons/event.png",
                Title = $"event status updated - {evt.Status}",
                Description = evt.Description,
                DateTime = evt.StartDate,
                IsActive = true,
                Stage = evt.Stage,
                StageItemId = evt.StageItemId,
                UserCreated = evt.UserCreated,
                DateCreated = evt.DateCreated,
                Entities = JsonSerializer.Serialize(new { EventId = id })
            };

            await _summaryService.CreateAsync(summary);

            return CreatedAtAction(nameof(GetById), new { id }, id);
        }

        [HttpPut("{id}")]
        [Consumes("application/json")]
        [Produces("application/json")]
        public async Task<IActionResult> Update(int id, [FromBody] SalesActivityEvent evt)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != evt.Id)
                return BadRequest("ID mismatch");

            // EventTitle is now optional

            if (evt.StartDate == default)
                return BadRequest("Start Date is required");

            if (evt.EndDate == default)
                return BadRequest("End Date is required");

            if (evt.StartTime == default)
                return BadRequest("Start Time is required");

            if (evt.EndTime == default)
                return BadRequest("End Time is required");

            if (string.IsNullOrEmpty(evt.Stage))
                return BadRequest("Stage is required");

            if (!SalesStage.IsValid(evt.Stage))
                return BadRequest($"Invalid stage. Valid stages are: {string.Join(", ", SalesStage.ValidStages)}");

            if (string.IsNullOrEmpty(evt.StageItemId))
                return BadRequest("Stage Item ID is required");

            // For Lead stage, validate that StageItemId is a valid integer
            if (evt.Stage.Equals(SalesStage.Lead, StringComparison.OrdinalIgnoreCase))
            {
                if (!int.TryParse(evt.StageItemId, out int leadId))
                    return BadRequest("For Lead stage, StageItemId must be a valid sales_leads table ID");
            }

            // Get existing event to check if status has changed
            var existingEvent = await _activityEventService.GetByIdAsync(id);
            if (existingEvent == null)
                return NotFound($"Event with ID {id} not found");

            bool statusChanged = !string.Equals(existingEvent.Status, evt.Status, StringComparison.OrdinalIgnoreCase);

            // Check if comments have been updated
            bool commentsChanged = existingEvent != null && existingEvent.Comments != evt.Comments;

            // Validate dates
            if (evt.EndDate < evt.StartDate)
                return BadRequest("End Date cannot be earlier than Start Date");

            if (evt.EndDate == evt.StartDate && evt.EndTime <= evt.StartTime)
                return BadRequest("End Time must be later than Start Time when Start Date and End Date are the same");

            // Set default values
            evt.Description ??= string.Empty;
            evt.Comments ??= string.Empty;
            evt.Status ??= "Scheduled";
            evt.Priority ??= "Normal";
            evt.EventLocation ??= string.Empty;
            evt.Participant ??= string.Empty;
            evt.FileUrl ??= string.Empty;
            evt.EventId ??= string.Empty;
            //evt.As ??= string.Empty;

            evt.StartDate = evt.StartDate.Date.Add(evt.StartTime);
            evt.EndDate = evt.EndDate.Date.Add(evt.EndTime);
            evt.StartTime = evt.StartTime;
            evt.EndTime = evt.EndTime;
            evt.DateUpdated = DateTime.UtcNow;

            var success = await _activityEventService.UpdateAsync(evt);
            if (!success)
                return NotFound($"Event with ID {id} not found");

            // If comments were updated, create an external comment
            if (commentsChanged && !string.IsNullOrWhiteSpace(evt.Comments))
            {
                var externalComment = new SalesExternalComment
                {
                    Title = "Event Comment Updated",
                    Description = evt.Comments,
                    DateTime = DateTime.UtcNow,
                    Stage = evt.Stage,
                    StageItemId = evt.StageItemId,
                    IsActive = true,
                    ActivityId = $"Event-{id}",
                    UserCreated = evt.UserUpdated ?? evt.UserCreated,
                    DateCreated = DateTime.UtcNow
                };

                await _externalCommentService.CreateAsync(externalComment);
            }

            // Create summary entry if status has changed
            if (statusChanged)
            {
                var summary = new SalesSummary
                {
                    IconUrl = "/icons/event.png",
                    Title = $"event status updated - {evt.Status}",
                    Description = evt.Description,
                    DateTime = evt.StartDate,
                    IsActive = true,
                    Stage = evt.Stage,
                    StageItemId = evt.StageItemId,
                    UserCreated = evt.UserUpdated ?? evt.UserCreated,
                    DateCreated = DateTime.UtcNow,
                    Entities = JsonSerializer.Serialize(new { EventId = id })
                };

                await _summaryService.CreateAsync(summary);
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var evt = await _activityEventService.GetByIdAsync(id);
            if (evt == null)
                return NotFound($"Event with ID {id} not found");

            var success = await _activityEventService.DeleteAsync(id);
            if (!success)
                return NotFound($"Event with ID {id} not found");

            // Create a summary entry for the deletion
            var summary = new SalesSummary
            {
                IconUrl = "/icons/event.png",
                Title = "event deleted",
                Description = evt.Description,
                DateTime = DateTime.UtcNow,
                IsActive = true,
                Stage = evt.Stage,
                StageItemId = evt.StageItemId,
                UserCreated = evt.UserUpdated ?? evt.UserCreated,
                DateCreated = DateTime.UtcNow,
                Entities = JsonSerializer.Serialize(new { EventId = id })
            };

            await _summaryService.CreateAsync(summary);

            return NoContent();
        }
    }
}