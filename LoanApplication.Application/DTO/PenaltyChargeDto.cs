using System;
using System.Collections.Generic;
using System.Text;

namespace LoanApplication.Application.DTO
{
    public class PenaltyChargeDto
    {
        public int PenaltyId { get; set; }
        public int LoanAccountId { get; set; }
        public int EmiScheduleId { get; set; }
        public string PenaltyType { get; set; } 
        public decimal PenaltyAmount { get; set; }
        public int DelayDays { get; set; }
        public DateTime AppliedDate { get; set; }
        public string Status { get; set; } 
    }
}
