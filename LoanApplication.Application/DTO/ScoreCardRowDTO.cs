using System;
using System.Collections.Generic;
using System.Text;

namespace LoanApplication.Application.DTO
{
    public class ScoreCardRowDTO
    {
        public string Status { get; set; } = string.Empty;     // Excellent, Very Good
        public string Decision { get; set; } = string.Empty;   // Auto Approved / Manual Review / Auto Reject
        public int Applicants { get; set; }
        public int Approved { get; set; }
        public int Rejected { get; set; }
        public int Pending { get; set; }
    }
}
