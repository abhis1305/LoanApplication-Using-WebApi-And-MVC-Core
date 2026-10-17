
using LoanApplication.Application.DTO;
using LoanApplication.Application.Interface;
using LoanApplication.Domain.Entities;
using LoanApplication.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LoanApplication.Infrastructure.Repositories
{
    public class CibilService : ICibilService
    {
        private readonly AppDBContext db;

        public CibilService(AppDBContext db)
        {
            this.db = db;
        }

        public async Task<CibilCustomerProfileDto?> GetCustomerProfileAsync(int userId)
        {
            var user = await db.Users
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null || user.CustomerId == null)
                return null;

            var customer = await db.Customers
                .FirstOrDefaultAsync(c => c.CustomerId == user.CustomerId.Value);

            if (customer == null)
                return null;

            return new CibilCustomerProfileDto
            {
                CustomerId = customer.CustomerId,
                CustomerName = $"{customer.FirstName} {customer.LastName}".Trim(),
                PanNumber = customer.Pan
            };
        }

        public async Task<CibilScoreResponseDto> GetCibilScoreAsync(int customerId)
        {
            var customer = await db.Customers
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);

            if (customer == null)
                throw new KeyNotFoundException("Customer not found.");

            var report = await db.CibilReports
                .Where(c => c.CustomerId == customerId)
                .OrderByDescending(c => c.CheckDate)
                .ThenByDescending(c => c.CibilReportId)
                .FirstOrDefaultAsync();

            if (report == null)
                throw new KeyNotFoundException(
                    "CIBIL report not found. Generate the score first.");

            ValidateCustomerDetails(customer);

            decimal income = customer.MonthlyIncome!.Value;
            int age = customer.Age!.Value;
            decimal investment = customer.MonthlyInvestment ?? 0m;

            int incomeScore = GetIncomeScore(income);
            int employmentScore = GetEmploymentScore(customer.EmploymentType);
            int ageScore = GetAgeScore(age);

            decimal foir = Math.Round(investment / income * 100m, 2);
            int foirScore = GetFoirScore(foir);

            return new CibilScoreResponseDto
            {
                CibilReportId = report.CibilReportId,
                CustomerId = customer.CustomerId,
                PanNo = report.PanNo,
                IncomeScore = incomeScore,
                EmploymentScore = employmentScore,
                AgeScore = ageScore,
                FOIR = foir,
                FOIRScore = foirScore,
                CibilScore = report.CibilScore,
                CibilStatus = GetCibilStatus(report.CibilScore),
                CheckDate = report.CheckDate
            };
        }

        public async Task<CibilScoreResponseDto> GenerateScoreAsync(int customerId)
        {
            var customer = await db.Customers
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);

            if (customer == null)
                throw new KeyNotFoundException("Customer not found.");

            ValidateCustomerDetails(customer);

            decimal income = customer.MonthlyIncome!.Value;
            int age = customer.Age!.Value;
            decimal investment = customer.MonthlyInvestment ?? 0m;

            // Calculate individual scores
            int incomeScore = GetIncomeScore(income);
            int employmentScore = GetEmploymentScore(customer.EmploymentType);
            int ageScore = GetAgeScore(age);

            // FOIR = Monthly financial obligations / Monthly income * 100
            decimal foir = Math.Round(investment / income * 100m, 2);
            int foirScore = GetFoirScore(foir);

            // Calculate total CIBIL score
            int finalScore =
                incomeScore +
                employmentScore +
                ageScore +
                foirScore;

            string status = GetCibilStatus(finalScore);

            // Debug logs
            Console.WriteLine("========== CIBIL SCORE DEBUG ==========");
            Console.WriteLine($"Customer ID        : {customer.CustomerId}");
            Console.WriteLine($"Customer Name      : {customer.FirstName} {customer.LastName}");
            Console.WriteLine($"Monthly Income     : {income}");
            Console.WriteLine($"Income Score       : {incomeScore}");
            Console.WriteLine($"Employment Type    : {customer.EmploymentType}");
            Console.WriteLine($"Employment Score   : {employmentScore}");
            Console.WriteLine($"Age                : {age}");
            Console.WriteLine($"Age Score          : {ageScore}");
            Console.WriteLine($"Monthly Investment : {investment}");
            Console.WriteLine($"FOIR               : {foir}%");
            Console.WriteLine($"FOIR Score         : {foirScore}");
            Console.WriteLine($"Final CIBIL Score  : {finalScore}");
            Console.WriteLine($"CIBIL Status       : {status}");
            Console.WriteLine("=======================================");

            // Save CIBIL report
            var report = new CibilReport
            {
                CustomerId = customer.CustomerId,
                PanNo = customer.Pan,
                CibilScore = finalScore,
                CheckDate = DateTime.UtcNow
            };

            db.CibilReports.Add(report);
            await db.SaveChangesAsync();

            return new CibilScoreResponseDto
            {
                CibilReportId = report.CibilReportId,
                CustomerId = customer.CustomerId,
                PanNo = report.PanNo,
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

        private static void ValidateCustomerDetails(Customer customer)
        {
            if (!customer.Age.HasValue)
                throw new InvalidOperationException(
                    "Please complete your age before generating the CIBIL score.");

            if (!customer.MonthlyIncome.HasValue ||
                customer.MonthlyIncome.Value <= 0)
                throw new InvalidOperationException(
                    "Please enter a valid monthly income before generating the CIBIL score.");

            if (string.IsNullOrWhiteSpace(customer.EmploymentType))
                throw new InvalidOperationException(
                    "Please complete your employment type before generating the CIBIL score.");
        }

        private static int GetIncomeScore(decimal income)
        {
            if (income < 25000m)
                return 100;

            if (income < 50000m)
                return 200;

            if (income <= 100000m)
                return 300;

            return 400;
        }

        // Matches registration values: Private, Government, Self Employed
        private static int GetEmploymentScore(string? employment)
        {
            string type = employment?.Trim().ToLowerInvariant() ?? "";

            return type switch
            {
                "government" => 200,

                "private" or "salaried" => 150,

                "self employed" or "self-employed" => 100,

                // Support legacy registration values
                "business" => 100,

                _ => 0
            };
        }

        private static int GetAgeScore(int age)
        {
            if (age >= 21 && age <= 24)
                return 50;

            if (age >= 25 && age <= 45)
                return 150;

            if (age >= 46 && age <= 60)
                return 100;

            return 0;
        }

        private static int GetFoirScore(decimal foir)
        {
            if (foir < 30m)
                return 250;

            if (foir < 50m)
                return 150;

            if (foir < 60m)
                return 75;

            return 0;
        }

        private static string GetCibilStatus(int score)
        {
            if (score >= 900)
                return "Excellent";

            if (score >= 800)
                return "Very Good";

            if (score >= 750)
                return "Good";

            if (score >= 700)
                return "Average";

            if (score >= 650)
                return "Risky";

            return "Reject";
        }
    }
}
