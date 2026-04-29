using System;
using System.Linq;
using System.Collections.Generic;
using ERP.API.Models.DTOs;
using ERP.API.Models;
using ERP.API.Services;
using System.Threading.Tasks;
using System.Data;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Controllers
{
	[ApiController]
	[Route("api/[controller]")]

	public class DashboardController : ControllerBase
	{
		private readonly AppDbContext _context;
		private readonly IDbConnection _dbConnection;
		private readonly ERP.API.Services.ISalesRepDashboardService _salesRepDashboardService;
		private readonly SalesActivityMeetingService _meetingService;
		private readonly SalesActivityCallService _callService;
		private readonly SalesActivityEventService _eventService;

		public DashboardController(
			AppDbContext context,
			IDbConnection dbConnection,
			ERP.API.Services.ISalesRepDashboardService salesRepDashboardService,
			SalesActivityMeetingService meetingService,
			SalesActivityCallService callService,
			SalesActivityEventService eventService)
		{
			_context = context;
			_dbConnection = dbConnection;
			_salesRepDashboardService = salesRepDashboardService;
			_meetingService = meetingService;
			_callService = callService;
			_eventService = eventService;
		}
		// ...existing code...
		/// <summary>
		/// Get today's schedule & appointments for a sales rep (meetings, calls, events)
		/// </summary>
		/// <param name="userId">User ID of the sales rep</param>
		/// <returns>List of today's activities</returns>
		[HttpGet("salesrep-todays-schedule")]
		public async Task<IActionResult> GetSalesRepTodaysSchedule([FromQuery] int? userId = null)
		{
			int targetUserId = userId ?? 0;
			if (targetUserId == 0)
			{
				return BadRequest(new { message = "UserId is required" });
			}

			var today = DateTime.Today;
			var tomorrow = today.AddDays(1);

			// Meetings
			var meetings = (await _meetingService.GetAllAsync(
				"assigned_to = @UserId AND meeting_date_time >= @Today AND meeting_date_time < @Tomorrow",
				new { UserId = targetUserId.ToString(), Today = today, Tomorrow = tomorrow })
			).ToList();

			// Calls
			var calls = (await _callService.GetAllAsync(
				"assigned_to = @UserId AND call_datetime >= @Today AND call_datetime < @Tomorrow",
				new { UserId = targetUserId.ToString(), Today = today, Tomorrow = tomorrow })
			).ToList();

			// Events
			var events = (await _eventService.GetAllAsync(
				"assigned_to = @UserId AND start_date >= @Today AND start_date < @Tomorrow",
				new { UserId = targetUserId.ToString(), Today = today, Tomorrow = tomorrow })
			).ToList();

			var items = new List<SalesRepScheduleItemDto>();

			// Map meetings
			items.AddRange(meetings.Select(m => new SalesRepScheduleItemDto
			{
				Type = "Meeting",
				Title = m.MeetingTitle ?? "Meeting",
				Location = m.Address ?? m.City ?? m.Area,
				With = m.Participant ?? m.CustomerName,
				StartDateTime = m.MeetingDateTime,
				Status = m.Status,
				Label = m.MeetingType,
				Description = m.Description
			}));

			// Map calls
			items.AddRange(calls.Select(c => new SalesRepScheduleItemDto
			{
				Type = "Call",
				Title = c.CallTitle ?? "Call",
				Location = null,
				With = c.CallWith ?? c.Participants,
				StartDateTime = c.CallDateTime,
				Status = c.Status,
				Label = c.CallType,
				Description = c.Description
			}));

			// Map events
			items.AddRange(events.Select(e => new SalesRepScheduleItemDto
			{
				Type = "Event",
				Title = e.EventTitle,
				Location = e.EventLocation,
				With = e.Participant,
				StartDateTime = e.StartDate.Date.Add(e.StartTime),
				Status = e.Status,
				Label = null,
				Description = e.Description
			}));

			// Sort by time
			items = items.OrderBy(i => i.StartDateTime).ToList();

			var result = new SalesRepScheduleDto { Items = items };
			return Ok(result);
	}
        
		[HttpGet("salesrep-dashboard-pipeline")]
		public async Task<IActionResult> GetSalesRepDashboard([FromQuery] int? userId = null)
		{
			int targetUserId = userId ?? 0;
			if (targetUserId == 0)
			{
				return BadRequest(new { message = "UserId is required" });
			}
			var data = await _salesRepDashboardService.GetDashboardDataAsync(targetUserId);
			return Ok(data);
		}
		// 5. Get all roles and their names
		// [HttpGet("all-roles")]
		// public async Task<IActionResult> GetAllRoles()
		// {
		// 	var sql = "SELECT roleid, rolename FROM public.roles ORDER BY rolename";
		// 	var roles = (await _dbConnection.QueryAsync(sql)).ToList();
		// 	return Ok(roles);
		// }

		// 1. Lead Distribution Over Time
		[HttpGet("lead-distribution")]
		public async Task<IActionResult> GetLeadDistribution()
		{
			var today = DateTime.UtcNow.Date;
			var weekAgo = today.AddDays(-6);
			var data = await _context.SalesLeads
				.Where(l => l.DateCreated >= weekAgo)
				.GroupBy(l => l.DateCreated.Value.Date)
				.Select(g => new {
					Day = g.Key,
					Assigned = g.Count(x => x.AssignedTo != null),
					Unassigned = g.Count(x => x.AssignedTo == null)
				})
				.OrderBy(x => x.Day)
				.ToListAsync();
			return Ok(data);
		}

		// 2. Lead Aging Summary
		[HttpGet("lead-aging-summary")]
		public async Task<IActionResult> GetLeadAgingSummary()
		{
			var leads = await _context.SalesLeads.ToListAsync();
			var today = DateTime.UtcNow.Date;
			var agedThreshold = 7; // days
			var newLeads = leads.Count(l => (today - l.DateCreated.Value.Date).Days <= agedThreshold);
			var agedLeads = leads.Count(l => (today - l.DateCreated.Value.Date).Days > agedThreshold);
			var avgAge = leads.Any() ? leads.Average(l => (today - l.DateCreated.Value.Date).Days) : 0;
			var oldest = leads.Any() ? leads.Max(l => (today - l.DateCreated.Value.Date).Days) : 0;
			return Ok(new {
				New = newLeads,
				Aged = agedLeads,
				AvgAge = avgAge,
				Oldest = oldest
			});
		}

		// 3. User Roles & Permissions Summary: count of userroles (role names) for each region
		[HttpGet("user-roles-summary")]
		public async Task<IActionResult> GetUserRolesSummary()
		{
			// Join teamhierarchy, userroles, and roles to get region, rolename, and count
			var sql = @"
				SELECT th.region, r.rolename, COUNT(*) as count
				FROM public.teamhierarchy th
				JOIN public.userroles ur ON th.userid = ur.userid
				JOIN public.roles r ON ur.roleid = r.roleid
				GROUP BY th.region, r.rolename
				ORDER BY th.region, r.rolename
			";
			var data = (await _dbConnection.QueryAsync(sql)).ToList();
			var total = data.Sum(x => (int)x.count);
			return Ok(new { data, total });
		}

		// 4. Manager & Sales Rep Distribution: fetch rolename and their count based on region
		[HttpGet("manager-sales-distribution")]
		public async Task<IActionResult> GetManagerSalesDistribution()
		{
			var sql = @"
				SELECT th.region, r.rolename, COUNT(*) as count
				FROM public.teamhierarchy th
				JOIN public.userroles ur ON th.userid = ur.userid
				JOIN public.roles r ON ur.roleid = r.roleid
				GROUP BY th.region, r.rolename
				ORDER BY th.region, r.rolename
			";
			var data = (await _dbConnection.QueryAsync(sql)).ToList();
			return Ok(data);
	}
		/// <summary>
		/// Get top selling products based on quotation sales
		/// </summary>
		public class TopSellingProductsRequest
		{
			public int UserId { get; set; }
		
		}

		[HttpPost("top-selling-products")]
		public async Task<IActionResult> GetTopSellingProducts([FromBody] TopSellingProductsRequest request)
		{
			if (request == null || request.UserId <= 0)
				return BadRequest(new { message = "UserId is required" });

			var sql = @"
				SELECT 
					im.id AS ItemId,
					im.item_name AS ItemName,
					im.image_url AS ImageUrl,
					SUM(sp.qty) AS UnitsSold,
					MAX(q.quotation_date) AS LastSoldDate
				FROM public.sales_product sp
				JOIN public.item_master im ON sp.item_id = im.id
				JOIN public.sales_quotations q ON sp.stage_item_id ~ '^[0-9]+$' AND CAST(sp.stage_item_id AS INTEGER) = q.id
				WHERE sp.is_active = true AND im.is_active = true
				GROUP BY im.id, im.item_name, im.image_url
				ORDER BY UnitsSold DESC, LastSoldDate DESC
				LIMIT @TopCount
			";
			var result = (await _dbConnection.QueryAsync<ERP.API.Models.DTOs.TopSellingProductDto>(sql, new { TopCount = 5 })).ToList();
			return Ok(result);
		}
    
		/// <summary>
		/// Get sales manager dashboard pipeline by stage
		/// </summary>
		[HttpGet("salesmanager-dashboard-pipeline")]
		public async Task<IActionResult> GetSalesManagerDashboardPipeline([FromQuery] int? managerId = null)
		{
			if (managerId == null || managerId <= 0)
			{
				return BadRequest(new { message = "managerId is required" });
			}

			// Get all userIds under this manager (excluding users with rolename 'Manager')
			var teamUserIds = (from th in _context.TeamHierarchy
							   join ur in _context.UserRoles on th.UserId equals ur.UserId
							   join r in _context.Roles on ur.RoleId equals r.RoleId
							   where th.UserId != null && th.UserId > 0 && r.RoleName != null && r.RoleName != "Manager" && th.Region != null
							   && _context.TeamHierarchy.Any(m => m.UserId == managerId && m.Region == th.Region)
							   select th.UserId).ToList();

			// 1. Qualification: leads with Status = 'Qualified' or 'Converted' assigned to team
			var qualificationCount = await _context.SalesLeads.CountAsync(l => (l.Status == "Qualified" || l.Status == "Converted") && l.AssignedTo != null && teamUserIds.Contains(l.AssignedTo.Value));

			// 2. Proposal: all created quotations assigned to team

			var proposalCount = await _dbConnection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM sales_quotations WHERE user_created = ANY(@UserIds)", new { UserIds = teamUserIds.ToArray() });

			// 3. Negotiation: quotations with Status = 'Negotiation' assigned to team
			var negotiationCount = await _dbConnection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM sales_quotations WHERE status = @status AND user_created = ANY(@UserIds)", new { status = "Negotiation", UserIds = teamUserIds.ToArray() });

			// 4. Solution Presentation: SalesDemo count assigned to team
			var solutionPresentationCount = await _dbConnection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM sales_demos WHERE user_id = ANY(@UserIds)", new { UserIds = teamUserIds.ToArray() });

			// 5. Closed Won: SalesOrder count assigned to team
			var closedWonCount = await _dbConnection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM sales_orders WHERE user_created = ANY(@UserIds)", new { UserIds = teamUserIds.ToArray() });

			var result = new List<object>
			{
				new { Stage = "Qualification", Count = qualificationCount },
				new { Stage = "Proposal", Count = proposalCount },
				new { Stage = "Negotiation", Count = negotiationCount },
				new { Stage = "Solution Presentation", Count = solutionPresentationCount },
				new { Stage = "Closed Won", Count = closedWonCount }
			};

			return Ok(result);
		}

		/// <summary>
		/// Get count of leads, opportunities, and closed won (delivered) for sales manager dashboard
		/// </summary>
		[HttpGet("salesmanager-dashboard-conversion-counts")]
		public async Task<IActionResult> GetSalesManagerDashboardConversionCounts([FromQuery] int? managerId = null)
		{
			if (managerId == null || managerId <= 0)
			{
				return BadRequest(new { message = "managerId is required" });
			}

			// Get all userIds under this manager (including the manager if needed)
			var teamUserIds = (from th in _context.TeamHierarchy
							   join ur in _context.UserRoles on th.UserId equals ur.UserId
							   join r in _context.Roles on ur.RoleId equals r.RoleId
							   where th.UserId != null && th.UserId > 0 && r.RoleName != null && r.RoleName != "Manager" && th.Region != null
							   && _context.TeamHierarchy.Any(m => m.UserId == managerId && m.Region == th.Region)
							   select th.UserId).ToList();

			// Count of Leads assigned to team
			var leadsCount = await _context.SalesLeads.CountAsync(l => l.AssignedTo != null && teamUserIds.Contains(l.AssignedTo.Value));

			// Count of Opportunities (sales_quotations) assigned to team
			var opportunitiesCount = await _dbConnection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM sales_quotations WHERE user_created = ANY(@UserIds)", new { UserIds = teamUserIds.ToArray() });

			// Count of Closed Won (delivered) (sales_orders) assigned to team
			var closedWonCount = await _dbConnection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM sales_orders WHERE user_created = ANY(@UserIds)", new { UserIds = teamUserIds.ToArray() });

			return Ok(new {
				Leads = leadsCount,
				Opportunities = opportunitiesCount,
				ClosedWon = closedWonCount
			});
		}
	}
}