using LoanApplication.Application.DTO;
using LoanApplication.Application.Interfcae;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LoanApplication.Api.Controllers
{
    [Route("api/v1/admin/[controller]")]
    [ApiController]
    public class PenaltyController : ControllerBase
    {
        private readonly IPenaltyService service;
        public PenaltyController(IPenaltyService service)
        {
            this.service = service;
        }

        [HttpGet]
        [Route("GetAllPenalties")]
        public async Task<IActionResult> GetAllPenalties()
        {
            var penalties = await service.GetAllPenalties();
            return Ok(penalties);
        }

        [HttpGet]
        [Route("GetPenaltyById/{penaltyId:int}")]
        public async Task<IActionResult> GetPenaltyById(int penaltyId)
        {
            var penalty = await service.GetPenaltyById(penaltyId);
            if (penalty == null)
            {
                return NotFound(new { message = "Penalty not found" });
            }
            return Ok(penalty);
        }

        [HttpGet]
        [Route("GetPenaltiesByLoanAccountId/{loanAccountId:int}")]
        public async Task<IActionResult> GetPenaltiesByLoanAccountId(int loanAccountId)
        {
            var penalties = await service.GetPenaltiesByLoanAccountId(loanAccountId);
            return Ok(penalties);
        }

        [HttpPost]
        [Route("ApplyPenalty")]
        public async Task<IActionResult> ApplyPenalty([FromBody] CreatePenaltyChargeDto dto)
        {
            await service.ApplyPenalty(dto);
            return Ok(new { message = "Penalty Added successfully!" });
        }

        [HttpPatch]
        [Route("WaivePenalty/{penaltyId:int}")]
        public async Task<IActionResult> WaivePenalty(int penaltyId, [FromBody] WaivePenaltyDto dto)
        {
            await service.WaivePenalty(penaltyId, dto);
            return Ok(new { message = "Penalty waived successfully!" });
        }

        
    }
}
