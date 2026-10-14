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
    public class DisbursementService : IDisbursementService
    {
        private readonly AppDBContext db;

        public DisbursementService(AppDBContext db)
        {
            this.db = db;
        }

        public async Task<List<Disbursement>> GetAllAsync()
        {
            return await db.Disbursements
                .Include(x => x.LoanDeal)
                .ToListAsync();
        }

        public async Task<Disbursement?> GetByIdAsync(int id)
        {
            return await db.Disbursements
                .Include(x => x.LoanDeal)
                .FirstOrDefaultAsync(
                    x => x.DisbursementId == id);
        }

        public async Task<Disbursement> CreateAsync( CreateDisbursementDto dto)
        {
            var deal = await db.LoanDeals
                .FirstOrDefaultAsync(
                    x => x.DealId == dto.DealId);

            if (deal == null)
                throw new InvalidOperationException(
                    "Loan deal not found.");

            var existing = await db.Disbursements
                .AnyAsync(x => x.DealId == dto.DealId);

            if (existing)
                throw new InvalidOperationException(
                    "Disbursement already exists for this deal.");

            var disbursement = new Disbursement
            {
                DealId = dto.DealId,
                DisburseAmount = dto.DisburseAmount,
                BankPartner = dto.BankPartner,
                DisbursementDate = dto.DisbursementDate,
                Status = "Disbursed"
            };

            db.Disbursements.Add(disbursement);
            await db.SaveChangesAsync();

            return disbursement;
        }
    }
    

    
}
