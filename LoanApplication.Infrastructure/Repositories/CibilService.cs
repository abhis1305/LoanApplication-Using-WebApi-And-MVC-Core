using LoanApplication.Application.DTO;
using LoanApplication.Application.Interface;
using LoanApplication.Domain.Entities;
using LoanApplication.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LoanApplication.Infrastructure.Repositories
{
    public class CibilService : ICibilService
    {
        AppDBContext db;

        public CibilService(AppDBContext db )
        {
            this.db = db;
        }

        public async Task<CibilScoreResponseDto> GetCibilScoreAsync(int customerId)
        {
            var data = await db.CibilReports
                .Where(c => c.CustomerId == customerId)
                .OrderByDescending(c => c.CheckDate)
                .ThenByDescending(c => c.CibilReportId)
                .FirstOrDefaultAsync();

            if (data == null)
            {
                throw new KeyNotFoundException("CIBIL report not found. Generate the score first.");
            }

            var customer = await db.Customers
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);

            if (customer == null)
            {
                throw new KeyNotFoundException("Customer not found.");
            }

            int incomeScore = GetIncomeScore(customer.MonthlyIncome);
            int employmentScore = GetEmploymentScore(customer.EmploymentType);
            int ageScore = GetAgeScore(customer.Age);

            decimal foir = customer.MonthlyIncome == 0
                ? 0
                : Math.Round(
                    customer.MonthlyInvestement / customer.MonthlyIncome * 100m, 2);

            int foirScore = GetFoirScore(foir);

            return new CibilScoreResponseDto
            {
                CibilReportId = data.CibilReportId,
                CustomerId = customer.CustomerId,
                PanNo = data.PanNo,
                IncomeScore = incomeScore,
                EmploymentScore = employmentScore,
                AgeScore = ageScore,
                FOIR = foir,
                FOIRScore = foirScore,
                CibilScore = data.CibilScore,
                CibilStatus = GetCibilStatus(data.CibilScore),
                CheckDate = data.CheckDate
            };
        }

        public async Task<CibilScoreResponseDto> GenerateScoreAsync(int customerId)
        {
            // Fetch existing customer
            var customer = await db.Customers.FirstOrDefaultAsync(c => c.CustomerId == customerId);

            if (customer == null)
                throw new KeyNotFoundException("Customer not found.");

            // Calculate individual scores
            int incomeScore = GetIncomeScore(customer.MonthlyIncome);
            int employmentScore =GetEmploymentScore(customer.EmploymentType);
            int ageScore = GetAgeScore(customer.Age);
            decimal foir = customer.MonthlyIncome == 0
                ? 0
                : Math.Round(customer.MonthlyInvestement /customer.MonthlyIncome * 100m, 2);
            int foirScore = GetFoirScore(foir);

            // Calculate final score
            int finalScore =incomeScore + employmentScore + ageScore + foirScore;
            string status = GetCibilStatus(finalScore);

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
            if (income < 25000)
            {
                return 100;
            }
            else if (income >= 25000 && income < 50000)
            {
                return 200;
            }
            else if (income >= 50000 && income <= 100000)
            {
                return 300;
            }
            else
            {
                return 400;
            }
        }
        private static int GetEmploymentScore(string employment)
        {
            if (employment.Trim().ToLower() == "government")
            {
                return 200;
            }
            else if (employment.Trim().ToLower() == "private")
            {
                return 150;
            }
            else if (employment.Trim().ToLower() == "self" ||employment.Trim().ToLower() == "self-employed")
            {
                return 100;
            }
            else
            {
                return 0;
            }
        }

        private static int GetAgeScore(int age)
        {
            if (age >= 21 && age <= 24)
            {
                return 50;
            }
            else if (age >= 25 && age <= 45)
            {
                return 150;
            }
            else if (age >= 46 && age <= 60)
            {
                return 100;
            }
            else
            {
                return 0;
            }
        }

        private static int GetFoirScore(decimal foir)
        {
            if (foir < 30)
            {
                return 250;
            }
            else if (foir >= 30 && foir < 50)
            {
                return 150;
            }
            else if (foir >= 50 && foir < 60)
            {
                return 75;
            }
            else
            {
                return 0;
            }
        }

        private static string GetCibilStatus(int score)
        {
            if (score >= 900)
            {
                return "Excellent";
            }
            else if (score >= 800)
            {
                return "Very Good";
            }
            else if (score >= 750)
            {
                return "Good";
            }
            else if (score >= 700)
            {
                return "Average";
            }
            else if (score >= 650)
            {
                return "Risky";
            }
            else
            {
                return "Reject";
            }
        }
    }
}
