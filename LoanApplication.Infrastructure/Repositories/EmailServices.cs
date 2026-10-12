
using LoanApplication.Application.Interface;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace LoanApplication.Infrastructure.Repositories
{
    public class EmailServices : IEmailService
    {
        private readonly IConfiguration conf;

        public EmailServices(IConfiguration conf)
        {
            this.conf = conf;
        }

        public async Task SendEmailAsync(
            string toEmail,
            string subject,
            string body,
            string filePath)
        {
            using MailMessage mail = new MailMessage();

            mail.From = new MailAddress(
                conf["EmailSettings:Email"]!);

            mail.To.Add(toEmail);
            mail.Subject = subject;
            mail.Body = body;
            mail.IsBodyHtml = true;

            // Attach one PDF file
            if (!string.IsNullOrWhiteSpace(filePath) &&
                File.Exists(filePath))
            {
                mail.Attachments.Add(
                    new Attachment(filePath));
            }

            using SmtpClient client =
                new SmtpClient("smtp.gmail.com", 587);

            client.Credentials = new NetworkCredential(
                conf["EmailSettings:Email"],
                conf["EmailSettings:Password"]);

            client.EnableSsl = true;

            await client.SendMailAsync(mail);
        }
    }
}
