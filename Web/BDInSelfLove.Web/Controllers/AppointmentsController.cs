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

    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentsController : BaseController
    {
        private const string Body = "Body";
        private const string Subject = "Subject";
        private const string Cancellation = "Cancellation";
        private const string Confirmation = "Confirmation";
        private const string NewAppointment = "NewAppointment";
        private const string DateEmailFormat = "dd MMMM HH:mm";
        private const string AwaitingApproval = "AwaitingApproval";

        private readonly IStringLocalizer<AppointmentsController> localizer;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IAppointmentService appointmentService;
        private readonly IEmailSender emailSender;

        public AppointmentsController(
            IEmailSender emailSender,
            IAppointmentService appointmentService,
            UserManager<ApplicationUser> userManager,
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
            DateTime[] utcTimeSlots = availabilityInput.TimeSlots
                .Select(ts => TimezoneHelper.ToUTCTime(ts, this.TimezoneCookieValue)).ToArray();

            var result = await this.appointmentService
                .Create(utcTimeSlots, TimezoneHelper.ToUTCTime(availabilityInput.Date, this.TimezoneCookieValue));

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
                     a.Start = TimezoneHelper.ToLocalTime(a.Start, this.TimezoneCookieValue);
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

            // Delete slot if admin cancels unoccupied appointment & don't send emails
            if (appointment.UserId == null && userIsAdmin)
            {
                await this.appointmentService.Delete(appointment);
                return this.Ok();
            }

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

        private async Task UpdateUserTimezone()
        {
            string userCurrentWindowsTimezoneId = TimezoneHelper.GetUserWindowsTimezone(this.TimezoneCookieValue).Id;
            var user = await this.userManager.GetUserAsync(this.User);

            if (user.WindowsTimezoneId.ToLower().CompareTo(userCurrentWindowsTimezoneId.ToLower()) != 0)
            {
                user.WindowsTimezoneId = userCurrentWindowsTimezoneId;
                await this.userManager.UpdateAsync(user);
            }
        }

        private async Task SendBookingEmails(Appointment appointment)
        {
            ApplicationUser admin = (await this.userManager.GetUsersInRoleAsync(GlobalValues.AdministratorRoleName)).FirstOrDefault();
            DateTime adminTimeAppointmentStart = TimezoneHelper.ToLocalTime(appointment.UtcStart, this.TimezoneCookieValue);
            string adminEmailBody = this.FillInEmailTemplate(Body, this.localizer[NewAppointment]);
            string adminEmailSubject = this.FillInEmailTemplate(Subject, adminTimeAppointmentStart.ToString(DateEmailFormat));

            // Send email to admin
            await this.emailSender.SendEmailAsync(
                from: appointment.User.Email,
                fromName: appointment.User.UserName,
                to: admin.Email,
                subject: adminEmailSubject,
                htmlContent: adminEmailBody);

            DateTime userTimeAppointmentStart = TimezoneHelper.ToLocalTime(appointment.UtcStart, this.TimezoneCookieValue);
            string userEmailBody = this.FillInEmailTemplate(Body, this.localizer[AwaitingApproval]);
            string userEmailSubject = this.FillInEmailTemplate(Subject, userTimeAppointmentStart.ToString(DateEmailFormat));

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
                TimezoneHelper.ToLocalTime(appointmentFromDb.UtcStart, this.TimezoneCookieValue);

            string emailBody = this.FillInEmailTemplate(Body, this.localizer[Confirmation]);
            string emailSubject = this.FillInEmailTemplate(Subject, userTimeAppointmentStart.ToString(DateEmailFormat));
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
            DateTime adminTimeAppointmentStart = TimezoneHelper.ToLocalTime(appointment.UtcStart, this.TimezoneCookieValue);
            string adminEmailSubject = this.FillInEmailTemplate(Subject, adminTimeAppointmentStart.ToString(DateEmailFormat));

            DateTime userTimeAppointmentStart = TimezoneHelper.ToLocalTime(appointment.UtcStart, this.TimezoneCookieValue);
            string userEmailSubject = this.FillInEmailTemplate(Subject, userTimeAppointmentStart.ToString(DateEmailFormat));

            var emailBody = this.FillInEmailTemplate(Body, this.localizer[Cancellation]);

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
