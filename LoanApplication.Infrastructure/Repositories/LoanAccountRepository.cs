using LoanApplication.Domain.Entities;
using LoanApplication.Application.Interfcae;
using LoanApplication.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace LoanApplication.Infrastructure.Repositories
{
    public class LoanAccountRepository : ILoanAccountRepository
    {
        AppDBContext db;
        IEmiScheduleRepository emiRepo;
        public LoanAccountRepository(AppDBContext db, IEmiScheduleRepository emi)
        {
            this.db = db;
            this.emiRepo = emi;
        }

        public async Task<LoanAccount> AddLoanAccount(LoanAccount loanAccount)
        {
            db.LoanAccounts.Add(loanAccount);
            await db.SaveChangesAsync();
            return loanAccount;
        }

        public async Task<LoanAccount> CreateLoanAccount(int dealId)
        {
            var deal = await db.LoanDeals.FindAsync(dealId);
            if (deal == null || deal.CurrentStatus != "Approved")
            {
                throw new Exception("Loan not approved");
            }

            var disbursement = await db.Disbursements
                .FirstOrDefaultAsync(d => d.DealId == dealId && d.Status == "Disbursed");

            if (disbursement == null)
            {
                throw new Exception("Loan not disbursed");
            }

            if (await GetByLoanDealId(dealId) != null)
            {
                throw new Exception("Account already exists");
            }

            int months = deal.TenureMonths;
            decimal rate;

            if (months == 36)
            {
                rate = 11.5m;
            }
            else if (months == 60 || months == 72 || months == 120)
            {
                rate = 11m;
            }
            else
            {
                throw new Exception("Invalid tenure");
            }

            decimal amount = deal.ApprovedAmount;
            decimal r = rate / 1200m;

            decimal emi = amount * r *
                (decimal)Math.Pow((double)(1 + r), months) /
                ((decimal)Math.Pow((double)(1 + r), months) - 1);

            var account = new LoanAccount
            {
                DealId = dealId,
                CustomerId = deal.CustomerId,
                LoanAccountNo = "LA-" + dealId,
                LoanAmount = amount,
                OutstandingPrincipal = amount,
                LoanStatus = "Active",
                InterestRate = rate,
                TenureMonths = months,
                EmiAmount = Math.Round(emi, 2),
                DisbursementDate = disbursement.DisbursementDate,
                TotalPaidAmount = 0,
                CreatedAt = DateTime.Now
            };
            var savedAccount = await AddLoanAccount(account);
            await emiRepo.AddEmiSchedule(savedAccount.LoanAccountId);
            return savedAccount;
        }

        public async Task<LoanAccount> GetByLoanDealId(int loanDealId)
        {
            return await db.LoanAccounts.FirstOrDefaultAsync(l => l.DealId == loanDealId);
        }

        public async Task<LoanAccount> GetLoanAccountById(int loanAccountId)
        {
            return await db.LoanAccounts.FirstOrDefaultAsync(l => l.LoanAccountId == loanAccountId);
        }

        public async Task<LoanAccount> UpdateLoanAccount(LoanAccount loanAccount)
        {
            db.LoanAccounts.Update(loanAccount);
            await db.SaveChangesAsync();
            return loanAccount;
        }

    }
}
