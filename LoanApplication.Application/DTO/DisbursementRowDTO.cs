using System;
using System.Collections.Generic;
using System.Text;

namespace LoanApplication.Application.DTO
{
    public class DisbursementRowDTO
    {
        public string Month { get; set; } = string.Empty;
        public int Loans { get; set; }
        public decimal Amount { get; set; }
    }
}
