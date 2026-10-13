using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace LoanApplication.Application.DTO
{
    public class CompleteProfileDto
    {
        [Range(18, 60, ErrorMessage = "Age must be between 18 and 60")]
        public int Age { get; set; }

        [Required(ErrorMessage = "Mobile number is required")]
        [RegularExpression(
            @"^[6-9]\d{9}$",
            ErrorMessage = "Enter a valid 10-digit mobile number")]
        public string MobileNo { get; set; } = string.Empty;

        [Required(ErrorMessage = "PAN is required")]
        [RegularExpression(
            @"^[A-Z]{5}[0-9]{4}[A-Z]$",
            ErrorMessage = "Enter a valid PAN format")]
        public string Pan { get; set; } = string.Empty;

        [Required(ErrorMessage = "Aadhaar number is required")]
        [RegularExpression(
            @"^[0-9]{12}$",
            ErrorMessage = "Aadhaar must contain 12 digits")]
        public string AadhaarNo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Employment type is required")]
        [StringLength(50)]
        public string EmploymentType { get; set; } = string.Empty;

        [Range(
            typeof(decimal), "1000", "10000000",
            ErrorMessage = "Enter valid monthly income")]
        public decimal MonthlyIncome { get; set; }

        [Range(
            typeof(decimal), "0", "10000000",
            ErrorMessage = "Enter valid monthly investment")]
        public decimal MonthlyInvestment { get; set; }
    }
}
