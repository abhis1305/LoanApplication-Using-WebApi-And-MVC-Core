using System;
using System.Collections.Generic;
using System.Text;

namespace LoanApplication.Application.DTO
{
    public class RecentPaymentDTO
    {
        public int PaymentId { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentName { get; set; } = string.Empty;
        public decimal PaidAmount { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
    }
}
