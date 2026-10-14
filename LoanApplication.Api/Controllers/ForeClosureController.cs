using LoanApplication.Application.DTO;
using LoanApplication.Application.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LoanApplication.Api.Controllers
{
    [Route("api/v1/user/[controller]")]
    [ApiController]
    [Authorize]
    public class ForeClosureController : ControllerBase
    {
        private readonly IForeClosureService service;

        public ForeClosureController(IForeClosureService service)
        {
            this.service=service;
        }

        // POST: api/v1/user/ForeClosure/Request
        [HttpPost("Request")]
        public async Task<IActionResult> CreateRequest([FromBody] CreateForeClosureRequestDto dto)
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out int userId))
            {
                return Unauthorized("User ID not found in token.");
            }

            try
            {
                var result = await service.CreateRequest(dto, userId);

                return CreatedAtAction(nameof(GetMyRequests),new { },new
                    {
                        message = "Foreclosure request submitted successfully.",
                        data = result
                    });
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
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/v1/user/ForeClosure/MyRequests
        [HttpGet("MyRequests")]
        public async Task<IActionResult> GetMyRequests()
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out int userId))
            {
                return Unauthorized("User ID not found in token.");
            }

            try
            {
                var requests = await service.GetMyRequests(userId);

                return Ok(requests);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }
    }
}
