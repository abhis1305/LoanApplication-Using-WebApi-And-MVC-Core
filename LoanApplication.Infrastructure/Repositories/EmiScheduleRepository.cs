using LoanApplication.Application.Interfcae;
using LoanApplication.Domain.Entities;
using LoanApplication.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace LoanApplication.Infrastructure.Repositories
{
    public class EmiScheduleRepository : IEmiScheduleRepository
    {
        AppDBContext db;
        public EmiScheduleRepository(AppDBContext db)
        {
            this.db = db;
        }
        public async Task<EmiSchedule> AddEmiSchedule(EmiSchedule emiSchedule)
        {
            db.EmiSchedules.Add(emiSchedule);
            await db.SaveChangesAsync();
            return emiSchedule;
        }

        public async Task AddEmiSchedule(int loanAccountId)
        {
            var loan = await db.LoanAccounts.FindAsync(loanAccountId);
            if (loan == null)
            {
                throw new Exception("Loan not found");
            }

            decimal balance = loan.LoanAmount;
            decimal rate = loan.InterestRate / 1200m;

            for (int i = 1; i <= loan.TenureMonths; i++)
            {
                decimal interest = Math.Round(balance * rate, 2);
                decimal principal = loan.EmiAmount - interest;

                if (i == loan.TenureMonths)
                {
                    principal = balance;
                }

                var emi = new EmiSchedule
                {
                    LoanAccountId = loanAccountId,
                    InstallmentNo = i,
                    DueDate = loan.DisbursementDate.AddMonths(i),
                    Emi = Math.Round(principal + interest, 2),
                    PrincipalAmount = principal,
                    InterestAmount = interest,
                    OpeningBalance = balance,
                    ClosingBalance = balance - principal,
                    PaymentStatus = "Pending"
                };

                db.EmiSchedules.Add(emi);
                balance -= principal;
            }

            await db.SaveChangesAsync();
        }

        public async Task<EmiSchedule> GetEmiScheduleById(int emiScheduleId)
        {
            return await db.EmiSchedules.Where(x => x.EmiScheduleId == emiScheduleId).FirstOrDefaultAsync();
        }

        public async Task<List<EmiSchedule>> GetEmiSchedulesByLoanAccountId(int loanAccountId)
        {
            return await db.EmiSchedules.Where(x => x.LoanAccountId == loanAccountId).ToListAsync();
        }

        public async Task<EmiSchedule> UpdateEmiSchedule(EmiSchedule emiSchedule)
        {
            db.EmiSchedules.Update(emiSchedule);
            await db.SaveChangesAsync();
            return emiSchedule;
        }
    }
}
