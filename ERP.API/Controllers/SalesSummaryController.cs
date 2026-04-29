using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using ERP.API.Models;
using ERP.API.Services;
using System.Linq;
using System.Text.Json;
using Npgsql;
using Dapper;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SalesSummaryController : ControllerBase
    {
        private readonly SalesSummaryService _summaryService;
        private readonly SalesActivityMeetingService _meetingService;
        private readonly SalesActivityCallService _callService;
        private readonly SalesActivityTaskService _taskService;
        private readonly SalesActivityEventService _eventService;
        private readonly string _connectionString;

        public SalesSummaryController(
            SalesSummaryService summaryService,
            SalesActivityMeetingService meetingService,
            SalesActivityCallService callService,
            SalesActivityTaskService taskService,
            SalesActivityEventService eventService,
            IConfiguration configuration)
        {
            _summaryService = summaryService;
            _meetingService = meetingService;
            _callService = callService;
            _taskService = taskService;
            _eventService = eventService;
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? 
                throw new ArgumentNullException(nameof(configuration), "DefaultConnection string is not configured");
        }

        [HttpGet("comprehensive/{stageItemId}")]
        public async Task<ActionResult<IEnumerable<SalesSummary>>> GetComprehensiveSummary(string stageItemId)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                var query = @"
                    WITH AllActivities AS (
                        -- Get all summaries for the stage item
                        SELECT 
                            id,
                            icon_url,
                            title,
                            description,
                            date_time,
                            isactive,
                            stage,
                            stage_item_id,
                            entities,
                            user_created,
                            date_created,
                            user_updated,
                            date_updated
                        FROM sales_summaries
                        WHERE stage_item_id = @StageItemId
                        AND isactive = true

                        UNION ALL

                        -- Include any recent activities not yet in summary
                        SELECT 
                            0 as id,
                            '/icons/general.png' as icon_url,
                            status || ' status' as title,
                            description,
                            date_created as date_time,
                            isactive,
                            stage,
                            stage_item_id,
                            NULL as entities,
                            user_created,
                            date_created,
                            user_updated,
                            date_updated
                        FROM (
                            -- Activities from meetings
                            SELECT 
                                meeting_title as description,
                                status,
                                isactive,
                                stage,
                                stage_item_id,
                                user_created,
                                date_created,
                                user_updated,
                                date_updated
                            FROM sales_activity_meetings
                            WHERE stage_item_id = @StageItemId
                            AND isactive = true

                            UNION ALL

                            -- Activities from calls
                            SELECT 
                                call_agenda as description,
                                status,
                                isactive,
                                stage,
                                stage_item_id,
                                user_created,
                                date_created,
                                user_updated,
                                date_updated
                            FROM sales_activity_calls
                            WHERE stage_item_id = @StageItemId
                            AND isactive = true

                            UNION ALL

                            -- Activities from events
                            SELECT 
                                event_title as description,
                                status,
                                isactive,
                                stage,
                                stage_item_id,
                                user_created,
                                date_created,
                                user_updated,
                                date_updated
                            FROM sales_activity_events
                            WHERE stage_item_id = @StageItemId
                            AND isactive = true

                            UNION ALL

                            -- Activities from tasks
                            SELECT 
                                task_name as description,
                                status,
                                isactive,
                                stage,
                                stage_item_id,
                                user_created,
                                date_created,
                                user_updated,
                                date_updated
                            FROM sales_activity_tasks
                            WHERE stage_item_id = @StageItemId
                            AND isactive = true

                            UNION ALL

                            -- External comments
                            SELECT 
                                description,
                                'Comment' as status,
                                isactive,
                                stage,
                                stage_item_id,
                                user_created,
                                date_created,
                                user_updated,
                                date_updated
                            FROM sales_external_comments
                            WHERE stage_item_id = @StageItemId
                            AND isactive = true
                        ) as activities
                    )
                    SELECT DISTINCT * FROM AllActivities
                    ORDER BY date_time DESC";

                var parameters = new DynamicParameters();
                parameters.Add("StageItemId", stageItemId);

                var activities = await connection.QueryAsync<SalesSummary>(query);
                return Ok(activities);
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SalesSummary>>> GetAll()
        {
            var summaries = await _summaryService.GetAllAsync("isactive = true ORDER BY date_time DESC", new { });
            return Ok(summaries);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SalesSummary>> GetById(int id)
        {
            var summary = await _summaryService.GetByIdAsync(id);
            if (summary == null)
                return NotFound($"Summary with ID {id} not found");

            return Ok(summary);
        }

        [HttpGet("stage/{stageItemId}")]
        public async Task<ActionResult<IEnumerable<SalesSummary>>> GetByStageItemId(string stageItemId)
        {
            var summaries = await _summaryService.GetAllAsync(
                "stage_item_id = @StageItemId AND isactive = true ORDER BY date_time DESC",
                new { StageItemId = stageItemId });
            return Ok(summaries);
        }

        /// <summary>
        /// Filter summaries by stage and stageItemId (POST)
        /// </summary>
        [HttpPost("stage/filter")]
        public async Task<ActionResult<IEnumerable<SalesSummary>>> GetByStageAndStageItemId([FromBody] StageFilterRequest request)
        {
            if (request == null || (string.IsNullOrEmpty(request.Stage) && string.IsNullOrEmpty(request.StageItemId)))
                return BadRequest("At least one filter (stage or stageItemId) must be provided.");

            var whereClause = "isactive = true";
            var parameters = new Dictionary<string, object>();

            if (!string.IsNullOrEmpty(request.Stage))
            {
                whereClause += " AND stage = @Stage";
                parameters.Add("Stage", request.Stage);
            }
            if (!string.IsNullOrEmpty(request.StageItemId))
            {
                whereClause += " AND stage_item_id = @StageItemId";
                parameters.Add("StageItemId", request.StageItemId);
            }
            whereClause += " ORDER BY date_time DESC";

            var summaries = await _summaryService.GetAllAsync(whereClause, parameters);
            return Ok(summaries);
        }
    // Request model for stage and stageItemId filter
    public class StageFilterRequest
    {
        public string Stage { get; set; }
        public string StageItemId { get; set; }
    }

        [HttpPost("filter")]
        public async Task<ActionResult<IEnumerable<SalesSummary>>> GetFiltered([FromBody] SalesSummaryRequest request)
        {
            var whereClause = "isactive = true";
            var parameters = new Dictionary<string, object>();

            if (!string.IsNullOrEmpty(request.Stage))
            {
                whereClause += " AND stage = @Stage";
                parameters.Add("Stage", request.Stage);
            }

            if (!string.IsNullOrEmpty(request.StageItemId))
            {
                if (!string.IsNullOrEmpty(request.Stage))
                {
                    whereClause += " OR stage_item_id = @StageItemId";
                }
                else
                {
                    whereClause += " AND stage_item_id = @StageItemId";
                }
                parameters.Add("StageItemId", request.StageItemId);
            }

            whereClause += " ORDER BY date_time DESC";

            var summaries = await _summaryService.GetAllAsync(whereClause, parameters);
            return Ok(summaries);
        }

        [HttpPost]
        public async Task<ActionResult<int>> Create(SalesSummary summary)
        {
            summary.IsActive = true;
            summary.DateTime = DateTime.UtcNow;
           
            var id = await _summaryService.CreateAsync(summary);
            return CreatedAtAction(nameof(GetById), new { id }, id);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, SalesSummary summary)
        {
            if (id != summary.Id)
                return BadRequest("ID mismatch");

            summary.DateUpdated = DateTime.UtcNow;
            await _summaryService.UpdateAsync(summary);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _summaryService.DeleteAsync(id);
            if (!success)
                return NotFound($"Summary with ID {id} not found");

            return NoContent();
        }
    }
}