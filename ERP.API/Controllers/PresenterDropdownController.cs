using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ERP.API.Services;

namespace ERP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowAll")]
    [Produces("application/json")]
    public class PresenterDropdownController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<PresenterDropdownController> _logger;

        public PresenterDropdownController(IUserService userService, ILogger<PresenterDropdownController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpGet]
        [Route("presenterDropdown")]
        [ProducesResponseType(typeof(IEnumerable<UserDropdownDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<UserDropdownDto>>> GetPresenterDropdown()
        {
            try
            {
                var result = await _userService.GetPresenterDropdownAsync();
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error fetching presenter dropdown: {Message}", ex.Message);
                return StatusCode(500, "Failed to fetch presenter dropdown");
            }
        }

        [HttpGet]
        [Route("salesRepresentativeDropdown")]
        [ProducesResponseType(typeof(IEnumerable<UserDropdownDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<UserDropdownDto>>> GetSalesRepresentativeDropdown()
        {
            try
            {
                var result = await _userService.GetSalesRepresentativeDropdownAsync();
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error fetching sales representative dropdown: {Message}", ex.Message);
                return StatusCode(500, "Failed to fetch sales representative dropdown");
            }
        }
    }
}
