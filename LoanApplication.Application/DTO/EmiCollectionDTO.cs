using System;
using System.Collections.Generic;
using System.Text;

namespace LoanApplication.Application.DTO
{
    public class EmiCollectionDTO
    {
        public int BilledCount { get; set; }
        public decimal BilledAmount { get; set; }
        public int CollectedCount { get; set; }
        public decimal CollectedAmount { get; set; }
        public int PendingCount { get; set; }
        public decimal PendingAmount { get; set; }
        public double EfficiencyPercent { get; set; }
    }
}
