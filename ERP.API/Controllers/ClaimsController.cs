using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ERP.API.Models;
using ERP.API.Models.DTOs;
using System.Linq;

namespace ERP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClaimsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ClaimsController(AppDbContext context)
        {
            _context = context;
        }

        // Try to extract current user id from JWT claims, fallback to 1 if unavailable
        private int GetCurrentUserId()
        {
            var userIdClaim = User?.Claims?.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier || c.Type == "sub");
            return userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId) ? userId : 1;
        }

        // GET: api/Claims
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Claim>>> GetClaims()
        {
            // Include items so callers can see per-item rows for each claim
            return await _context.Claims
                .Include(c => c.Items)
                .ToListAsync();
        }

        // GET: api/Claims/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Claim>> GetClaim(int id)
        {
            var claim = await _context.Claims
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (claim == null)
            {
                return NotFound();
            }

            return claim;
        }

        // POST: api/Claims
        [HttpPost]
        public async Task<ActionResult<Claim>> PostClaim([FromBody] ClaimCreateDto dto)
        {
            var claim = new Claim
            {
                ClaimNo = dto.ClaimNo,
                ClaimDate = dto.ClaimDate,
                UserName = dto.UserName,
                ClaimType = dto.ClaimType,
                ModeOfTravel = dto.ModeOfTravel,
                DateCreated = DateTime.UtcNow
            };

            // set the creating user so DB triggers/tasks have owner_id
            claim.UserCreated = GetCurrentUserId();

            // Auto-generate claim_no if not provided or is default value
            if (string.IsNullOrEmpty(claim.ClaimNo) || claim.ClaimNo == "string")
            {
                var currentYear = DateTime.Now.Year;
                var count = await _context.Claims.CountAsync(c => c.ClaimNo.StartsWith($"CLM/{currentYear}/"));
                claim.ClaimNo = $"CLM/{currentYear}/{(count + 1):D3}";
            }

            if (dto.Items != null && dto.Items.Any())
            {
                claim.Items = dto.Items.Select(i => new ClaimItem
                {
                    FromPlace = i.FromPlace,
                    ToPlace = i.ToPlace,
                    ModeOfTravel = i.ModeOfTravel,
                    ExpenseType = i.ExpenseType,
                    Amount = i.Amount ?? 0,
                    ActualKm = i.ActualKm,
                    Comments = i.Comments,
                    BillUrl = i.BillUrl
                }).ToList();
            }

            _context.Claims.Add(claim);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetClaim), new { id = claim.Id }, claim);
        }

        // PUT: api/Claims/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutClaim(int id, [FromBody] Claim claim)
        {
            if (id != claim.Id)
            {
                return BadRequest();
            }

            claim.DateUpdated = DateTime.UtcNow;
            _context.Entry(claim).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ClaimExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Claims/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClaim(int id)
        {
            var claim = await _context.Claims.FindAsync(id);
            if (claim == null)
            {
                return NotFound();
            }

            _context.Claims.Remove(claim);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ClaimExists(int id)
        {
            return _context.Claims.Any(e => e.Id == id);
        }
    }
}