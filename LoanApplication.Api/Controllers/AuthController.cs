using LoanApplication.Application.DTO;
using LoanApplication.Application.Interfcae;
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

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            bool result =await service.RegisterUserAsync(dto);
            if (!result)
            {
                return BadRequest(new{message = "Email already exists."});
            }
            return Ok(new{message = "Registration successful."});
        }

        // LOGIN

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var result =await service.LoginAsync(dto);
            if (result.Token == null &&result.PreAuthToken == null)
            {
                return Unauthorized(result);
            }
            return Ok(result);
        }

        // GOOGLE LOGIN

        [HttpPost("google")]
        public async Task<IActionResult> GoogleLogin(GoogleAuthDto dto)
        {
            var result =await service.GoogleLoginAsync(dto);
            if (result.Token == null &&result.PreAuthToken == null)
            {
                return Unauthorized(result);
            }
            return Ok(result);
        }

        // VERIFY 2FA
        
        [HttpPost("verify-2fa")]
        public async Task<IActionResult> VerifyTwoFactor(Verify2FaDto dto)
        {
            var result =await service.VerifyTwoFactorAsync(dto);
            if (result.Token == null)
            {
                return Unauthorized(result);
            }
            return Ok(result);
        }

        // ENABLE 2FA

        [Authorize]
        [HttpPost("enable-2fa")]
        public async Task<IActionResult> EnableTwoFactor()
        {
            var userIdClaim =User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim,out int userId))
            {
                return Unauthorized(new
                {
                    message = "Invalid user."
                });
            }
            var result =await service.EnableTwoFactorAsync(userId);
            if (result == null)
            {
                return NotFound(new
                {
                    message = "User not found."
                });
            }
            return Ok(result);
        }
    }
}