using LoanApplication.Application.Interfcae;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LoanApplication.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoanAccountController : ControllerBase
    {
        ILoanAccountRepository loan;
        public LoanAccountController(ILoanAccountRepository loan)
        {
            this.loan = loan;
        }

        [HttpPost]
        [Route("CreateLA/{dealId}")]
        public async Task<IActionResult> CreateLoanAccount(int dealId)
        {
            var account = await loan.CreateLoanAccount(dealId);

            return Ok(new
            {
                message = "Loan Account Created Successfully",
                data = new
                {
                    account.LoanAccountId,
                    account.DealId,
                    account.CustomerId,
                    account.LoanAccountNo,
                    account.LoanAmount,
                    account.OutstandingPrincipal,
                    account.LoanStatus,
                    account.InterestRate,
                    account.TenureMonths,
                    account.EmiAmount,
                    account.DisbursementDate,
                    account.TotalPaidAmount,
                    account.CreatedAt
                }
            });
        }
      
        [HttpGet]
        [Route("GetLA/{id}")]
        public async Task<IActionResult> LoanAccountById(int id)
        {
            var account = await loan.GetLoanAccountById(id);
            if (account == null)
            {
                return NotFound(new { message = "Loan Account Not Found" });
            }
            return Ok(account);
        }
    }
}