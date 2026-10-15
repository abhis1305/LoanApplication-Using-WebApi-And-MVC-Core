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
    public class ForeClosureService : IForeClosureService
    {
        private readonly AppDBContext db;

        public ForeClosureService(AppDBContext db)
        {
            this.db = db;
        }

    

        public async Task<ForeClosureRequest> CreateRequest(CreateForeClosureRequestDto dto, int userId)
        {
            var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null || user.CustomerId == null)
               {
                throw new UnauthorizedAccessException("Customer profile not found.");   
               }

            var loan = await db.LoanAccounts.FirstOrDefaultAsync(l =>l.LoanAccountId == dto.LoanAccountId &&
                    l.CustomerId == user.CustomerId.Value);

            if (loan == null)
            {
                throw new KeyNotFoundException("Loan account not found for this customer.");
            }

            if (!string.Equals(loan.LoanStatus, "Active",StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Foreclosure is only available for active loans.");
            }

            var isFull = string.Equals(dto.ForeClosureType, "Full",StringComparison.OrdinalIgnoreCase);

            var isPartial = string.Equals(dto.ForeClosureType, "Partial",StringComparison.OrdinalIgnoreCase);

            if (!isFull && !isPartial)
            {
                throw new ArgumentException("ForeClosureType must be Full or Partial.");
            }

            if (dto.ExpectedClosureDate.Date < DateTime.UtcNow.Date)
            {
                throw new ArgumentException("Expected closure date cannot be in the past.");
            }

            if (isPartial && (dto.PartialAmount <= 0 || dto.PartialAmount >= loan.OutstandingPrincipal))
            {
                throw new ArgumentException("Partial amount must be positive and less than outstanding principal.");
            }

            var hasPendingRequest = await db.ForeClosureRequests.AnyAsync(r => r.LoanAccountId == loan.LoanAccountId &&
                    r.Status == "Pending");

            if (hasPendingRequest)
            {
                throw new InvalidOperationException("A pending foreclosure request already exists for this loan.");
            }

            var request = new ForeClosureRequest
            {
                LoanAccountId = loan.LoanAccountId,
                ForeClosureType = isFull ? "Full" : "Partial",
                ForeClosureAmount = isFull ? loan.OutstandingPrincipal : dto.PartialAmount,
                PartialAmount = isPartial ? dto.PartialAmount : 0,
                RequestedDate = DateTime.UtcNow,
                ExpectedClosureDate = dto.ExpectedClosureDate,
                Reason = dto.Reason.Trim(),
                Status = "Pending",
                IsPaid = false
            };

            await db.ForeClosureRequests.AddAsync(request);
            await db.SaveChangesAsync();

            return request;
        }

 

        public async Task<List<ForeClosureRequest>> GetMyRequests(int userId)
        {
            var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null || user.CustomerId == null)
            {
                throw new UnauthorizedAccessException("Customer profile not found.");
            }

            return await db.ForeClosureRequests.AsNoTracking().Where(r => r.LoanAccount.CustomerId == user.CustomerId.Value)
                .OrderByDescending(r => r.RequestedDate).ToListAsync();
        }


        public async Task<List<ForeClosureRequest>> GetAllRequests()
        {
            return await db.ForeClosureRequests
                .AsNoTracking()
                .OrderByDescending(r => r.RequestedDate)
                .ToListAsync();
        }

        public async Task<ForeClosureRequest?> GetRequestById(int requestId)
        {
            return await db.ForeClosureRequests
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.RequestId == requestId);
        }

        public async Task<ForeClosureRequest> ApproveRequest(int requestId)
        {
            var request = await db.ForeClosureRequests
                .FirstOrDefaultAsync(r => r.RequestId == requestId);

            if (request == null)
            {
                throw new KeyNotFoundException("Foreclosure request not found.");
            }

            if (!string.Equals(request.Status, "Pending",
                StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "Only pending requests can be approved.");
            }

            request.Status = "Approved";
            request.ApprovedDate = DateTime.UtcNow;

            await db.SaveChangesAsync();

            return request;
        }

        public async Task<ForeClosureRequest> RejectRequest(int requestId)
        {
            var request = await db.ForeClosureRequests
                .FirstOrDefaultAsync(r => r.RequestId == requestId);

            if (request == null)
            {
                throw new KeyNotFoundException("Foreclosure request not found.");
            }

            if (!string.Equals(request.Status, "Pending",
                StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "Only pending requests can be rejected.");
            }

            request.Status = "Rejected";

            await db.SaveChangesAsync();

            return request;
        }
    }
}
