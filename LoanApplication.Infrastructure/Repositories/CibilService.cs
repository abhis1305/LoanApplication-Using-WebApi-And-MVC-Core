using LoanApp.Models;
using LoanApplication.Application.DTO;
using LoanApplication.Application.Interface;
using LoanApplication.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LoanApplication.Infrastructure.Repositories
{
    public class CibilService : ICibilService
    {
        private readonly AppDBContext _context;

        public CibilService(AppDBContext context)
        {
            _context = context;
        }

        public async Task<CibilScoreResponseDto> GenerateScoreAsync(
            int customerId)
        {
            // Fetch existing customer
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);

            if (customer == null)
                throw new KeyNotFoundException("Customer not found.");

            // Calculate individual scores
            int incomeScore = GetIncomeScore(customer.MonthlyIncome);

            int employmentScore =
                GetEmploymentScore(customer.EmploymentType);

            int ageScore = GetAgeScore(customer.Age);

            decimal foir = customer.MonthlyIncome == 0
                ? 0
                : Math.Round(
                    customer.MonthlyInvestement /
                    customer.MonthlyIncome * 100m, 2);

            int foirScore = GetFoirScore(foir);

            // Calculate final score
            int finalScore =
                incomeScore + employmentScore + ageScore + foirScore;

            string status = GetCibilStatus(finalScore);

            // Save CIBIL report
            var report = new CibilReport
            {
                CustomerId = customer.CustomerId,
                PanNo = customer.Pan,
                CibilScore = finalScore,
                CheckDate = DateTime.UtcNow
            };

            _context.CibilReports.Add(report);
            await _context.SaveChangesAsync();

            // Return generated result
            return new CibilScoreResponseDto
            {
                CibilReportId = report.CibilReportId,
                CustomerId = customer.CustomerId,
                PanNo = customer.Pan,
                IncomeScore = incomeScore,
                EmploymentScore = employmentScore,
                AgeScore = ageScore,
                FOIR = foir,
                FOIRScore = foirScore,
                CibilScore = finalScore,
                CibilStatus = status,
                CheckDate = report.CheckDate
            };
        }

        private static int GetIncomeScore(decimal income)
        {
            if (income < 25000m) return 100;
            if (income < 50000m) return 200;
            if (income <= 100000m) return 300;
            return 400;
        }

        private static int GetEmploymentScore(string employment)
        {
            return employment.Trim().ToLowerInvariant() switch
            {
                "government" => 200,
                "private" => 150,
                "self" => 100,
                "self-employed" => 100,
                _ => 0
            };
        }

        private static int GetAgeScore(int age)
        {
            if (age >= 21 && age <= 24) return 50;
            if (age >= 25 && age <= 45) return 150;
            if (age >= 46 && age <= 60) return 100;
            return 0;
        }

        private static int GetFoirScore(decimal foir)
        {
            if (foir < 30m) return 250;
            if (foir < 50m) return 150;
            if (foir < 60m) return 75;
            return 0;
        }

        private static string GetCibilStatus(int score)
        {
            if (score >= 900) return "Excellent";
            if (score >= 800) return "Very Good";
            if (score >= 750) return "Good";
            if (score >= 700) return "Average";
            if (score >= 650) return "Risky";
            return "Reject";
        }
    }
}