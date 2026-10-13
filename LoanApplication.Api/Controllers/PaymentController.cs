using LoanApplication.Application.DTO;
using LoanApplication.Application.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LoanApplication.Api.Controllers
{
    [Authorize]
    [Route("api/v1/user/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService service;
        public PaymentController(IPaymentService service)
        {
            this.service = service;
        }

        [HttpPost("PayEmi")]
        public async Task<IActionResult> PayEmi([FromBody] PayEmiDto dto)
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out int userId))
            {
                return Unauthorized(new { message = "Valid user authentication is required." });
            }

            try
            {
                var payment = await service.PayEmi(dto, userId);
                return Ok(payment);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
