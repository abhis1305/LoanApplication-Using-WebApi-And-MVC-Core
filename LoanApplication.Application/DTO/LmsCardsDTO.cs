using System;
using System.Collections.Generic;
using System.Text;

namespace LoanApplication.Application.DTO
{
    public class LmsCardsDTO
    {
        public int TotalLoans { get; set; }
        public int ActiveLoans { get; set; }
        public int ClosedLoans { get; set; }
    }
}
