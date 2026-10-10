
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoanApplication.Domain.Entities
{
    public class WalletTransaction
    {
        [Key]
        public int TransactionId { get; set; }

        public int WalletId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        // Credit or Debit
        [Required]
        public string TransactionType { get; set; } = "";

        // Success
        [Required]
        public string Status { get; set; } = "";

        public string? Description { get; set; }

        public string? ReferenceId { get; set; }

        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

        public Wallet? Wallet { get; set; }
    }
}
