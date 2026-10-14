using LoanApplication.Application.DTO;
using LoanApplication.Application.Interface;
using LoanApplication.Domain.Entities;
using LoanApplication.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LoanApplication.Infrastructure.Repositories
{
    public class ScoreCardService : IScoreCardService
    {
        private readonly AppDBContext db;

        public ScoreCardService(AppDBContext db)
        {
            this.db = db;
        }

        public async Task<ScoreCardResponseDto> GenerateScoreCardAsync(int customerId)
        {
            // Fetch customer
            var customer = await db.Customers
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);

            if (customer == null)
            {
                throw new KeyNotFoundException("Customer not found.");
            }

            // Validate monthly income
            if (!customer.MonthlyIncome.HasValue ||
                customer.MonthlyIncome.Value <= 0)
            {
                throw new InvalidOperationException(
                    "Please enter a valid monthly income before generating the scorecard.");
            }

            // Fetch latest CIBIL report
            var cibil = await db.CibilReports
                .Where(c => c.CustomerId == customerId)
                .OrderByDescending(c => c.CheckDate)
                .ThenByDescending(c => c.CibilReportId)
                .FirstOrDefaultAsync();

            if (cibil == null)
            {
                throw new InvalidOperationException(
                    "Generate the CIBIL score before generating the scorecard.");
            }

            // Convert nullable values safely
            decimal monthlyIncome = customer.MonthlyIncome.Value;
            decimal monthlyInvestment = customer.MonthlyInvestment ?? 0m;

            // Determine risk category
            string riskCategory = GetRiskCategory(cibil.CibilScore);

            // Calculate allowable EMI
            decimal maximumEmi = monthlyIncome * 0.40m;

            decimal availableEmi = Math.Max(
                0m,
                maximumEmi - monthlyInvestment);

            // Loan options
            var options = new[]
            {
                (Rate: 11.3m, Tenure: 36),
                (Rate: 11.0m, Tenure: 60),
                (Rate: 11.0m, Tenure: 72)
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

                db.ScoreCards.Add(scoreCard);
                await db.SaveChangesAsync();

                savedOptions.Add(new LoanOptionDto
                {
                    ScoreCardId = scoreCard.ScoreCardId,
                    InterestRate = option.Rate,
                    TenureInMonths = option.Tenure,
                    EligibleLoanAmount = amount
                });
            }

            // Return scorecard response
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
            double monthlyRate = (double)annualRate / 1200.0;
            double months = tenure;
            double monthlyEmi = (double)emi;

            double amount;

            if (monthlyRate == 0)
            {
                amount = monthlyEmi * months;
            }
            else
            {
                double factor = Math.Pow(1 + monthlyRate, months);

                amount = monthlyEmi *
                    (factor - 1) /
                    (monthlyRate * factor);
            }

            return Math.Round((decimal)amount, 2);
        }

        public async Task<List<CustomerScoreCardDto>> GetAllCustomersAsync()
        {
            var customers = await db.Customers
                .Select(c => new CustomerScoreCardDto
                {
                    CustomerId = c.CustomerId,
                    Name = c.FirstName + " " + c.LastName,
                    Email = c.Email,
                    Income = c.MonthlyIncome
                })
                .ToListAsync();

            return customers;
        }

        private static string GetRiskCategory(int score)
        {
            if (score >= 800)
            {
                return "Low";
            }

            if (score >= 650)
            {
                return "Medium";
            }

            return "High";
        }
    }
}