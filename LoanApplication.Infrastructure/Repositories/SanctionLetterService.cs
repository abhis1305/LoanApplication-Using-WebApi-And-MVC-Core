
using AutoMapper;
using LoanApplication.Application.DTO;
using LoanApplication.Application.Interface;
using LoanApplication.Domain.Entities;
using LoanApplication.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LoanApplication.Infrastructure.Repositories
{
    public class SanctionLetterService : ISanctionLetterService
    {
        private readonly AppDBContext db;
        private readonly IMapper mapper;
        private readonly IEmailService emailService;

        public SanctionLetterService(AppDBContext db, IMapper mapper, IEmailService emailService)
        {
            this.db = db;
            this.mapper = mapper;
            this.emailService = emailService;
        }

        // Get approved loans
        public async Task<List<ApprovedLoanDto>> GetApprovedLoansAsync()
        {
            var loans = await db.LoanDeals
                .Include(x => x.Customer)
                .Where(x => x.CurrentStatus == "Approved")
                .ToListAsync();

            return mapper.Map<List<ApprovedLoanDto>>(loans);
        }

        // Save sanction details
        //public async Task<SanctionLetterDocumentDto> GenerateSanctionLetterAsync(int dealId)
        //{
        //    var deal = await db.LoanDeals
        //        .FirstOrDefaultAsync(x =>
        //            x.DealId == dealId &&
        //            x.CurrentStatus == "Approved");

        //    if (deal == null)
        //        throw new InvalidOperationException("Approved loan not found.");

        //    var existing = await db.SanctionLetters
        //        .FirstOrDefaultAsync(x => x.DealId == dealId);

        //    if (existing != null)
        //        throw new InvalidOperationException("Sanction letter already exists.");

        //    var letter = new SanctionLetter
        //    {
        //        DealId = deal.DealId,
        //        LoanAmount = deal.ApprovedAmount,
        //        InterestRate = deal.InterestRate,
        //        TenureMonths = deal.TenureMonths,
        //        EmiAmount = deal.EmiAmount,
        //        CreatedAt = DateTime.Now
        //    };

        //    db.SanctionLetters.Add(letter);
        //    await db.SaveChangesAsync();

        //    return mapper.Map<SanctionLetterDocumentDto>(letter);
        //}


        public async Task<SanctionLetterDocumentDto>
            GenerateSanctionLetterAsync(int dealId)
        {
            var deal = await db.LoanDeals
                .Include(x => x.Customer)
                .FirstOrDefaultAsync(x =>
                    x.DealId == dealId &&
                    x.CurrentStatus == "Approved");

            if (deal == null)
                throw new InvalidOperationException(
                    "Approved loan not found.");

            if (string.IsNullOrWhiteSpace(deal.Customer?.Email))
                throw new InvalidOperationException(
                    "Customer email not found.");

            var existing = await db.SanctionLetters
                .FirstOrDefaultAsync(x => x.DealId == dealId);

            if (existing != null)
                throw new InvalidOperationException(
                    "Sanction letter already exists.");

            var letter = new SanctionLetter
            {
                DealId = deal.DealId,
                LoanAmount = deal.ApprovedAmount,
                InterestRate = deal.InterestRate,
                TenureMonths = deal.TenureMonths,
                EmiAmount = deal.EmiAmount,
                CreatedAt = DateTime.Now
            };

            db.SanctionLetters.Add(letter);
            await db.SaveChangesAsync();

            string customerName =
                $"{deal.Customer.FirstName} {deal.Customer.LastName}";

            string subject = "Loan Sanction Details";

                string body = $@"
            <h2>Loan Sanction Details</h2>
            <p>Dear {System.Net.WebUtility.HtmlEncode(customerName)},</p>
            <p>Your loan has been approved successfully.</p>
            <p><b>Deal ID:</b> {deal.DealId}</p>
            <p><b>Loan Type:</b> {System.Net.WebUtility.HtmlEncode(deal.LoanType)}</p>
            <p><b>Approved Amount:</b> {deal.ApprovedAmount}</p>
            <p><b>Interest Rate:</b> {deal.InterestRate}%</p>
            <p><b>Tenure:</b> {deal.TenureMonths} months</p>
            <p><b>EMI Amount:</b> {deal.EmiAmount}</p>
            <p>Regards,<br/>Loan Application Team</p>";

            // PDF is not generated yet, so send email without attachment.
            await emailService.SendEmailAsync(
                deal.Customer.Email,
                subject,
                body,
                string.Empty);

            return mapper.Map<SanctionLetterDocumentDto>(letter);
        }


        // Get existing sanction details
        public async Task<SanctionLetterDocumentDto?> GetSanctionLetterAsync(int dealId)
        {
            var letter = await db.SanctionLetters
                .FirstOrDefaultAsync(x => x.DealId == dealId);

            return letter == null
                ? null
                : mapper.Map<SanctionLetterDocumentDto>(letter);
        }

        // Fetch data for the sanction letter
        public async Task<SanctionLetterDataDto?> GetSanctionLetterDataAsync(int dealId)
        {
            var deal = await db.LoanDeals
                .Include(x => x.Customer)
                .FirstOrDefaultAsync(x =>
                    x.DealId == dealId &&
                    x.CurrentStatus == "Approved");

            if (deal == null)
                return null;

            var data = mapper.Map<SanctionLetterDataDto>(deal);

            var letter = await db.SanctionLetters
                .FirstOrDefaultAsync(x => x.DealId == dealId);

            data.SanctionDate = letter?.CreatedAt ?? DateTime.Now;

            return data;
        }
    }
}
