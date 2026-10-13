using LoanApplication.Application.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LoanApplication.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        IReportService service;
        public ReportsController(IReportService service)
        {
            this.service = service;
        }

        //los
        [HttpGet]
        [Route("LosCards")]
        public async Task<IActionResult> LosCards()
        {
            var d = await service.FetchLosCardsAsync();
            return Ok(d);
        }

        [HttpGet]
        [Route("ProductWise")]
        public async Task<IActionResult> ProductWise(DateTime? from, DateTime? to)
        {
            var d = await service.FetchProductWiseAsync(from, to);
            return Ok(d);
        }

        [HttpGet]
        [Route("Tat")]
        public async Task<IActionResult> Tat(DateTime? from, DateTime? to)
        {
            var d = await service.FetchTatAsync(from, to);
            return Ok(d);
        }

        //los
        [HttpGet]
        [Route("LmsCards")]
        public async Task<IActionResult> LmsCards()
        {
            var d = await service.FetchLmsCardsAsync();
            return Ok(d);
        }

        [HttpGet]
        [Route("Portfolio")]
        public async Task<IActionResult> Portfolio()
        {
            var d = await service.FetchPortfolioAsync();
            return Ok(d);
        }

        [HttpGet]
        [Route("CibilRange")]
        public async Task<IActionResult> CibilRange(DateTime? from, DateTime? to)
        {
            var d = await service.FetchCibilRangeAsync(from, to);
            return Ok(d);
        }

        [HttpGet]
        [Route("ScoreCardWise")]
        public async Task<IActionResult> ScoreCardWise(DateTime? from, DateTime? to)
        {
            var d = await service.FetchScoreCardWiseAsync(from, to);
            return Ok(d);
        }

        [HttpGet]
        [Route("EmiCollection")]
        public async Task<IActionResult> EmiCollection(DateTime? from, DateTime? to)
        {
            var d = await service.FetchEmiCollectionAsync(from, to);
            return Ok(d);
        }

        [HttpGet]
        [Route("Overdue")]
        public async Task<IActionResult> Overdue()
        {
            var d = await service.FetchOverdueAsync();
            return Ok(d);
        }

        [HttpGet]
        [Route("Disbursement")]
        public async Task<IActionResult> Disbursement(DateTime? from, DateTime? to)
        {
            var d = await service.FetchDisbursementAsync(from, to);
            return Ok(d);
        }

        [HttpGet]
        [Route("Prepayment")]
        public async Task<IActionResult> Prepayment(DateTime? from, DateTime? to)
        {
            var d = await service.FetchPrepaymentAsync(from, to);
            return Ok(d);
        }
    }
}
