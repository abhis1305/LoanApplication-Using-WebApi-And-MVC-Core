using System;
using System.Collections.Generic;
using System.Text;

namespace LoanApplication.Application.DTO
{
    public class PrepaymentRowDTO
    {
        public string Type { get; set; } = string.Empty;
        public int Accounts { get; set; }
        public decimal Amount { get; set; }
    }
}
