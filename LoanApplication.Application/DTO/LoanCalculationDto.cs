using System;
using System.Collections.Generic;
using System.Text;

namespace LoanApplication.Application.DTO
{
public class LoanCalculationDto
    {
        public decimal EligibleAmount { get; set; }
        public int TenureYears { get; set; }
        public int TenureMonths { get; set; }
        public double InterestRate { get; set; }
        public decimal EmiAmount { get; set; }
    }
}
