
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoanApplication.Domain.Entities
{
    public class Wallet
    {
        [Key]
        public int WalletId { get; set; }

        public int CustomerId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Balance { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Customer? Customer { get; set; }

        public List<WalletTransaction> Transactions { get; set; }= new List<WalletTransaction>();
    }
}
