namespace LoanApplication.Application.DTO
{
    public class LoanDealDto
    {
        public int DealId { get; set; }

        public int CustomerId { get; set; }

        public string CustomerName { get; set; } = string.Empty;

        public string? Email { get; set; }

        public string? Mobile { get; set; }

        public string? Pan { get; set; }

        public decimal? Salary { get; set; }

        public int? CibilScore { get; set; }

        public string LoanType { get; set; } = string.Empty;

        public decimal LoanAmount { get; set; }

        public double InterestRate { get; set; }

        public int TenureMonths { get; set; }

        public decimal EmiAmount { get; set; }

        public decimal ApprovedAmount { get; set; }

        public string CurrentStatus { get; set; } = string.Empty;

        public string? RejectionReason { get; set; }

        public DateTime AppliedDate { get; set; }
    }
}
