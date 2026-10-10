using LoanApplication.Application.DTO;
using LoanApplication.Application.Interface;
using Microsoft.AspNetCore.Mvc;

namespace LoanApplication.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CibilController : ControllerBase
{
    private readonly ICibilService _service;

    public CibilController(ICibilService service)
    {
        _service = service;
    }

    [HttpPost("GenerateScore")]
    public async Task<IActionResult> GenerateScore(
        [FromBody] CibilScoreRequestDto request)
    {
        try
        {
            var result = await _service.GenerateScoreAsync(
                request.CustomerId);

            return Ok(new
            {
                success = true,
                message = "Credit score generated successfully.",
                data = result
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                success = false,
                message = ex.Message,
                data = (object?)null
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                success = false,
                message = ex.Message,
                data = (object?)null
            });
        }
    }
}