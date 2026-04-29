using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using ERP.API.Models;
using ERP.API.Services;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InternalDiscussionController : ControllerBase
    {
        private readonly InternalDiscussionService _internalDiscussionService;
        private readonly SalesSummaryService _summaryService;

        public InternalDiscussionController(
            InternalDiscussionService internalDiscussionService,
            SalesSummaryService summaryService)
        {
            _internalDiscussionService = internalDiscussionService;
            _summaryService = summaryService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<InternalDiscussion>>> GetAll()
        {
            var discussions = await _internalDiscussionService.GetAllAsync();
            return Ok(discussions);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<InternalDiscussion>> GetById(int id)
        {
            var discussion = await _internalDiscussionService.GetByIdAsync(id);
            if (discussion == null)
                return NotFound($"Discussion with ID {id} not found");

            return Ok(discussion);
        }

        [HttpGet("stage/{stage}/{stageItemId}")]
        public async Task<ActionResult<IEnumerable<InternalDiscussion>>> GetByStage(string stage, string stageItemId)
        {
            var discussions = await _internalDiscussionService.GetByStageAsync(stage, stageItemId);
            if (!discussions.Any())
                return NotFound($"No discussions found for stage {stage} and item ID {stageItemId}");

            return Ok(discussions);
        }

        [HttpGet("replies/{parentId}")]
        public async Task<ActionResult<IEnumerable<InternalDiscussion>>> GetReplies(int parentId)
        {
            var replies = await _internalDiscussionService.GetRepliesAsync(parentId);
            if (!replies.Any())
                return NotFound($"No replies found for discussion ID {parentId}");

            return Ok(replies);
        }

        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<object>> Create([FromBody] InternalDiscussion discussion)
        {
            try
            {
                // Validate required fields
                var validationErrors = new List<string>();
                if (string.IsNullOrEmpty(discussion.Comment))
                    validationErrors.Add("Comment is required");
                if (string.IsNullOrEmpty(discussion.Stage))
                    validationErrors.Add("Stage is required");
                if (string.IsNullOrEmpty(discussion.StageItemId))
                    validationErrors.Add("Stage Item ID is required");
                if (string.IsNullOrEmpty(discussion.UserName))
                    validationErrors.Add("Username is required");

                if (validationErrors.Any())
                {
                    return BadRequest(new
                    {
                        message = "Validation failed",
                        statusCode = 400,
                        errors = validationErrors
                    });
                }

                discussion.DateCreated = DateTime.UtcNow;
                var id = await _internalDiscussionService.CreateAsync(discussion);

                var summary = new SalesSummary
                {
                    Title = "Internal discussion added",
                    Description = $"New internal comment added to {discussion.Stage} by {discussion.UserName}",
                    DateTime = DateTime.UtcNow,
                    Stage = discussion.Stage,
                    StageItemId = discussion.StageItemId,
                    IsActive = true,
                    Entities = System.Text.Json.JsonSerializer.Serialize(new 
                    { 
                        discussion.Stage,
                        discussion.StageItemId,
                        DiscussionId = id,
                        discussion.UserName
                    })
                };
                await _summaryService.CreateAsync(summary);

                // Return a clean JSON response
                return Created($"api/InternalDiscussion/{id}", new
                {
                    id,
                    comment = discussion.Comment,
                    stage = discussion.Stage,
                    stageItemId = discussion.StageItemId,
                    userName = discussion.UserName,
                    dateCreated = discussion.DateCreated
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "An error occurred while creating the discussion",
                    statusCode = 500,
                    errors = new[] { ex.Message }
                });
            }
        }        [HttpPut("{id}")]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(int id, [FromBody] InternalDiscussion discussion)
        {
            if (id != discussion.Id)
                return BadRequest("ID mismatch");

            // Validate required fields
            if (string.IsNullOrEmpty(discussion.Comment))
                return BadRequest("Comment is required");
            if (string.IsNullOrEmpty(discussion.Stage))
                return BadRequest("Stage is required");
            if (string.IsNullOrEmpty(discussion.StageItemId))
                return BadRequest("Stage Item ID is required");
            if (string.IsNullOrEmpty(discussion.UserName))
                return BadRequest("Username is required");

            discussion.DateUpdated = DateTime.UtcNow;
            await _internalDiscussionService.UpdateAsync(discussion);

            var summary = new SalesSummary
            {
                Title = "Internal discussion updated",
                Description = $"Internal comment updated in {discussion.Stage} by {discussion.UserName}",
                DateTime = DateTime.UtcNow,
                Stage = discussion.Stage,
                StageItemId = discussion.StageItemId,
                IsActive = true,
                Entities = System.Text.Json.JsonSerializer.Serialize(new 
                { 
                    Stage = discussion.Stage,
                    StageItemId = discussion.StageItemId,
                    DiscussionId = id,
                    UserName = discussion.UserName
                })
            };
            await _summaryService.CreateAsync(summary);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var discussion = await _internalDiscussionService.GetByIdAsync(id);
            if (discussion == null)
                return NotFound($"Discussion with ID {id} not found");

            var summary = new SalesSummary
            {
                Title = "Internal discussion deleted",
                Description = $"Internal comment deleted from {discussion.Stage}",
                DateTime = DateTime.UtcNow,
                Stage = discussion.Stage,
                StageItemId = discussion.StageItemId,
                IsActive = true,
                Entities = System.Text.Json.JsonSerializer.Serialize(new 
                { 
                    Stage = discussion.Stage,
                    StageItemId = discussion.StageItemId,
                    DiscussionId = id
                })
            };
            await _summaryService.CreateAsync(summary);

            await _internalDiscussionService.DeleteAsync(id);
            return NoContent();
        }

        [HttpPost("{id}/seen/{userId}")]
        public async Task<IActionResult> MarkAsSeen(int id, string userId)
        {
            var success = await _internalDiscussionService.MarkAsSeenAsync(id, userId);
            if (!success)
                return NotFound($"Discussion with ID {id} not found");

            return NoContent();
        }
    }
}
