using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using ERP.API.Models;
using ERP.API.Services;
using System.Text.Json;
using Dapper;
using System.Data;
using System.Linq;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SalesExternalCommentController : ControllerBase
    {
        private readonly SalesExternalCommentService _externalCommentService;
        private readonly SalesSummaryService _summaryService;
        private readonly IDbConnection _connection;

        public SalesExternalCommentController(
            SalesExternalCommentService externalCommentService,
            SalesSummaryService summaryService,
            IDbConnection connection)
        {
            _externalCommentService = externalCommentService ?? throw new ArgumentNullException(nameof(externalCommentService));
            _summaryService = summaryService ?? throw new ArgumentNullException(nameof(summaryService));
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SalesExternalComment>>> GetAll()
        {
            var whereClause = @"isactive = true 
                AND description IS NOT NULL 
                AND description != ''
                AND description != 'string'
                AND LENGTH(TRIM(description)) > 0 
                ORDER BY date_time DESC";
            var comments = await _externalCommentService.GetAllAsync(whereClause, new { });
            return Ok(comments);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SalesExternalComment>> GetById(int id)
        {
            var comment = await _externalCommentService.GetByIdAsync(id);
            if (comment == null)
                return NotFound($"Comment with ID {id} not found");

            return Ok(comment);
        }

        [HttpGet("stage/{stage}/{stageItemId}")]
        public async Task<ActionResult<IEnumerable<SalesExternalComment>>> GetByStage(string stage, string stageItemId)
        {
            var query = @"SELECT DISTINCT ON (activity_id) 
                id, title, description, date_time, stage, 
                stage_item_id, isactive, activity_id, user_created, 
                date_created, user_updated, date_updated
            FROM sales_external_comments
            WHERE isactive = true 
                AND stage = @Stage 
                AND stage_item_id = @StageItemId 
                AND description IS NOT NULL 
                AND description != ''
                AND description != 'string'
                AND LENGTH(TRIM(description)) > 0
            ORDER BY activity_id, 
                COALESCE(date_updated, date_created) DESC";
            
            var comments = await _connection.QueryAsync<SalesExternalComment>(query, new { Stage = stage, StageItemId = stageItemId });
            return Ok(comments);
        }

        [HttpPost]
        public async Task<ActionResult<int>> Create(SalesExternalComment comment)
        {
            if (comment == null)
                return BadRequest("Comment cannot be null");

            // Enforce "comment added" title and prevent "string" default value
            comment.Title = "comment added";
            
            // Prevent "string" default value for description
            if (string.IsNullOrEmpty(comment.Description) || comment.Description == "string")
                return BadRequest("Description cannot be empty or default value");

            comment.IsActive = true;
            comment.DateTime = DateTime.UtcNow;
            comment.DateCreated = DateTime.UtcNow;

            var id = await _externalCommentService.CreateAsync(comment);

            // Create a summary entry with consistent title
            var summary = new SalesSummary
            {
                Title = "comment added",
                Description = comment.Description,
                DateTime = comment.DateTime ?? DateTime.UtcNow,
                IsActive = true,
                Stage = comment.Stage,
                StageItemId = comment.StageItemId,
                UserCreated = comment.UserCreated,
                DateCreated = comment.DateCreated ?? DateTime.UtcNow,
                Entities = JsonSerializer.Serialize(new { CommentId = id, ActivityId = comment.ActivityId })
            };

            await _summaryService.CreateAsync(summary);
            return CreatedAtAction(nameof(GetById), new { id }, id);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, SalesExternalComment comment)
        {
            if (id != comment.Id)
                return BadRequest("ID mismatch");

            // Enforce "comment updated" title and prevent "string" default value
            comment.Title = "comment updated";
            
            // Prevent "string" default value for description
            if (string.IsNullOrEmpty(comment.Description) || comment.Description == "string")
                return BadRequest("Description cannot be empty or default value");

            comment.DateUpdated = DateTime.UtcNow;
            await _externalCommentService.UpdateAsync(comment);

            // Update summary with consistent title
            var summary = new SalesSummary
            {
                Title = "comment updated",
                Description = comment.Description,
                DateTime = DateTime.UtcNow,
                IsActive = true,
                Stage = comment.Stage,
                StageItemId = comment.StageItemId,
                UserCreated = comment.UserUpdated ?? comment.UserCreated,
                DateCreated = DateTime.UtcNow,
                Entities = JsonSerializer.Serialize(new { CommentId = id, ActivityId = comment.ActivityId })
            };

            await _summaryService.CreateAsync(summary);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _externalCommentService.DeleteAsync(id);
            if (!success)
                return NotFound($"Comment with ID {id} not found");

            return NoContent();
        }

        [HttpPost("activity")]
        public async Task<ActionResult<int>> CreateActivityComment(SalesExternalComment comment)
        {
            try
            {
                // Check for existing comment with same activity ID to avoid duplicates
                var existingComments = await _connection.QueryAsync<SalesExternalComment>(
                    "SELECT * FROM sales_external_comments WHERE activity_id = @ActivityId AND isactive = true",
                    new { ActivityId = comment.ActivityId }
                );

                if (existingComments.Any())
                {
                    return BadRequest("Comment already exists for this activity");
                }

                var activityType = DetermineActivityType(comment.ActivityId);
                var title = string.IsNullOrEmpty(comment.Title) || comment.Title == "string" 
                    ? GetActivityTitle(activityType)
                    : comment.Title;
                    
                var description = string.IsNullOrEmpty(comment.Description) || comment.Description == "string" 
                    ? GetDefaultActivityDescription(activityType)
                    : comment.Description;

                var parameters = new DynamicParameters();
                parameters.Add("p_user_id", comment.UserCreated);
                parameters.Add("p_title", title);
                parameters.Add("p_description", description);
                parameters.Add("p_stage", comment.Stage);
                parameters.Add("p_stage_item_id", comment.StageItemId);
                parameters.Add("p_activity_id", comment.ActivityId);
                parameters.Add("p_activity_type", activityType);

                await _connection.ExecuteAsync(
                    "CALL sp_manage_activity_external_comments(@p_user_id, @p_title, @p_description, @p_stage, @p_stage_item_id, @p_activity_id, @p_activity_type, NULL)",
                    parameters
                );

                // Create summary entry for activity comment
                var summary = new SalesSummary
                {
                    Title = title,
                    Description = description,
                    DateTime = DateTime.UtcNow,
                    IsActive = true,
                    Stage = comment.Stage,
                    StageItemId = comment.StageItemId,
                    UserCreated = comment.UserCreated,
                    DateCreated = DateTime.UtcNow,
                    Entities = JsonSerializer.Serialize(new { ActivityId = comment.ActivityId })
                };

                await _summaryService.CreateAsync(summary);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error creating activity comment: {ex.Message}");
            }
        }

        [HttpPut("activity/{id}")]
        public async Task<IActionResult> UpdateActivityComment(int id, SalesExternalComment comment)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("p_user_id", comment.UserUpdated ?? comment.UserCreated);
                parameters.Add("p_title", comment.Title ?? "Activity Comment Update");
                parameters.Add("p_description", comment.Description);
                parameters.Add("p_stage", comment.Stage);
                parameters.Add("p_stage_item_id", comment.StageItemId);
                parameters.Add("p_activity_id", comment.ActivityId);
                parameters.Add("p_activity_type", DetermineActivityType(comment.ActivityId));
                parameters.Add("p_comment_id", id);

                await _connection.ExecuteAsync(
                    "CALL sp_manage_activity_external_comments(@p_user_id, @p_title, @p_description, @p_stage, @p_stage_item_id, @p_activity_id, @p_activity_type, @p_comment_id)",
                    parameters
                );

                // Create summary entry for updated comment
                var summary = new SalesSummary
                {
                    Title = $"Updated {comment.Stage} activity comment",
                    Description = comment.Description,
                    DateTime = DateTime.UtcNow,
                    IsActive = true,
                    Stage = comment.Stage,
                    StageItemId = comment.StageItemId,
                    UserCreated = comment.UserUpdated ?? comment.UserCreated,
                    DateCreated = DateTime.UtcNow,
                    Entities = JsonSerializer.Serialize(new { CommentId = id, ActivityId = comment.ActivityId })
                };

                await _summaryService.CreateAsync(summary);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error updating activity comment: {ex.Message}");
            }
        }

        [HttpGet("activity/{activityId}")]
        public async Task<ActionResult<IEnumerable<SalesExternalComment>>> GetActivityComments(string activityId)
        {
            if (string.IsNullOrEmpty(activityId))
                return BadRequest("Activity ID cannot be null or empty");

            var whereClause = @"isactive = true 
                AND activity_id = @ActivityId 
                AND description IS NOT NULL 
                AND description != '' 
                AND description != 'string'
                AND LENGTH(TRIM(description)) > 0
                ORDER BY date_time DESC";
            var parameters = new { ActivityId = activityId };
            var comments = await _externalCommentService.GetAllAsync(whereClause, parameters);
            return Ok(comments);
        }

        private string DetermineActivityType(string? activityId)
        {
            if (string.IsNullOrEmpty(activityId)) return "Unknown";
            
            // Format is typically: Type-ID (e.g., "Meeting-123", "Call-456")
            var parts = activityId.Split('-');
            return parts.Length > 0 ? parts[0] : "Unknown";
        }

        private string GetActivityTitle(string activityType)
        {
            return activityType switch
            {
                "Event" => "Event",
                "Meeting" => "Meeting Scheduled",
                "Call" => "New Call Comment",
                "Task" => "New Task Comment",
                _ => "New Activity Comment"
            };
        }

        private string GetDefaultActivityDescription(string activityType)
        {
            return activityType switch
            {
                "Event" => $"Event scheduled on {DateTime.UtcNow:MMMM dd}",
                "Meeting" => $"Meeting scheduled for {DateTime.UtcNow:MMMM dd}",
                "Call" => "Call successfully completed",
                "Task" => "Task created",
                _ => "Activity created"
            };
        }
    }
}