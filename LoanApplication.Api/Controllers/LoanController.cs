using LoanApplication.Application.DTO;
using LoanApplication.Application.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LoanApplication.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoanController : ControllerBase
    {
        private readonly ILoanDealService Service;

        public LoanController(ILoanDealService Service)
        {
            this.Service = Service;
        }


        [HttpGet("calculate")]
        public async Task<IActionResult> Calculate(int customerId,decimal loanAmount,int tenureYears)
        {
            
                var result = await Service.CalculateLoanAsync(customerId, loanAmount, tenureYears);

                return Ok(result);
            
        }


        [HttpPost("apply")]

        public async Task<IActionResult> ApplyLoan(
            [FromBody] CreateLoanDealDto dto)
        {
            
                var result = await Service.ApplyLoanAsync(dto);

                return Ok(new
                {
                    message = "Loan application submitted successfully.",
                    data = result
                });
            
            
        }


        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingLoans()
        {
            var loans = await Service.GetPendingLoansAsync();
            return Ok(loans);
        }


        [HttpGet("{dealId:int}")]
        public async Task<IActionResult> GetLoanById(int dealId)
        {
            var loan = await Service.GetLoanByIdAsync(dealId);

            if (loan == null)
                return NotFound(new { message = "Loan deal not found." });

            return Ok(loan);
        }


        [HttpPut("{dealId:int}/approve")]
        public async Task<IActionResult> ApproveLoan(int dealId, LoanDealReviewDto dto)
        {
              await Service.ApproveLoanAsync(dealId, dto);

                return Ok(new
                {
                    message = "Loan approved successfully."
                });
            
    
        }

        [HttpPut("{dealId:int}/reject")]
        public async Task<IActionResult> RejectLoan(int dealId, LoanDealReviewDto dto)
        {
                await Service.RejectLoanAsync(dealId, dto);

                return Ok(new
                {
                    message = "Loan rejected successfully."
                });
             
        }
    }
}

