
using LoanApplication.Application.DTO;
using LoanApplication.Application.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LoanApplication.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService service;
        public AuthController(IAuthService service)
        {
            this.service = service;
        }

        // REGISTER
        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            bool result = await service.RegisterUserAsync(dto);
            if (!result)
            {
                return BadRequest(new
                {
                    message = "Email already exists."
                });
            }
            return Ok(new
            {
                message = "Registration successful."
            });
        }

        // LOGIN
        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var result = await service.LoginAsync(dto);
            // First-time 2FA setup returns SetupToken.
            // Existing 2FA returns PreAuthToken.
            // Normal login returns Access Token.
            if (result.Token == null &&
                result.PreAuthToken == null &&
                result.SetupToken == null)
            {
                return Unauthorized(result);
            }
            return Ok(result);
        }

        // GOOGLE LOGIN
        [HttpPost]
        [Route("google")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleAuthDto dto)
        {
            var result = await service.GoogleLoginAsync(dto);
            if (result.Token == null &&
                result.PreAuthToken == null &&
                result.SetupToken == null)
            {
                return Unauthorized(result);
            }
            return Ok(result);
        }

        // COMPLETE PROFILE
        [Authorize(Policy = "AccessTokenOnly")]
        [HttpPut]
        [Route("complete-profile")]
        public async Task<IActionResult> CompleteProfile([FromBody] CompleteProfileDto dto)
        {
            var purpose = User.FindFirst("Purpose")?.Value;
            if (purpose != "Access")
            {
                return Unauthorized(new
                {
                    message = "A valid access token is required."
                });
            }
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdValue, out int userId))
            {
                return Unauthorized(new
                {
                    message = "Invalid user identity."
                });
            }
            var result = await service.CompleteProfileAsync(userId, dto);
            if (!result)
            {
                return NotFound(new
                {
                    message = "User or customer profile not found."
                });
            }

            return Ok(new
            {
                message = "Profile completed successfully."
            });
        }

        // VERIFY 2FA
        [HttpPost]
        [Route("verify-2fa")]
        public async Task<IActionResult> VerifyTwoFactor([FromBody] Verify2FaDto dto)
        {
            var result = await service.VerifyTwoFactorAsync(dto);
            if (result.Token == null)
            {
                return Unauthorized(result);
            }
            return Ok(result);
        }

        // ENABLE 2FA - FIRST-TIME SETUP ONLY
        [Authorize]
        [HttpPost]
        [Route("enable-2fa")]
        public async Task<IActionResult> EnableTwoFactor()
        {
            // The setup token must be authenticated and valid.
            // It must not be a normal Access token.
            var purpose = User.FindFirst("Purpose")?.Value;
            if (purpose != "2FA_Setup")
            {
                return Unauthorized(new
                {
                    message =
                        "A valid 2FA setup token is required."
                });
            }
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdValue, out int userId))
            {
                return Unauthorized(new
                {
                    message = "Invalid user identity."
                });
            }
            try
            {
                var result = await service.EnableTwoFactorAsync(userId);
                if (result == null)
                {
                    return NotFound(new
                    {
                        message = "User not found."
                    });
                }
                return Ok(result);
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
