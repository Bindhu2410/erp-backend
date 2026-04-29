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
    public class SalesLeadsBusinessChallengeController : ControllerBase
    {
        private readonly SalesLeadsBusinessChallengeService _businessChallengeService;
        private readonly SalesSummaryService _summaryService;
        private readonly SalesLeadService _salesLeadService;

        public SalesLeadsBusinessChallengeController(
            SalesLeadsBusinessChallengeService businessChallengeService,
            SalesSummaryService summaryService,
            SalesLeadService salesLeadService)
        {
            _businessChallengeService = businessChallengeService;
            _summaryService = summaryService;
            _salesLeadService = salesLeadService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SalesLeadsBusinessChallenge>>> GetAll()
        {
            var challenges = await _businessChallengeService.GetAllAsync();
            return Ok(challenges);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SalesLeadsBusinessChallenge>> GetById(int id)
        {
            var challenge = await _businessChallengeService.GetByIdAsync(id);
            if (challenge == null)
                return NotFound($"Business Challenge with ID {id} not found");

            return Ok(challenge);
        }

        [HttpGet("lead/{salesLeadId}")]
        public async Task<ActionResult<IEnumerable<SalesLeadsBusinessChallenge>>> GetByLeadId(int salesLeadId)
        {
            var challenges = await _businessChallengeService.GetAllAsync("sales_leads_id = @SalesLeadId", new { SalesLeadId = salesLeadId });
            return Ok(challenges);
        }

        [HttpPost]
        public async Task<ActionResult<int>> Create(SalesLeadsBusinessChallenge challenge)
        {
             if(challenge.IsActive == null)
            {
                challenge.IsActive = true;
            }
            var id = await _businessChallengeService.CreateAsync(challenge);

            if (challenge.SalesLeadsId.HasValue)
            {
                var lead = await _salesLeadService.GetByIdAsync(challenge.SalesLeadsId.Value);
                if (lead != null)
                {
                    var summary = new SalesSummary
                    {
                        Title = $"Business Challenge created",
                        Description = $"New business challenge '{challenge.Challenges}' added to lead {lead.CustomerName}",
                        DateTime = DateTime.UtcNow,
                        Stage = "lead",
                        StageItemId = lead.Id.ToString(),
                        IsActive = true,
                        Entities = System.Text.Json.JsonSerializer.Serialize(new 
                        { 
                            LeadId = lead.Id,
                            ChallengeId = id,
                            ChallengeName = challenge.Challenges,
                            Solution = challenge.Solution,
                            CustomerName = lead.CustomerName 
                        })
                    };
                    await _summaryService.CreateAsync(summary);
                }
            }

            return CreatedAtAction(nameof(GetById), new { id }, id);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, SalesLeadsBusinessChallenge challenge)
        {
            if (id != challenge.Id)
                return BadRequest("ID mismatch");
 if(challenge.IsActive == null)
            {
                challenge.IsActive = true;
            }
            await _businessChallengeService.UpdateAsync(challenge);

            if (challenge.SalesLeadsId.HasValue)
            {
                var lead = await _salesLeadService.GetByIdAsync(challenge.SalesLeadsId.Value);
                if (lead != null)
                {
                    var summary = new SalesSummary
                    {
                        Title = $"Business Challenge updated",
                        Description = $"Business challenge '{challenge.Challenges}' updated for lead {lead.CustomerName}",
                        DateTime = DateTime.UtcNow,
                        Stage = "lead",
                        StageItemId = lead.Id.ToString(),
                        IsActive = true,
                        Entities = System.Text.Json.JsonSerializer.Serialize(new 
                        { 
                            LeadId = lead.Id,
                            ChallengeId = id,
                            ChallengeName = challenge.Challenges,
                            Solution = challenge.Solution,
                            CustomerName = lead.CustomerName 
                        })
                    };
                    await _summaryService.CreateAsync(summary);
                }
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var challenge = await _businessChallengeService.GetByIdAsync(id);
            if (challenge == null)
                return NotFound($"Business Challenge with ID {id} not found");

            if (challenge.SalesLeadsId.HasValue)
            {
                var lead = await _salesLeadService.GetByIdAsync(challenge.SalesLeadsId.Value);
                if (lead != null)
                {
                    var summary = new SalesSummary
                    {
                        Title = $"Business Challenge deleted - {challenge.Challenges}",
                        Description = $"Business challenge '{challenge.Challenges}' removed from lead {lead.CustomerName}",
                        DateTime = DateTime.UtcNow,
                        Stage = "lead",
                        StageItemId = lead.Id.ToString(),
                        IsActive = true,
                        Entities = System.Text.Json.JsonSerializer.Serialize(new 
                        { 
                            LeadId = lead.Id,
                            ChallengeId = id,
                            ChallengeName = challenge.Challenges,
                            Solution = challenge.Solution,
                            CustomerName = lead.CustomerName 
                        })
                    };
                    await _summaryService.CreateAsync(summary);
                }
            }

            await _businessChallengeService.DeleteAsync(id);
            return NoContent();
        }
    }
}