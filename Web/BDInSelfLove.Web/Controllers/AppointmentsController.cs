namespace BDInSelfLove.Web.Controllers
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;

    using BDInSelfLove.Common;
    using BDInSelfLove.Data.Models;
    using BDInSelfLove.Services.Data.Appointments;
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Services.Messaging;
    using BDInSelfLove.Web.InputModels.Appointment;
    using BDInSelfLove.Web.ViewModels.Appointment;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Localization;

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
        public async Task<IActionResult> Index(string timezone)
        {
            await this.UpdateUserTimezone();
            return this.View(await this.GetIndexViewModel(timezone));
        }

        [HttpPost]
        [Route("Create")]
        [Authorize(Roles = GlobalValues.AdministratorRoleName)]
        public async Task<IActionResult> Create([FromBody] AvailabilityInputModel availabilityInput)
        {
            var timezoneId = await this.TimezoneId();

            // TODO: this needs to be in middleware
            DateTime[] utcTimeSlots = availabilityInput.TimeSlots
                .Select(ts => TimezoneHelper.ToUTCTime(ts, timezoneId)).ToArray();

            var result = await this.appointmentService
                .Create(utcTimeSlots, availabilityInput.Date);

            return this.Ok();
        }

        [HttpPost]
        [Authorize]
        [Route("Book")]
        public async Task<IActionResult> Book([FromBody] AppointmentInputModel inputModel)
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
        public async Task<ActionResult> Approve([FromBody] AppointmentManipulateModel input)
        {
            Appointment appointment = await this.appointmentService.Approve(input.Id);
            await this.SendApprovalEmail(appointment);
            return this.Ok();
        }

        [HttpPost]
        [Authorize]
        [Route("Cancel")]
        public async Task<ActionResult> Cancel([FromBody] AppointmentManipulateModel input)
        {
            var user = await this.userManager.GetUserAsync(this.User);
            bool userIsAdmin = this.User.IsInRole(GlobalValues.AdministratorRoleName);
            Appointment appointment = await this.appointmentService.GetById(input.Id);

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
        public async Task<ActionResult> SetWorkingHours([FromBody] WorkingHoursInputModel input)
        {
            // Currently working only with 00 minutes
            var localStartHour = DateTime.Now.Date.AddHours(double.Parse(input.StartHour.Split(':')[0]));
            var localEndHour = DateTime.Now.Date.AddHours(double.Parse(input.EndHour.Split(':')[0]));

            var timezoneId = await this.TimezoneId();

            GlobalValues.WorkDayStartUTC = TimezoneHelper.ToUTCTime(localStartHour, timezoneId);
            GlobalValues.WorkDayEndUTC = TimezoneHelper.ToUTCTime(localEndHour, timezoneId);

            return this.Ok();
        }

        // Helper methods
        private string FillInEmailTemplate(string templateName, string element)
        {
            return string.Format(this.localizer[templateName], element);
        }

        private async Task UpdateUserTimezone()
        {
            if (this.TimezoneIdFromCookie == null)
            {
                return;
            }

            string userCurrentWindowsTimezoneId = TimezoneHelper.GetTimezone(this.TimezoneIdFromCookie).Id;
            var user = await this.userManager.GetUserAsync(this.User);

            if (user != null && user.WindowsTimezoneId.ToLower().CompareTo(userCurrentWindowsTimezoneId.ToLower()) != 0)
            {
                user.WindowsTimezoneId = userCurrentWindowsTimezoneId;
                await this.userManager.UpdateAsync(user);
            }
        }

        private async Task SendBookingEmails(Appointment appointment)
        {
            var admin = await this.GetAdmin();
            var timezoneId = await this.TimezoneId();
            DateTime adminTimeAppointmentStart = TimezoneHelper.ToLocalTime(appointment.UtcStart, timezoneId);
            string adminEmailBody = this.FillInEmailTemplate(Body, this.localizer[NewAppointment]);
            string adminEmailSubject = this.FillInEmailTemplate(Subject, adminTimeAppointmentStart.ToString(DateEmailFormat));

            // Send email to admin
            await this.emailSender.SendEmailAsync(
                from: appointment.User.Email,
                fromName: appointment.User.UserName,
                to: admin.Email,
                subject: adminEmailSubject,
                htmlContent: adminEmailBody);

            DateTime userTimeAppointmentStart = TimezoneHelper.ToLocalTime(appointment.UtcStart, timezoneId);
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
            string adminEmail = (await this.GetAdmin()).Email;
            DateTime userTimeAppointmentStart =
                TimezoneHelper.ToLocalTime(appointmentFromDb.UtcStart, await this.TimezoneId());

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
            var admin = await this.GetAdmin();
            var timezoneId = await this.TimezoneId();
            DateTime adminTimeAppointmentStart = TimezoneHelper.ToLocalTime(appointment.UtcStart, timezoneId);
            string adminEmailSubject = this.FillInEmailTemplate(Subject, adminTimeAppointmentStart.ToString(DateEmailFormat));

            DateTime userTimeAppointmentStart = TimezoneHelper.ToLocalTime(appointment.UtcStart, timezoneId);
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

        private async Task<ApplicationUser> GetAdmin()
        {
            return (await this.userManager.GetUsersInRoleAsync(GlobalValues.AdministratorRoleName)).FirstOrDefault();
        }

        private async Task<AppointmentIndexViewModel> GetIndexViewModel(string queryTimezone)
        {
            var timezoneId = await this.TimezoneId(queryTimezone);

            string userId = this.userManager.GetUserId(this.User);
            bool userIsAdmin = this.User.IsInRole(GlobalValues.AdministratorRoleName);

            var appointments = (await this.appointmentService
                .GetAll(userIsAdmin, userId)
                .To<AppointmentViewModel>()
                .ToArrayAsync())
                .Select(a =>
                {
                    a.Start = TimezoneHelper.ToLocalTime(a.Start, timezoneId);
                    if (userIsAdmin)
                    {
                        return a;
                    }
                    else if (userId != null)
                    {
                        // user is logged
                        if (userId != a.UserId && a.UserId != null)
                        {
                            // aptmnt is another user's
                            // Clear data for occupied appointments, but still provide to client to display
                            return new AppointmentViewModel
                            {
                                Start = a.Start,
                                IsUnavailable = true,
                            };
                        }

                        // appt is own
                        return a;
                    }
                    else
                    {
                        // user is n/a
                        if (DateTime.Compare(a.Start, DateTime.UtcNow) <= 0)
                        {
                            // Old slot
                            return new AppointmentViewModel
                            {
                                Start = a.Start,
                                IsUnavailable = true,
                            };
                        }
                        else
                        {
                            return new AppointmentViewModel
                            {
                                Start = a.Start,
                                IsUnavailable = a.UserId != null ? true : false ,
                            };
                        }
                    }

                    //// not own appointment      occupied         user not admin
                    //if ((a.UserId != userId && a.UserId != null && !userIsAdmin) ||
                    //    DateTime.Compare(a.Start, DateTime.UtcNow) <= 0)
                    //{
                    //    a.Start = TimezoneHelper.ToLocalTime(a.Start, timezoneId);

                    //    // Clear data for occupied appointments, but still provide to client to display
                    //    return new AppointmentViewModel
                    //    {
                    //        Start = a.Start,
                    //        IsUnavailable = true,
                    //    };
                    //}
                    //a.Start = TimezoneHelper.ToLocalTime(a.Start, timezoneId);
                    //return a;
                })
                .OrderBy(x => x.Start)
                .ToArray();

            // TODO: does grayed out appointment have a title?
            return new AppointmentIndexViewModel
            {
                WorkdayStart = TimezoneHelper.ToLocalTime(GlobalValues.WorkDayStartUTC, timezoneId),
                WorkdayEnd = TimezoneHelper.ToLocalTime(GlobalValues.WorkDayEndUTC, timezoneId),
                Appointments = appointments,
            };
        }

        private async Task<string> TimezoneId(string queryTimezone = null)
        {
            if (this.TimezoneIdFromCookie == null)
            {
                var user = await this.userManager.GetUserAsync(this.User);
                return user?.WindowsTimezoneId ?? queryTimezone;
            }

            return this.TimezoneIdFromCookie;
        }
    }
}
