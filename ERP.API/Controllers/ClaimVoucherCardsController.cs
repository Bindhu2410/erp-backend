using Microsoft.AspNetCore.Mvc;
using ERP.API.Models;
using ERP.API.Services;

namespace ERP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClaimVoucherCardsController : ControllerBase
    {
        private readonly IClaimVoucherCardService _claimVoucherCardService;

        public ClaimVoucherCardsController(IClaimVoucherCardService claimVoucherCardService)
        {
            _claimVoucherCardService = claimVoucherCardService;
        }

        /// <summary>
        /// Get claim voucher card statistics - new this week, week total amount, month count, month total amount
        /// </summary>
        /// <returns>ClaimVoucherCard statistics</returns>
        [HttpGet]
        public async Task<ActionResult<ClaimVoucherCard>> GetClaimVoucherCards()
        {
            try
            {
                var card = await _claimVoucherCardService.GetClaimVoucherCardsAsync();

                return Ok(new
                {
                    success = true,
                    data = card
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error retrieving claim voucher cards.", error = ex.Message });
            }
        }
    }
}
