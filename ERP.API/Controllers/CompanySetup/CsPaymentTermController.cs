using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using ERP.API.Models;
using ERP.API.Models.DTOs.CompanySetup;
using ERP.API.Services.CompanySetup;
using Swashbuckle.AspNetCore.Annotations;

namespace ERP.API.Controllers.CompanySetup
{
    [ApiController]
    [Route("api/[controller]")]
    public class CsPaymentTermController : ControllerBase
    {
        private readonly ICsPaymentTermService _paymentTermService;
        private readonly ILogger<CsPaymentTermController> _logger;

        public CsPaymentTermController(ICsPaymentTermService paymentTermService, ILogger<CsPaymentTermController> logger)
        {
            _paymentTermService = paymentTermService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResponse<CsPaymentTermDto>>> Search([FromQuery] CsPaymentTermSearchDto searchDto)
        {
            try
            {
                var result = await _paymentTermService.SearchAsync(searchDto);
                return Ok(new {
                    message = "Payment terms search completed successfully.",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new {
                    message = "An error occurred while searching payment terms.",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CsPaymentTermDto>> GetById(int id)
        {
            try
            {
                var result = await _paymentTermService.GetByIdAsync(id);
                if (result == null)
                    return NotFound(new { message = $"Payment term with ID {id} not found", data = (object?)null });
                return Ok(new {
                    message = "Payment term retrieved successfully.",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new {
                    message = "An error occurred while retrieving payment term.",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        [HttpPost]
        public async Task<ActionResult<int>> Create(CsPaymentTermDto dto)
        {
            try
            {
                var id = await _paymentTermService.CreateAsync(dto);
                return Ok(new {
                    message = "Payment term created successfully.",
                    data = new { Id = id }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new {
                    message = "An error occurred while creating payment term.",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        [HttpPut]
        public async Task<ActionResult<bool>> Update(CsPaymentTermDto dto)
        {
            try
            {
                var result = await _paymentTermService.UpdateAsync(dto);
                if (!result)
                    return NotFound(new { message = "Payment term not found or update failed.", data = (object?)null });
                return Ok(new {
                    message = "Payment term updated successfully.",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new {
                    message = "An error occurred while updating payment term.",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<bool>> Delete(int id)
        {
            try
            {
                var result = await _paymentTermService.DeleteAsync(id);
                if (!result)
                    return NotFound(new { message = "Payment term not found or delete failed.", data = (object?)null });
                return Ok(new {
                    message = "Payment term deleted successfully.",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new {
                    message = "An error occurred while deleting payment term.",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        [HttpGet("company/{companyId}")]
        public async Task<ActionResult<PagedResponse<CsPaymentTermDto>>> GetByCompany(
            int companyId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _paymentTermService.GetByCompanyAsync(companyId, pageNumber, pageSize);
                return Ok(new {
                    message = "Payment terms retrieved successfully by company.",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new {
                    message = "An error occurred while retrieving payment terms by company.",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        [HttpGet("all")]
        [SwaggerOperation(Summary = "Gets all payment terms across all companies")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllPaymentTerms()
        {
            try
            {
                var result = await _paymentTermService.GetAllPaymentTermsAsync();
                return Ok(new {
                    message = "Payment terms retrieved successfully.",
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAllPaymentTerms endpoint");
                return StatusCode(500, new {
                    message = "An error occurred while retrieving payment terms.",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }
    }
}
