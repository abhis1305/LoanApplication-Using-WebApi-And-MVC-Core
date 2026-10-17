
using AutoMapper;
using LoanApplication.Application.DTO;
using LoanApplication.Application.Interface;
using LoanApplication.Domain.Entities;
using LoanApplication.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace LoanApplication.Infrastructure.Repositories
{
    public class SanctionLetterService : ISanctionLetterService
    {
        private readonly AppDBContext db;
        private readonly IMapper mapper;
        private readonly IEmailService emailService;

        private readonly IWebHostEnvironment env;

        public SanctionLetterService(AppDBContext db, IMapper mapper, IEmailService emailService, IWebHostEnvironment env)
        {
            this.db = db;
            this.mapper = mapper;
            this.emailService = emailService;
            this.env = env;
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



        public async Task<SanctionLetterDocumentDto> GenerateSanctionLetterAsync(int dealId)
        {
            var deal = await db.LoanDeals
                .Include(x => x.Customer)
                .FirstOrDefaultAsync(x =>
                    x.DealId == dealId &&
                    x.CurrentStatus == "Approved");

            if (deal == null)
                throw new InvalidOperationException("Approved loan not found.");

            if (string.IsNullOrWhiteSpace(deal.Customer?.Email))
                throw new InvalidOperationException("Customer email not found.");

            var existing = await db.SanctionLetters
                .FirstOrDefaultAsync(x => x.DealId == dealId);

            if (existing != null)
                throw new InvalidOperationException("Sanction letter already exists.");

            var letter = new SanctionLetter
            {
                DealId = deal.DealId,
                LoanAmount = deal.ApprovedAmount,
                InterestRate = deal.InterestRate,
                TenureMonths = deal.TenureMonths,
                EmiAmount = deal.EmiAmount,
                CreatedAt = DateTime.Now
            };

            string folderPath = Path.Combine(
                env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot"),
                "Documents");

            Directory.CreateDirectory(folderPath);

            string fileName = $"SanctionLetter_{dealId}.pdf";
            string filePath = Path.Combine(folderPath, fileName);

            string customerName =
                $"{deal.Customer.FirstName} {deal.Customer.LastName}".Trim();

            // Generate PDF
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Content().Column(column =>
                    {
                        column.Spacing(12);

                        column.Item().AlignCenter()
                            .Text("LOAN SANCTION LETTER")
                            .Bold().FontSize(22);

                        column.Item().PaddingTop(15)
                            .Text($"Date: {letter.CreatedAt:dd-MM-yyyy}");

                        column.Item()
                            .Text($"Application No: APP{deal.DealId:D3}");

                        column.Item()
                            .Text($"Deal ID: {deal.DealId}");

                        column.Item()
                            .Text($"Dear {customerName},");

                        column.Item()
                            .Text("We are pleased to inform you that your loan has been sanctioned with the following details:");

                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(3);
                            });

                            void AddRow(string label, string value)
                            {
                                table.Cell().Border(1).Padding(8).Text(label).Bold();
                                table.Cell().Border(1).Padding(8).Text(value);
                            }

                            AddRow("Customer Name", customerName);
                            AddRow("Loan Type", deal.LoanType);
                            AddRow("Approved Amount", deal.ApprovedAmount.ToString("N2"));
                            AddRow("Interest Rate", $"{deal.InterestRate}%");
                            AddRow("Tenure", $"{deal.TenureMonths} Months");
                            AddRow("Monthly EMI", deal.EmiAmount.ToString("N2"));
                        });

                        column.Item().PaddingTop(20)
                            .Text("Regards,")
                            .Bold();

                        column.Item()
                            .Text("Loan Application Team");
                    });
                });
            }).GeneratePdf(filePath);

            try
            {
                db.SanctionLetters.Add(letter);
                await db.SaveChangesAsync();

                string subject = "Your Loan Sanction Letter";

                string body = $@"
            <h2>Loan Sanction Letter</h2>
            <p>Dear {System.Net.WebUtility.HtmlEncode(customerName)},</p>
            <p>Your loan has been sanctioned successfully.</p>
            <p>Please find your sanction letter attached to this email.</p>
            <p>Regards,<br/>Loan Application Team</p>";

                // Pass the actual PDF path as the attachment argument.
                await emailService.SendEmailAsync(
                    deal.Customer.Email,
                    subject,
                    body,
                    filePath);

                return mapper.Map<SanctionLetterDocumentDto>(letter);
            }
            catch
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);

                throw;
            }
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


        //public async Task<SanctionLetterDocumentDto>
        //    GenerateSanctionLetterAsync(int dealId)
        //{
        //    var deal = await db.LoanDeals
        //        .Include(x => x.Customer)
        //        .FirstOrDefaultAsync(x =>
        //            x.DealId == dealId &&
        //            x.CurrentStatus == "Approved");

        //    if (deal == null)
        //        throw new InvalidOperationException(
        //            "Approved loan not found.");

        //    if (string.IsNullOrWhiteSpace(deal.Customer?.Email))
        //        throw new InvalidOperationException(
        //            "Customer email not found.");

        //    var existing = await db.SanctionLetters
        //        .FirstOrDefaultAsync(x => x.DealId == dealId);

        //    if (existing != null)
        //        throw new InvalidOperationException(
        //            "Sanction letter already exists.");

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

        //    string customerName =
        //        $"{deal.Customer.FirstName} {deal.Customer.LastName}";

        //    string subject = "Loan Sanction Details";

        //        string body = $@"
        //    <h2>Loan Sanction Details</h2>
        //    <p>Dear {System.Net.WebUtility.HtmlEncode(customerName)},</p>
        //    <p>Your loan has been approved successfully.</p>
        //    <p><b>Deal ID:</b> {deal.DealId}</p>
        //    <p><b>Loan Type:</b> {System.Net.WebUtility.HtmlEncode(deal.LoanType)}</p>
        //    <p><b>Approved Amount:</b> {deal.ApprovedAmount}</p>
        //    <p><b>Interest Rate:</b> {deal.InterestRate}%</p>
        //    <p><b>Tenure:</b> {deal.TenureMonths} months</p>
        //    <p><b>EMI Amount:</b> {deal.EmiAmount}</p>
        //    <p>Regards,<br/>Loan Application Team</p>";

        //    // PDF is not generated yet, so send email without attachment.
        //    await emailService.SendEmailAsync(
        //        deal.Customer.Email,
        //        subject,
        //        body,
        //        string.Empty);

        //    return mapper.Map<SanctionLetterDocumentDto>(letter);
        //}


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
