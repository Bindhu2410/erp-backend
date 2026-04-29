using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ERP.API.Models;
using ERP.API.Models.DTOs.CompanySetup;
using ERP.API.Services.CompanySetup;

namespace ERP.API.Controllers.CompanySetup
{
    [ApiController]
    [Route("api/[controller]")]
    public class CsOpeningBalanceController : ControllerBase
    {
        private readonly ICsOpeningBalanceService _openingBalanceService;

        public CsOpeningBalanceController(ICsOpeningBalanceService openingBalanceService)
        {
            _openingBalanceService = openingBalanceService;
        }


        [HttpGet]
        public async Task<IActionResult> Search([FromQuery] CsOpeningBalanceSearchDto searchDto)
        {
            try
            {
                var result = await _openingBalanceService.SearchAsync(searchDto);
                return Ok(new { message = "Search successful", data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while searching.", error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _openingBalanceService.GetByIdAsync(id);
                if (result == null)
                    return NotFound(new { message = $"Opening balance with id {id} not found.", data = (object?)null });
                return Ok(new { message = "Retrieved successfully", data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the opening balance.", error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CsOpeningBalanceDto dto)
        {
            try
            {
                var id = await _openingBalanceService.CreateAsync(dto);
                return Ok(new { message = "Created successfully", data = id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the opening balance.", error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] CsOpeningBalanceDto dto)
        {
            try
            {
                var result = await _openingBalanceService.UpdateAsync(dto);
                if (!result)
                    return NotFound(new { message = "Opening balance not found for update.", data = (object?)null });
                return Ok(new { message = "Updated successfully", data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the opening balance.", error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _openingBalanceService.DeleteAsync(id);
                if (!result)
                    return NotFound(new { message = $"Opening balance with id {id} not found for deletion.", data = (object?)null });
                return Ok(new { message = "Deleted successfully", data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the opening balance.", error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpGet("company/{companyId}/period/{periodId}")]
        public async Task<IActionResult> GetByCompanyPeriod(
            int companyId,
            int periodId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _openingBalanceService.GetByCompanyPeriodAsync(companyId, periodId, pageNumber, pageSize);
                return Ok(new { message = "Retrieved successfully", data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving opening balances by company and period.", error = ex.Message, stackTrace = ex.StackTrace });
            }
        }
    }
}
