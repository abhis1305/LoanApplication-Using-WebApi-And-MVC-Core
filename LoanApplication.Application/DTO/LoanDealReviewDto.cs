using System;
using System.Collections.Generic;
using System.Text;

namespace LoanApplication.Application.DTO
{
    public class LoanDealReviewDto
    {
        public int OfficerId { get; set; }

        public decimal ApprovedAmount { get; set; }

        public string? RejectionReason { get; set; }
    }
}
