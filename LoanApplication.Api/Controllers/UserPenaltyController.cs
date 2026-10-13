using LoanApplication.Application.Interfcae;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LoanApplication.Api.Controllers
{
    [Route("api/v1/user/[controller]")]
    [ApiController]
    public class UserPenaltyController : ControllerBase
    {
        private readonly IPenaltyService service;
        public UserPenaltyController(IPenaltyService service)
        {
            this.service = service;
        }

        [HttpGet("MyPenalties")]
        public async Task<IActionResult> GetMyPenalties([FromQuery] int userId)
        {
            if (userId <= 0)
            {
                return BadRequest(new
                {
                    message = "A valid userId is required."
                });
            }

            var penalties = await service.GetMyPenalties(userId);

            return Ok(penalties);
        }

        [AllowAnonymous]
        [HttpGet("Test")]
        public IActionResult Test()
        {
            return Ok(new
            {
                message = "Public endpoint is working."
            });
        }
    }
}
