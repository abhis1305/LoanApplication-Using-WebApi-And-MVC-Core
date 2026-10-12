
using LoanApplication.Application.DTO;
using LoanApplication.Application.Interfcae;
using LoanApplication.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using LoanApplication.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace LoanApplication.Infrastructure.Repositories
{
    public class DashboardService : IDashboardService
    {
        AppDBContext db;

        public DashboardService(AppDBContext db)
        {
            this.db = db;
        }

        private async Task<LoanAccount?> GetActiveLoanAsync(int customerId)
        {
            return await db.LoanAccounts
                .Where(l => l.CustomerId == customerId && l.LoanStatus == "Active")
                .OrderByDescending(l => l.LoanAccountId)
                .FirstOrDefaultAsync();
        }

        public async Task<DashboardCardsDTO> FetchCardsAsync(int customerId)
        {
            var totalApplications = await db.LoanDeals.CountAsync(d => d.CustomerId == customerId);

            var activeLoans = await db.LoanAccounts
                .CountAsync(l => l.CustomerId == customerId && l.LoanStatus == "Active");

            var emisPaid = await db.EmiSchedules
                .CountAsync(e => e.LoanAccount!.CustomerId == customerId && e.PaymentStatus == "Paid");

            var upcoming = await db.EmiSchedules
                .Where(e => e.LoanAccount!.CustomerId == customerId && e.PaymentStatus != "Paid")
                .OrderBy(e => e.DueDate)
                .Select(e => new { e.Emi, e.DueDate })
                .FirstOrDefaultAsync();

            var totalPaid = await db.LoanPayments
                .Where(p => p.LoanAccount!.CustomerId == customerId && p.PaymentStatus == "Success")
                .SumAsync(p => (decimal?)p.PaidAmount) ?? 0;

            return new DashboardCardsDTO
            {
                TotalApplications = totalApplications,
                ActiveLoans = activeLoans,
                EmisPaid = emisPaid,
                UpcomingEmiAmount = upcoming?.Emi ?? 0,
                UpcomingEmiDueDate = upcoming?.DueDate,
                TotalAmountPaid = totalPaid
            };
        }

        public async Task<List<DocumentDTO>> FetchDocumentsAsync(int customerId)
        {
            return await db.KycDocuments
                .Where(k => k.CustomerId == customerId)
                .Select(k => new DocumentDTO
                {
                    DocumentId = k.DocumentId,
                    DocumentType = k.DocumentType,
                    VerificationStatus = k.VerificationStatus
                })
                .ToListAsync();
        }

        public async Task<EmiSummaryDTO?> FetchEmiSummaryAsync(int customerId)
        {
            var loan = await GetActiveLoanAsync(customerId);
            if (loan == null) return null;

            var emis = db.EmiSchedules.Where(e => e.LoanAccountId == loan.LoanAccountId);
            var totalEmis = await emis.CountAsync();

            var paid = emis.Where(e => e.PaymentStatus == "Paid");
            var paidCount = await paid.CountAsync();
            var paidAmount = await paid.SumAsync(e => (decimal?)e.Emi) ?? 0;

            var unpaid = emis.Where(e => e.PaymentStatus != "Paid");
            var unpaidCount = await unpaid.CountAsync();
            var unpaidAmount = await unpaid.SumAsync(e => (decimal?)e.Emi) ?? 0;

            var next = await unpaid
                .OrderBy(e => e.DueDate)
                .Select(e => new { e.Emi, e.DueDate })
                .FirstOrDefaultAsync();

            return new EmiSummaryDTO
            {
                TotalEmis = totalEmis,
                PaidCount = paidCount,
                PaidAmount = paidAmount,
                UpcomingCount = next == null ? 0 : 1,
                UpcomingAmount = next?.Emi ?? 0,
                RemainingCount = Math.Max(unpaidCount - 1, 0),
                RemainingAmount = unpaidAmount - (next?.Emi ?? 0),
                NextEmiDueDate = next?.DueDate,
                NextEmiAmount = next?.Emi ?? 0
            };
        }

        public async Task<LoanProgressDTO?> FetchProgressAsync(int customerId)
        {
            var deal = await db.LoanDeals
                .Where(d => d.CustomerId == customerId)
                .OrderByDescending(d => d.AppliedDate)
                .FirstOrDefaultAsync();
            if (deal == null) return null;

            var kycTotal = await db.KycDocuments.CountAsync(k => k.CustomerId == customerId);
            var kycVerified = await db.KycDocuments
                .CountAsync(k => k.CustomerId == customerId && k.VerificationStatus == "Verified");
            var underwriting = await db.DealReviews.AnyAsync(r => r.DealId == deal.DealId);

            var sanctionDate = await db.SanctionLetters
                .Where(s => s.DealId == deal.DealId)
                .Select(s => (DateTime?)s.CreatedAt)
                .FirstOrDefaultAsync();
            var disburseDate = await db.Disbursements
                .Where(x => x.DealId == deal.DealId)
                .Select(x => (DateTime?)x.DisbursementDate)
                .FirstOrDefaultAsync();
            var closureDate = await db.LoanClosures
                .Where(c => c.LoanAccount!.DealId == deal.DealId)
                .Select(c => (DateTime?)c.ClosureDate)
                .FirstOrDefaultAsync();

            return new LoanProgressDTO
            {
                ApplicationId = deal.DealId,
                CurrentStatus = deal.CurrentStatus,
                Stages = new List<ProgressStageDTO>
                {
                    new ProgressStageDTO { Stage = "Application Submitted", Done = true, Date = deal.AppliedDate },
                    new ProgressStageDTO { Stage = "KYC Verified", Done = kycTotal > 0 && kycTotal == kycVerified },
                    new ProgressStageDTO { Stage = "Underwriting", Done = underwriting },
                    new ProgressStageDTO { Stage = "Sanctioned", Done = sanctionDate != null, Date = sanctionDate },
                    new ProgressStageDTO { Stage = "Disbursed", Done = disburseDate != null, Date = disburseDate },
                    new ProgressStageDTO { Stage = "Closed", Done = closureDate != null, Date = closureDate }
                }
            };
        }

        public async Task<List<RecentPaymentDTO>> FetchRecentPaymentsAsync(int customerId)
        {
            return await db.LoanPayments
                .Where(p => p.LoanAccount!.CustomerId == customerId)
                .OrderByDescending(p => p.PaymentDate)
                .Take(5)
                .Select(p => new RecentPaymentDTO
                {
                    PaymentId = p.PaymentId,
                    PaymentDate = p.PaymentDate,
                    PaymentName = p.PaymentName,
                    PaidAmount = p.PaidAmount,
                    PaymentStatus = p.PaymentStatus
                })
                .ToListAsync();
        }

        public async Task<ActiveLoanDTO?> FetchActiveLoanAsync(int customerId)
        {
            var loan = await GetActiveLoanAsync(customerId);
            if (loan == null) return null;

            var loanType = await db.LoanDeals
                .Where(d => d.DealId == loan.DealId)
                .Select(d => d.LoanType)
                .FirstOrDefaultAsync();

            var emisPaid = await db.EmiSchedules
                .CountAsync(e => e.LoanAccountId == loan.LoanAccountId && e.PaymentStatus == "Paid");

            return new ActiveLoanDTO
            {
                LoanAccountId = loan.LoanAccountId,
                LoanAccountNo = loan.LoanAccountNo,
                LoanType = loanType ?? "",
                LoanStatus = loan.LoanStatus,
                LoanAmount = loan.LoanAmount,
                DisbursementDate = loan.DisbursementDate,
                TenureMonths = loan.TenureMonths,
                InterestRate = (decimal)loan.InterestRate,
                EmiAmount = loan.EmiAmount,
                EmisPaid = emisPaid,
                EmisRemaining = loan.TenureMonths - emisPaid,
                OutstandingAmount = loan.OutstandingPrincipal,
                PercentPaid = loan.TenureMonths > 0
                    ? Math.Round(emisPaid * 100.0 / loan.TenureMonths, 1) : 0
            };
        }
    }
}
