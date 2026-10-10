using LoanApplication.Application.DTO;
using LoanApplication.Application.Interface;
using Microsoft.AspNetCore.Mvc;

namespace LoanApplication.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
    }
}
