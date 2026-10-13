using System;
using System.Collections.Generic;
using System.Text;

namespace LoanApplication.Application.DTO
{
    public class LoanDealDto
    {
        public int DealId { get; set; }

        public int CustomerId { get; set; }

        public string LoanType { get; set; } = string.Empty;

        public decimal LoanAmount { get; set; }

        public double InterestRate { get; set; }

        public int TenureMonths { get; set; }

        public decimal EmiAmount { get; set; }

        public decimal ApprovedAmount { get; set; }

        public string CurrentStatus { get; set; } = string.Empty;

        public DateTime AppliedDate { get; set; }
    }
}
