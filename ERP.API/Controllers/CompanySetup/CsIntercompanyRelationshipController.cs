using System;
using System.Linq;
using System.Threading.Tasks;
using ERP.API.Models;
using ERP.API.Models.CompanySetup;
using ERP.API.Models.DTOs.CompanySetup;
using ERP.API.Services.CompanySetup;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers.CompanySetup
{
    [Route("api/[controller]")]
    [ApiController]
    public class CsIntercompanyRelationshipController : ControllerBase
    {
        private readonly ICsIntercompanyRelationshipService _relationshipService;

        public CsIntercompanyRelationshipController(ICsIntercompanyRelationshipService relationshipService)
        {
            _relationshipService = relationshipService;
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var relationship = await _relationshipService.GetByIdAsync(id);
                if (relationship == null)
                    return NotFound(new { message = $"Relationship with id {id} not found.", data = (object?)null });
                return Ok(new { message = "Relationship retrieved successfully", data = MapToDto(relationship) });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the relationship.", error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpGet("search")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Search([FromQuery] CsIntercompanyRelationshipSearchDto searchDto)
        {
            try
            {
                var (data, totalRecords, filteredRecords) = await _relationshipService.SearchAsync(searchDto);
                var dtoList = data.Select(MapToDto).ToList();
                return Ok(new {
                    message = "Search completed successfully",
                    data = dtoList,
                    totalCount = totalRecords,
                    pageSize = 10, // You might want to make this configurable
                    pageNumber = 1 // You might want to get this from searchDto
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while searching relationships.", error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpGet("company/{companyId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByCompany(int companyId, [FromQuery] bool activeOnly = true)
        {
            try
            {
                var relationships = await _relationshipService.GetByCompanyAsync(companyId, activeOnly);
                return Ok(new { message = "Relationships retrieved successfully", data = relationships.Select(MapToDto).ToArray() });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving relationships by company.", error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CsIntercompanyRelationshipDto relationshipDto)
        {
            try
            {
                var relationship = MapFromDto(relationshipDto);
                var relationshipId = await _relationshipService.CreateAsync(relationship);
                relationshipDto.RelationshipId = relationshipId;
                return CreatedAtAction(nameof(GetById), new { id = relationshipId }, new { message = "Relationship created successfully", data = relationshipDto });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the relationship.", error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] CsIntercompanyRelationshipDto relationshipDto)
        {
            try
            {
                if (id != relationshipDto.RelationshipId)
                    return BadRequest(new { message = "ID in URL does not match ID in body." });
                var success = await _relationshipService.UpdateAsync(MapFromDto(relationshipDto));
                if (!success)
                    return NotFound(new { message = $"Relationship with id {id} not found for update.", data = (object?)null });
                return Ok(new { message = "Relationship updated successfully", data = (object?)null });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the relationship.", error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var success = await _relationshipService.DeleteAsync(id);
                if (!success)
                    return NotFound(new { message = $"Relationship with id {id} not found for deletion.", data = (object?)null });
                return Ok(new { message = "Relationship deleted successfully", data = (object?)null });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the relationship.", error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        private static CsIntercompanyRelationshipDto MapToDto(CsIntercompanyRelationship relationship)
        {
            return new CsIntercompanyRelationshipDto
            {
                RelationshipId = relationship.RelationshipId,
                CompanyId1 = relationship.CompanyId1,
                CompanyId2 = relationship.CompanyId2,
                Company1Name = relationship.Company1Name,
                Company2Name = relationship.Company2Name,
                RelationshipType = relationship.RelationshipType,
                EffectiveDate = relationship.EffectiveDate,
                EndDate = relationship.EndDate,
                Notes = relationship.Notes,
                CreatedAt = relationship.CreatedAt,
                UpdatedAt = relationship.UpdatedAt
            };
        }

        private static CsIntercompanyRelationship MapFromDto(CsIntercompanyRelationshipDto dto)
        {
            return new CsIntercompanyRelationship
            {
                RelationshipId = dto.RelationshipId,
                CompanyId1 = dto.CompanyId1,
                CompanyId2 = dto.CompanyId2,
                RelationshipType = dto.RelationshipType,
                EffectiveDate = dto.EffectiveDate,
                EndDate = dto.EndDate,
                Notes = dto.Notes
            };
        }
    }
}
