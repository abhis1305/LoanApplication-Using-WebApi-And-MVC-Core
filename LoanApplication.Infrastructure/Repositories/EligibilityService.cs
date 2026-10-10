using LoanApplication.Application.DTO;
using LoanApplication.Application.Interface;
using LoanApplication.Domain.Entities;
using LoanApplication.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LoanApplication.Infrastructure.Repositories
{
    public class EligibilityService : IEligibilityService
    {
        AppDBContext db;

        public EligibilityService(AppDBContext db)
        {
            this.db = db;
        }

        public async Task<EligibilityResponseDto> CheckEligibilityAsync(int customerId)
        {
            var customer = await db.Customers.FirstOrDefaultAsync(c => c.CustomerId == customerId);
            if (customer == null)
                throw new KeyNotFoundException("Customer not found.");

            var cibil = await db.CibilReports.Where(c => c.CustomerId == customerId)
                .OrderByDescending(c => c.CheckDate)
                .ThenByDescending(c => c.CibilReportId)
                .FirstOrDefaultAsync();

            if (cibil == null)
                throw new InvalidOperationException("Generate the CIBIL score first.");

            var scoreCard = await db.ScoreCards
                .Where(s => s.CustomerId == customerId)
                .OrderByDescending(s => s.ScoreCardId)
                .FirstOrDefaultAsync();

            if (scoreCard == null)
                throw new InvalidOperationException(
                    "Generate the scorecard first.");

            string status;
            string? rejectionReason = null;

            if (cibil.CibilScore >= 800)
            {
                status = "Approved";
            }
            else if (cibil.CibilScore >= 650)
            {
                status = "Pending";
            }
            else
            {
                status = "Rejected";
                rejectionReason = "CIBIL score is below 650.";
            }

            var eligibility = new EligibilityResult
            {
                CustomerId = customerId,
                CibilScore = cibil.CibilScore,
                IsEligible = status,
                LoanAmount = scoreCard.EligibleLoanAmount,
                RejectionReason = rejectionReason
            };

            db.EligibilityResults.Add(eligibility);
            await db.SaveChangesAsync();

            string riskCategory =
                cibil.CibilScore >= 800 ? "Low" :
                cibil.CibilScore >= 650 ? "Medium" : "High";

            return new EligibilityResponseDto
            {
                EligibilityId = eligibility.EligibilityId,
                CustomerId = customerId,
                CibilScore = cibil.CibilScore,
                RiskCategory = riskCategory,
                EligibilityStatus = status,
                EligibleLoanAmount = scoreCard.EligibleLoanAmount,
                RejectionReason = rejectionReason
            };
        }

        public async Task<List<EligibilityResponseDto>> GetPendingRequestsAsync()
        {
            var requests = await db.EligibilityResults
                .Where(e => e.IsEligible == "Pending")
                .ToListAsync();

            return requests.Select(e => new EligibilityResponseDto
            {
                EligibilityId = e.EligibilityId,
                CustomerId = e.CustomerId,
                CibilScore = e.CibilScore,
                RiskCategory = e.CibilScore >= 800 ? "Low" :
                               e.CibilScore >= 650 ? "Medium" : "High",
                EligibilityStatus = e.IsEligible!,
                EligibleLoanAmount = e.LoanAmount,
                RejectionReason = e.RejectionReason
            }).ToList();
        }

        public async Task<EligibilityResponseDto> UpdateOfficerDecisionAsync(int eligibilityId,bool approved,string? remarks)
        {
            var eligibility = await db.EligibilityResults
                .FirstOrDefaultAsync(e => e.EligibilityId == eligibilityId);

            if (eligibility == null)
                throw new KeyNotFoundException(
                    "Eligibility record not found.");

            if (eligibility.IsEligible != "Pending")
                throw new InvalidOperationException(
                    "Only pending requests can be approved or rejected.");

            eligibility.IsEligible = approved ? "Approved" : "Rejected";

            eligibility.RejectionReason = approved ? null :
                (string.IsNullOrWhiteSpace(remarks)
                    ? "Rejected by loan officer."
                    : remarks);

            await db.SaveChangesAsync();

            int score = eligibility.CibilScore;

            return new EligibilityResponseDto
            {
                EligibilityId = eligibility.EligibilityId,
                CustomerId = eligibility.CustomerId,
                CibilScore = score,
                RiskCategory = score >= 800 ? "Low" :
                               score >= 650 ? "Medium" : "High",
                EligibilityStatus = eligibility.IsEligible!,
                EligibleLoanAmount = eligibility.LoanAmount,
                RejectionReason = eligibility.RejectionReason
            };
        }

        public async Task<EligibilityResponseDto> GetEligibilityResultAsync(int eligibilityId)
        {
            var eligibility = await db.EligibilityResults
                .FirstOrDefaultAsync(e => e.EligibilityId == eligibilityId);

            if (eligibility == null)
                throw new KeyNotFoundException(
                    "Eligibility record not found.");

            int score = eligibility.CibilScore;

            return new EligibilityResponseDto
            {
                EligibilityId = eligibility.EligibilityId,
                CustomerId = eligibility.CustomerId,
                CibilScore = score,
                RiskCategory = score >= 800 ? "Low" :
                               score >= 650 ? "Medium" : "High",
                EligibilityStatus = eligibility.IsEligible!,
                EligibleLoanAmount = eligibility.LoanAmount,
                RejectionReason = eligibility.RejectionReason
            };
        }
    }
}
