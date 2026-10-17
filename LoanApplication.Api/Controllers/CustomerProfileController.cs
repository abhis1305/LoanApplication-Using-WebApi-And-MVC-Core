using LoanApplication.Application.DTO;
using LoanApplication.Application.Interface;
using LoanApplication.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LoanApplication.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerProfileController : ControllerBase
    {
        private readonly ICustomerProfileService service;

        public CustomerProfileController(
            ICustomerProfileService service)
        {
            this.service = service;
        }

        // GET CUSTOMER PROFILE
        [Authorize(Policy = "AccessTokenOnly")]
        [HttpGet]
        [Route("me")]
        public async Task<IActionResult> GetMyProfile(
            CancellationToken cancellationToken)
        {
            var purpose = User.FindFirst("Purpose")?.Value;

            if (purpose != "Access")
            {
                return Unauthorized(new
                {
                    message = "A valid access token is required."
                });
            }

            var userIdValue =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out int userId))
            {
                return Unauthorized(new
                {
                    message = "Invalid user identity."
                });
            }

            var result = await service.GetMyProfileAsync(
                userId.ToString(),
                cancellationToken);

            if (result == null)
            {
                return NotFound(new
                {
                    message = "Customer profile not found."
                });
            }

            return Ok(result);
        }

        // UPDATE CUSTOMER PROFILE
        [Authorize(Policy = "AccessTokenOnly")]
        [HttpPut]
        [Route("me")]
        public async Task<IActionResult> UpdateMyProfile(
            [FromBody] UpdateCustomerProfileDto dto,
            CancellationToken cancellationToken)
        {
            var purpose = User.FindFirst("Purpose")?.Value;

            if (purpose != "Access")
            {
                return Unauthorized(new
                {
                    message = "A valid access token is required."
                });
            }

            var userIdValue =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out int userId))
            {
                return Unauthorized(new
                {
                    message = "Invalid user identity."
                });
            }

            try
            {
                var result = await service.UpdateMyProfileAsync(
                    userId.ToString(),
                    dto,
                    cancellationToken);

                if (result == null)
                {
                    return NotFound(new
                    {
                        message = "Customer profile not found."
                    });
                }

                return Ok(new
                {
                    message = "Profile updated successfully.",
                    data = result
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new
                {
                    message = ex.Message
                });
            }
        }
    }
}
