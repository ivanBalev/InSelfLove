using Smtp2Go.Api;
using Smtp2Go.Api.Models.Emails;
using System;
using System.Threading.Tasks;

namespace InSelfLove.Services.Messaging
{
    public class SMTP2GOEmailSender : IEmailSender
    {
        private readonly Smtp2GoApiService client;
        private readonly string adminEmail;

        public SMTP2GOEmailSender(string apiKey, string adminEmail)
        {
            this.client = new Smtp2GoApiService(apiKey);
            this.adminEmail = adminEmail;
        }

        public async Task SendEmailAsync(string from, string to, string subject, string htmlContent)
        {
            if (string.IsNullOrWhiteSpace(subject) && string.IsNullOrWhiteSpace(htmlContent))
            {
                throw new ArgumentException("Subject and message should be provided.");
            }

            EmailMessage message = new(
                htmlContent,
                subject,
                this.adminEmail,
                to);

            message.AddCustomHeader("Reply-To", from);

            await this.client.SendEmail(message);
        }
    }
}
