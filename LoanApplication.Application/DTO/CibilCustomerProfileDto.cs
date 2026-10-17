namespace LoanApplication.Application.DTO
{
    public class CibilCustomerProfileDto
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string? PanNumber { get; set; }
    }
}