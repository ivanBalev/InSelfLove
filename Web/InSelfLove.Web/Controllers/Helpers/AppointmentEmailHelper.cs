namespace InSelfLove.Web.Controllers.Helpers
{
    using System.Threading.Tasks;

    using InSelfLove.Data.Models;
    using InSelfLove.Services.Data.Helpers;
    using InSelfLove.Services.Messaging;
    using InSelfLove.Web.ViewModels.Appointment;

    public class AppointmentEmailHelper : IAppointmentEmailHelper
    {
        private readonly IViewRender viewRender;
        private readonly IEmailSender emailSender;

        public AppointmentEmailHelper(IViewRender viewRender, IEmailSender emailSender)
        {
            this.viewRender = viewRender;
            this.emailSender = emailSender;
        }

        public async Task SendEmail(
            Appointment appointment,
            bool fromAdmin,
            string status,
            ApplicationUser admin,
            ApplicationUser user)
        {
            // Get current user timezone
            var recipientTimezone = fromAdmin ? user.Timezone : admin.Timezone;

            // Define data for email
            var model = new AppointmentEmail()
            {
                Start = TimezoneHelper.ToLocalTime(appointment.UtcStart, recipientTimezone),
                Status = status,
                Description = appointment.Description,
                IsOnSite = appointment.IsOnSite,
            };

            // Compose email body
            var emailBody = await this.viewRender.RenderPartialViewToString("_EmailBody", model);

            // Send email
            await this.emailSender.SendEmailAsync(
                from: fromAdmin ? admin.Email : user.Email,
                fromName: fromAdmin ? AppConstants.SystemName : user.UserName,
                to: fromAdmin ? user.Email : admin.Email,
                subject: "Терапевтична сесия",
                htmlContent: emailBody);
        }
    }
}
