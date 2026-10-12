using AutoMapper;
using LoanApplication.Application.DTO;
using LoanApplication.Application.Interfcae;
using LoanApplication.Domain.Entities;
using LoanApplication.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace LoanApplication.Infrastructure.Repositories
{
    public class PenaltyService : IPenaltyService
    {
        private readonly AppDBContext db;
        private readonly IMapper mapper;
        public PenaltyService(AppDBContext db, IMapper mapper)
        {
            this.db = db;
            this.mapper = mapper;
        }


        public async Task<List<PenaltyChargeDto>> GetAllPenalties()
        {
            var penalties = await db.PenaltyCharges.AsNoTracking().ToListAsync();
            var data = mapper.Map<List<PenaltyChargeDto>>(penalties);
            return data;
        }


        public async Task ApplyPenalty(CreatePenaltyChargeDto dto)
        {
            // 1. Find the EMI and validate its loan account.
            var emi = await db.EmiSchedules
                .FirstOrDefaultAsync(e =>e.EmiScheduleId == dto.EmiScheduleId && e.LoanAccountId == dto.LoanAccountId);

            if (emi == null)
            {
                throw new KeyNotFoundException("EMI schedule not found for the specified loan account.");
            }

            // 2. Do not apply penalties to paid EMIs.
            if (string.Equals(emi.PaymentStatus, "Paid",StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Penalty cannot be applied to a paid EMI.");
            }

            // 3. Calculate overdue days.
            var today = DateTime.UtcNow.Date;
            var dueDate = emi.DueDate.Date;

            if (today <= dueDate){
                throw new InvalidOperationException("Penalty cannot be applied before the EMI becomes overdue.");
            }

            var delayDays = (today - dueDate).Days;

            // 4. Prevent duplicate penalties for the same EMI and type.
            var alreadyExists = await db.PenaltyCharges.AnyAsync(p =>
                p.EmiScheduleId == dto.EmiScheduleId &&
                p.LoanAccountId == dto.LoanAccountId &&
                p.PenaltyType == dto.PenaltyType &&
                p.Status != "Waived");

            if (alreadyExists)
            {
                throw new InvalidOperationException("A penalty of this type already exists for this EMI.");
            }

            // 5. Example policy: ₹100 per overdue day.
            const decimal chargePerDay = 100m;
            var penaltyAmount = delayDays * chargePerDay;

            // 6. Create and save the penalty.
            var penalty = new PenaltyCharge
            {
                LoanAccountId = emi.LoanAccountId,
                EmiScheduleId = emi.EmiScheduleId,
                PenaltyType = dto.PenaltyType,
                PenaltyAmount = penaltyAmount,
                DelayDays = delayDays,
                AppliedDate = today,
                Status = "Pending"
            };

            await db.PenaltyCharges.AddAsync(penalty);
            await db.SaveChangesAsync();
        }


        public async Task<List<PenaltyChargeDto>> GetPenaltiesByLoanAccountId(int loanAccountId)
        {
            var penalty = await db.PenaltyCharges.AsNoTracking().Where(x => x.LoanAccountId == loanAccountId).ToListAsync();
            var data = mapper.Map<List<PenaltyChargeDto>>(penalty);
            return data;
        }

        public async Task<PenaltyChargeDto?> GetPenaltyById(int penaltyId)
        {
            var penalty = await db.PenaltyCharges.AsNoTracking().FirstOrDefaultAsync(x => x.PenaltyId == penaltyId);

            if (penalty == null)
            {
                return null;
            }
            var data = mapper.Map<PenaltyChargeDto>(penalty);
            return data;
        }

        public async Task WaivePenalty(int penaltyId, WaivePenaltyDto dto)
        {
            var penalty = await db.PenaltyCharges.FirstOrDefaultAsync(x => x.PenaltyId == penaltyId);
            if(penalty == null)
            {
                throw new KeyNotFoundException("Penalty not found!");
            }
            if (string.Equals(penalty.Status, "Paid", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("A Paid penalty cannot be Waived!");
            }
            if (string.Equals(penalty.Status, "Waived", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("This penalty has already been waived.");
            }

            penalty.Status = "Waived";
            penalty.WaiverReason = dto.Reason;
            penalty.WaivedDate = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }
    }
}
