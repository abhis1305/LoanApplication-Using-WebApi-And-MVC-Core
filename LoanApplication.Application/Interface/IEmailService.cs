using System;
using System.Collections.Generic;
using System.Text;

namespace LoanApplication.Application.Interface
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body,string filePath);
    }
}
