using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using ERP.API.Models.DTOs.CompanySetup;
using ERP.API.Services.CompanySetup;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;

namespace ERP.API.Controllers.CompanySetup
{
    [ApiController]
    [Route("api/[controller]")]
    // [SwaggerTag("Create, read, update and delete branch warehouses")]
    public class CsBranchWarehouseController : ControllerBase
    {
        private readonly ICsBranchWarehouseService _service;
        private readonly ILogger<CsBranchWarehouseController> _logger;

        public CsBranchWarehouseController(ICsBranchWarehouseService service, ILogger<CsBranchWarehouseController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new branch warehouse
        /// </summary>
        /// <param name="createDto">Branch warehouse details</param>
        /// <returns>Newly created branch warehouse ID</returns>
        /// <response code="201">Branch warehouse created successfully</response>
        /// <response code="400">Invalid input data</response>
        /// <response code="500">Internal server error</response>
        [HttpPost]
        [SwaggerOperation(Summary = "Creates a new branch warehouse")]
        [ProducesResponseType(typeof(WarehouseResponse), 201)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(typeof(ErrorResponse), 500)]
        public async Task<IActionResult> Create([FromBody] CsBranchWarehouseDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { message = "Invalid model state", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
                }
                var id = await _service.CreateBranchWarehouseAsync(createDto);
                return CreatedAtAction(nameof(GetById), new { id }, new { message = "Branch warehouse created successfully", data = new { WarehouseId = id } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating branch warehouse");
                return StatusCode(500, new { message = "An error occurred while creating branch warehouse", error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        /// <summary>
        /// Updates an existing branch warehouse
        /// </summary>
        /// <param name="updateDto">Updated branch warehouse details</param>
        /// <returns>Success response</returns>
        /// <response code="200">Branch warehouse updated successfully</response>
        /// <response code="400">Invalid input data</response>
        /// <response code="404">Branch warehouse not found</response>
        /// <response code="500">Internal server error</response>
        [HttpPut]
        [SwaggerOperation(Summary = "Updates an existing branch warehouse")]
        [ProducesResponseType(typeof(SuccessResponse), 200)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(typeof(ErrorResponse), 404)]
        [ProducesResponseType(typeof(ErrorResponse), 500)]
        public async Task<IActionResult> Update([FromBody] CsBranchWarehouseDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { message = "Invalid model state", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
                }
                var result = await _service.UpdateBranchWarehouseAsync(updateDto);
                if (!result)
                    return NotFound(new { message = "Branch warehouse not found for update.", data = (object?)null });
                return Ok(new { message = "Branch warehouse updated successfully", data = (object?)null });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating branch warehouse");
                return StatusCode(500, new { message = "An error occurred while updating branch warehouse", error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        /// <summary>
        /// Deletes a branch warehouse
        /// </summary>
        /// <param name="id">Branch warehouse ID</param>
        /// <returns>Success response</returns>
        /// <response code="200">Branch warehouse deleted successfully</response>
        /// <response code="404">Branch warehouse not found</response>
        /// <response code="500">Internal server error</response>
        [HttpDelete("{id}")]
        [SwaggerOperation(Summary = "Deletes a branch warehouse")]
        [ProducesResponseType(typeof(SuccessResponse), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 404)]
        [ProducesResponseType(typeof(ErrorResponse), 500)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _service.DeleteBranchWarehouseAsync(id);
                if (!result)
                    return NotFound(new { message = "Branch warehouse with id " + id + " not found for deletion.", data = (object?)null });
                return Ok(new { message = "Branch warehouse deleted successfully", data = (object?)null });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting branch warehouse");
                return StatusCode(500, new { message = "An error occurred while deleting branch warehouse", error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        /// <summary>
        /// Gets a branch warehouse by ID
        /// </summary>
        /// <param name="id">The warehouse ID</param>
        /// <returns>Branch warehouse details</returns>
        /// <response code="200">Returns the branch warehouse</response>
        /// <response code="404">Branch warehouse not found</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("{id}")]
        [SwaggerOperation(Summary = "Gets a branch warehouse by ID")]
        [ProducesResponseType(typeof(WarehouseResponse), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 404)]
        [ProducesResponseType(typeof(ErrorResponse), 500)]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var warehouse = await _service.GetBranchWarehouseByIdAsync(id);
                if (warehouse == null)
                    return NotFound(new { message = "Branch warehouse with id " + id + " not found.", data = (object?)null });
                return Ok(new { message = "Branch warehouse retrieved successfully", data = warehouse });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting branch warehouse by ID");
                return StatusCode(500, new { message = "An error occurred while retrieving branch warehouse by ID", error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        /// <summary>
        /// Gets warehouses by branch ID
        /// </summary>
        /// <param name="branchId">The branch ID</param>
        /// <returns>List of warehouses for the branch</returns>
        /// <response code="200">Returns the list of warehouses</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("branch/{branchId}")]
        [SwaggerOperation(Summary = "Gets warehouses by branch ID")]
        [ProducesResponseType(typeof(IEnumerable<WarehouseResponse>), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 500)]
        public async Task<IActionResult> GetByBranchId(int branchId)
        {
            try
            {
                var warehouses = await _service.GetBranchWarehousesByBranchAsync(branchId);
                return Ok(new { message = "Branch warehouses retrieved successfully by branch", data = warehouses, count = warehouses.Count() });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting branch warehouses by branch");
                return StatusCode(500, new { message = "An error occurred while retrieving branch warehouses by branch", error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        /// <summary>
        /// Gets warehouses dropdown list by branch ID
        /// </summary>
        /// <param name="branchId">The branch ID</param>
        /// <returns>List of warehouses for dropdown</returns>
        /// <response code="200">Returns the list of warehouses</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("dropdown/{branchId}")]
        [SwaggerOperation(Summary = "Gets warehouses dropdown list by branch ID")]
        [ProducesResponseType(typeof(IEnumerable<WarehouseDropdownResponse>), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 500)]
        public async Task<IActionResult> GetDropdownByBranchId(int branchId)
        {
            try
            {
                var warehouses = await _service.GetBranchWarehousesDropdownAsync(branchId);
                return Ok(new { message = "Branch warehouses dropdown retrieved successfully", data = warehouses, count = warehouses.Count() });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting branch warehouses dropdown");
                return StatusCode(500, new { message = "An error occurred while retrieving branch warehouses dropdown", error = ex.Message, stackTrace = ex.StackTrace });
            }
        }
    }
}
