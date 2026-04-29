using System.Threading.Tasks;
using ERP.API.Models;
using ERP.API.Models.CompanySetup;
using ERP.API.Models.DTOs.CompanySetup;
using ERP.API.Services.CompanySetup;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System;

namespace ERP.API.Controllers.CompanySetup
{
    [ApiController]
    [Route("api/[controller]")]
    public class CsIntercompanyAccountController : ControllerBase
    {
        private readonly ICsIntercompanyAccountService _intercompanyAccountService;

        public CsIntercompanyAccountController(ICsIntercompanyAccountService intercompanyAccountService)
        {
            _intercompanyAccountService = intercompanyAccountService;
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var account = await _intercompanyAccountService.GetByIdAsync(id);
                if (account == null)
                    return NotFound(new { message = $"Intercompany account with id {id} not found.", data = (object?)null });
                return Ok(new { message = "Intercompany account retrieved successfully", data = MapToDto(account) });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the intercompany account.", error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpGet("relationship")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByRelationship([FromQuery] CsIntercompanyAccountSearchDto searchDto)
        {
            try
            {
                var (data, totalRecords, filteredRecords) = await _intercompanyAccountService.GetByRelationshipAsync(searchDto);
                var dtoList = data.Select(MapToDto).ToList();
                return Ok(new {
                    message = "Intercompany accounts retrieved successfully by relationship",
                    data = dtoList,
                    totalCount = totalRecords,
                    pageSize = searchDto.PageSize,
                    pageNumber = searchDto.PageNumber
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving intercompany accounts by relationship.", error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CsIntercompanyAccountDto accountDto)
        {
            try
            {
                var account = MapFromDto(accountDto);
                var accountId = await _intercompanyAccountService.CreateAsync(account);
                accountDto.IntercompanyAccountId = accountId;
                return CreatedAtAction(nameof(GetById), new { id = accountId }, new { message = "Intercompany account created successfully", data = accountDto });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the intercompany account.", error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] CsIntercompanyAccountDto accountDto)
        {
            try
            {
                if (id != accountDto.IntercompanyAccountId)
                    return BadRequest(new { message = "ID in URL does not match ID in body." });
                var success = await _intercompanyAccountService.UpdateAsync(MapFromDto(accountDto));
                if (!success)
                    return NotFound(new { message = $"Intercompany account with id {id} not found for update.", data = (object?)null });
                return Ok(new { message = "Intercompany account updated successfully", data = (object?)null });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the intercompany account.", error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var success = await _intercompanyAccountService.DeleteAsync(id);
                if (!success)
                    return NotFound(new { message = $"Intercompany account with id {id} not found for deletion.", data = (object?)null });
                return Ok(new { message = "Intercompany account deleted successfully", data = (object?)null });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the intercompany account.", error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        private static CsIntercompanyAccountDto MapToDto(CsIntercompanyAccount account)
        {
            return new CsIntercompanyAccountDto
            {
                IntercompanyAccountId = account.IntercompanyAccountId,
                RelationshipId = account.RelationshipId,
                TransactionType = account.TransactionType,
                Company1ReceivableAccountId = account.Company1ReceivableAccountId,
                Company2PayableAccountId = account.Company2PayableAccountId,
                Company1TaxTreatmentRule = account.Company1TaxTreatmentRule,
                Company2TaxTreatmentRule = account.Company2TaxTreatmentRule,
                IsActive = account.IsActive,
                CreatedAt = account.CreatedAt,
                UpdatedAt = account.UpdatedAt
            };
        }

        private static CsIntercompanyAccount MapFromDto(CsIntercompanyAccountDto dto)
        {
            return new CsIntercompanyAccount
            {
                IntercompanyAccountId = dto.IntercompanyAccountId,
                RelationshipId = dto.RelationshipId,
                TransactionType = dto.TransactionType,
                Company1ReceivableAccountId = dto.Company1ReceivableAccountId,
                Company2PayableAccountId = dto.Company2PayableAccountId,
                Company1TaxTreatmentRule = dto.Company1TaxTreatmentRule,
                Company2TaxTreatmentRule = dto.Company2TaxTreatmentRule,
                IsActive = dto.IsActive
            };
        }
    }
}
