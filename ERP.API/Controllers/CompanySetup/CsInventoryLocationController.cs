using System;
using System.Collections.Generic;
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
    public class CsInventoryLocationController : ControllerBase
    {
        private readonly ICsInventoryLocationService _service;

        public CsInventoryLocationController(ICsInventoryLocationService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var results = await _service.GetAllAsync();
                return Ok(new { message = "Inventory locations retrieved successfully", data = results });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving inventory locations.", error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpGet]
        [Route("search")]
        public async Task<IActionResult> Search([FromQuery] CsInventoryLocationSearchDto searchDto)
        {
            try
            {
                var results = await _service.SearchAsync(searchDto);
                return Ok(new {
                    message = "Inventory locations search completed successfully",
                    data = results.Data,
                    pageNumber = searchDto.PageNumber,
                    pageSize = searchDto.PageSize,
                    totalRecords = results.TotalRecords,
                    filteredRecords = results.FilteredRecords
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while searching inventory locations.", error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _service.GetByIdAsync(id);
                if (result == null)
                    return NotFound(new { message = $"Inventory location with id {id} not found.", data = (object?)null });
                return Ok(new { message = "Inventory location retrieved successfully", data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the inventory location.", error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CsInventoryLocationDto locationDto)
        {
            try
            {
                var result = await _service.CreateAsync(locationDto);
                return CreatedAtAction(nameof(GetById), new { id = result.LocationId }, new { message = "Inventory location created successfully", data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the inventory location.", error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CsInventoryLocationDto locationDto)
        {
            try
            {
                if (id != locationDto.LocationId)
                    return BadRequest(new { message = "ID in URL does not match ID in body." });
                await _service.UpdateAsync(locationDto);
                // If UpdateAsync returns void, check existence before update or refactor service to return bool
                // For now, assume update always succeeds if no exception is thrown
                return Ok(new { message = "Inventory location updated successfully", data = (object?)null });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the inventory location.", error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _service.DeleteAsync(id);
                // If DeleteAsync returns void, check existence before delete or refactor service to return bool
                // For now, assume delete always succeeds if no exception is thrown
                return Ok(new { message = "Inventory location deleted successfully", data = (object?)null });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the inventory location.", error = ex.Message, stackTrace = ex.StackTrace });
            }
        }
    }
}
