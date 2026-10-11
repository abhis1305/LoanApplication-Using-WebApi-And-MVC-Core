
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LoanApplication.Application.DTO
{
    public class AddMoneyDTO
    {
        [Required]
        public int CustomerId { get; set; }

        [Range(0.01, 1000000)]
        public decimal Amount { get; set; }

        public string? Description { get; set; }
    }

    public class WalletPaymentDTO
    {
        [Required]
        public int CustomerId { get; set; }

        [Range(0.01, 1000000)]
        public decimal Amount { get; set; }

        public string? Description { get; set; }
    }

    public class WalletResponseDTO
    {
        public int WalletId { get; set; }
        public int CustomerId { get; set; }
        public decimal Balance { get; set; }
        public string Message { get; set; } = "";
    }

    public class WalletTransactionDTO
    {
        public int TransactionId { get; set; }
        public int WalletId { get; set; }
        public decimal Amount { get; set; }
        public string TransactionType { get; set; } = "";
        public string Status { get; set; } = "";
        public string? Description { get; set; }
        public string? ReferenceId { get; set; }
        public DateTime TransactionDate { get; set; }
    }

    public class WalletHistoryResponseDTO
    {
        public int CustomerId { get; set; }
        public decimal Balance { get; set; }
        public List<WalletTransactionDTO> Transactions { get; set; }
            = new List<WalletTransactionDTO>();
    }
}
