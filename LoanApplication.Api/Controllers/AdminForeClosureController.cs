using LoanApplication.Application.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LoanApplication.Api.Controllers
{
    [Route("api/v1/admin/[controller]")]
    [ApiController]
    [Authorize(Roles = "Loan Officer")]
    public class AdminForeClosureController : ControllerBase
    {
        private readonly IForeClosureService service;

        public AdminForeClosureController(IForeClosureService service)
        {
            this.service = service;
        }

        [HttpGet("GetAllRequests")]
        public async Task<IActionResult> GetAllRequests()
        {
            var requests = await service.GetAllRequests();
            return Ok(requests);
        }

        [HttpGet("GetRequestById/{requestId:int}")]
        public async Task<IActionResult> GetRequestById(int requestId)
        {
            var request = await service.GetRequestById(requestId);

            if (request == null)
            {
                return NotFound(new { message = "Foreclosure request not found." });
            }

            return Ok(request);
        }

        [HttpPatch("ApproveRequest/{requestId:int}")]
        public async Task<IActionResult> ApproveRequest(int requestId)
        {
            try
            {
                var request = await service.ApproveRequest(requestId);
                return Ok(new
                {
                    message = "Foreclosure request approved successfully.",
                    data = request
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPatch("RejectRequest/{requestId:int}")]
        public async Task<IActionResult> RejectRequest(int requestId)
        {
            try
            {
                var request = await service.RejectRequest(requestId);
                return Ok(new
                {
                    message = "Foreclosure request rejected successfully.",
                    data = request
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
