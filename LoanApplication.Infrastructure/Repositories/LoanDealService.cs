using AutoMapper;
using LoanApplication.Application.DTO;
using LoanApplication.Application.Interface;
using LoanApplication.Domain.Entities;
using LoanApplication.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace LoanApplication.Infrastructure.Repositories
{
    public class LoanDealService : ILoanDealService
    {
        private readonly AppDBContext db;
        private readonly IMapper mapper;

        public LoanDealService(AppDBContext db, IMapper mapper)
        {
            this.db = db;
            this.mapper = mapper;
        }

        private static int GetTenureMonths(int years) =>
           years switch
           {
               3 => 36,
               5 => 60,
               6 => 72,
               10 => 120,
               _ => throw new ArgumentException(
                   "Tenure must be 3, 5, 6 or 10 years.")
           };

        private static double GetInterestRate(int years) =>
            years switch
            {
                3 => 11.5,
                5 or 6 or 10 => 11.0,
                
            };

        private static decimal CalculateEmi(
            decimal amount, double annualRate, int months)
        {
            double r = annualRate / 12 / 100;
            double factor = Math.Pow(1 + r, months);

            double emi = (double)amount * r * factor / (factor - 1);

            return Math.Round((decimal)emi, 2);
        }

        private async Task<decimal> GetEligibleAmountAsync(int customerId)
        {
            var eligibility = await db.EligibilityResults
                .Where(x => x.CustomerId == customerId)
                .OrderByDescending(x => x.EligibilityId)
                .FirstOrDefaultAsync();

            if (eligibility == null || eligibility.IsEligible != "Yes")
                throw new InvalidOperationException(
                    "Customer is not eligible to apply for a loan.");

            return eligibility.LoanAmount;
        }

        public async Task<LoanCalculationDto> CalculateLoanAsync(
            int customerId, decimal loanAmount, int tenureYears)
        {
            decimal eligibleAmount = await GetEligibleAmountAsync(customerId);

            if (loanAmount <= 0 || loanAmount > eligibleAmount)
                throw new ArgumentException(
                    $"Loan amount must be greater than zero and not exceed ₹{eligibleAmount}.");

            int months = GetTenureMonths(tenureYears);
            double rate = GetInterestRate(tenureYears);

            return new LoanCalculationDto
            {
                EligibleAmount = eligibleAmount,
                TenureYears = tenureYears,
                TenureMonths = months,
                InterestRate = rate,
                EmiAmount = CalculateEmi(loanAmount, rate, months)
            };
        }

        public async Task<LoanDealDto> ApplyLoanAsync(CreateLoanDealDto dto)
        {
            var calculation = await CalculateLoanAsync(
                dto.CustomerId, dto.LoanAmount, dto.TenureYears);

            var deal = mapper.Map<LoanDeal>(dto);

            deal.InterestRate = calculation.InterestRate;
            deal.TenureMonths = calculation.TenureMonths;
            deal.EmiAmount = calculation.EmiAmount;
            deal.ApprovedAmount = 0;
            deal.CurrentStatus = "Pending";
            deal.RejectionReason = null;
            deal.AppliedDate = DateTime.Now;

            db.LoanDeals.Add(deal);
            await db.SaveChangesAsync();

            return mapper.Map<LoanDealDto>(deal);
        }


        //public async Task<List<LoanDealDto>> GetPendingLoansAsync()
        //{
        //    var pendingLoans = await db.LoanDeals
        //        .Where(x => x.CurrentStatus == "Pending")
        //        .OrderBy(x => x.AppliedDate)
        //        .ToListAsync();

        //    return mapper.Map<List<LoanDealDto>>(pendingLoans);
        //}

        public async Task<List<LoanDealDto>> GetPendingLoansAsync()
        {
            var loans = await db.LoanDeals
                .Include(x => x.Customer)
                .Where(x => x.CurrentStatus == "Pending")
                .OrderBy(x => x.AppliedDate)
                .ToListAsync();

            return mapper.Map<List<LoanDealDto>>(loans);
        }

        public async Task<LoanDealDto?> GetLoanByIdAsync(int dealId)
        {
            var loan = await db.LoanDeals
                .Include(x => x.Customer)
                .FirstOrDefaultAsync(x => x.DealId == dealId);

            if (loan == null)
                return null;

            var result = mapper.Map<LoanDealDto>(loan);

            result.CustomerName = loan.Customer!.FirstName + " " + loan.Customer.LastName;
            result.Email = loan.Customer.Email;
            result.Mobile = loan.Customer.MobileNo;
            result.Pan = loan.Customer.Pan;
            result.Salary = loan.Customer.MonthlyIncome;

            var cibil = await db.CibilReports
                .Where(x => x.CustomerId == loan.CustomerId)
                .OrderByDescending(x => x.CheckDate)
                .FirstOrDefaultAsync();

            if (cibil != null)
            {
                result.CibilScore = cibil.CibilScore;
            }

            return result;
        }



        public async Task<bool> ApproveLoanAsync(int dealId, LoanDealReviewDto dto)
        {
            var deal = await db.LoanDeals.FirstOrDefaultAsync(x => x.DealId == dealId);

            if (deal == null)
                throw new KeyNotFoundException("Loan deal not found.");

            if (deal.CurrentStatus != "Pending")
                throw new InvalidOperationException(
                    "Only pending loans can be approved.");

            if (dto.ApprovedAmount <= 0 ||
                dto.ApprovedAmount > deal.LoanAmount)
                throw new ArgumentException(
                    "Approved amount must be greater than zero and cannot exceed the requested loan amount.");

            deal.CurrentStatus = "Approved";
            deal.ApprovedAmount = dto.ApprovedAmount;
            deal.RejectionReason = null;

            db.DealReviews.Add(new DealReview
            {
                DealId = deal.DealId,
                OfficerId = dto.OfficerId,
                Status = "Approved"
            });

            await db.SaveChangesAsync();
            return true;
        }


        public async Task<bool> RejectLoanAsync(int dealId, LoanDealReviewDto dto)
        {
            var deal = await db.LoanDeals
                .FirstOrDefaultAsync(x => x.DealId == dealId);

            if (deal == null)
                throw new KeyNotFoundException("Loan deal not found.");

            if (deal.CurrentStatus != "Pending")
                throw new InvalidOperationException(
                    "Only pending loans can be rejected.");

            if (string.IsNullOrWhiteSpace(dto.RejectionReason))
                throw new ArgumentException(
                    "Rejection reason is required.");

            deal.CurrentStatus = "Rejected";
            deal.RejectionReason = dto.RejectionReason.Trim();
            deal.ApprovedAmount = 0;

            db.DealReviews.Add(new DealReview
            {
                DealId = deal.DealId,
                OfficerId = dto.OfficerId,
                Status = "Rejected"
            });

            await db.SaveChangesAsync();
            return true;
        }
    }
}


    

