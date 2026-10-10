using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoanApplication.Domain.Entities
{
    public class ScoreCard
    {
        [Key]
        public int ScoreCardId { get; set; }

        [ForeignKey("Customer")]
        public int CustomerId { get; set; }

        public string RiskCategory { get; set; } = string.Empty;

        public decimal InterestRate { get; set; }

        public int TenureInMonths { get; set; }

        public decimal EligibleLoanAmount { get; set; }

        public Customer? Customer { get; set; }
    }
}