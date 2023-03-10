namespace InSelfLove.Services.Messaging
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using SendGrid;
    using SendGrid.Helpers.Mail;

    public class SendGridEmailSender : IEmailSender
    {
        private readonly SendGridClient client;
        private readonly string environment;

        public SendGridEmailSender(string apiKey, string environment)
        {
            this.client = new SendGridClient(apiKey);
            this.environment = environment;
        }

        public async Task SendEmailAsync(string from, string fromName, string to, string subject, string htmlContent, IEnumerable<EmailAttachment> attachments = null)
        {
            // Validate
            if (string.IsNullOrWhiteSpace(subject) && string.IsNullOrWhiteSpace(htmlContent))
            {
                throw new ArgumentException("Subject and message should be provided.");
            }

            if (!this.environment.ToLower().Equals("production"))
            {
                return;
            }

            // Get addresses & construct message
            var fromAddress = new EmailAddress(from, fromName);
            var toAddress = new EmailAddress(to);
            var message = MailHelper.CreateSingleEmail(fromAddress, toAddress, subject, null, htmlContent);

            // Add attachments (For now, this feature is not used)
            if (attachments?.Any() == true)
            {
                foreach (var attachment in attachments)
                {
                    message.AddAttachment(attachment.FileName, Convert.ToBase64String(attachment.Content), attachment.MimeType);
                }
            }

            try
            {
                // Send email
                var response = await this.client.SendEmailAsync(message);
            }
            catch (Exception e)
            {
                throw;
            }
        }
    }
}
