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
                message = "Loan Account Created Success",
                data = account
            });
        }

        [HttpGet]
        [Route("GetLA")]
        public async Task<IActionResult> LoanAccountList()
        {
            return Ok(new
            {
                message = "Use GetLA/{id} to fetch a specific loan account"
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