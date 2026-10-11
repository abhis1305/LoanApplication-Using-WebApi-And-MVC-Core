using System;
using System.Collections.Generic;
using System.Text;

namespace LoanApplication.Application.DTO
{
    public class ActiveLoanDTO
    {
        public int LoanAccountId { get; set; }
        public string LoanAccountNo { get; set; } = string.Empty;
        public string LoanType { get; set; } = string.Empty;
        public string LoanStatus { get; set; } = string.Empty;
        public decimal LoanAmount { get; set; }
        public DateTime? DisbursementDate { get; set; }
        public int TenureMonths { get; set; }
        public decimal InterestRate { get; set; }
        public decimal EmiAmount { get; set; }
        public int EmisPaid { get; set; }
        public int EmisRemaining { get; set; }
        public decimal OutstandingAmount { get; set; }
        public double PercentPaid { get; set; }
    }
}
