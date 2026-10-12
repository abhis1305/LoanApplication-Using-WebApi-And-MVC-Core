using System;
using System.Collections.Generic;
using System.Text;

namespace LoanApplication.Application.DTO
{
    public class CreateLoanDealDto
    {
        public int CustomerId { get; set; }
        public string LoanType { get; set; } = string.Empty;
        public decimal LoanAmount { get; set; }

        public int TenureYears { get; set; }

        public string BankName { get; set; } = string.Empty;
        public string BankAccountNumber { get; set; } = string.Empty;
        public string IFSCCode { get; set; } = string.Empty;
        public int EmiDay { get; set; }
    }
}
