using LoanApplication.Application.DTO;
using LoanApplication.Application.Interface;
using Microsoft.AspNetCore.Mvc;

namespace LoanApplication.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EligibilityController : ControllerBase
    {
        private readonly IEligibilityService _service;

        public EligibilityController(IEligibilityService service)
        {
            _service = service;
        }



        // GET: api/Eligibility/Result/6
        [HttpGet("Result/{eligibilityId}")]
        public async Task<IActionResult> GetEligibilityResult(int eligibilityId)
        {
            try
            {
                var result = await _service.GetEligibilityResultAsync(eligibilityId);

                return Ok(new
                {
                    success = true,
                    message = "Eligibility result fetched successfully.",
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
        }





        // POST: api/Eligibility/Check
        [HttpPost("Check")]
        public async Task<IActionResult> Check(
            [FromBody] EligibilityRequestDto request)
        {
            try
            {
                var result = await _service.CheckEligibilityAsync(
                    request.CustomerId);

                return Ok(new
                {
                    success = true,
                    message = "Eligibility checked successfully.",
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

        // GET: api/Eligibility/PendingRequests
        [HttpGet("PendingRequests")]
        public async Task<IActionResult> PendingRequests()
        {
            var result = await _service.GetPendingRequestsAsync();

            return Ok(new
            {
                success = true,
                message = "Pending eligibility requests fetched successfully.",
                data = result
            });
        }

        // PUT: api/Eligibility/OfficerDecision/1
        [HttpPut("OfficerDecision/{eligibilityId}")]
        public async Task<IActionResult> OfficerDecision(
            int eligibilityId,
            [FromBody] OfficerDecisionDto request)
        {
            try
            {
                var result = await _service.UpdateOfficerDecisionAsync(
                    eligibilityId,
                    request.Approved,
                    request.Remarks);

                return Ok(new
                {
                    success = true,
                    message = "Officer decision saved successfully.",
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