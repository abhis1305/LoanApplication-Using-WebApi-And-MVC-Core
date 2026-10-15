using System;
using System.Collections.Generic;
using System.Text;

namespace LoanApplication.Application.DTO
{
    public class CibilRangeRowDTO
    {
        public string Range { get; set; } = string.Empty;
        public int Applicants { get; set; }
        public int Approved { get; set; }
        public double ApprovalPercent { get; set; }
    }
}
