using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace LoanApplication.Application.DTO
{

    public class CustomerProfileDto
    {
        public int UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName =>$"{FirstName} {LastName}".Trim();
        public string Email { get; set; } = string.Empty;
        public string MobileNo { get; set; } = string.Empty;
        public string Pan { get; set; } = string.Empty;
        public string AadhaarNo { get; set; } = string.Empty;
        public string EmploymentType { get; set; } = string.Empty;
        public decimal MonthlyIncome { get; set; }
        public decimal MonthlyInvestment { get; set; }
        public int Age { get; set; }
    }

    

    public class UpdateCustomerProfileDto
    {
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;
        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;
        [Required]
        [EmailAddress]
        [StringLength(256)]
        public string Email { get; set; } = string.Empty;
        [Required]
        [Phone]
        [StringLength(20)]
        public string MobileNo { get; set; } = string.Empty;
        [Range(18, 100)]
        public int Age { get; set; }
        [Required]
        [RegularExpression("^(Salaried|Business|Self Employed)$",ErrorMessage = "Employment type is invalid.")]
        public string EmploymentType { get; set; } = string.Empty;
        [Range(typeof(decimal), "0", "999999999")]
        public decimal MonthlyIncome { get; set; }

        [Range(typeof(decimal), "0", "999999999")]
        public decimal MonthlyInvestment { get; set; }
    }


}
