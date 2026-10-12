using System;
using System.Collections.Generic;
using System.Text;

namespace LoanApplication.Application.DTO
{
    public class ProductWiseRowDTO
    {
        public string Product { get; set; } = string.Empty;   // LoanType
        public int TotalApps { get; set; }
        public int Approved { get; set; }
        public int Disbursed { get; set; }
        public double DisbursementPercent { get; set; }
    }
}
