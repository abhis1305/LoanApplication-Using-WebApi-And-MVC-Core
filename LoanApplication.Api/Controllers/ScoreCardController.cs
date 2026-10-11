using LoanApplication.Application.DTO;
using LoanApplication.Application.Interface;
using Microsoft.AspNetCore.Mvc;

namespace LoanApplication.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScoreCardController : ControllerBase
    {
        private readonly IScoreCardService _service;

        public ScoreCardController(IScoreCardService service)
        {
            _service = service;
        }

        [HttpPost("Generate")]
        public async Task<IActionResult> Generate(
            [FromBody] ScoreCardRequestDto request)
        {
            try
            {
                var result = await _service.GenerateScoreCardAsync(
                    request.CustomerId);

                return Ok(new
                {
                    success = true,
                    message = "Scorecard generated successfully.",
                    data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
    }
}