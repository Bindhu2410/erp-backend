using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using ERP.API.Models;
using ERP.API.Services;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SalesPincodeController : ControllerBase
    {
        private readonly SalesPincodeService _salesPincodeService;

        public SalesPincodeController(SalesPincodeService salesPincodeService)
        {
            _salesPincodeService = salesPincodeService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SalesPincode>>> GetAll()
        {
            var pincodes = await _salesPincodeService.GetAllAsync();
            return Ok(pincodes);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<IEnumerable<SalesPincode>>> GetById(int id)
        {
            var pincode = await _salesPincodeService.GetByIdAsync(id);
            if (pincode == null)
                return NotFound($"Pincode with ID {id} not found");

            return Ok(new[] { pincode });
        }

        [HttpGet("area/{areaId}")]
        public async Task<ActionResult<IEnumerable<SalesPincode>>> GetByAreaId(int areaId)
        {
            var pincodes = await _salesPincodeService.GetAllAsync("sales_areas_id = @AreaId", new { AreaId = areaId });
            return Ok(pincodes);
        }

        [HttpPost]
        public async Task<ActionResult<int>> Create(SalesPincode pincode)
        {
            var id = await _salesPincodeService.CreateAsync(pincode);
            return CreatedAtAction(nameof(GetById), new { id }, id);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, SalesPincode pincode)
        {
            if (id != pincode.Id)
                return BadRequest();

            await _salesPincodeService.UpdateAsync(pincode);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _salesPincodeService.DeleteAsync(id);
            return NoContent();
        }
    }
}