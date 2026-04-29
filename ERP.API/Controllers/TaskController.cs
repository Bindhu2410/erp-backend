using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using ERP.API.Models;
using Microsoft.Extensions.Configuration;
using Dapper;
using Npgsql;
using System.Linq;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TaskController : ControllerBase
    {
        private readonly string _connectionString;

        public TaskController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        // GET: api/Task
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskModel>>> GetTasks()
        {
            using var connection = new NpgsqlConnection(_connectionString);
            var tasks = await connection.QueryAsync<TaskModel>(@"
                SELECT t.id, t.created_at AS CreatedAt, t.updated_at AS UpdatedAt, t.user_created AS UserCreated, t.user_updated AS UserUpdated,
                       t.task_id AS TaskId, t.task_name AS TaskName, t.parent_task_id AS ParentTaskId, t.description AS Description,
                       t.task_type AS TaskType, t.status AS Status, t.priority AS Priority, t.due_date AS DueDate, t.stage AS Stage,
                       t.stage_item_id AS StageItemId, t.owner_id AS OwnerId, t.assignee_id AS AssigneeId,
                       -- t.activity_status AS ActivityStatus, t.activity_id AS ActivityId,
                       owner.firstname || ' ' || owner.lastname AS OwnerName,
                       assignee.firstname || ' ' || assignee.lastname AS AssigneeName
                FROM public.task t
                LEFT JOIN public.users owner ON t.owner_id = owner.userid
                LEFT JOIN public.users assignee ON t.assignee_id = assignee.userid
                ORDER BY t.id DESC
            ");
            return Ok(tasks);
        }

        // GET: api/Task/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<TaskModel>> GetTask(int id)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            var task = await connection.QueryFirstOrDefaultAsync<TaskModel>(@"
                SELECT t.id, t.created_at AS CreatedAt, t.updated_at AS UpdatedAt, t.user_created AS UserCreated, t.user_updated AS UserUpdated,
                       t.task_id AS TaskId, t.task_name AS TaskName, t.parent_task_id AS ParentTaskId, t.description AS Description,
                       t.task_type AS TaskType, t.status AS Status, t.priority AS Priority, t.due_date AS DueDate, t.stage AS Stage,
                       t.stage_item_id AS StageItemId, t.owner_id AS OwnerId, t.assignee_id AS AssigneeId,
                       -- t.activity_status AS ActivityStatus, t.activity_id AS ActivityId,
                       owner.firstname || ' ' || owner.lastname AS OwnerName,
                       assignee.firstname || ' ' || assignee.lastname AS AssigneeName
                FROM public.task t
                LEFT JOIN public.users owner ON t.owner_id = owner.userid
                LEFT JOIN public.users assignee ON t.assignee_id = assignee.userid
                WHERE t.id = @Id
            ", new { Id = id });
            if (task == null)
                return NotFound();
            return Ok(task);
        }

        // POST: api/Task
        [HttpPost]
        public async Task<ActionResult<TaskModel>> CreateTask([FromBody] TaskModel task)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            var sql = @"
                INSERT INTO public.task (
                    created_at, updated_at, user_created, user_updated, task_name, parent_task_id, description, task_type, status, priority, due_date, stage, stage_item_id, owner_id, assignee_id
                ) VALUES (
                    @CreatedAt, @UpdatedAt, @UserCreated, @UserUpdated, @TaskName, @ParentTaskId, @Description, @TaskType, @Status, @Priority, @DueDate, @Stage, @StageItemId, @OwnerId, @AssigneeId
                )
                RETURNING id, created_at AS CreatedAt, updated_at AS UpdatedAt, user_created AS UserCreated, user_updated AS UserUpdated,
                          task_id AS TaskId, task_name AS TaskName, parent_task_id AS ParentTaskId, description AS Description,
                          task_type AS TaskType, status AS Status, priority AS Priority, due_date AS DueDate, stage AS Stage,
                          stage_item_id AS StageItemId, owner_id AS OwnerId, assignee_id AS AssigneeId;
            ";
            var createdTask = await connection.QuerySingleAsync<TaskModel>(sql, task);
            return CreatedAtAction(nameof(GetTask), new { id = createdTask.Id }, createdTask);
        }

        // PUT: api/Task/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, [FromBody] TaskModel task)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            var sql = @"
                UPDATE public.task SET
                    updated_at = @UpdatedAt,
                    user_updated = @UserUpdated,
                    status = @Status,
                    activity_status = @ActivityStatus
                WHERE id = @Id;
            ";
            
            var affectedLines = await connection.ExecuteAsync(sql, new {
                UpdatedAt = System.DateTime.UtcNow,
                UserUpdated = task.UserUpdated ?? task.UserCreated,
                Status = task.Status ?? task.Status, // Try to handle casing if possible via model mapping, but here we just use the model property
                ActivityStatus = task.ActivityStatus,
                Id = id
            });

            if (affectedLines == 0)
                return NotFound();
            return NoContent();
        }

        // PUT: api/Task/{id}/status
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateTaskStatus(int id, [FromBody] TaskStatusUpdateModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Status))
                return BadRequest(new { message = "Status is required." });

            using var connection = new NpgsqlConnection(_connectionString);
            var sql = @"UPDATE public.task SET status = @Status, description = COALESCE(@Comments, description) WHERE id = @Id RETURNING id;";
            var updatedId = await connection.ExecuteScalarAsync<int?>(sql, new { Status = model.Status, Comments = model.Comments, Id = id });
            if (updatedId == null)
                return NotFound(new { message = $"Task with ID {id} not found." });
            return Ok(new { message = "Task status updated successfully." });
        }

        // DELETE: api/Task/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            var affectedLines = await connection.ExecuteAsync("DELETE FROM public.task WHERE id = @Id", new { Id = id });
            if (affectedLines == 0)
                return NotFound();
            return NoContent();
        }

            // GET: api/Task/by-stage?stage={stage}&stageItemId={stageItemId}
            [HttpGet("by-stage")]
            public async Task<ActionResult<IEnumerable<TaskModel>>> GetTasksByStage([FromQuery] string stage, [FromQuery] string stageItemId)
            {
                using var connection = new NpgsqlConnection(_connectionString);
                var sql = @"
                    SELECT id, created_at AS CreatedAt, updated_at AS UpdatedAt, user_created AS UserCreated, user_updated AS UserUpdated,
                           task_id AS TaskId, task_name AS TaskName, parent_task_id AS ParentTaskId, description AS Description,
                           task_type AS TaskType, status AS Status, priority AS Priority, due_date AS DueDate, stage AS Stage,
                           stage_item_id AS StageItemId, owner_id AS OwnerId, assignee_id AS AssigneeId,
                           activity_status AS ActivityStatus, activity_id AS ActivityId
                    FROM public.task
                    WHERE stage = @Stage AND stage_item_id = @StageItemId
                    ORDER BY id DESC
                ";
                var tasks = await connection.QueryAsync<TaskModel>(sql, new { Stage = stage, StageItemId = stageItemId });
                return Ok(tasks);
            }

            // GET: api/Task/my-tasks?userId={userId}&type={type}
            [HttpGet("my-tasks")]
            public async Task<ActionResult<IEnumerable<TaskModel>>> GetMyTasks([FromQuery] int userId, [FromQuery] string type = "all")
            {
                using var connection = new NpgsqlConnection(_connectionString);
                var sql = "";

                if (type.ToLower() == "assigned")
                {
                    // Return only tasks where user is assignee
                    sql = @"
                        SELECT t.id, t.created_at AS CreatedAt, t.updated_at AS UpdatedAt, 
                               t.user_created AS UserCreated, t.user_updated AS UserUpdated,
                               t.task_id AS TaskId, t.task_name AS TaskName, t.parent_task_id AS ParentTaskId, 
                               t.description AS Description, t.task_type AS TaskType, t.status AS Status, 
                               t.priority AS Priority, t.due_date AS DueDate, t.stage AS Stage,
                               t.stage_item_id AS StageItemId, t.owner_id AS OwnerId, t.assignee_id AS AssigneeId,
                               t.activity_status AS ActivityStatus, t.activity_id AS ActivityId,
                               owner.firstname || ' ' || owner.lastname AS OwnerName,
                               assignee.firstname || ' ' || assignee.lastname AS AssigneeName
                        FROM public.task t
                        LEFT JOIN public.users owner ON t.owner_id = owner.userid
                        LEFT JOIN public.users assignee ON t.assignee_id = assignee.userid
                        WHERE t.assignee_id = @UserId
                        ORDER BY t.id DESC
                    ";
                }
                else
                {
                    // Return tasks where user is either owner or assignee
                    sql = @"
                        SELECT t.id, t.created_at AS CreatedAt, t.updated_at AS UpdatedAt, 
                               t.user_created AS UserCreated, t.user_updated AS UserUpdated,
                               t.task_id AS TaskId, t.task_name AS TaskName, t.parent_task_id AS ParentTaskId, 
                               t.description AS Description, t.task_type AS TaskType, t.status AS Status, 
                               t.priority AS Priority, t.due_date AS DueDate, t.stage AS Stage,
                               t.stage_item_id AS StageItemId, t.owner_id AS OwnerId, t.assignee_id AS AssigneeId,
                               t.activity_status AS ActivityStatus, t.activity_id AS ActivityId,
                               owner.firstname || ' ' || owner.lastname AS OwnerName,
                               assignee.firstname || ' ' || assignee.lastname AS AssigneeName
                        FROM public.task t
                        LEFT JOIN public.users owner ON t.owner_id = owner.userid
                        LEFT JOIN public.users assignee ON t.assignee_id = assignee.userid
                        WHERE t.owner_id = @UserId OR t.assignee_id = @UserId
                        ORDER BY t.id DESC
                    ";
                }

                var tasks = await connection.QueryAsync<TaskModel>(sql, new { UserId = userId });
                return Ok(tasks);
            }
    }

    public class TaskStatusUpdateModel
    {
        public string Status { get; set; }
        public string? Comments { get; set; } // Optional, if you want to allow comments on approval
    }
}
