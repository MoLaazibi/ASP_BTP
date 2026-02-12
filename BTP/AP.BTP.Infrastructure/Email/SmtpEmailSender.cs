using AP.BTP.Application.Interfaces;
using AP.BTP.Application.Models;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace AP.BTP.Infrastructure.Email
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly EmailConfiguration emailConfiguration;

        public SmtpEmailSender(IOptions<EmailConfiguration> emailConfiguration)
        {
            this.emailConfiguration = emailConfiguration.Value;
        }

        public async Task<bool> SendEmail(EmailMessage email)
        {
            using var client = new SmtpClient(emailConfiguration.SmtpServer, emailConfiguration.SmtpPort)
            {
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(emailConfiguration.SmtpUserName, emailConfiguration.SmtpPassword),
                EnableSsl = emailConfiguration.EnableSsl
            };
            var emailMessage = new MailMessage()
            {
                From = new MailAddress(emailConfiguration.FromAddress, emailConfiguration.FromName),
                Subject = email.Subject,
                Body = email.Body,
                IsBodyHtml = email.IsHtml
            };
            emailMessage.To.Add(email.To);

            await client.SendMailAsync(emailMessage);
            return true;
        }
    }
}
