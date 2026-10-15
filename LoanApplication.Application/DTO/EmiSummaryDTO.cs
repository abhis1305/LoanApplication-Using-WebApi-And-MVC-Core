using System;
using System.Collections.Generic;
using System.Text;

namespace LoanApplication.Application.DTO
{
    public class EmiSummaryDTO
    {
        public int TotalEmis { get; set; }
        public int PaidCount { get; set; }
        public decimal PaidAmount { get; set; }
        public int UpcomingCount { get; set; }
        public decimal UpcomingAmount { get; set; }
        public int RemainingCount { get; set; }
        public decimal RemainingAmount { get; set; }
        public DateTime? NextEmiDueDate { get; set; }
        public decimal NextEmiAmount { get; set; }
    }
}
