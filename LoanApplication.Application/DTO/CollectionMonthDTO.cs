using System;
using System.Collections.Generic;
using System.Text;

namespace LoanApplication.Application.DTO
{
    public class CollectionMonthDTO
    {
        public string Month { get; set; } = string.Empty;       // e.g. 2026-10
        public decimal BilledAmount { get; set; }
        public decimal CollectedAmount { get; set; }
        public double EfficiencyPercent { get; set; }
    }
}
