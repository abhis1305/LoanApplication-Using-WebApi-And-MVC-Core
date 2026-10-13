using System;
using System.Collections.Generic;
using System.Text;

namespace LoanApplication.Application.DTO
{
    public class RegisterDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string MobileNo { get; set; }
        public string Pan { get; set; }
        public string AadhaarNo { get; set; }
        public string EmploymentType { get; set; }
        public decimal MonthlyIncome { get; set; }
        public decimal MonthlyInvestment { get; set; }
        //public int RoleId { get; set; } = 2; // Default Customer Role
    }


    public class LoginDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }


    public class AuthResponseDto
    {
        public bool RequiresTwoFactor { get; set; }
        public string? PreAuthToken { get; set; }
        public string? Token { get; set; }
        public string? Role { get; set; }
        public string Message { get; set; }
    }

    public class Verify2FaDto
    {
        public string PreAuthToken { get; set; }
        public string Code { get; set; }
    }

    public class Setup2FaDto
    {
        public string ManualEntryKey { get; set; }
        public string QrCodeUri { get; set; }
    }

    public class GoogleAuthDto
    {
        public string IdToken { get; set; }
    }
}
