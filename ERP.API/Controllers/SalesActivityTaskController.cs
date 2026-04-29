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
    public class SalesActivityTaskController : ControllerBase
    {
        private readonly SalesActivityTaskService _activityTaskService;
        private readonly SalesSummaryService _summaryService;
        private readonly SalesExternalCommentService _externalCommentService;

        public SalesActivityTaskController(
            SalesActivityTaskService activityTaskService,
            SalesSummaryService summaryService,
            SalesExternalCommentService externalCommentService)
        {
            _activityTaskService = activityTaskService;
            _summaryService = summaryService;
            _externalCommentService = externalCommentService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SalesActivityTask>>> GetAll()
        {
            var tasks = await _activityTaskService.GetAllAsync();
            return Ok(tasks);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SalesActivityTask>> GetById(int id)
        {
            var task = await _activityTaskService.GetByIdAsync(id);
            if (task == null)
                return NotFound($"Task with ID {id} not found");

            return Ok(task);
        }

        [HttpGet("stage/{stage}/{stageItemId}")]
        public async Task<ActionResult<IEnumerable<SalesActivityTask>>> GetByStage(string stage, string stageItemId)
        {
            var tasks = await _activityTaskService.GetByStageAsync(stage, stageItemId);
            return Ok(tasks);
        }

        [HttpGet("upcoming")]
        public async Task<ActionResult<IEnumerable<SalesActivityTask>>> GetUpcomingTasks()
        {
            var tasks = await _activityTaskService.GetUpcomingTasksAsync();
            return Ok(tasks);
        }

        [HttpPost]
        public async Task<ActionResult<int>> Create([FromBody] SalesActivityTask task)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Validate required fields
            if (string.IsNullOrEmpty(task.TaskName))
                return BadRequest("Task Name is required");

            // Set default values
            task.IsActive = true;
            task.Comments ??= string.Empty;
            task.Status ??= "Open";
            task.Priority ??= "Normal";
            //task.AssignedTo ??= string.Empty;
            task.DateCreated = DateTime.UtcNow;

            var id = await _activityTaskService.CreateAsync(task);

            // If there are comments, create an external comment
            if (!string.IsNullOrWhiteSpace(task.Comments))
            {
                var externalComment = new SalesExternalComment
                {
                    Title = "New Task Comment",
                    Description = task.Comments,
                    DateTime = DateTime.UtcNow,
                    Stage = task.Stage,
                    StageItemId = task.StageItemId,
                    IsActive = true,
                    ActivityId = $"Task-{id}",
                    UserCreated = task.UserCreated,
                    DateCreated = DateTime.UtcNow
                };

                await _externalCommentService.CreateAsync(externalComment);
            }

            // Create summary entry
            var summary = new SalesSummary
            {
                IconUrl = "/icons/task.png",
                Title = $"Task status updated - {task.Status}",
                Description = task.TaskName,
                DateTime = task.DueDate,
                IsActive = true,
                Stage = task.Stage,
                StageItemId = task.StageItemId,
                UserCreated = task.UserCreated,
                DateCreated = task.DateCreated,
                Entities = JsonSerializer.Serialize(new { TaskId = id })
            };

            await _summaryService.CreateAsync(summary);

            return CreatedAtAction(nameof(GetById), new { id }, id);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, SalesActivityTask task)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != task.Id)
                return BadRequest("ID mismatch");

            // Check if comments have been updated
            var existingTask = await _activityTaskService.GetByIdAsync(id);
            bool commentsChanged = existingTask != null && existingTask.Comments != task.Comments;

            task.Comments ??= string.Empty;
            task.Status ??= "Open";
            task.Priority ??= "Normal";
            //task.AssignedTo ??= string.Empty;
            task.DateUpdated = DateTime.UtcNow;

            var success = await _activityTaskService.UpdateAsync(task);
            if (!success)
                return NotFound($"Task with ID {id} not found");

            // If comments were updated, create an external comment
            if (commentsChanged && !string.IsNullOrWhiteSpace(task.Comments))
            {
                var externalComment = new SalesExternalComment
                {
                    Title = "Task Comment Updated",
                    Description = task.Comments,
                    DateTime = DateTime.UtcNow,
                    Stage = task.Stage,
                    StageItemId = task.StageItemId,
                    IsActive = true,
                    ActivityId = $"Task-{id}",
                    UserCreated = task.UserUpdated ?? task.UserCreated,
                    DateCreated = DateTime.UtcNow
                };

                await _externalCommentService.CreateAsync(externalComment);
            }

            // Create summary entry
            var summary = new SalesSummary
            {
                IconUrl = "/icons/task.png",
                Title = $"Task updated - {task.Status}",
                Description = task.TaskName,
                DateTime = task.DueDate,
                IsActive = true,
                Stage = task.Stage,
                StageItemId = task.StageItemId,
                UserCreated = task.UserUpdated ?? task.UserCreated,
                DateCreated = DateTime.UtcNow,
                Entities = JsonSerializer.Serialize(new { TaskId = id })
            };

            await _summaryService.CreateAsync(summary);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            // Get the task before deleting
            var task = await _activityTaskService.GetByIdAsync(id);
            if (task == null)
                return NotFound($"Task with ID {id} not found");

            var success = await _activityTaskService.DeleteAsync(id);
            if (!success)
                return NotFound($"Task with ID {id} not found");

            // Create a summary entry for the deletion
            var summary = new SalesSummary
            {
                IconUrl = "/icons/task.png",
                Title = "task deleted",
                Description = task.TaskName,
                DateTime = DateTime.UtcNow,
                IsActive = true,
                Stage = task.Stage,
                StageItemId = task.StageItemId,
                UserCreated = task.UserUpdated ?? task.UserCreated,
                DateCreated = DateTime.UtcNow,
                Entities = JsonSerializer.Serialize(new { TaskId = id })
            };

            await _summaryService.CreateAsync(summary);

            return NoContent();
        }

        [HttpGet("subtasks/{parentTaskId}")]
        public async Task<ActionResult<IEnumerable<SalesActivityTask>>> GetByParentTaskId(int parentTaskId)
        {
            var tasks = await _activityTaskService.GetByParentTaskIdAsync(parentTaskId);
            return Ok(tasks);
        }
    }
}