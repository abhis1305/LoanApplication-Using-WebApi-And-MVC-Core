namespace LoanApplication.Application.DTO
{
    public class CustomerScoreCardDto
    {
        public int CustomerId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public decimal? Income { get; set; }
    }
}