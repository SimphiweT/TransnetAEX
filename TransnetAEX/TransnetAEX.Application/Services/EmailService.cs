using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using TransnetAEX.Application.Interfaces;
using TransnetAEX.Domain.Configurations;

namespace TransnetAEX.Application.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;

        public EmailService(IOptions<EmailSettings> options)
        {
            _settings = options.Value;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            using var client = new SmtpClient(_settings.SmtpServer, _settings.Port)
            {
                Credentials = new NetworkCredential(_settings.Username, _settings.Password),
                EnableSsl = true
            };

            using var mailMessage = new MailMessage
            {
                From = new MailAddress(_settings.SenderEmail, _settings.SenderName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };

            mailMessage.To.Add(toEmail);

            try
            {
                await client.SendMailAsync(mailMessage);
                Console.WriteLine($"📧 Email sent successfully to {toEmail}");
            }
            catch (SmtpException ex)
            {
                Console.WriteLine($"❌ Email sending failed: {ex.Message}");
                throw new Exception($"Email sending failed: {ex.Message}", ex);
            }
        }
    }
}
