using System.Threading.Tasks;
using ERP.API.Models.DTOs;
using ERP.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("api/[controller]")]
    public class EWayBillController : ControllerBase
    {
        private readonly IEWayBillService _eWayBillService;

        public EWayBillController(IEWayBillService eWayBillService)
        {
            _eWayBillService = eWayBillService;
        }

        [HttpPost("generate/{issueId}")]
        public async Task<IActionResult> Generate(int issueId)
        {
            var result = await _eWayBillService.GenerateEWayBillAsync(issueId);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpGet("{ewayBillNo}")]
        public async Task<IActionResult> Get(string ewayBillNo)
        {
            var result = await _eWayBillService.GetEWayBillAsync(ewayBillNo);
            return Ok(result);
        }

        [HttpPost("cancel")]
        public async Task<IActionResult> Cancel([FromBody] EWayBillCancelRequestDto request)
        {
            var result = await _eWayBillService.CancelEWayBillAsync(request);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("update-vehicle")]
        public async Task<IActionResult> UpdateVehicle([FromBody] EWayBillUpdateVehicleRequestDto request)
        {
            var result = await _eWayBillService.UpdateVehicleAsync(request);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
    }
}
