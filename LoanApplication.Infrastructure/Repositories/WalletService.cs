
using LoanApplication.Application.DTO;
using LoanApplication.Application.Interface;
using LoanApplication.Domain.Entities;
using LoanApplication.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LoanApplication.Infrastructure.Repositories
{
    public class WalletService : IWalletService
    {
        AppDBContext db;

        public WalletService(AppDBContext db)
        {
            this.db = db;
        }

        public async Task<WalletResponseDTO> AddMoneyAsync(AddMoneyDTO dto)
        {
            if (dto.Amount <= 0)
                throw new ArgumentException("Amount must be greater than zero.");

            using var transaction = await db.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.Serializable);

            var customer = await db.Customers
                .FirstOrDefaultAsync(x => x.CustomerId == dto.CustomerId);

            if (customer == null)
                throw new KeyNotFoundException("Customer not found.");

            var wallet = await db.Wallets
                .FirstOrDefaultAsync(x => x.CustomerId == dto.CustomerId);

            if (wallet == null)
                throw new KeyNotFoundException("Wallet not found.");

            wallet.Balance += dto.Amount;

            var walletTransaction = new WalletTransaction
            {
                WalletId = wallet.WalletId,
                Amount = dto.Amount,
                TransactionType = "Credit",
                Status = "Success",
                Description = dto.Description ?? "Test money added",
                ReferenceId = Guid.NewGuid().ToString(),
                TransactionDate = DateTime.UtcNow
            };

            db.WalletTransactions.Add(walletTransaction);

            await db.SaveChangesAsync();
            await transaction.CommitAsync();

            return new WalletResponseDTO
            {
                WalletId = wallet.WalletId,
                CustomerId = wallet.CustomerId,
                Balance = wallet.Balance,
                Message = "Test money added successfully."
            };
        }

        public async Task<WalletResponseDTO> GetBalanceAsync(int customerId)
        {
            var wallet = await db.Wallets
                .FirstOrDefaultAsync(x => x.CustomerId == customerId);

            if (wallet == null)
                throw new KeyNotFoundException("Wallet not found.");

            return new WalletResponseDTO
            {
                WalletId = wallet.WalletId,
                CustomerId = wallet.CustomerId,
                Balance = wallet.Balance,
                Message = "Wallet balance fetched successfully."
            };
        }

        public async Task<WalletResponseDTO> MakePaymentAsync(
            WalletPaymentDTO dto)
        {
            if (dto.Amount <= 0)
                throw new ArgumentException("Payment amount must be greater than zero.");

            using var transaction = await db.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.Serializable);

            var wallet = await db.Wallets
                .FirstOrDefaultAsync(x => x.CustomerId == dto.CustomerId);

            if (wallet == null)
                throw new KeyNotFoundException("Wallet not found.");

            if (wallet.Balance < dto.Amount)
                throw new InvalidOperationException("Insufficient wallet balance.");

            wallet.Balance -= dto.Amount;

            var walletTransaction = new WalletTransaction
            {
                WalletId = wallet.WalletId,
                Amount = dto.Amount,
                TransactionType = "Debit",
                Status = "Success",
                Description = dto.Description ?? "Wallet payment",
                ReferenceId = Guid.NewGuid().ToString(),
                TransactionDate = DateTime.UtcNow
            };

            db.WalletTransactions.Add(walletTransaction);

            await db.SaveChangesAsync();
            await transaction.CommitAsync();

            return new WalletResponseDTO
            {
                WalletId = wallet.WalletId,
                CustomerId = wallet.CustomerId,
                Balance = wallet.Balance,
                Message = "Payment successful."
            };
        }

        public async Task<WalletHistoryResponseDTO> GetTransactionsAsync(
            int customerId)
        {
            var wallet = await db.Wallets
                .FirstOrDefaultAsync(x => x.CustomerId == customerId);

            if (wallet == null)
                throw new KeyNotFoundException("Wallet not found.");

            var data = await db.WalletTransactions
                .Where(x => x.WalletId == wallet.WalletId)
                .OrderByDescending(x => x.TransactionDate)
                .ThenByDescending(x => x.TransactionId)
                .ToListAsync();

            var transactions = new List<WalletTransactionDTO>();

            foreach (var item in data)
            {
                transactions.Add(new WalletTransactionDTO
                {
                    TransactionId = item.TransactionId,
                    WalletId = item.WalletId,
                    Amount = item.Amount,
                    TransactionType = item.TransactionType,
                    Status = item.Status,
                    Description = item.Description,
                    ReferenceId = item.ReferenceId,
                    TransactionDate = item.TransactionDate
                });
            }

            return new WalletHistoryResponseDTO
            {
                CustomerId = customerId,
                Balance = wallet.Balance,
                Transactions = transactions
            };
        }

        public async Task<WalletResponseDTO> CreateWalletAsync(int customerId)
        {
            var customer = await db.Customers
                .FirstOrDefaultAsync(x => x.CustomerId == customerId);

            if (customer == null)
            {
                throw new KeyNotFoundException("Customer not found.");
            }

            var existingWallet = await db.Wallets
                .FirstOrDefaultAsync(x => x.CustomerId == customerId);

            if (existingWallet != null)
            {
                throw new InvalidOperationException(
                    "Wallet already exists for this customer.");
            }

            var wallet = new Wallet
            {
                CustomerId = customerId,
                Balance = 0,
                CreatedAt = DateTime.UtcNow
            };

            db.Wallets.Add(wallet);
            await db.SaveChangesAsync();

            return new WalletResponseDTO
            {
                WalletId = wallet.WalletId,
                CustomerId = wallet.CustomerId,
                Balance = wallet.Balance,
                Message = "Wallet created successfully."
            };
        }
    }
}
