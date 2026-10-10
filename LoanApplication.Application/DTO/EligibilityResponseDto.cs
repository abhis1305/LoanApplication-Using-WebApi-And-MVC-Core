namespace LoanApplication.Application.DTO
{
    public class EligibilityResponseDto
    {
        public int EligibilityId { get; set; }

        public int CustomerId { get; set; }

        public int CibilScore { get; set; }

        public string RiskCategory { get; set; } = string.Empty;

        public string EligibilityStatus { get; set; } = string.Empty;

        public decimal EligibleLoanAmount { get; set; }

        public string? RejectionReason { get; set; }
    }
}