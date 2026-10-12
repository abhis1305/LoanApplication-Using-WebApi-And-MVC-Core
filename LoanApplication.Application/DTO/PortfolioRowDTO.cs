using System;
using System.Collections.Generic;
using System.Text;

namespace LoanApplication.Application.DTO
{
    public class PortfolioRowDTO
    {
        public string Product { get; set; } = string.Empty;
        public int ActiveLoans { get; set; }
        public decimal Outstanding { get; set; }
        public double Percent { get; set; }
    }
}
