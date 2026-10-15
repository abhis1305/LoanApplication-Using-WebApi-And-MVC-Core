using System;
using System.Collections.Generic;
using System.Text;

namespace LoanApplication.Application.DTO
{
    public class DashboardCardsDTO
    {
        public int TotalApplications { get; set; }
        public int ActiveLoans { get; set; }
        public int EmisPaid { get; set; }
        public decimal UpcomingEmiAmount { get; set; }
        public DateTime? UpcomingEmiDueDate { get; set; }
        public decimal TotalAmountPaid { get; set; }
    }
}
