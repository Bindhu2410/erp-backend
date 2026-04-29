using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using ERP.API.Models.DTOs.CompanySetup;
using ERP.API.Services.CompanySetup;
using Microsoft.Extensions.Logging;

namespace ERP.API.Controllers.CompanySetup
{
    [ApiController]
    [Route("api/[controller]")]
    public class CsWarehouseController : ControllerBase
    {
        private readonly ICsWarehouseService _warehouseService;
        private readonly ILogger<CsWarehouseController> _logger;

        public CsWarehouseController(ICsWarehouseService warehouseService, ILogger<CsWarehouseController> logger)
        {
            _warehouseService = warehouseService;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new warehouse
        /// </summary>
        /// <param name="createDto">The warehouse information</param>
        /// <returns>The creation result with warehouse ID if successful</returns>
        /// <response code="201">Returns the warehouse creation result</response>
        /// <response code="400">If the request data is invalid or warehouse code already exists</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateWarehouse([FromBody] CreateCsWarehouseDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        message = "Invalid model state",
                        errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                    });
                }
                var result = await _warehouseService.CreateWarehouseAsync(createDto);
                if (result.Success)
                {
                    return CreatedAtAction(
                        nameof(GetWarehouseById),
                        new { warehouseId = result.WarehouseId },
                        new
                        {
                            message = result.Message,
                            data = new 
                            { 
                                WarehouseId = result.WarehouseId,
                                WarehouseCode = result.WarehouseCode
                            }
                        });
                }
                else
                {
                    return BadRequest(new
                    {
                        message = result.Message
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating warehouse: {WarehouseName}", createDto.WarehouseName);
                return StatusCode(500, new
                {
                    message = "An error occurred while creating the warehouse",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        /// <summary>
        /// Updates an existing warehouse
        /// </summary>
        /// <param name="updateDto">The updated warehouse information</param>
        /// <returns>Success status</returns>
        /// <response code="200">If the warehouse was updated successfully</response>
        /// <response code="400">If the request data is invalid</response>
        /// <response code="404">If the warehouse was not found</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateWarehouse([FromBody] UpdateCsWarehouseDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        message = "Invalid model state",
                        errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                    });
                }
                var result = await _warehouseService.UpdateWarehouseAsync(updateDto);
                if (result.Success)
                {
                    return Ok(new
                    {
                        message = result.Message,
                        data = (object?)null
                    });
                }
                else
                {
                    if (result.Message.Contains("not found"))
                    {
                        return NotFound(new
                        {
                            message = result.Message,
                            data = (object?)null
                        });
                    }
                    else
                    {
                        return BadRequest(new
                        {
                            message = result.Message
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating warehouse: {WarehouseId}", updateDto.WarehouseId);
                return StatusCode(500, new
                {
                    message = "An error occurred while updating the warehouse",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        /// <summary>
        /// Deletes a warehouse
        /// </summary>
        /// <param name="warehouseId">The warehouse ID</param>
        /// <returns>Success status</returns>
        /// <response code="200">If the warehouse was deleted successfully</response>
        /// <response code="404">If the warehouse was not found</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpDelete("{warehouseId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteWarehouse(int warehouseId)
        {
            try
            {
                var result = await _warehouseService.DeleteWarehouseAsync(warehouseId);
                if (result.Success)
                {
                    return Ok(new
                    {
                        message = result.Message,
                        data = (object?)null
                    });
                }
                else
                {
                    return NotFound(new
                    {
                        message = result.Message,
                        data = (object?)null
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting warehouse: {WarehouseId}", warehouseId);
                return StatusCode(500, new
                {
                    message = "An error occurred while deleting the warehouse",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        /// <summary>
        /// Gets a warehouse by ID
        /// </summary>
        /// <param name="warehouseId">The warehouse ID</param>
        /// <returns>The warehouse information</returns>
        /// <response code="200">Returns the warehouse information</response>
        /// <response code="404">If the warehouse was not found</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpGet("{warehouseId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetWarehouseById(int warehouseId)
        {
            try
            {
                var warehouse = await _warehouseService.GetWarehouseByIdAsync(warehouseId);
                if (warehouse != null)
                {
                    return Ok(new
                    {
                        message = "Warehouse retrieved successfully",
                        data = warehouse
                    });
                }
                else
                {
                    return NotFound(new
                    {
                        message = "Warehouse not found",
                        data = (object?)null
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting warehouse by ID: {WarehouseId}", warehouseId);
                return StatusCode(500, new
                {
                    message = "An error occurred while retrieving the warehouse",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        /// <summary>
        /// Gets warehouses by company ID
        /// </summary>
        /// <param name="companyId">The company ID</param>
        /// <returns>List of warehouses for the company</returns>
        /// <response code="200">Returns the list of warehouses</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpGet("company/{companyId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetWarehousesByCompany(int companyId)
        {
            try
            {
                var warehouses = await _warehouseService.GetWarehousesByCompanyAsync(companyId);
                return Ok(new
                {
                    message = "Warehouses retrieved successfully",
                    data = warehouses,
                    count = warehouses.Count()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting warehouses by company: {CompanyId}", companyId);
                return StatusCode(500, new
                {
                    message = "An error occurred while retrieving warehouses",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        /// <summary>
        /// Gets warehouses by branch ID
        /// </summary>
        /// <param name="branchId">The branch ID</param>
        /// <returns>List of warehouses for the branch</returns>
        /// <response code="200">Returns the list of warehouses</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpGet("branch/{branchId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetWarehousesByBranch(int branchId)
        {
            try
            {
                var warehouses = await _warehouseService.GetWarehousesByBranchAsync(branchId);
                return Ok(new
                {
                    message = "Warehouses retrieved successfully",
                    data = warehouses,
                    count = warehouses.Count()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting warehouses by branch: {BranchId}", branchId);
                return StatusCode(500, new
                {
                    message = "An error occurred while retrieving warehouses",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        /// <summary>
        /// Gets warehouses dropdown for a company
        /// </summary>
        /// <param name="companyId">The company ID</param>
        /// <returns>List of warehouses for dropdown</returns>
        /// <response code="200">Returns the list of warehouses for dropdown</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpGet("dropdown/company/{companyId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetWarehousesDropdownByCompany(int companyId)
        {
            try
            {
                var warehouses = await _warehouseService.GetWarehousesDropdownByCompanyAsync(companyId);
                return Ok(new
                {
                    message = "Warehouses dropdown retrieved successfully",
                    data = warehouses,
                    count = warehouses.Count()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting warehouses dropdown by company: {CompanyId}", companyId);
                return StatusCode(500, new
                {
                    message = "An error occurred while retrieving warehouses dropdown",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        /// <summary>
        /// Gets warehouses dropdown for a branch
        /// </summary>
        /// <param name="branchId">The branch ID</param>
        /// <returns>List of warehouses for dropdown</returns>
        /// <response code="200">Returns the list of warehouses for dropdown</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpGet("dropdown/branch/{branchId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetWarehousesDropdownByBranch(int branchId)
        {
            try
            {
                var warehouses = await _warehouseService.GetWarehousesDropdownByBranchAsync(branchId);
                return Ok(new
                {
                    message = "Warehouses dropdown retrieved successfully",
                    data = warehouses,
                    count = warehouses.Count()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting warehouses dropdown by branch: {BranchId}", branchId);
                return StatusCode(500, new
                {
                    message = "An error occurred while retrieving warehouses dropdown",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        /// <summary>
        /// Gets all warehouses across all companies
        /// </summary>
        /// <returns>List of all warehouses in the system</returns>
        /// <response code="200">Returns the complete list of warehouses</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpGet("all")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllWarehouses()
        {
            try
            {
                var warehouses = await _warehouseService.GetAllWarehousesAsync();
                return Ok(new
                {
                    message = "Warehouses retrieved successfully",
                    data = warehouses
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAllWarehouses endpoint");
                return StatusCode(500, new
                {
                    message = "An error occurred while processing the request",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }
    }
}
