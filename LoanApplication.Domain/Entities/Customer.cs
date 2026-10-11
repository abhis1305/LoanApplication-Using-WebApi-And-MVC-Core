using System.ComponentModel.DataAnnotations;

namespace LoanApp.Models
{
    public class Customer
    {
        [Key]
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "First Name is required")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last Name is required")]
        public string LastName { get; set; } = string.Empty;

        // Optional during Google registration
        [Range(18, 60, ErrorMessage = "Age must be between 18 and 60")]
        public int? Age { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Format")]
        public string Email { get; set; } = string.Empty;

        // Google-only account does not need a password
        public string? Password { get; set; }

        // Optional until profile completion
        [RegularExpression(
            @"^[6-9]\d{9}$",
            ErrorMessage = "Enter valid 10 digit mobile number")]
        public string? MobileNo { get; set; }

        [RegularExpression(
            @"^[A-Z]{5}[0-9]{4}[A-Z]{1}$",
            ErrorMessage = "Invalid PAN Number")]
        public string? Pan { get; set; }

        [RegularExpression(
            @"^[0-9]{12}$",
            ErrorMessage = "Aadhaar must be 12 digits")]
        public string? AadhaarNo { get; set; }

        public string? EmploymentType { get; set; }

        [Range(1000, 10000000, ErrorMessage = "Enter valid income")]
        public decimal? MonthlyIncome { get; set; }

        public decimal? MonthlyInvestment { get; set; }

        public bool IsEmailVerified { get; set; } = false;
        public bool IsTwoFactorEnabled { get; set; } = false;
        public string? TwoFactorSecret { get; set; }

        // Navigation Properties
        public ICollection<KycDocument>? KycDocuments { get; set; }
        public ICollection<CibilReport>? CibilReports { get; set; }
        public ICollection<EligibilityResult>? EligibilityResults { get; set; }
        public ICollection<ScoreCard>? ScoreCards { get; set; }
        public ICollection<LoanDeal>? LoanDeals { get; set; }
    }
}