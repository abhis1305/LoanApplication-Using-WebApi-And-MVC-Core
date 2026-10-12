using System;
using System.Collections.Generic;
using System.Text;

namespace LoanApplication.Application.DTO
{
    public class TATRowDTO
    {
        public string Stage { get; set; } = string.Empty;
        public int Count { get; set; }
        public double AvgDays { get; set; }
        public double MinDays { get; set; }
        public double MaxDays { get; set; }
    }
}
