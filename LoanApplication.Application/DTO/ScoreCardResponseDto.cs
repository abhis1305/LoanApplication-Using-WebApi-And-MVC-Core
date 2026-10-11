namespace LoanApplication.Application.DTO
{
    public class ScoreCardResponseDto
    {
        public int CustomerId { get; set; }

        public int CibilScore { get; set; }

        public string RiskCategory { get; set; } = string.Empty;

        public decimal MaximumAllowableEmi { get; set; }

        public decimal AvailableEmi { get; set; }

        public List<LoanOptionDto> LoanOptions { get; set; } = new();
    }

    public class LoanOptionDto
    {
        public int ScoreCardId { get; set; }

        public decimal InterestRate { get; set; }

        public int TenureInMonths { get; set; }

        public decimal EligibleLoanAmount { get; set; }
    }
}