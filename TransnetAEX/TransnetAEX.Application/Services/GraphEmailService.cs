using Azure.Identity;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TransnetAEX.Application.Interfaces;
using TransnetAEX.Domain.Configurations;
using Microsoft.Extensions.Configuration;

namespace TransnetAEX.Infrastructure.Services
{
    public class GraphEmailService : IEmailService
    {
        private readonly MicrosoftGraphSettings _settings;

        public GraphEmailService(IConfiguration config)
        {
            _settings = config.GetSection("MicrosoftGraph").Get<MicrosoftGraphSettings>()
                        ?? throw new InvalidOperationException("MicrosoftGraph section missing in configuration.");
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            try
            {
                // --- 1. Authenticate using Azure.Identity ---
                var credential = new ClientSecretCredential(
                    _settings.TenantId,
                    _settings.ClientId,
                    _settings.ClientSecret
                );

                // --- 2. Create Graph client ---
                var graphClient = new GraphServiceClient(credential, new[] { "https://graph.microsoft.com/.default" });

                // --- 3. Build the message ---
                var message = new Microsoft.Graph.Models.Message
                {
                    Subject = subject,
                    Body = new Microsoft.Graph.Models.ItemBody
                    {
                        ContentType = Microsoft.Graph.Models.BodyType.Html,
                        Content = htmlBody
                    },
                    ToRecipients = new List<Microsoft.Graph.Models.Recipient>
                    {
                        new Microsoft.Graph.Models.Recipient
                        {
                            EmailAddress = new Microsoft.Graph.Models.EmailAddress
                            {
                                Address = toEmail
                            }
                        }
                    }
                };

                // --- 4. Send mail ---
                await graphClient.Users[_settings.SenderEmail]
                    .SendMail.PostAsync(new Microsoft.Graph.Users.Item.SendMail.SendMailPostRequestBody
                    {
                        Message = message,
                        SaveToSentItems = true
                    });

                Console.WriteLine($"✅ Email sent successfully to {toEmail}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Graph email send failed: {ex.Message}");
                throw;
            }
        }
    }
}
