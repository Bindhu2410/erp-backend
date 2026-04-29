using Dapper;
using ERP.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class DemoCalendarController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly string _connectionString;

        public DemoCalendarController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        /// <summary>
        /// Check availability for all items of a product for a given date.
        /// GET /api/DemoCalendar/availability?productId=5&date=2026-04-05
        /// 
        /// Parameters:
        ///   productId: The product ID to fetch items for.
        ///   date: The date to check availability (yyyy-MM-dd).
        /// 
        /// Response:
        ///   [
        ///     {
        ///       "itemid": 1,
        ///       "itemname": "Item Name",
        ///       "itemcode": "CODE123",
        ///       "bookeddates": [
        ///         { "from": "2026-04-01", "to": "2026-04-05", "demoId": 10, "demoName": "Demo A" },
        ///         ...
        ///       ]
        ///     },
        ///     ...
        ///   ]
        ///
        /// To fetch all bookings for a month, use /api/DemoCalendar/bookings?month=4&year=2026
        /// </summary>
        [HttpPost("availability")]
        public async Task<IActionResult> CheckAvailability([FromBody] ERP.API.Models.AvailabilityRequest request)
        {
            var fromDate = new DateTime(request.Year, request.Month, request.Day);
            DateTime toDate = (request.ToDay.HasValue && request.ToMonth.HasValue && request.ToYear.HasValue)
                ? new DateTime(request.ToYear.Value, request.ToMonth.Value, request.ToDay.Value)
                : fromDate;

            using var connection = new NpgsqlConnection(_connectionString);

            // Get the item
            var item = await _context.ItemMasters.FindAsync(request.ItemId);
            if (item == null)
                return NotFound();

            // Get all bookings for this item that overlap the date range
            var bookings = await connection.QueryAsync(@"
                SELECT b.booked_from, b.booked_to, b.demo_id, sd.demo_name
                FROM demo_item_bookings b
                JOIN sales_demos sd ON b.demo_id = sd.id
                WHERE b.item_id = @ItemId
                  AND b.is_active = true
                  AND b.status = 'Booked'
                  AND b.booked_from <= @ToDate
                  AND (b.booked_to IS NULL OR b.booked_to >= @FromDate)
            ", new { ItemId = request.ItemId, FromDate = fromDate.Date, ToDate = toDate.Date });

            var bookedDates = bookings.Select(b => new {
                from = ((DateTime)b.booked_from).ToString("yyyy-MM-dd"),
                to = b.booked_to != null ? ((DateTime)b.booked_to).ToString("yyyy-MM-dd") : null,
                demoId = b.demo_id,
                demoName = b.demo_name
            }).ToList();

            var result = new[]
            {
                new {
                    itemid = item.Id,
                    itemname = item.ItemName,
                    itemcode = item.ItemCode,
                    bookeddates = bookedDates
                }
            };

            return Ok(result);
        }

        /// <summary>
        /// Get all booked date ranges for a specific item — used to highlight dates in calendar/date-picker.
        /// GET /api/DemoCalendar/booked-dates?itemId=5
        /// </summary>
        [HttpGet("booked-dates")]
        public async Task<IActionResult> GetBookedDates([FromQuery] int itemId)
        {
            using var connection = new NpgsqlConnection(_connectionString);

            var bookings = await connection.QueryAsync(@"
                SELECT b.id, b.demo_id, b.booked_from AS ""from"", b.booked_to AS ""to"",
                       b.status, b.qty,
                       sd.customer_name, sd.demo_name
                FROM demo_item_bookings b
                JOIN sales_demos sd ON b.demo_id = sd.id
                WHERE b.item_id = @ItemId
                  AND b.is_active = true
                ORDER BY b.booked_from ASC
            ", new { ItemId = itemId });

            return Ok(bookings);
        }

        /// <summary>
        /// Get all active bookings for a month — used to render a full calendar grid.
        /// GET /api/DemoCalendar/bookings?month=4&amp;year=2026
        /// </summary>
        [HttpGet("bookings")]
        public async Task<IActionResult> GetBookings([FromQuery] int? month, [FromQuery] int? year)
        {
            using var connection = new NpgsqlConnection(_connectionString);

            var sql = @"
                SELECT b.id, b.demo_id, b.item_id, b.qty,
                       b.booked_from, b.booked_to, b.status,
                       im.item_name, im.item_code,
                       sd.customer_name, sd.demo_name, sd.address
                FROM demo_item_bookings b
                JOIN item_master im ON b.item_id = im.id
                JOIN sales_demos sd ON b.demo_id = sd.id
                WHERE b.is_active = true";

            object param;
            if (month.HasValue && year.HasValue)
            {
                var start = new DateTime(year.Value, month.Value, 1);
                var end = start.AddMonths(1).AddDays(-1);
                sql += " AND b.booked_from <= @End AND (b.booked_to IS NULL OR b.booked_to >= @Start)";
                sql += " ORDER BY b.booked_from ASC";
                param = new { Start = start, End = end };
            }
            else
            {
                sql += " ORDER BY b.booked_from ASC";
                param = new { };
            }

            var bookings = await connection.QueryAsync(sql, param);
            return Ok(bookings);
        }

        /// <summary>
        /// Get all bookings (history + upcoming) for one specific item.
        /// GET /api/DemoCalendar/item/{itemId}/bookings
        /// </summary>
        [HttpGet("item/{itemId}/bookings")]
        public async Task<IActionResult> GetItemBookings(int itemId)
        {
            using var connection = new NpgsqlConnection(_connectionString);

            var bookings = await connection.QueryAsync(@"
                SELECT b.id, b.demo_id, b.qty,
                       b.booked_from, b.booked_to, b.status, b.is_active, b.date_created,
                       im.item_name, im.item_code,
                       sd.customer_name, sd.demo_name, sd.address, sd.demo_date
                FROM demo_item_bookings b
                JOIN item_master im ON b.item_id = im.id
                JOIN sales_demos sd ON b.demo_id = sd.id
                WHERE b.item_id = @ItemId
                ORDER BY b.booked_from DESC
            ", new { ItemId = itemId });

            return Ok(bookings);
        }

        /// <summary>
        /// Create bookings for all items in a demo when it is approved.
        /// POST /api/DemoCalendar/book
        /// Body: { "demoId": 12, "bookedFrom": "2026-04-05", "bookedTo": "2026-04-12" }
        /// </summary>
        [HttpPost("book")]
        public async Task<IActionResult> BookDemo([FromBody] DemoBookRequest request)
        {
            if (request.DemoId <= 0 || request.BookedFrom == default)
                return BadRequest("DemoId and BookedFrom are required.");

            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();
            using var transaction = await connection.BeginTransactionAsync();

            try
            {
                // Get all items for this demo
                var demoItems = await connection.QueryAsync<dynamic>(@"
                    SELECT item_id, qty FROM sales_demo_items
                    WHERE demo_id = @DemoId AND is_active = true AND item_id IS NOT NULL
                ", new { DemoId = request.DemoId }, transaction);

                if (!demoItems.Any())
                {
                    await transaction.RollbackAsync();
                    return BadRequest("No active items found for this demo.");
                }

                // Check for conflicts before inserting
                var conflicts = new List<object>();
                foreach (var di in demoItems)
                {
                    var conflict = await connection.QueryFirstOrDefaultAsync<dynamic>(@"
                        SELECT b.id, im.item_name, sd.customer_name, b.booked_from, b.booked_to
                        FROM demo_item_bookings b
                        JOIN item_master im ON b.item_id = im.id
                        JOIN sales_demos sd ON b.demo_id = sd.id
                        WHERE b.item_id = @ItemId
                          AND b.is_active = true
                          AND b.status = 'Booked'
                          AND b.booked_from <= @BookedTo
                          AND (b.booked_to IS NULL OR b.booked_to >= @BookedFrom)
                    ", new
                    {
                        ItemId = (int)di.item_id,
                        BookedFrom = request.BookedFrom.Date,
                        BookedTo = (request.BookedTo ?? request.BookedFrom).Date
                    }, transaction);

                    if (conflict != null)
                        conflicts.Add(conflict);
                }

                if (conflicts.Count > 0)
                {
                    await transaction.RollbackAsync();
                    return Conflict(new
                    {
                        message = "One or more items are already booked for the selected dates.",
                        conflicts
                    });
                }

                // Insert booking rows
                foreach (var di in demoItems)
                {
                    await connection.ExecuteAsync(@"
                        INSERT INTO demo_item_bookings (item_id, demo_id, qty, booked_from, booked_to, status, is_active, date_created)
                        VALUES (@ItemId, @DemoId, @Qty, @BookedFrom, @BookedTo, 'Booked', true, NOW())
                    ", new
                    {
                        ItemId = (int)di.item_id,
                        DemoId = request.DemoId,
                        Qty = (int)(di.qty ?? 1),
                        BookedFrom = request.BookedFrom.Date,
                        BookedTo = request.BookedTo?.Date
                    }, transaction);
                }

                await transaction.CommitAsync();
                return Ok(new { message = "Demo items booked successfully.", demoId = request.DemoId });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "Failed to book demo items.", error = ex.Message });
            }
        }

        /// <summary>
        /// Release all bookings for a demo when items are returned.
        /// PUT /api/DemoCalendar/release/{demoId}
        /// </summary>
        [HttpPut("release/{demoId}")]
        public async Task<IActionResult> ReleaseDemo(int demoId)
        {
            using var connection = new NpgsqlConnection(_connectionString);

            var affected = await connection.ExecuteAsync(@"
                UPDATE demo_item_bookings
                SET status = 'Returned', is_active = false
                WHERE demo_id = @DemoId AND is_active = true
            ", new { DemoId = demoId });

            if (affected == 0)
                return NotFound(new { message = "No active bookings found for this demo." });

            return Ok(new { message = "Bookings released successfully.", demoId, itemsReleased = affected });
        }
    }

    public class DemoBookRequest
    {
        public int DemoId { get; set; }
        public DateTime BookedFrom { get; set; }
        public DateTime? BookedTo { get; set; }
    }
}
