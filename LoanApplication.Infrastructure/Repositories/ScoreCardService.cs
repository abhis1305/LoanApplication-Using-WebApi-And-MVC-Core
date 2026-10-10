using LoanApp.Models;
using LoanApplication.Application.DTO;
using LoanApplication.Application.Interface;
using LoanApplication.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LoanApplication.Infrastructure.Repositories
{
    public class ScoreCardService : IScoreCardService
    {
        private readonly AppDBContext _context;

        public ScoreCardService(AppDBContext context)
        {
            _context = context;
        }

        public async Task<ScoreCardResponseDto> GenerateScoreCardAsync(
            int customerId)
        {
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);

            if (customer == null)
                throw new KeyNotFoundException("Customer not found.");

            var cibil = await _context.CibilReports
                .Where(c => c.CustomerId == customerId)
                .OrderByDescending(c => c.CheckDate)
                .ThenByDescending(c => c.CibilReportId)
                .FirstOrDefaultAsync();

            if (cibil == null)
                throw new InvalidOperationException(
                    "Generate the CIBIL score before generating the scorecard.");

            string riskCategory = GetRiskCategory(cibil.CibilScore);

            decimal maximumEmi = customer.MonthlyIncome * 0.40m;

            decimal availableEmi = Math.Max(
                0m,
                maximumEmi - customer.MonthlyInvestement);

            var options = new[]
            {
                (Rate: 11.3m, Tenure: 36),
                (Rate: 11m, Tenure: 60),
                (Rate: 11m, Tenure: 72)
            };

            var savedOptions = new List<LoanOptionDto>();

            foreach (var option in options)
            {
                decimal amount = CalculateLoanAmount(
                    availableEmi,
                    option.Rate,
                    option.Tenure);

                var scoreCard = new ScoreCard
                {
                    CustomerId = customer.CustomerId,
                    RiskCategory = riskCategory,
                    InterestRate = option.Rate,
                    TenureInMonths = option.Tenure,
                    EligibleLoanAmount = amount
                };

                _context.ScoreCards.Add(scoreCard);
                await _context.SaveChangesAsync();

                savedOptions.Add(new LoanOptionDto
                {
                    ScoreCardId = scoreCard.ScoreCardId,
                    InterestRate = option.Rate,
                    TenureInMonths = option.Tenure,
                    EligibleLoanAmount = amount
                });
            }

            return new ScoreCardResponseDto
            {
                CustomerId = customer.CustomerId,
                CibilScore = cibil.CibilScore,
                RiskCategory = riskCategory,
                MaximumAllowableEmi = maximumEmi,
                AvailableEmi = availableEmi,
                LoanOptions = savedOptions
            };
        }

        private static decimal CalculateLoanAmount(
            decimal emi,
            decimal annualRate,
            int tenure)
        {
            double r = (double)annualRate / 1200.0;
            double n = tenure;
            double monthlyEmi = (double)emi;

            double amount;

            if (r == 0)
            {
                amount = monthlyEmi * n;
            }
            else
            {
                double factor = Math.Pow(1 + r, n);

                amount = monthlyEmi *
                    (factor - 1) / (r * factor);
            }

            return Math.Round((decimal)amount, 2);
        }

        private static string GetRiskCategory(int score)
        {
            if (score >= 800) return "Low";
            if (score >= 650) return "Medium";

            return "High";
        }
    }
}