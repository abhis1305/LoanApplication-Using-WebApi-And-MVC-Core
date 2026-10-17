using LoanApplication.Application.DTO;
using LoanApplication.Application.Interface;
using LoanApplication.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LoanApplication.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "User")]
    public class CibilController : ControllerBase
    {
        ICibilService service;

        public CibilController(ICibilService service)
        {
            this.service = service;
        }

        [HttpPost]
        [Route("GenerateScore")]
        public async Task<IActionResult> GenerateScore(CibilScoreRequestDto request)
        {
            try
            {
                var data = await service.GenerateScoreAsync(request.CustomerId);

                return Ok(new
                {
                    message = "Credit score generated successfully.",
                    data = data
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    message = ex.Message
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        [HttpGet]
        [Route("GetScore/{customerId}")]
        public async Task<IActionResult> GetScore(int customerId)
        {
            try
            {
                var data = await service.GetCibilScoreAsync(customerId);
                return Ok(data);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("Profile")]
        public async Task<IActionResult> GetCustomerProfile()
        {
            var userIdValue = User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdValue, out var userId))
            {
                return Unauthorized(new
                {
                    message = "Invalid user token."
                });
            }

            var profile = await service.GetCustomerProfileAsync(userId);

            if (profile == null)
            {
                return NotFound(new
                {
                    message = "Customer profile not found."
                });
            }

            return Ok(new
            {
                message = "Customer profile retrieved successfully.",
                data = profile
            });
        }
    }
}
