using LoanApplication.Application.Interfcae;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LoanApplication.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        IDashboardService service;
        public DashboardController(IDashboardService service)
        {
            this.service = service;
        }

        [HttpGet]
        [Route("Cards/{customerId}")]
        public async Task<IActionResult> Cards(int customerId)
        {
            var d = await service.FetchCardsAsync(customerId);
            return Ok(d);
        }

        [HttpGet]
        [Route("Progress/{customerId}")]
        public async Task<IActionResult> Progress(int customerId)
        {
            var d = await service.FetchProgressAsync(customerId);
            if (d == null)
                return NotFound(new { message = "No Loan Application Found" });
            return Ok(d);
        }

        [HttpGet]
        [Route("ActiveLoan/{customerId}")]
        public async Task<IActionResult> ActiveLoan(int customerId)
        {
            var d = await service.FetchActiveLoanAsync(customerId);
            if (d == null)
                return NotFound(new { message = "No Active Loan Found" });
            return Ok(d);
        }

        [HttpGet]
        [Route("EmiSummary/{customerId}")]
        public async Task<IActionResult> EmiSummary(int customerId)
        {
            var d = await service.FetchEmiSummaryAsync(customerId);
            if (d == null)
                return NotFound(new { message = "No Active Loan Found" });
            return Ok(d);
        }

        [HttpGet]
        [Route("RecentPayments/{customerId}")]
        public async Task<IActionResult> RecentPayments(int customerId)
        {
            var d = await service.FetchRecentPaymentsAsync(customerId);
            return Ok(d);
        }

        [HttpGet]
        [Route("Documents/{customerId}")]
        public async Task<IActionResult> Documents(int customerId)
        {
            var d = await service.FetchDocumentsAsync(customerId);
            return Ok(d);
        }
    }
}
