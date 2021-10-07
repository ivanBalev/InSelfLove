namespace BDInSelfLove.Web.Controllers
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using BDInSelfLove.Common;
    using BDInSelfLove.Data.Models;
    using BDInSelfLove.Services.Data.Calendar;
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Services.Messaging;
    using BDInSelfLove.Web.InputModels.Appointment;
    using BDInSelfLove.Web.ViewModels.Appointment;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Localization;
    using TimeZoneConverter;

    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentsController : BaseController
    {
        private const string IANATimezoneCookieName = "timezoneIANA";

        private readonly IAppointmentService appointmentService;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IEmailSender emailSender;
        private readonly IStringLocalizer<AppointmentsController> localizer;

        public AppointmentsController(
            IAppointmentService appointmentService,
            UserManager<ApplicationUser> userManager,
            IEmailSender emailSender,
            IStringLocalizer<AppointmentsController> localizer)
        {
            this.appointmentService = appointmentService;
            this.userManager = userManager;
            this.emailSender = emailSender;
            this.localizer = localizer;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Index()
        {
            await this.UpdateUserTimezone();
            return this.View();
        }

        [HttpPost]
        [Route("Create")]
        [Authorize(Roles = GlobalValues.AdministratorRoleName)]
        public async Task<IActionResult> Create([FromForm] AvailabilityInputModel availabilityInput)
        {
            DateTime[] utcTimeSlots = this.GetUtcTimeSlots(availabilityInput.TimeSlots).ToArray();
            await this.appointmentService.Create(utcTimeSlots, availabilityInput.Date);
            return this.Ok();
        }

        [HttpGet]
        [Route("GetAll")]
        public async Task<ActionResult<AppointmentViewModel[]>> GetAll()
        {
            string userId = this.userManager.GetUserId(this.User);
            bool userIsAdmin = this.User.IsInRole(GlobalValues.AdministratorRoleName);

            var appointments = (await this.appointmentService
                .GetAll(userId, userIsAdmin)
                .To<AppointmentViewModel>()
                .ToArrayAsync())
                .Select(a =>
                 {
                     // Convert start times to user local time
                     a.Start = TimeZoneInfo.ConvertTimeFromUtc(a.Start, this.GetUserWindowsTimezone());
                     return a;
                 }).ToArray();

            return appointments;
        }

        [HttpPost]
        [Route("Book")]
        public async Task<IActionResult> Book([FromForm] AppointmentInputModel inputModel)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest();
            }

            var user = await this.userManager.GetUserAsync(this.User);
            var appointment = await this.appointmentService.Book(inputModel.Id, inputModel.Description, user.Id);
            await this.SendBookingEmails(appointment);
            return this.Ok();
        }

        [HttpPost]
        [Authorize(Roles = GlobalValues.AdministratorRoleName)]
        [Route("Approve")]
        public async Task<ActionResult> Approve([FromForm] int id)
        {
            Appointment appointment = await this.appointmentService.Approve(id);
            await this.SendApprovalEmail(appointment);
            return this.Ok();
        }

        [HttpPost]
        [Route("Cancel")]
        public async Task<ActionResult> Cancel([FromForm] int id)
        {
            var user = await this.userManager.GetUserAsync(this.User);
            bool userIsAdmin = this.User.IsInRole(GlobalValues.AdministratorRoleName);
            Appointment appointment = await this.appointmentService.GetById(id);

            // Allow only admin to cancel others' appointments
            if (appointment.UserId != user.Id && !userIsAdmin)
            {
                return this.BadRequest();
            }

            // Delete slot & don't send emails if admin cancels unoccupied appointment slot
            if (appointment.UserId == null && userIsAdmin)
            {
                await this.appointmentService.Delete(appointment);
                return this.Ok();
            }

            // Cancel slot
            await this.appointmentService.Cancel(appointment);
            await this.SendCancellationEmail(user, userIsAdmin, appointment);

            return this.Ok();
        }

        [HttpPost]
        [Authorize(Roles = GlobalValues.AdministratorRoleName)]
        [Route("SetWorkingHours")]
        public ActionResult SetWorkingHours([FromForm] string startHour, [FromForm] string endHour)
        {
            // Currently working only with 00 minutes
            GlobalValues.WorkDayStart = int.Parse(startHour.Split(':')[0]);
            GlobalValues.WorkDayEnd = int.Parse(endHour.Split(':')[0]);
            return this.Ok();
        }

        [HttpGet]
        [Route("GetWorkingHours")]
        public ActionResult<int[]> GetWorkingHours()
        {
            return new int[] { GlobalValues.WorkDayStart, GlobalValues.WorkDayEnd };
        }

        // Helper methods
        private string FillInEmailTemplate(string templateName, string element)
        {
            return string.Format(this.localizer[templateName], element);
        }

        private DateTime ConvertToLocalTime(string windowsTimezoneId, DateTime utcTime)
        {
            TimeZoneInfo userWindowsTimezone = TZConvert.GetTimeZoneInfo(windowsTimezoneId);
            DateTime userLocalTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, userWindowsTimezone);
            return userLocalTime;
        }

        private async Task UpdateUserTimezone()
        {
            string userCurrentWindowsTimezoneId = this.GetUserWindowsTimezone().Id;
            var user = await this.userManager.GetUserAsync(this.User);

            if (user.WindowsTimezoneId.ToLower().CompareTo(userCurrentWindowsTimezoneId.ToLower()) != 0)
            {
                user.WindowsTimezoneId = userCurrentWindowsTimezoneId;
                await this.userManager.UpdateAsync(user);
            }
        }

        private TimeZoneInfo GetUserWindowsTimezone()
        {
            return TZConvert.GetTimeZoneInfo(this.HttpContext.Request.Cookies[IANATimezoneCookieName]);
        }

        private DateTime[] GetUtcTimeSlots(DateTime[] timeSlots)
        {
            DateTime[] adjustedTimeSlots = new DateTime[timeSlots.Length];

            for (int i = 0; i < timeSlots.Length; i++)
            {
                adjustedTimeSlots[i] = TimeZoneInfo.ConvertTimeToUtc(timeSlots[i], this.GetUserWindowsTimezone());
            }

            return adjustedTimeSlots;
        }

        private async Task SendBookingEmails(Appointment appointment)
        {
            ApplicationUser admin = (await this.userManager.GetUsersInRoleAsync(GlobalValues.AdministratorRoleName)).FirstOrDefault();
            DateTime adminTimeAppointmentStart = this.ConvertToLocalTime(admin.WindowsTimezoneId, appointment.UtcStart);
            string adminEmailBody = this.FillInEmailTemplate("Body", this.localizer["NewAppointment"]);
            string adminEmailSubject = this.FillInEmailTemplate("Subject", adminTimeAppointmentStart.ToString("dd MMMM HH:mm"));

            // Send email to admin
            await this.emailSender.SendEmailAsync(
                from: appointment.User.Email,
                fromName: appointment.User.UserName,
                to: admin.Email,
                subject: adminEmailSubject,
                htmlContent: adminEmailBody);

            DateTime userTimeAppointmentStart = this.ConvertToLocalTime(this.GetUserWindowsTimezone().Id, appointment.UtcStart);
            string userEmailBody = this.FillInEmailTemplate("Body", this.localizer["AwaitingApproval"]);
            string userEmailSubject = this.FillInEmailTemplate("Subject", userTimeAppointmentStart.ToString("dd MMMM HH:mm"));

            // Send email to user
            await this.emailSender.SendEmailAsync(
                from: admin.Email,
                fromName: GlobalValues.SystemName,
                to: appointment.User.Email,
                subject: userEmailSubject,
                htmlContent: userEmailBody);
        }

        private async Task SendApprovalEmail(Appointment appointmentFromDb)
        {
            string adminEmail =
                            (await this.userManager.GetUsersInRoleAsync(GlobalValues.AdministratorRoleName)).FirstOrDefault().Email;
            DateTime userTimeAppointmentStart =
                this.ConvertToLocalTime(appointmentFromDb.User.WindowsTimezoneId, appointmentFromDb.UtcStart);

            string emailBody = this.FillInEmailTemplate("Body", this.localizer["Confirmation"]);
            string emailSubject = this.FillInEmailTemplate("Subject", userTimeAppointmentStart.ToString("dd MMMM HH:mm"));
            await this.emailSender.SendEmailAsync(
                from: adminEmail,
                fromName: GlobalValues.SystemName,
                to: appointmentFromDb.User.Email,
                subject: emailSubject,
                htmlContent: emailBody);
        }

        private async Task SendCancellationEmail(ApplicationUser user, bool userIsAdmin, Appointment appointment)
        {
            var admin = (await this.userManager.GetUsersInRoleAsync(GlobalValues.AdministratorRoleName)).FirstOrDefault();
            DateTime adminTimeAppointmentStart = this.ConvertToLocalTime(admin.WindowsTimezoneId, appointment.UtcStart);
            string adminEmailSubject = this.FillInEmailTemplate("Subject", adminTimeAppointmentStart.ToString("dd MMMM HH:mm"));

            DateTime userTimeAppointmentStart = this.ConvertToLocalTime(this.GetUserWindowsTimezone().Id, appointment.UtcStart);
            string userEmailSubject = this.FillInEmailTemplate("Subject", userTimeAppointmentStart.ToString("dd MMMM HH:mm"));

            var emailBody = this.FillInEmailTemplate("Body", this.localizer["Cancellation"]);

            // Send email to user if admin cancels or vice versa
            if (userIsAdmin)
            {
                await this.emailSender.SendEmailAsync(
                    from: admin.Email,
                    fromName: GlobalValues.SystemName,
                    to: user.Email,
                    subject: userEmailSubject,
                    htmlContent: emailBody);
            }
            else
            {
                await this.emailSender.SendEmailAsync(
                    from: user.Email,
                    fromName: user.UserName,
                    to: admin.Email,
                    subject: adminEmailSubject,
                    htmlContent: emailBody);
            }
        }
    }
}
