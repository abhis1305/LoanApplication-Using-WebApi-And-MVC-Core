using System;
using System.Collections.Generic;
using System.Text;

namespace LoanApplication.Application.DTO
{
   public class SanctionLetterDataDto
    {
        public string ApplicationNo { get; set; } = string.Empty;
        public DateTime SanctionDate { get; set; }

        public string ApplicantName { get; set; } = string.Empty;
        public string MobileNo { get; set; } = string.Empty;

        public string LoanType { get; set; } = string.Empty;
        public decimal LoanAmountSanctioned { get; set; }

        public double InterestRate { get; set; }
        public int LoanTenure { get; set; }
        public string TotalProcessingCharges { get; set; } = string.Empty;
        public decimal OriginationFee { get; set; }

        public int SanctionLetterValidity { get; set; }
        public decimal AmountOfEmi { get; set; }

        public string PropertyAddress { get; set; } = string.Empty;
    }
}
