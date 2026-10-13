using System;
using System.Collections.Generic;
using System.Text;

namespace LoanApplication.Application.DTO
{
    public class LoanPaymentDto
    {
        public int PaymentId { get; set; }
        public int LoanAccountId { get; set; }
        public string PaymentName { get; set; } = string.Empty;
        public DateTime PaymentDate { get; set; }
        public decimal PaidAmount { get; set; }
        public string PaymentMode { get; set; } = string.Empty;
        public string TransactionReference { get; set; } = string.Empty;
        public decimal PrincipalPaid { get; set; }
        public decimal InterestPaid { get; set; }
        public decimal PenaltyPaid { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
    }
}
