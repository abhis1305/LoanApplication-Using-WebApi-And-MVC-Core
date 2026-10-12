using System;
using System.Collections.Generic;
using System.Text;

namespace LoanApplication.Application.DTO
{
    public class BucketRowDTO
    {
        public string Bucket { get; set; } = string.Empty;
        public int Accounts { get; set; }
        public decimal Amount { get; set; }
        public double Percent { get; set; }
    }
}
