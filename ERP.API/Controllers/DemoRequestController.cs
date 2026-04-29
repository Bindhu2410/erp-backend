using ERP.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Npgsql;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class DemoRequestController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DemoRequestController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all demo requests (status = "Requested" or "Demo Requested")
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<object>), 200)]
        public async Task<ActionResult<IEnumerable<object>>> GetDemoRequests()
        {
            var connectionString = _context.Database.GetConnectionString();
            using var connection = new NpgsqlConnection(connectionString);
            
            var sql = @"
                SELECT 
                    d.id,
                    d.status,
                    d.customer_name,
                    d.demo_date,
                    d.demo_contact,
                    d.demo_name,
                    d.demo_approach,
                    d.address,
                    d.contact_mobile_num,
                    d.leadid as lead_id,
                    d.opportunity_id,
                    d.comments,
                    d.user_created,
                    d.date_created,
                    COALESCE(
                        JSON_AGG(
                            JSON_BUILD_OBJECT(
                                'id', i.id,
                                'itemId', i.item_id,
                                'qty', i.qty,
                                'amount', i.amount,
                                'unitPrice', i.unit_price,
                                'itemCode', im.item_code,
                                'itemName', im.item_name,
                                'catNo', im.cat_no,
                                'hsnCode', im.hsn,
                                'taxPercentage', im.tax_percentage,
                                'brand', im.brand,
                                'specification', im.specification
                            )
                        ) FILTER (WHERE i.id IS NOT NULL AND i.item_id IS NOT NULL), 
                        '[]'::json
                    ) as items
                FROM sales_demos d
                LEFT JOIN (
                    SELECT DISTINCT ON (demo_id, item_id) 
                        id, demo_id, item_id, qty, amount, unit_price
                    FROM sales_demo_items 
                    WHERE is_active = true AND item_id IS NOT NULL
                    ORDER BY demo_id, item_id, id DESC
                ) i ON d.id = i.demo_id
                LEFT JOIN item_master im ON i.item_id = im.id
                WHERE d.status IN ('Requested', 'Demo Requested')
                GROUP BY d.id, d.status, d.customer_name, d.demo_date, d.demo_contact, 
                         d.demo_name, d.demo_approach, d.address, d.contact_mobile_num, 
                         d.leadid, d.opportunity_id, d.comments, d.user_created, d.date_created
                ORDER BY d.date_created DESC";
            
            var result = await connection.QueryAsync(sql);
            return Ok(result);
        }

        /// <summary>
        /// Debug: Get all demos with their statuses
        /// </summary>
        [HttpGet("debug/all-statuses")]
        [ProducesResponseType(200)]
        public async Task<ActionResult> GetAllDemoStatuses()
        {
            var allDemos = await _context.SalesDemo
                .Select(sd => new { sd.Id, sd.Status, sd.CustomerName })
                .ToListAsync();

            return Ok(allDemos);
        }

        /// <summary>
        /// Get demo request by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<object>> GetDemoRequestById(int id)
        {
            var demoRequest = await _context.SalesDemo
                .Where(sd => sd.Id == id && (sd.Status == "Requested" || sd.Status == "Demo Requested"))
                .Select(sd => new {
                    sd.Id,
                    sd.Status,
                    sd.CustomerName,
                    sd.DemoDate,
                    sd.DemoContact,
                    sd.DemoName,
                    sd.DemoApproach,
                    sd.Address,
                    sd.ContactMobileNum,
                    sd.LeadId,
                    sd.OpportunityId,
                    sd.Comments,
                    sd.UserCreated,
                    sd.DateCreated
                })
                .FirstOrDefaultAsync();

            if (demoRequest == null)
                return NotFound(new { message = $"Demo request with ID {id} not found." });

            return Ok(demoRequest);
        }

        /// <summary>
        /// Update demo request status
        /// </summary>
        [HttpPut("{id}/status")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> UpdateDemoRequestStatus(int id, [FromBody] UpdateStatusRequest request)
        {
            var connectionString = _context.Database.GetConnectionString();
            using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();
            using var transaction = await connection.BeginTransactionAsync();

            try
            {
                // Get demo request details
                var demo = await connection.QueryFirstOrDefaultAsync(
                    "SELECT id, user_created, customer_name, demo_name FROM sales_demos WHERE id = @Id",
                    new { Id = id }, transaction);

                if (demo == null)
                    return NotFound(new { message = $"Demo request with ID {id} not found." });

                // Update status
                await connection.ExecuteAsync(
                    "UPDATE sales_demos SET status = @Status WHERE id = @Id",
                    new { Status = request.Status, Id = id }, transaction);

                // Create task if status is Approved
                if (request.Status == "Approved")
                {
                    await connection.ExecuteAsync(@"
                        INSERT INTO task (task_name, description, task_type, status, priority, due_date, owner_id, assignee_id, stage, stage_item_id)
                        VALUES (@TaskName, @Description, @TaskType, @Status, @Priority, @DueDate, @OwnerId, @AssigneeId, @Stage, @StageItemId)",
                        new {
                            TaskName = $"Demo Request Approved - {demo.customer_name}",
                            Description = $"Demo request for {demo.demo_name} has been approved. Please proceed with demo preparation.",
                            TaskType = "Main",
                            Status = "Open",
                            Priority = "Medium",
                            DueDate = DateTime.UtcNow.AddDays(3),
                            OwnerId = demo.user_created,
                            AssigneeId = demo.user_created,
                            Stage = "Demo",
                            StageItemId = id.ToString()
                        }, transaction);
                }
                // Create task if status is Reschedule
                else if (request.Status == "Reschedule")
                {
                    // Update demo with reschedule date and notes if provided
                    if (request.RescheduleDate.HasValue || !string.IsNullOrEmpty(request.Notes))
                    {
                        await connection.ExecuteAsync(@"
                            UPDATE sales_demos 
                            SET demo_date = COALESCE(@RescheduleDate, demo_date),
                                comments = COALESCE(@Notes, comments)
                            WHERE id = @Id",
                            new { RescheduleDate = request.RescheduleDate, Notes = request.Notes, Id = id }, transaction);
                    }

                    await connection.ExecuteAsync(@"
                        INSERT INTO task (task_name, description, task_type, status, priority, due_date, owner_id, assignee_id, stage, stage_item_id)
                        VALUES (@TaskName, @Description, @TaskType, @Status, @Priority, @DueDate, @OwnerId, @AssigneeId, @Stage, @StageItemId)",
                        new {
                            TaskName = $"Demo Rescheduled - {demo.customer_name}",
                            Description = $"Demo for {demo.demo_name} has been rescheduled. {(request.RescheduleDate.HasValue ? $"New date: {request.RescheduleDate:yyyy-MM-dd}. " : "")}Reason: {request.Notes ?? "Product not available for the original date."}. Please coordinate with customer for new schedule.",
                            TaskType = "Main",
                            Status = "Open",
                            Priority = "High",
                            DueDate = request.RescheduleDate ?? DateTime.UtcNow.AddDays(1),
                            OwnerId = demo.user_created,
                            AssigneeId = demo.user_created,
                            Stage = "Demo",
                            StageItemId = id.ToString()
                        }, transaction);
                }

                await transaction.CommitAsync();
                return Ok(new { message = "Status updated successfully" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "Failed to update status", error = ex.Message });
            }
        }
    }

    public class UpdateStatusRequest
    {
        public string Status { get; set; }
        public DateTime? RescheduleDate { get; set; }
        public string Notes { get; set; }
    }
}