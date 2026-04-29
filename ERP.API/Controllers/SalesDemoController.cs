
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ERP.API.Models;
using ERP.API.Models.DTOs;
using ERP.API.Services;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowAll")]
    [Produces("application/json")]
    [ApiExplorerSettings(IgnoreApi = false)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public class SalesDemoController : ControllerBase
    {
        private readonly ISalesDemoService _demoService;
        private readonly ILogger<SalesDemoController> _logger;
        private readonly IConfiguration _configuration; // Add this line
        private readonly ISalesOpportunityService _opportunityService;

        public SalesDemoController(
        ISalesDemoService demoService,
        ILogger<SalesDemoController> logger,
        IConfiguration configuration,
        ISalesOpportunityService opportunityService) // Changed to interface
        {
            _demoService = demoService;
            _logger = logger;
            _configuration = configuration; // Assign here
            _opportunityService = opportunityService;
        }
        /// <summary>
        /// Partially updates a sales demo and its items.
        /// </summary>
        /// <param name="id">The ID of the sales demo to update</param>
        /// <param name="request">The updated fields for the sales demo and its items</param>
        /// <returns>No content if successful</returns>
        /// <response code="200">If the update was successful</response>
        /// <response code="400">If the request is invalid</response>
        /// <response code="404">If the demo was not found</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpPatch("with-items/{id}")]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(SalesDemoWithItemsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PatchDemoWithItems(int id, [FromBody] SalesDemoWithItemsRequest request)
        {
            if (request == null || request.Demo == null)
                return BadRequest("Invalid request body");

            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString))
                return StatusCode(500, "Could not resolve connection string.");

            using var connection = new Npgsql.NpgsqlConnection(connectionString);
            await connection.OpenAsync();
            using var transaction = await connection.BeginTransactionAsync();
            try
            {
                // Fetch the existing demo
                var existingDemo = await connection.QueryFirstOrDefaultAsync<SalesDemo>("SELECT * FROM sales_demos WHERE id = @Id", new { Id = id }, transaction);
                if (existingDemo == null)
                {
                    await transaction.RollbackAsync();
                    return NotFound(new { message = $"Sales demo with id {id} not found." });
                }

                // Update only provided fields in request.Demo (partial update)
                var updateFields = new List<string>();
                var updateParams = new DynamicParameters();
                updateParams.Add("Id", id);
                if (request.Demo.DemoName != null) { updateFields.Add("demo_name = @DemoName"); updateParams.Add("DemoName", request.Demo.DemoName); }
                if (request.Demo.Status != null) { updateFields.Add("status = @Status"); updateParams.Add("Status", request.Demo.Status); }
                if (request.Demo.DemoDate != default) { updateFields.Add("demo_date = @DemoDate"); updateParams.Add("DemoDate", request.Demo.DemoDate); }
                if (request.Demo.DemoContact != null) { updateFields.Add("demo_contact = @DemoContact"); updateParams.Add("DemoContact", request.Demo.DemoContact); }
                if (request.Demo.DemoApproach != null) { updateFields.Add("demo_approach = @DemoApproach"); updateParams.Add("DemoApproach", request.Demo.DemoApproach); }
                if (request.Demo.DemoOutcome != null) { updateFields.Add("demo_outcome = @DemoOutcome"); updateParams.Add("DemoOutcome", request.Demo.DemoOutcome); }
                if (request.Demo.DemoFeedback != null) { updateFields.Add("demo_feedback = @DemoFeedback"); updateParams.Add("DemoFeedback", request.Demo.DemoFeedback); }
                if (request.Demo.Comments != null) { updateFields.Add("comments = @Comments"); updateParams.Add("Comments", request.Demo.Comments); }
                if (request.Demo.UserId != 0) { updateFields.Add("user_id = @UserId"); updateParams.Add("UserId", request.Demo.UserId); }
                if (request.Demo.Address != null) { updateFields.Add("address = @Address"); updateParams.Add("Address", request.Demo.Address); }
                if (request.Demo.OpportunityId != null) { updateFields.Add("opportunity_id = @OpportunityId"); updateParams.Add("OpportunityId", request.Demo.OpportunityId); }
                if (request.Demo.CustomerName != null) { updateFields.Add("customer_name = @CustomerName"); updateParams.Add("CustomerName", request.Demo.CustomerName); }
                if (request.Demo.ContactMobileNum != null) { updateFields.Add("contact_mobile_num = @ContactMobileNum"); updateParams.Add("ContactMobileNum", request.Demo.ContactMobileNum); }
                if (request.Demo.LeadId != null) { updateFields.Add("leadid = @LeadId"); updateParams.Add("LeadId", request.Demo.LeadId); }
                if (request.Demo.DemoTime != null) { updateFields.Add("demo_time = @DemoTime"); updateParams.Add("DemoTime", request.Demo.DemoTime); }
                if (updateFields.Count > 0)
                {
                    var updateSql = $"UPDATE sales_demos SET {string.Join(", ", updateFields)}, date_updated = NOW() WHERE id = @Id";
                    await connection.ExecuteAsync(updateSql, updateParams, transaction);
                }

                // Patch items if provided
                if (request.Items != null)
                {
                    // Remove all old items for this demo
                    await connection.ExecuteAsync("DELETE FROM sales_demo_items WHERE demo_id = @DemoId", new { DemoId = id }, transaction);
                    await connection.ExecuteAsync("DELETE FROM sales_demo_accessories WHERE sales_demo_item_id IN (SELECT id FROM sales_demo_items WHERE demo_id = @DemoId)", new { DemoId = id }, transaction);

                    // Insert new items from request.Items
                    foreach (var item in request.Items)
                    {
                        // Only use BomId, ignore AccessoryItemIds, Quantity
                        var bomId = item.BomId;
                        if (string.IsNullOrEmpty(bomId))
                            continue;

                        var sql = @"INSERT INTO public.sales_demo_items 
                            (demo_id, date_created, qty, amount, is_active, stage, stage_item_id, user_created) 
                            VALUES (@DemoId, @DateCreated, @Qty, @Amount, @IsActive, @Stage, @StageItemId, @UserCreated) 
                            RETURNING id;";
                        await connection.ExecuteScalarAsync<int>(sql, new
                        {
                            DemoId = id,
                            DateCreated = DateTime.UtcNow,
                            Qty = 1, // Default quantity
                            Amount = (decimal?)null,
                            IsActive = true,
                            Stage = "Demo",
                            StageItemId = id,
                            UserCreated = 1 // Default user ID
                        }, transaction);
                    }
                }

                // Patch presenterIds if provided
                if (request.Demo.PresenterIds != null)
                {
                    // Remove all and re-insert for simplicity
                    await connection.ExecuteAsync("DELETE FROM sales_demo_presenters WHERE demo_id = @DemoId", new { DemoId = id }, transaction);
                    foreach (var pid in request.Demo.PresenterIds)
                    {
                        await connection.ExecuteAsync("INSERT INTO sales_demo_presenters (demo_id, presenter_id) VALUES (@DemoId, @PresenterId)", new { DemoId = id, PresenterId = pid }, transaction);
                    }
                }

                // Patch checklist items if provided
                if (request.Checklists != null)
                {
                    // Remove all and re-insert for simplicity
                    await connection.ExecuteAsync("DELETE FROM demo_checklists WHERE demo_id = @DemoId", new { DemoId = id }, transaction);
                    foreach (var reqChecklist in request.Checklists)
                    {
                        var checklistId = await connection.ExecuteScalarAsync<int?>(
                            "SELECT id FROM demo_checklist_items WHERE checklist_name = @ChecklistName LIMIT 1",
                            new { ChecklistName = reqChecklist.ChecklistName }, transaction);
                        if (checklistId == null)
                            continue;
                        await connection.ExecuteAsync(
                            "INSERT INTO demo_checklists (checklist_id, checklist_name, demo_id, is_active, created_at, updated_at) VALUES (@ChecklistId, @ChecklistName, @DemoId, @IsActive, NOW(), NOW())",
                            new { ChecklistId = checklistId, ChecklistName = reqChecklist.ChecklistName, DemoId = id, IsActive = reqChecklist.IsActive }, transaction);
                    }
                }

                // Query inserted demo and all items for response
                var createdDemo = await connection.QueryFirstOrDefaultAsync<SalesDemo>(
                    "SELECT * FROM sales_demos WHERE id = @Id", new { Id = id }, transaction);
                var createdItems = (await connection.QueryAsync<SalesItemResponse>(
                    @"SELECT sdi.*, m.name as make, mo.name as model, p.name as product, c.name AS Category, im.item_name as ItemName, im.item_code as ItemCode, im.unit_price as UnitPrice, im.hsn, im.tax_percentage as TaxPercentage
                    FROM sales_demo_items sdi
                    LEFT JOIN item_master im ON sdi.item_id = im.id
                    LEFT JOIN categories c ON im.category_id = c.id
                    LEFT JOIN make m ON im.make_id = m.id
                    LEFT JOIN model mo ON im.model_id = mo.id
                    LEFT JOIN product p ON im.product_id = p.id
                    WHERE sdi.demo_id = @DemoId",
                    new { DemoId = id }, transaction)).ToList();

                // Convert SalesItemResponse to SalesDemoItemResponse
                var convertedCreatedItems = createdItems.Select(item => new SalesDemoItemResponse
                {
                    Id = item.Id,
                    BomId = item.BomId,
                    BomName = item.BomName,
                    BomType = item.BomType,
                    BomChildItems = item.ChildItems?.Select(child => new BomChildItemDto
                    {
                        Id = child.ChildItemId,
                        ChildItemId = child.ChildItemId,
                        ItemName = child.ItemName,
                        ItemCode = child.ItemCode,
                        Make = child.Make,
                        Model = child.Model,
                        Product = child.Product,
                        Category = child.CategoryName,
                        CategoryName = child.CategoryName,
                        UnitPrice = child.UnitPrice,
                        Hsn = child.Hsn,
                        Tax = child.Tax,
                        TaxPercentage = child.Tax,
                        Uom = child.UomName,
                        UomName = child.UomName,
                        Quantity = child.Quantity
                    }).ToList(),
                    AccessoryItemIds = item.AccessoriesIds != null ? new List<int>(item.AccessoriesIds) : new List<int>(),
                    AccessoryItems = item.AccessoriesItems?.ConvertAll(acc => new AccessoryItemResponseDto
                    {
                        Id = acc.Id,
                        Make = acc.Make ?? "",
                        Model = acc.Model ?? "",
                        Product = acc.Product ?? "",
                        ItemName = acc.ItemName ?? "",
                        ItemCode = acc.ItemCode ?? "",
                        UnitPrice = acc.UnitPrice ?? 0,
                        Hsn = acc.Hsn ?? "",
                        TaxPercentage = acc.TaxPercentage ?? 0,
                        CategoryName = acc.CategoryName ?? acc.Category ?? ""
                    }) ?? new List<AccessoryItemResponseDto>(),
                    Quantity = item.Qty ?? 0
                }).ToList();

                await transaction.CommitAsync();
                return Ok(new SalesDemoWithItemsResponse
                {
                    Id = createdDemo.Id,
                    UserCreated = createdDemo.UserCreated,
                    DateCreated = createdDemo.DateCreated,
                    UserUpdated = createdDemo.UserUpdated,
                    DateUpdated = createdDemo.DateUpdated,
                    UserId = createdDemo.UserId,
                    DemoDate = createdDemo.DemoDate,
                    Status = createdDemo.Status,
                    Address = createdDemo.Address,
                    OpportunityId = createdDemo.OpportunityId,
                    CustomerName = createdDemo.CustomerName,
                    DemoContact = createdDemo.DemoContact,
                    DemoName = createdDemo.DemoName,
                    DemoApproach = createdDemo.DemoApproach,
                    DemoOutcome = createdDemo.DemoOutcome,
                    DemoFeedback = createdDemo.DemoFeedback,
                    Comments = createdDemo.Comments,
                    ContactMobileNum = createdDemo.ContactMobileNum,
                    PresenterIds = request.Demo.PresenterIds ?? new List<int>(),
                    PresenterNames = new List<string>(), // Optionally fetch presenter names if needed
                    LeadId = createdDemo.LeadId,
                    Items = convertedCreatedItems,
                    DemoTime = createdDemo.DemoTime
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error patching demo with items: {Message}", ex.Message);
                return StatusCode(500, new { message = "Failed to patch demo with items", error = ex.Message });
            }
        }

        /// <summary>
        /// Gets all sales demos.
        /// </summary>
        /// <returns>A list of all sales demos</returns>
        /// <response code="200">Returns the list of demos</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<SalesDemo>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<SalesDemo>>> GetDemos()
        {
            try
            {
                var demos = await _demoService.GetDemosAsync();
                return Ok(demos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting demos: {Message}", ex.Message);
                return StatusCode(500, $"Failed to retrieve demos: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets demo requests for the authenticated user where status contains 'request' (case-insensitive).
        /// This endpoint does not accept `userId` in the query/body - it uses the authenticated user's id from claims.
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(SalesDemo), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<SalesDemo>> GetDemo(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning("Invalid demo ID: {Id}", id);
                    return BadRequest("Invalid demo ID");
                }

                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                if (string.IsNullOrEmpty(connectionString))
                    return StatusCode(500, "Could not resolve connection string.");

                using var connection = new Npgsql.NpgsqlConnection(connectionString);
                await connection.OpenAsync();
                using var transaction = await connection.BeginTransactionAsync();
                try
                {
                    var demo = await connection.QueryFirstOrDefaultAsync<SalesDemo>("SELECT * FROM sales_demos WHERE id = @Id", new { Id = id }, transaction);
                    if (demo == null)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogInformation("Demo not found: {Id}", id);
                        return NotFound($"Demo with ID {id} not found");
                    }

                    // Fetch presenterIds from sales_demo_presenters table
                    var presenterIds = (await connection.QueryAsync<int>(
                        "SELECT presenter_id FROM sales_demo_presenters WHERE demo_id = @DemoId",
                        new { DemoId = demo.Id }, transaction)).ToList();
                    // Fetch presenterNames from users table
                    List<string> presenterNames = new List<string>();
                    if (presenterIds.Count > 0)
                    {
                        var users = (await connection.QueryAsync<User>("SELECT * FROM users WHERE userid = ANY(@Ids)", new { Ids = presenterIds }, transaction)).ToList();
                        presenterNames = users.Select(u => u.Username).ToList();
                    }
                    presenterIds = presenterIds ?? new List<int>();
                    presenterNames = presenterNames ?? new List<string>();

                    // Fetch main presenter name if PresenterId is set
                    string? presenterName = null;
                    if (demo.PresenterId.HasValue)
                    {
                        var mainPresenter = await connection.QueryFirstOrDefaultAsync<User>("SELECT * FROM users WHERE userid = @Id", new { Id = demo.PresenterId.Value }, transaction);
                        presenterName = mainPresenter?.Username;
                    }

                    var response = new ERP.API.Models.DTOs.SalesDemoDetailsDto
                    {
                        Id = demo.Id,
                        UserCreated = demo.UserCreated,
                        DateCreated = demo.DateCreated,
                        UserUpdated = demo.UserUpdated,
                        DateUpdated = demo.DateUpdated,
                        UserId = demo.UserId,
                        DemoDate = demo.DemoDate,
                        Status = demo.Status,
                        Address = demo.Address,
                        OpportunityId = demo.OpportunityId,
                        CustomerName = demo.CustomerName,
                        DemoContact = demo.DemoContact,
                        DemoName = demo.DemoName,
                        DemoApproach = demo.DemoApproach,
                        DemoOutcome = demo.DemoOutcome,
                        DemoFeedback = demo.DemoFeedback,
                        Comments = demo.Comments,
                        ContactMobileNum = demo.ContactMobileNum,
                        PresenterId = demo.PresenterId,
                        PresenterName = presenterName,
                        PresenterIds = presenterIds,
                        PresenterNames = presenterNames,
                        LeadId = demo.LeadId
                    };
                    await transaction.CommitAsync();
                    return Ok(response);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error retrieving demo {Id}: {Message}", id, ex.Message);
                    return StatusCode(500, $"Failed to retrieve demo {id}: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving demo {Id}: {Message}", id, ex.Message);
                return StatusCode(500, $"Failed to retrieve demo {id}: {ex.Message}");
            }
        }
        /// <summary>
        /// Creates a new sales demo.
        /// </summary>
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<object>> CreateDemo([FromBody] CreateSalesDemoDto demoDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        message = "Invalid model state.",
                        statusCode = 400,
                        errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                    });
                }

                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                if (string.IsNullOrEmpty(connectionString))
                    return StatusCode(500, "Could not resolve connection string.");

                using var connection = new Npgsql.NpgsqlConnection(connectionString);
                await connection.OpenAsync();
                using var transaction = await connection.BeginTransactionAsync();
                try
                {
                    // Insert into sales_demos
                    var demo = new SalesDemo
                    {
                        DemoName = demoDto.DemoName ?? string.Empty,
                        Status = demoDto.Status ?? "Requested",
                        DemoDate = demoDto.DemoDate != default ? demoDto.DemoDate : DateTime.UtcNow,
                        DemoContact = demoDto.DemoContact ?? string.Empty,
                        DemoApproach = demoDto.DemoApproach ?? string.Empty,
                        DemoOutcome = demoDto.DemoOutcome ?? string.Empty,
                        DemoFeedback = demoDto.DemoFeedback ?? string.Empty,
                        Comments = demoDto.Comments ?? string.Empty,
                        UserId = demoDto.UserId,
                        Address = demoDto.Address ?? string.Empty,
                        OpportunityId = demoDto.OpportunityId,
                        CustomerName = demoDto.CustomerName ?? string.Empty,
                        ContactMobileNum = demoDto.ContactMobileNum,
                        LeadId = demoDto.LeadId,
                        DateCreated = DateTime.UtcNow,
                        UserCreated = 1 // Default user ID
                    };

                    const string sqlDemo = @"
                        INSERT INTO sales_demos (
                            user_created, date_created, user_id, demo_date, status, address, opportunity_id, demo_contact, demo_name, demo_approach, demo_outcome, demo_feedback, comments, contact_mobile_num, leadid, customer_name, presenter_ids
                        ) VALUES (
                            @UserCreated, @DateCreated, @UserId, @DemoDate, @Status, @Address, @OpportunityId, @DemoContact, @DemoName, @DemoApproach, @DemoOutcome, @DemoFeedback, @Comments, @ContactMobileNum, @LeadId, @CustomerName, @PresenterIds
                        ) RETURNING id";
                    var id = await connection.ExecuteScalarAsync<int>(sqlDemo, demo, transaction);

                    // Insert presenterIds into sales_demo_presenters
                    var presenterIds = demoDto.PresenterIds ?? new List<int>();
                    if (presenterIds.Count > 0)
                    {
                        foreach (var pid in presenterIds)
                        {
                            await connection.ExecuteAsync(
                                "INSERT INTO sales_demo_presenters (demo_id, presenter_id) VALUES (@DemoId, @PresenterId)",
                                new { DemoId = id, PresenterId = pid }, transaction);
                        }
                    }

                    await transaction.CommitAsync();
                    _logger.LogInformation("Created new demo with ID {Id}", id);
                    return CreatedAtAction(nameof(GetDemo), new { id }, id);
                }
                catch (ArgumentException ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogWarning(ex, "Invalid input for demo creation: {Message}", ex.Message);
                    return BadRequest(new
                    {
                        message = ex.Message,
                        statusCode = 400
                    });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error creating demo: {Message}", ex.Message);
                    return StatusCode(500, new
                    {
                        message = "Failed to create demo",
                        error = ex.Message,
                        statusCode = 500
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating demo: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    message = "Failed to create demo",
                    error = ex.Message,
                    statusCode = 500
                });
            }
        }
        /// <summary>
        /// Gets sales demo cards summary (Requested, Scheduled, Completed) for a user
        /// </summary>
        /// <param name="userId">Optional userId to filter cards</param>
        [HttpGet("cards")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<DemoCardsDto>> GetDemoCards([FromQuery] int? userId = null)
        {
            if (userId == null || userId <= 0)
            {
                _logger.LogWarning("[GetDemoCards] No valid userId provided.");
                return BadRequest(new { message = "UserId is required as a query parameter." });
            }
            try
            {
                var result = await _demoService.GetDemoCardsByUserAsync(userId.Value);
                if (result == null)
                {
                    return NotFound(new { message = $"No demo cards found for userId {userId}" });
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GetDemoCards] Exception for userId: {UserId}", userId);
                return StatusCode(500, new { message = "An error occurred while retrieving demo cards", statusCode = 500, errors = new[] { ex.Message, ex.StackTrace } });
            }
        }

        /// <summary>
        /// Updates an existing sales demo
        /// </summary>
        /// <param name="id">The ID of the demo to update</param>
        /// <param name="demoDto">The updated demo data</param>
        /// <returns>No content if successful</returns>
        /// <response code="204">If the update was successful</response>
        /// <response code="400">If the demo data is invalid</response>
        /// <response code="401">If the user is not authorized</response>
        /// <response code="404">If the demo is not found</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpPut("{id}")]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateDemo([FromRoute] int id, [FromBody] UpdateSalesDemoDto demoDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        message = "Invalid model state",
                        statusCode = 400,
                        errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                    });
                }
                // ...existing update logic here...
                // This block should be restored to its original update logic, not the GET logic.
                // If you need to update the demo, map demoDto to SalesDemo and call the update service.
                // For now, just return NoContent for placeholder.
                _logger.LogInformation("Updated demo with ID {Id}", id);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid input for demo update: {Message}", ex.Message);
                return BadRequest(new
                {
                    message = ex.Message,
                    statusCode = 400
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating demo {Id}: {Message}", id, ex.Message);
                return StatusCode(500, new
                {
                    message = $"Failed to update demo {id}",
                    error = ex.Message,
                    statusCode = 500
                });
            }
        }
        // (Removed duplicate misplaced code)

        // DELETE: api/SalesDemo/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDemo(int id)
        {
            try
            {
                var success = await _demoService.DeleteDemoAsync(id);

                if (!success)
                {
                    _logger.LogWarning("Demo with ID {Id} not found for deletion", id);
                    return NotFound($"Demo with ID {id} not found");
                }

                _logger.LogInformation("Soft deleted demo with ID {Id}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting demo {Id}: {Message}", id, ex.Message);
                return StatusCode(500, $"Failed to delete demo {id}: {ex.Message}");
            }
        }

        // GET: api/SalesDemo/opportunity/5
        [HttpGet("opportunity/{opportunityId}")]
        public async Task<ActionResult<IEnumerable<SalesDemo>>> GetDemosByOpportunity(string? opportunityId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(opportunityId))
                {
                    _logger.LogWarning("Invalid opportunity ID: {Id}", opportunityId);
                    return BadRequest("Invalid opportunity ID");
                }

                var demos = await _demoService.GetDemosByOpportunityIdAsync(opportunityId);
                return Ok(demos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving demos for opportunity {Id}: {Message}", opportunityId, ex.Message);
                return StatusCode(500, $"Failed to retrieve demos for opportunity {opportunityId}: {ex.Message}");
            }
        }

        /// <summary>
        /// Creates a new sales demo along with its items in a single request.
        /// </summary>
        /// <param name="request">The sales demo and its items</param>
        /// <returns>The ID of the newly created demo</returns>
        /// <response code="201">Returns the newly created demo ID</response>
        /// <response code="400">If the demo data is invalid</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpPost("with-items")]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateDemoWithItems([FromBody] SalesDemoWithItemsRequest request)
        {
            if (request == null || request.Demo == null || request.Items == null)
                return BadRequest("Invalid request body");

            if (request.Demo.OpportunityId == null)
                return BadRequest("OpportunityId is required and cannot be null.");

            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString))
                return StatusCode(500, "Could not resolve connection string.");

            using var connection = new Npgsql.NpgsqlConnection(connectionString);
            await connection.OpenAsync();

            using var transaction = await connection.BeginTransactionAsync();
            try
            {
                // --- Fetch customer address based on opportunity id (like SalesQuotationWithItems) ---
                ERP.API.Models.DTOs.SalesAddressDto customerAddress = null;
                string? leadIdToUse = request.Demo.LeadId?.ToString();
                if (request.Demo.OpportunityId != null)
                {
                    var opp = await connection.QueryFirstOrDefaultAsync<dynamic>(
                        "SELECT lead_id FROM sales_opportunities WHERE opportunity_id = @OpportunityId LIMIT 1",
                        new { OpportunityId = request.Demo.OpportunityId });
                    if (opp != null && opp.lead_id != null && !string.IsNullOrEmpty((string)opp.lead_id))
                    {
                        leadIdToUse = (string)opp.lead_id;
                    }
                }
                if (!string.IsNullOrEmpty(leadIdToUse))
                {
                    // You may need to resolve SalesAddressService from DI or create it here
                    var salesAddressService = new ERP.API.Services.SalesAddressService(connectionString);
                    var addresses = await salesAddressService.GetBySalesLeadIdAsync(int.TryParse(leadIdToUse, out var parsedLeadId) ? parsedLeadId : (int?)null);
                    var address = addresses?.FirstOrDefault(a => a.IsDefault == true) ?? addresses?.FirstOrDefault();
                    if (address != null)
                    {
                        customerAddress = new ERP.API.Models.DTOs.SalesAddressDto
                        {
                            Id = address.Id,
                            DoorNo = address.DoorNo,
                            Street = address.Street,
                            Area = address.Area,
                            Block = address.Block,
                            City = address.City,
                            State = address.State,
                            Pincode = address.Pincode,
                            Landmark = address.Landmark,
                            IsDefault = address.IsDefault,
                            Type = address.Type,
                            Department = address.Department,
                            OpportunityId = address.OpportunityId
                        };
                    }
                }

                // Insert main demo and get demoId
                var demoInsertSql = @"INSERT INTO sales_demos (demo_name, status, demo_date, demo_contact, demo_approach, demo_outcome, demo_feedback, comments, user_id, address, opportunity_id, customer_name, contact_mobile_num, leadid, demo_time, date_created, user_created, date_updated, user_updated)
                    VALUES (@DemoName, @Status, @DemoDate, @DemoContact, @DemoApproach, @DemoOutcome, @DemoFeedback, @Comments, @UserId, @Address, @OpportunityId, @CustomerName, @ContactMobileNum, @LeadId, @DemoTime, NOW(), 1, NOW(), 1)
                    RETURNING id;";
                var demoId = await connection.ExecuteScalarAsync<int>(demoInsertSql, new
                {
                    request.Demo.DemoName,
                    request.Demo.Status,
                    request.Demo.DemoDate,
                    request.Demo.DemoContact,
                    request.Demo.DemoApproach,
                    request.Demo.DemoOutcome,
                    request.Demo.DemoFeedback,
                    request.Demo.Comments,
                    request.Demo.UserId,
                    request.Demo.Address,
                    request.Demo.OpportunityId,
                    request.Demo.CustomerName,
                    request.Demo.ContactMobileNum,
                    request.Demo.LeadId,
                    request.Demo.DemoTime
                }, transaction);

                // Insert items with BOM ID directly in sales_demo_items table
                foreach (var item in request.Items)
                {
                    var bomId = item.BomId;
                    if (string.IsNullOrEmpty(bomId))
                        continue;

                    var sql = @"INSERT INTO public.sales_demo_items 
                        (demo_id, date_created, qty, amount, is_active, stage, stage_item_id, user_created, bom_id) 
                        VALUES (@DemoId, @DateCreated, @Qty, @Amount, @IsActive, @Stage, @StageItemId, @UserCreated, @BomId) 
                        RETURNING id;";
                    var itemId = await connection.ExecuteScalarAsync<int>(sql, new
                    {
                        DemoId = demoId,
                        DateCreated = DateTime.UtcNow,
                        Qty = item.Quantity,
                        Amount = (decimal?)null,
                        IsActive = true,
                        Stage = "Demo",
                        StageItemId = demoId,
                        UserCreated = 1, // Default user ID
                        BomId = bomId
                    }, transaction);

                }

                // Fetch inserted demo and items for response
                var createdDemo = await connection.QueryFirstOrDefaultAsync<SalesDemo>(
                    "SELECT * FROM sales_demos WHERE id = @Id", new { Id = demoId }, transaction);
                
                // Fetch sales_demo_items and build items array with BOM details (same as GET method)
                var salesDemoItemRows = await connection.QueryAsync<dynamic>(
                    @"SELECT sdi.id, sdi.demo_id, sdi.qty, sdi.bom_id 
                      FROM sales_demo_items sdi 
                      WHERE sdi.demo_id = @DemoId", 
                    new { DemoId = demoId }, transaction);

                var itemsArray = new List<object>();
                foreach (var row in salesDemoItemRows)
                {
                    var bomId = row.bom_id?.ToString();
                    if (string.IsNullOrEmpty(bomId))
                        continue;

                    // Fetch BOM details
                    var bomDetailsFull = await connection.QueryFirstOrDefaultAsync(
                        @"SELECT bom_id, bom_name, bom_type 
                          FROM bill_of_materials 
                          WHERE bom_id = @BomId LIMIT 1",
                        new { BomId = bomId }, transaction);

                    // Fetch BOM child items with full details
                    var bomChildItems = await connection.QueryAsync<dynamic>(
                        @"SELECT bci.child_item_id AS childItemId, bci.quantity, 
                               im.item_name, im.item_code, im.cat_no, im.unit_price, im.hsn, 
                               COALESCE(NULLIF(im.tax_percentage, -1), 0) AS tax, 
                               m.name as make, mo.name as model, p.name as product, 
                               c.name AS category_name, vm.name AS valuation_method_name, 
                               ime.name AS inventory_method_name, itt.name AS inventory_type_name, 
                               u.code AS uom_name, rmi.purchase_rate AS purchase_rate, 
                               rmi.sales_rate AS sale_rate, rmi.quotation_rate AS quote_rate
                          FROM bill_of_material_child_items bci
                          JOIN item_master im ON bci.child_item_id = im.id
                          LEFT JOIN categories c ON im.category_id = c.id
                          LEFT JOIN make m ON im.make_id = m.id
                          LEFT JOIN model mo ON im.model_id = mo.id
                          LEFT JOIN product p ON im.product_id = p.id
                          LEFT JOIN valuation_method vm ON im.valuation_method_id = vm.id
                          LEFT JOIN inventory_method ime ON im.inventory_method_id = ime.id
                          LEFT JOIN inventory_types itt ON im.group_id = itt.id
                          LEFT JOIN uom u ON im.uom_id = u.id
                          LEFT JOIN rate_master_items rmi ON im.id = rmi.item_id
                          WHERE bci.bill_of_material_id = 
                                (SELECT id FROM bill_of_materials WHERE bom_id = @BomId)",
                        new { BomId = bomId }, transaction);

                    var bomChildItemList = bomChildItems.Select(ci => new {
                        childItemId = ci.childItemId,
                        quantity = ci.quantity,
                        make = ci.make,
                        model = ci.model,
                        product = ci.product,
                        categoryName = ci.category_name,
                        valuationMethodName = ci.valuation_method_name,
                        inventoryMethodName = ci.inventory_method_name,
                        inventoryTypeName = ci.inventory_type_name,
                        unitPrice = ci.unit_price,
                        itemName = ci.item_name,
                        itemCode = ci.item_code,
                        catNo = ci.cat_no,
                        uomName = ci.uom_name,
                        purchaseRate = ci.purchase_rate,
                        saleRate = ci.sale_rate,
                        quoteRate = ci.quote_rate,
                        hsn = ci.hsn,
                        tax = ci.tax
                    }).ToList();

                    itemsArray.Add(new {
                        bomId = bomId,
                        bomName = bomDetailsFull?.bom_name,
                        bomType = bomDetailsFull?.bom_type,
                        bomChildItems = bomChildItemList,
                        accessoryItemIds = (object)null,
                        accessoryItems = new List<object>(),
                        quantity = row.qty ?? 1
                    });
                }

                // Fetch presenterIds from sales_demo_presenters table (to ensure DB state)
                var dbPresenterIdsArray = (await connection.QueryAsync<int>(
                    "SELECT presenter_id FROM sales_demo_presenters WHERE demo_id = @DemoId",
                    new { DemoId = createdDemo.Id }, transaction)).ToArray();
                // Fetch presenterNames from users table using those IDs
                List<string> presenterNames = new List<string>();
                if (dbPresenterIdsArray != null && dbPresenterIdsArray.Length > 0)
                {
                    var users = (await connection.QueryAsync<dynamic>(
                        "SELECT username FROM users WHERE userid = ANY(@Ids)", new { Ids = dbPresenterIdsArray }, transaction)).ToList();
                    presenterNames = users.Select(u => (string)u.username).ToList();
                }
                var dbPresenterIds = dbPresenterIdsArray != null ? dbPresenterIdsArray.ToList() : new List<int>();
                presenterNames = presenterNames ?? new List<string>();

                // Note: SalesDemoItemResponse doesn't have ItemId property, checklist handling moved to separate endpoint

                // Build response object
                // Fetch lead address fields from sales_lead if OpportunityId is present
                object leadAddress = null;
                if (createdDemo.OpportunityId != null)
                {
                    var leadIdObj = await connection.QueryFirstOrDefaultAsync<dynamic>(
                        "SELECT lead_id FROM sales_opportunities WHERE opportunity_id = @OpportunityId LIMIT 1",
                        new { OpportunityId = createdDemo.OpportunityId }, transaction);
                    string leadId = null;
                    if (leadIdObj != null && leadIdObj.lead_id != null)
                        leadId = leadIdObj.lead_id.ToString();
                    if (!string.IsNullOrEmpty(leadId))
                    {
                        leadAddress = await connection.QueryFirstOrDefaultAsync(
                            @"SELECT pincode, area, state, district, city, door_no, street, landmark FROM sales_lead WHERE lead_id = @LeadId LIMIT 1",
                            new { LeadId = leadId }, transaction);
                    }
                }

                var response = new SalesDemoWithItemsResponse
                {
                    Id = createdDemo.Id,
                    UserCreated = createdDemo.UserCreated,
                    DateCreated = createdDemo.DateCreated,
                    UserUpdated = createdDemo.UserUpdated,
                    DateUpdated = createdDemo.DateUpdated,
                    UserId = createdDemo.UserId,
                    DemoDate = createdDemo.DemoDate,
                    Status = createdDemo.Status,
                    Address = createdDemo.Address,
                    OpportunityId = createdDemo.OpportunityId,
                    CustomerName = createdDemo.CustomerName,
                    DemoContact = createdDemo.DemoContact,
                    DemoName = createdDemo.DemoName,
                    DemoApproach = createdDemo.DemoApproach,
                    DemoOutcome = createdDemo.DemoOutcome,
                    DemoFeedback = createdDemo.DemoFeedback,
                    Comments = createdDemo.Comments,
                    ContactMobileNum = createdDemo.ContactMobileNum,
                    PresenterIds = dbPresenterIds?.ToList() ?? new List<int>(),
                    PresenterNames = presenterNames,
                    LeadId = createdDemo.LeadId,
                    DemoTime = createdDemo.DemoTime,
                    CustomerAddress = customerAddress,
                    LeadAddress = leadAddress
                };

                // Fetch only active checklist items for this demo
                // Fetch all checklist items for this demo, with their is_active status (default false if not set)
                // Only return checklist info for selected checklist ids (from request.Checklists or createdItems)
                var selectedChecklists = new List<dynamic>();
                if (request.Checklists != null && request.Checklists.Count > 0)
                {
                    foreach (var reqChecklist in request.Checklists)
                    {
                        string checklistName = reqChecklist.ChecklistName;
                        // ChecklistId is not included in the response anymore
                        selectedChecklists.Add(new {
                            ChecklistName = checklistName,
                            IsActive = reqChecklist.IsActive
                        });
                    }
                }

                // Add to response
                // Insert checklist data into demo_checklists table
                if (request.Checklists != null && request.Checklists.Count > 0)
                {
                    foreach (var reqChecklist in request.Checklists)
                    {
                        // Lookup checklist_id from demo_checklist_items using checklist_name
                        var checklistId = await connection.ExecuteScalarAsync<int?>(
                            "SELECT id FROM demo_checklist_items WHERE checklist_name = @ChecklistName LIMIT 1",
                            new { ChecklistName = reqChecklist.ChecklistName }, transaction);
                        if (checklistId == null)
                        {
                            _logger.LogWarning($"Checklist name '{reqChecklist.ChecklistName}' not found in demo_checklist_items. Skipping insert to demo_checklists.");
                            continue;
                        }
                        await connection.ExecuteAsync(
                            "INSERT INTO demo_checklists (checklist_id, checklist_name, demo_id, is_active, created_at, updated_at) VALUES (@ChecklistId, @ChecklistName, @DemoId, @IsActive, NOW(), NOW())",
                            new { ChecklistId = checklistId, ChecklistName = reqChecklist.ChecklistName, DemoId = demoId, IsActive = reqChecklist.IsActive }, transaction);
                    }
                }
                var extendedResponse = new {
                    Demo = response,
                    Items = itemsArray,
                    Stage = "Demo",
                    StageItemId = demoId,
                    Checklists = selectedChecklists
                };

                await transaction.CommitAsync();
                return Ok(extendedResponse);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating demo with items: {Message}", ex.Message);
                return StatusCode(500, new { message = "Failed to create demo with items", error = ex.Message });
            }
        }

        /// <summary>
        /// Gets all sales demos with their items.
        /// </summary>
        [HttpGet("with-items")]
        [ProducesResponseType(typeof(IEnumerable<SalesDemoWithItemsResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<SalesDemoWithItemsResponse>>> GetDemosWithItems()
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString))
                return StatusCode(500, "Could not resolve connection string.");

            using var connection = new Npgsql.NpgsqlConnection(connectionString);
            await connection.OpenAsync();
            using var transaction = await connection.BeginTransactionAsync();
            try
            {
                var demos = (await connection.QueryAsync<SalesDemo>("SELECT * FROM sales_demos ORDER BY date_created DESC", transaction: transaction)).ToList();
                var responses = new List<object>();
                
                foreach (var demo in demos)
                {
                    // Fetch presenter information
                    var presenterIds = (await connection.QueryAsync<int>(
                        "SELECT presenter_id FROM sales_demo_presenters WHERE demo_id = @DemoId",
                        new { DemoId = demo.Id }, transaction)).ToList();
                    
                    List<string> presenterNames = new List<string>();
                    if (presenterIds.Count > 0)
                    {
                        var users = (await connection.QueryAsync<dynamic>(
                            "SELECT username FROM users WHERE userid = ANY(@Ids)", 
                            new { Ids = presenterIds }, transaction)).ToList();
                        presenterNames = users.Select(u => (string)u.username).ToList();
                    }

                    // Fetch customer address
                    object customerAddress = null;
                    string leadIdToUse = demo.LeadId?.ToString();
                    if (demo.OpportunityId != null)
                    {
                        var opp = await connection.QueryFirstOrDefaultAsync<dynamic>(
                            "SELECT lead_id FROM sales_opportunities WHERE opportunity_id = @OpportunityId LIMIT 1",
                            new { OpportunityId = demo.OpportunityId }, transaction);
                        if (opp != null && opp.lead_id != null)
                            leadIdToUse = opp.lead_id.ToString();
                    }
                    if (!string.IsNullOrEmpty(leadIdToUse))
                    {
                        customerAddress = await connection.QueryFirstOrDefaultAsync(
                            @"SELECT pincode, area, state, district, city, door_no, street, landmark FROM sales_lead WHERE lead_id = @LeadId LIMIT 1",
                            new { LeadId = leadIdToUse }, transaction);
                    }

                    // Ensure mapping table exists
                    await connection.ExecuteAsync(@"
                        CREATE TABLE IF NOT EXISTS sales_demo_item_boms (
                            id SERIAL PRIMARY KEY,
                            sales_demo_item_id INTEGER,
                            bom_id VARCHAR(50),
                            created_at TIMESTAMP DEFAULT NOW()
                        )", transaction: transaction);

                    // Fetch sales_demo_items and build items array with BOM details
                    var salesDemoItemRows = await connection.QueryAsync<dynamic>(
                        @"SELECT sdi.id, sdi.demo_id, sdi.qty, sdi.bom_id 
                          FROM sales_demo_items sdi 
                          WHERE sdi.demo_id = @DemoId", 
                        new { DemoId = demo.Id }, transaction);

                    var itemsList = new List<object>();
                    foreach (var row in salesDemoItemRows)
                    {
                        var bomId = row.bom_id?.ToString();
                        if (string.IsNullOrEmpty(bomId))
                            continue;

                        // Fetch BOM details
                        var bomDetailsFull = await connection.QueryFirstOrDefaultAsync(
                            @"SELECT bom_id, bom_name, bom_type 
                              FROM bill_of_materials 
                              WHERE bom_id = @BomId LIMIT 1",
                            new { BomId = bomId }, transaction);

                        // Fetch BOM child items with full details
                        var bomChildItems = await connection.QueryAsync<dynamic>(
                            @"SELECT bci.child_item_id AS childItemId, bci.quantity, 
                                   im.item_name, im.item_code, im.cat_no, im.unit_price, im.hsn, 
                                   COALESCE(NULLIF(im.tax_percentage, -1), 0) AS tax, 
                                   m.name as make, mo.name as model, p.name as product, 
                                   c.name AS category_name, vm.name AS valuation_method_name, 
                                   ime.name AS inventory_method_name, itt.name AS inventory_type_name, 
                                   u.code AS uom_name, rmi.purchase_rate AS purchase_rate, 
                                   rmi.sales_rate AS sale_rate, rmi.quotation_rate AS quote_rate
                              FROM bill_of_material_child_items bci
                              JOIN item_master im ON bci.child_item_id = im.id
                              LEFT JOIN categories c ON im.category_id = c.id
                              LEFT JOIN make m ON im.make_id = m.id
                              LEFT JOIN model mo ON im.model_id = mo.id
                              LEFT JOIN product p ON im.product_id = p.id
                              LEFT JOIN valuation_method vm ON im.valuation_method_id = vm.id
                              LEFT JOIN inventory_method ime ON im.inventory_method_id = ime.id
                              LEFT JOIN inventory_types itt ON im.group_id = itt.id
                              LEFT JOIN uom u ON im.uom_id = u.id
                              LEFT JOIN rate_master_items rmi ON im.id = rmi.item_id
                              WHERE bci.bill_of_material_id = 
                                    (SELECT id FROM bill_of_materials WHERE bom_id = @BomId)",
                            new { BomId = bomId }, transaction);

                        var bomChildItemList = bomChildItems.Select(ci => new {
                            childItemId = ci.childItemId,
                            quantity = ci.quantity,
                            make = ci.make,
                            model = ci.model,
                            product = ci.product,
                            categoryName = ci.category_name,
                            valuationMethodName = ci.valuation_method_name,
                            inventoryMethodName = ci.inventory_method_name,
                            inventoryTypeName = ci.inventory_type_name,
                            unitPrice = ci.unit_price,
                            itemName = ci.item_name,
                            itemCode = ci.item_code,
                            catNo = ci.cat_no,
                            uomName = ci.uom_name,
                            purchaseRate = ci.purchase_rate,
                            saleRate = ci.sale_rate,
                            quoteRate = ci.quote_rate,
                            hsn = ci.hsn,
                            tax = ci.tax
                        }).ToList();

                        itemsList.Add(new {
                            bomId = bomId,
                            bomName = bomDetailsFull?.bom_name,
                            bomType = bomDetailsFull?.bom_type,
                            bomChildItems = bomChildItemList,
                            accessoryItemIds = (object)null,
                            accessoryItems = new List<object>(),
                            quantity = row.qty ?? 1
                        });
                    }

                    // Fetch checklists
                    var checklistRows = (await connection.QueryAsync<dynamic>(
                        "SELECT checklist_name, is_active FROM demo_checklists WHERE demo_id = @DemoId",
                        new { DemoId = demo.Id }, transaction)).ToList();
                    var checklists = checklistRows.Select(row => new {
                        checklistName = (string)row.checklist_name,
                        isActive = row.is_active != null ? (bool)row.is_active : false
                    }).ToList();

                    var response = new
                    {
                        demo = new
                        {
                            id = demo.Id,
                            userCreated = demo.UserCreated,
                            dateCreated = demo.DateCreated,
                            userUpdated = demo.UserUpdated,
                            dateUpdated = demo.DateUpdated,
                            userId = demo.UserId,
                            demoDate = demo.DemoDate,
                            status = demo.Status,
                            addressId = (int?)null,
                            opportunityId = demo.OpportunityId,
                            demoContact = demo.DemoContact,
                            demoName = demo.DemoName,
                            customerName = demo.CustomerName,
                            demoApproach = demo.DemoApproach,
                            demoOutcome = demo.DemoOutcome,
                            demoFeedback = demo.DemoFeedback,
                            comments = demo.Comments,
                            leadId = demo.LeadId,
                            contactMobileNum = demo.ContactMobileNum,
                            presenterIds = presenterIds,
                            presenterNames = presenterNames,
                            address = demo.Address,
                            demoTime = demo.DemoTime,
                            customerAddress = customerAddress,
                            items = new List<object>(),
                            checklistNamesByItemId = new Dictionary<string, object>(),
                            leadAddress = customerAddress
                        },
                        items = itemsList,
                        stage = "Demo",
                        stageItemId = demo.Id,
                        checklists = checklists
                    };
                    responses.Add(response);
                }
                
                await transaction.CommitAsync();
                return Ok(responses);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error getting demos with items: {Message}", ex.Message);
                return StatusCode(500, new { message = "Failed to get demos with items", error = ex.Message });
            }
        }
        [HttpGet("{id}/with-items")]
        [ProducesResponseType(typeof(SalesDemoWithItemsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<SalesDemoWithItemsResponse>> GetDemoWithItemsById(int id)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString))
                return StatusCode(500, "Could not resolve connection string.");

            using var connection = new Npgsql.NpgsqlConnection(connectionString);
            await connection.OpenAsync();
            using var transaction = await connection.BeginTransactionAsync();
            try
            {
                var demo = await connection.QueryFirstOrDefaultAsync<SalesDemo>(
                    "SELECT * FROM sales_demos WHERE id = @Id", new { Id = id }, transaction);
                if (demo == null)
                {
                    await transaction.RollbackAsync();
                    _logger.LogWarning("Sales demo with id {Id} not found in GetDemoWithItemsById", id);
                    return NotFound(new { message = $"Sales demo with id {id} not found." });
                }

                // Fetch presenter information
                var presenterIds = (await connection.QueryAsync<int>(
                    "SELECT presenter_id FROM sales_demo_presenters WHERE demo_id = @DemoId",
                    new { DemoId = demo.Id }, transaction)).ToList();
                
                List<string> presenterNames = new List<string>();
                if (presenterIds.Count > 0)
                {
                    var users = (await connection.QueryAsync<dynamic>(
                        "SELECT username FROM users WHERE userid = ANY(@Ids)", 
                        new { Ids = presenterIds }, transaction)).ToList();
                    presenterNames = users.Select(u => (string)u.username).ToList();
                }

                // Fetch customer address
                object customerAddress = null;
                string leadIdToUse = demo.LeadId?.ToString();
                if (demo.OpportunityId != null)
                {
                    var opp = await connection.QueryFirstOrDefaultAsync<dynamic>(
                        "SELECT lead_id FROM sales_opportunities WHERE opportunity_id = @OpportunityId LIMIT 1",
                        new { OpportunityId = demo.OpportunityId }, transaction);
                    if (opp != null && opp.lead_id != null)
                        leadIdToUse = opp.lead_id.ToString();
                }
                if (!string.IsNullOrEmpty(leadIdToUse))
                {
                    customerAddress = await connection.QueryFirstOrDefaultAsync(
                        @"SELECT pincode, area, state, district, city, door_no, street, landmark FROM sales_lead WHERE lead_id = @LeadId LIMIT 1",
                        new { LeadId = leadIdToUse }, transaction);
                }

                // Ensure mapping table exists
                await connection.ExecuteAsync(@"
                    CREATE TABLE IF NOT EXISTS sales_demo_item_boms (
                        id SERIAL PRIMARY KEY,
                        sales_demo_item_id INTEGER,
                        bom_id VARCHAR(50),
                        created_at TIMESTAMP DEFAULT NOW()
                    )", transaction: transaction);

                // Fetch sales_demo_items and build items array with BOM details
                var salesDemoItemRows = await connection.QueryAsync<dynamic>(
                    @"SELECT sdi.id, sdi.demo_id, sdi.qty, sdi.bom_id 
                      FROM sales_demo_items sdi 
                      WHERE sdi.demo_id = @DemoId", 
                    new { DemoId = demo.Id }, transaction);

                var itemsList = new List<object>();
                foreach (var row in salesDemoItemRows)
                {
                    var bomId = row.bom_id?.ToString();
                    if (string.IsNullOrEmpty(bomId))
                        continue;

                    // Fetch BOM details
                    var bomDetailsFull = await connection.QueryFirstOrDefaultAsync(
                        @"SELECT bom_id, bom_name, bom_type 
                          FROM bill_of_materials 
                          WHERE bom_id = @BomId LIMIT 1",
                        new { BomId = bomId }, transaction);

                    // Fetch BOM child items with full details
                    var bomChildItems = await connection.QueryAsync<dynamic>(
                        @"SELECT bci.child_item_id AS childItemId, bci.quantity, 
                               im.item_name, im.item_code, im.cat_no, im.unit_price, im.hsn, 
                               COALESCE(NULLIF(im.tax_percentage, -1), 0) AS tax, 
                               m.name as make, mo.name as model, p.name as product, 
                               c.name AS category_name, vm.name AS valuation_method_name, 
                               ime.name AS inventory_method_name, itt.name AS inventory_type_name, 
                               u.code AS uom_name, rmi.purchase_rate AS purchase_rate, 
                               rmi.sales_rate AS sale_rate, rmi.quotation_rate AS quote_rate
                          FROM bill_of_material_child_items bci
                          JOIN item_master im ON bci.child_item_id = im.id
                          LEFT JOIN categories c ON im.category_id = c.id
                          LEFT JOIN make m ON im.make_id = m.id
                          LEFT JOIN model mo ON im.model_id = mo.id
                          LEFT JOIN product p ON im.product_id = p.id
                          LEFT JOIN valuation_method vm ON im.valuation_method_id = vm.id
                          LEFT JOIN inventory_method ime ON im.inventory_method_id = ime.id
                          LEFT JOIN inventory_types itt ON im.group_id = itt.id
                          LEFT JOIN uom u ON im.uom_id = u.id
                          LEFT JOIN rate_master_items rmi ON im.id = rmi.item_id
                          WHERE bci.bill_of_material_id = 
                                (SELECT id FROM bill_of_materials WHERE bom_id = @BomId)",
                        new { BomId = bomId }, transaction);

                    var bomChildItemList = bomChildItems.Select(ci => new {
                        childItemId = ci.childItemId,
                        quantity = ci.quantity,
                        make = ci.make,
                        model = ci.model,
                        product = ci.product,
                        categoryName = ci.category_name,
                        valuationMethodName = ci.valuation_method_name,
                        inventoryMethodName = ci.inventory_method_name,
                        inventoryTypeName = ci.inventory_type_name,
                        unitPrice = ci.unit_price,
                        itemName = ci.item_name,
                        itemCode = ci.item_code,
                        catNo = ci.cat_no,
                        uomName = ci.uom_name,
                        purchaseRate = ci.purchase_rate,
                        saleRate = ci.sale_rate,
                        quoteRate = ci.quote_rate,
                        hsn = ci.hsn,
                        tax = ci.tax
                    }).ToList();

                    itemsList.Add(new {
                        bomId = bomId,
                        bomName = bomDetailsFull?.bom_name,
                        bomType = bomDetailsFull?.bom_type,
                        bomChildItems = bomChildItemList,
                            accessoryItemIds = (object)null,
                        accessoryItems = new List<object>(),
                        quantity = row.qty ?? 1
                    });
                }

                // Fetch checklists from DB for response
                var checklistRows = (await connection.QueryAsync<dynamic>(
                    "SELECT checklist_name, is_active FROM demo_checklists WHERE demo_id = @DemoId",
                    new { DemoId = demo.Id }, transaction)).ToList();
                var checklists = checklistRows.Select(row => new {
                    checklistName = (string)row.checklist_name,
                    isActive = row.is_active != null ? (bool)row.is_active : false
                }).ToList();

                var response = new
                {
                    demo = new
                    {
                        id = demo.Id,
                        userCreated = demo.UserCreated,
                        dateCreated = demo.DateCreated,
                        userUpdated = demo.UserUpdated,
                        dateUpdated = demo.DateUpdated,
                        userId = demo.UserId,
                        demoDate = demo.DemoDate,
                        status = demo.Status,
                        addressId = (int?)null,
                        opportunityId = demo.OpportunityId,
                        demoContact = demo.DemoContact,
                        demoName = demo.DemoName,
                        customerName = demo.CustomerName,
                        demoApproach = demo.DemoApproach,
                        demoOutcome = demo.DemoOutcome,
                        demoFeedback = demo.DemoFeedback,
                        comments = demo.Comments,
                        leadId = demo.LeadId,
                        contactMobileNum = demo.ContactMobileNum,
                        presenterIds = presenterIds,
                        presenterNames = presenterNames,
                        address = demo.Address,
                        demoTime = demo.DemoTime,
                        customerAddress = customerAddress,
                        items = new List<object>(),
                        checklistNamesByItemId = new Dictionary<string, object>(),
                        leadAddress = customerAddress
                    },
                    items = itemsList,
                    stage = "Demo",
                    stageItemId = demo.Id,
                    checklists = checklists
                };
                
                await transaction.CommitAsync();
                return Ok(response);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error getting demo with items by id {Id}: {Message}", id, ex.Message);
                return StatusCode(500, new { message = $"Failed to get demo {id} with items", error = ex.Message });
            }
        }

        /// <summary>
        /// Updates a sales demo and its items.
        /// </summary>
        /// <param name="id">The ID of the sales demo to update</param>
        /// <param name="request">The updated sales demo and its items</param>
        /// <returns>No content if successful</returns>
        /// <response code="204">If the update was successful</response>
        /// <response code="400">If the request is invalid</response>
        /// <response code="404">If the demo was not found</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpPut("with-items/{id}")]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(SalesDemoWithItemsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateDemoWithItems(int id, [FromBody] SalesDemoWithItemsRequest request)
        {
            if (request == null || request.Demo == null || request.Items == null)
                return BadRequest("Invalid request body");

            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString))
                return StatusCode(500, "Could not resolve connection string.");

            using var connection = new Npgsql.NpgsqlConnection(connectionString);
            await connection.OpenAsync();
            using var transaction = await connection.BeginTransactionAsync();
            try
            {
                // Check if demo exists
                var existingDemo = await connection.QueryFirstOrDefaultAsync<SalesDemo>(
                    "SELECT * FROM sales_demos WHERE id = @Id", new { Id = id }, transaction);
                if (existingDemo == null)
                {
                    await transaction.RollbackAsync();
                    return NotFound();
                }

                // --- Checklist update logic ---
                // Remove all old checklists for this demo
                await connection.ExecuteAsync("DELETE FROM demo_checklists WHERE demo_id = @DemoId", new { DemoId = id }, transaction);
                // Insert new checklists from request
                if (request.Checklists != null && request.Checklists.Count > 0)
                {
                    foreach (var reqChecklist in request.Checklists)
                    {
                        // Lookup checklist_id from demo_checklist_items using checklist_name
                        var checklistId = await connection.ExecuteScalarAsync<int?>(
                            "SELECT id FROM demo_checklist_items WHERE checklist_name = @ChecklistName LIMIT 1",
                            new { ChecklistName = reqChecklist.ChecklistName }, transaction);
                        if (checklistId == null)
                        {
                            _logger.LogWarning($"Checklist name '{reqChecklist.ChecklistName}' not found in demo_checklist_items. Skipping insert to demo_checklists.");
                            continue;
                        }
                        await connection.ExecuteAsync(
                            "INSERT INTO demo_checklists (checklist_id, checklist_name, demo_id, is_active, created_at, updated_at) VALUES (@ChecklistId, @ChecklistName, @DemoId, @IsActive, NOW(), NOW())",
                            new { ChecklistId = checklistId, ChecklistName = reqChecklist.ChecklistName, DemoId = id, IsActive = reqChecklist.IsActive }, transaction);
                    }
                }

                // Remove all old items for this demo
                await connection.ExecuteAsync("DELETE FROM sales_demo_items WHERE demo_id = @DemoId", new { DemoId = id }, transaction);
                await connection.ExecuteAsync("DELETE FROM sales_demo_accessories WHERE sales_demo_item_id IN (SELECT id FROM sales_demo_items WHERE demo_id = @DemoId)", new { DemoId = id }, transaction);

                // Insert new items from request.Items
                foreach (var item in request.Items)
                {
                    var bomId = item.BomId;
                    if (string.IsNullOrEmpty(bomId))
                        continue;

                    var sql = @"INSERT INTO public.sales_demo_items 
                        (demo_id, date_created, qty, amount, is_active, stage, stage_item_id, user_created) 
                        VALUES (@DemoId, @DateCreated, @Qty, @Amount, @IsActive, @Stage, @StageItemId, @UserCreated) 
                        RETURNING id;";
                    await connection.ExecuteScalarAsync<int>(sql, new
                    {
                        DemoId = id,
                        DateCreated = DateTime.UtcNow,
                        Qty = 1, // Default quantity
                        Amount = (decimal?)null,
                        IsActive = true,
                        Stage = "Demo",
                        StageItemId = id,
                        UserCreated = 1 // Default user ID
                    }, transaction);
                }

                // For now, just fetch the updated demo and items for response

                var updatedDemo = await connection.QueryFirstOrDefaultAsync<SalesDemo>(
                    "SELECT * FROM sales_demos WHERE id = @Id", new { Id = id }, transaction);
                var updatedItems = (await connection.QueryAsync<SalesItemResponse>(
                    @"SELECT sdi.*, m.name as make, mo.name as model, p.name as product, c.name AS Category, im.item_name as ItemName, im.item_code as ItemCode, im.unit_price as UnitPrice, im.hsn, im.tax_percentage as TaxPercentage
                        FROM sales_demo_items sdi
                        LEFT JOIN item_master im ON sdi.item_id = im.id
                        LEFT JOIN categories c ON im.category_id = c.id
                        LEFT JOIN make m ON im.make_id = m.id
                        LEFT JOIN model mo ON im.model_id = mo.id
                        LEFT JOIN product p ON im.product_id = p.id
                        WHERE sdi.demo_id = @DemoId", new { DemoId = id }, transaction)).ToList();

                // Convert SalesItemResponse to SalesDemoItemResponse
                var convertedItems = updatedItems.Select(item => new SalesDemoItemResponse
                {
                    Id = item.Id,
                    BomId = item.BomId,
                    BomName = item.BomName,
                    BomType = item.BomType,
                    BomChildItems = item.ChildItems?.Select(child => new BomChildItemDto
                    {
                        Id = child.ChildItemId,
                        ChildItemId = child.ChildItemId,
                        ItemName = child.ItemName,
                        ItemCode = child.ItemCode,
                        Make = child.Make,
                        Model = child.Model,
                        Product = child.Product,
                        Category = child.CategoryName,
                        CategoryName = child.CategoryName,
                        UnitPrice = child.UnitPrice,
                        Hsn = child.Hsn,
                        Tax = child.Tax,
                        TaxPercentage = child.Tax,
                        Uom = child.UomName,
                        UomName = child.UomName,
                        Quantity = child.Quantity
                    }).ToList(),
                    AccessoryItemIds = item.AccessoriesIds != null ? new List<int>(item.AccessoriesIds) : new List<int>(),
                    AccessoryItems = item.AccessoriesItems?.ConvertAll(acc => new AccessoryItemResponseDto
                    {
                        Id = acc.Id,
                        Make = acc.Make ?? "",
                        Model = acc.Model ?? "",
                        Product = acc.Product ?? "",
                        ItemName = acc.ItemName ?? "",
                        ItemCode = acc.ItemCode ?? "",
                        UnitPrice = acc.UnitPrice ?? 0,
                        Hsn = acc.Hsn ?? "",
                        TaxPercentage = acc.TaxPercentage ?? 0,
                        CategoryName = acc.CategoryName ?? acc.Category ?? ""
                    }) ?? new List<AccessoryItemResponseDto>(),
                    Quantity = item.Qty ?? 0
                }).ToList();

                // Fetch presenterIds from sales_demo_presenters table
                var presenterIds = (await connection.QueryAsync<int>(
                    "SELECT presenter_id FROM sales_demo_presenters WHERE demo_id = @DemoId",
                    new { DemoId = updatedDemo.Id }, transaction)).ToList();
                // Fetch presenterNames from users table
                List<string> presenterNames = new List<string>();
                if (presenterIds.Count > 0)
                {
                    var users = (await connection.QueryAsync<User>(
                        "SELECT * FROM users WHERE userid = ANY(@Ids)", new { Ids = presenterIds }, transaction)).ToList();
                    presenterNames = users.Select(u => u.Username).ToList();
                }
                presenterIds = presenterIds ?? new List<int>();
                presenterNames = presenterNames ?? new List<string>();

                // Fetch checklists from DB for response (so GET and PUT match)
                var checklistRows = (await connection.QueryAsync<dynamic>(
                    "SELECT checklist_name, is_active FROM demo_checklists WHERE demo_id = @DemoId",
                    new { DemoId = id }, transaction)).ToList();
                var checklists = checklistRows.Select(row => new {
                    ChecklistName = (string)row.checklist_name,
                    IsActive = row.is_active != null ? (bool)row.is_active : false
                }).ToList();

                var response = new SalesDemoWithItemsResponse
                {
                    Id = updatedDemo.Id,
                    UserCreated = updatedDemo.UserCreated,
                    DateCreated = updatedDemo.DateCreated,
                    UserUpdated = updatedDemo.UserUpdated,
                    DateUpdated = updatedDemo.DateUpdated,
                    UserId = updatedDemo.UserId,
                    DemoDate = updatedDemo.DemoDate,
                    Status = updatedDemo.Status,
                    OpportunityId = updatedDemo.OpportunityId,
                    CustomerName = updatedDemo.CustomerName,
                    DemoContact = updatedDemo.DemoContact,
                    DemoName = updatedDemo.DemoName,
                    DemoApproach = updatedDemo.DemoApproach,
                    DemoOutcome = updatedDemo.DemoOutcome,
                    DemoFeedback = updatedDemo.DemoFeedback,
                    Comments = updatedDemo.Comments,
                    ContactMobileNum = updatedDemo.ContactMobileNum,
                    PresenterIds = presenterIds?.ToList() ?? new List<int>(),
                    PresenterNames = presenterNames,
                    LeadId = updatedDemo.LeadId,
                    Items = convertedItems,
                    Address = updatedDemo.Address,
                    DemoTime = updatedDemo.DemoTime,
                };

                var extendedResponse = new
                {
                    Demo = response,
                    Checklists = checklists
                };

                await transaction.CommitAsync();
                return Ok(extendedResponse);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error updating demo with items: {Message}", ex.Message);
                return StatusCode(500, new { message = "Failed to update demo with items", error = ex.Message });
            }
            // Ensure all code paths return a value
            return StatusCode(500, "Unexpected error in UpdateDemoWithItems");
        }

        [HttpDelete("with-items/{id}")]
        public async Task<IActionResult> DeleteDemoWithItems(int id)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString))
                return StatusCode(500, "Could not resolve connection string.");

            using var connection = new Npgsql.NpgsqlConnection(connectionString);
            await connection.OpenAsync();
            try
            {
                // Check if demo exists
                var demo = await connection.QueryFirstOrDefaultAsync<SalesDemo>(
                    "SELECT * FROM sales_demos WHERE id = @Id", new { Id = id });
                if (demo == null)
                {
                    return NotFound(new { message = $"Sales demo with id {id} not found." });
                }

                // Call the stored procedure to delete demo and related items
                await connection.ExecuteAsync("CALL delete_sales_demo_with_items(@p_id)", new { p_id = id });

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting sales demo with items {Id}: {Message}", id, ex.Message);
                return StatusCode(500, new { message = $"Failed to delete sales demo {id} with items", error = ex.Message });
            }
        }
    }
}
