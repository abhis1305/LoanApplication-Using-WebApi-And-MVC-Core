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
    public class PaymentService : IPaymentService
    {
        private readonly AppDBContext db;
        private readonly IMapper mapper;
        public PaymentService(AppDBContext db,IMapper mapper)
        {
            this.db = db;
            this.mapper = mapper;
        }


        public async Task<LoanPaymentDto> PayEmi(PayEmiDto dto, int userId)
        {
            await using var transaction = await db.Database.BeginTransactionAsync();
            var emi = await db.EmiSchedules.Include(x => x.LoanAccount).FirstOrDefaultAsync(x => x.EmiScheduleId == dto.EmiScheduleId);

            if (emi == null)
            {
                throw new KeyNotFoundException("EMI schedule not found.");
            }

            if (emi.LoanAccount == null || emi.LoanAccount.CustomerId == null)
            {
                throw new InvalidOperationException("Loan account customer not found.");
            }

            var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == userId);


            if (user == null || user.CustomerId != emi.LoanAccount.CustomerId)
            {
                throw new UnauthorizedAccessException("This EMI does not belong to the current user.");
            }

            if (string.Equals(emi.PaymentStatus, "Paid", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("This EMI has already been paid.");
            }

            var payment = new LoanPayment
            {
                LoanAccountId = emi.LoanAccountId,
                PaymentName = "EMI Payment",
                PaymentDate = DateTime.UtcNow,
                PaidAmount = emi.Emi,
                PaymentMode = dto.PaymentMode,
                TransactionReference = $"SIM-EMI-{Guid.NewGuid():N}",
                PrincipalPaid = emi.PrincipalAmount,
                InterestPaid = emi.InterestAmount,
                PenaltyPaid = 0,
                PaymentStatus = "Success"
            };

            emi.PaymentStatus = "Paid";
            emi.PaidDate = DateTime.UtcNow;

            await db.LoanPayments.AddAsync(payment);
            await db.SaveChangesAsync();
            await transaction.CommitAsync();

            return mapper.Map<LoanPaymentDto>(payment);
        }



        public async Task<LoanPaymentDto> PayPenalty(int penaltyId, int userId)
        {
            await using var transaction = await db.Database.BeginTransactionAsync();

            var user = await db.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null || user.CustomerId == null)
                throw new UnauthorizedAccessException("Customer profile not found.");

            var penalty = await db.PenaltyCharges
                .Include(p => p.LoanAccount)
                .FirstOrDefaultAsync(p => p.PenaltyId == penaltyId);

            if (penalty == null ||
                penalty.LoanAccount == null ||
                penalty.LoanAccount.CustomerId != user.CustomerId)
            {
                throw new KeyNotFoundException("Penalty not found.");
            }

            if (!string.Equals(penalty.Status, "Pending",
                StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "Only pending penalties can be paid.");
            }

            var payment = new LoanPayment
            {
                LoanAccountId = penalty.LoanAccountId,
                PaymentName = "Penalty Payment",
                PaymentDate = DateTime.UtcNow,
                PaidAmount = penalty.PenaltyAmount,
                PaymentMode = "UPI",
                TransactionReference = $"SIM-PEN-{Guid.NewGuid():N}",
                PrincipalPaid = 0,
                InterestPaid = 0,
                PenaltyPaid = penalty.PenaltyAmount,
                PaymentStatus = "Success"
            };

            penalty.Status = "Paid";

            await db.LoanPayments.AddAsync(payment);
            await db.SaveChangesAsync();
            await transaction.CommitAsync();

            return mapper.Map<LoanPaymentDto>(payment);
        }
        public async Task<List<LoanPaymentDto>> GetMyPayments(int userId)
        {
            var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null || user.CustomerId == null)
            { 
                throw new UnauthorizedAccessException("Customer profile not found.");
            }

            var payments = await db.LoanPayments.AsNoTracking().Where(p => p.LoanAccount.CustomerId == user.CustomerId.Value)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            return mapper.Map<List<LoanPaymentDto>>(payments);
        }

        

        
    }
}
