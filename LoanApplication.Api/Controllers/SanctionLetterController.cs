
using LoanApplication.Application.Interface;
using Microsoft.AspNetCore.Mvc;

namespace LoanApplication.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SanctionLetterController : ControllerBase
    {
        private readonly ISanctionLetterService service;

        public SanctionLetterController(ISanctionLetterService service)
        {
            this.service = service;
        }

        // 1. Get all approved loans
        [HttpGet("approved-loans")]
        public async Task<IActionResult> GetApprovedLoans()
        {
            var data = await service.GetApprovedLoansAsync();
            return Ok(data);
        }

        // 2. Save sanction letter record for an approved loan
        [HttpPost("{dealId:int}/generate")]
        public async Task<IActionResult> GenerateSanctionLetter(int dealId)
        {
            var data = await service.GenerateSanctionLetterAsync(dealId);
            return Ok(data);
        }

        // 3. Get existing sanction letter details
        [HttpGet("{dealId:int}")]
        public async Task<IActionResult> GetSanctionLetter(int dealId)
        {
            var data = await service.GetSanctionLetterAsync(dealId);

            if (data == null)
            {
                return NotFound(new
                {
                    message = "Sanction letter not found."
                });
            }

            return Ok(data);
        }

        // 4. Fetch data required for sanction letter UI
        [HttpGet("{dealId:int}/data")]
        public async Task<IActionResult> GetSanctionLetterData(int dealId)
        {
            var data = await service.GetSanctionLetterDataAsync(dealId);

            if (data == null)
            {
                return NotFound(new
                {
                    message = "Approved loan not found."
                });
            }

            return Ok(data);
        }

        [HttpGet("{dealId:int}/view")]
        public IActionResult ViewSanctionLetterPdf(int dealId)
        {
            var fileName = $"SanctionLetter_{dealId}.pdf";
            var filePath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "Documents",
                fileName);

            if (!System.IO.File.Exists(filePath))
                return NotFound(new { message = "Sanction letter PDF not found." });

            return PhysicalFile(filePath, "application/pdf");
        }

        [HttpGet("{dealId:int}/download")]
        public IActionResult DownloadSanctionLetterPdf(int dealId)
        {
            var fileName = $"SanctionLetter_{dealId}.pdf";
            var filePath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "Documents",
                fileName);

            if (!System.IO.File.Exists(filePath))
                return NotFound(new { message = "Sanction letter PDF not found." });

            return PhysicalFile(filePath, "application/pdf", fileName);
        }
    }
}
