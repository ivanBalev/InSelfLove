namespace BDInSelfLove.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
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
    using TimeZoneConverter;

    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentsController : BaseController
    {
        private const string AppointmentEmailSubject = "Appointment";
        private const string AppointmentCancellationIntro = "I'm deeply sorry but I'm going to have to cancel the appointment.";
        private const string AppointmentConfirmationString = "Your appointment has been confirmed. See you soon!";
        private const string AppointmentAwaitingApprovalText = "Your request for an appointment has been received. Please wait for approval.";
        private const string IANATimezoneCookieName = "timezoneIANA";

        private readonly IAppointmentService appointmentService;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IEmailSender emailSender;

        public AppointmentsController(
            IAppointmentService appointmentService,
            UserManager<ApplicationUser> userManager,
            IEmailSender emailSender)
        {
            this.appointmentService = appointmentService;
            this.userManager = userManager;
            this.emailSender = emailSender;
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
            DateTime[] utcAppointments = availabilityInput.TimeSlots?.Select(ts => this.GetUtcTimeSlot(ts)).ToArray();
            await this.appointmentService.Create(utcAppointments, availabilityInput.Date);
            return this.Ok();
        }

        [HttpGet]
        [Route("GetAll")]
        public async Task<ActionResult<AppointmentViewModel[]>> GetAll()
        {
            string userId;
            TimeZoneInfo userTimezone;
            string ianaTimezoneCookieValue = this.HttpContext.Request.Cookies[IANATimezoneCookieName];

            // Get timezone from cookie or db & userId
            // TODO: cookie will always be set. no need to overcomplicate code with unplausible scenarios
            if (ianaTimezoneCookieValue != null)
            {
                userId = this.userManager.GetUserId(this.User);
                userTimezone = TZConvert.GetTimeZoneInfo(ianaTimezoneCookieValue);
            }
            else
            {
                var user = await this.userManager.GetUserAsync(this.User);
                userId = user.Id;
                userTimezone = TZConvert.GetTimeZoneInfo(user.WindowsTimezoneId);
            }

            // Get appointments from db & switch start times to user local time
            var appointments = (await this.appointmentService
                .GetAll(userId))
                .Select(a => AutoMapperConfig.MapperInstance.Map<AppointmentViewModel>(a))
                .Select(a =>
                {
                    a.Start = TimeZoneInfo.ConvertTimeFromUtc(a.Start, userTimezone);
                    return a;
                })
                .ToArray();

            return appointments;
        }

        [HttpPost]
        [Route("Book")]
        public async Task<IActionResult> Book([FromForm] AppointmentInputModel inputModel)
        {
            // Validate input model
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest();
            }

            // Get user & get appointment start in utc time
            ApplicationUser user = await this.userManager.GetUserAsync(this.User);
            DateTime userTimeAppointmentStart = DateTime.ParseExact(inputModel.Start, "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);
            TimeZoneInfo userWindowsTimezone = TZConvert.GetTimeZoneInfo(user.WindowsTimezoneId);
            DateTime utcAppointmentStart = TimeZoneInfo.ConvertTimeToUtc(userTimeAppointmentStart, userWindowsTimezone);

            // Book appointment
            int bookingResult = await this.appointmentService.Book(utcAppointmentStart, inputModel.Description, user.Id);
            if (bookingResult == 0)
            {
                return this.BadRequest();
            }

            // Get admin email & admin timezone appointment start time
            ApplicationUser admin = (await this.userManager.GetUsersInRoleAsync(GlobalValues.AdministratorRoleName)).FirstOrDefault();
            TimeZoneInfo adminWindowsTimezone = TZConvert.GetTimeZoneInfo(admin.WindowsTimezoneId);
            DateTime adminTimeAppointmentStart = TimeZoneInfo.ConvertTimeFromUtc(utcAppointmentStart, adminWindowsTimezone);

            // Notify admin & user via email
            await this.SendBookingEmails(user.Email, user.UserName, userTimeAppointmentStart, admin.Email, adminTimeAppointmentStart);
            return this.Ok();
        }

        [HttpPost]
        [Authorize(Roles = GlobalValues.AdministratorRoleName)]
        [Route("Approve")]
        public async Task<ActionResult> Approve([FromForm] int id)
        {
            // Approve appointment
            await this.appointmentService.Approve(id);

            // Get appointment info & admin email
            var appointmentFromDb = await this.appointmentService.GetById(id);
            var adminEmail = (await this.userManager.GetUsersInRoleAsync(GlobalValues.AdministratorRoleName)).FirstOrDefault().Email;

            // Generate email body & send user confirmation email
            var emailContent = $"<div>Hello, </div> <div></div> <div>{AppointmentConfirmationString}</div><div>Thanks!</div>";
            await this.emailSender.SendEmailAsync(adminEmail, GlobalValues.SystemName, appointmentFromDb.User.Email, this.GetEmailSubject(appointmentFromDb.UtcStart), emailContent);
            return this.Ok();
        }

        [HttpPost]
        [Route("Cancel")]
        public async Task<ActionResult> Cancel([FromForm] int id)
        {
            var currentUser = await this.userManager.GetUserAsync(this.User);
            var appointmentFromDb = await this.appointmentService.GetById(id);

            // Allow only admin to cancel others' appointments
            if (appointmentFromDb.UserId != currentUser.Id && !this.User.IsInRole(GlobalValues.AdministratorRoleName))
            {
                return this.BadRequest();
            }

            // Delete slot & don't send emails if admin cancels unoccupied appointment slot
            if (appointmentFromDb.UserId == null && this.User.IsInRole(GlobalValues.AdministratorRoleName))
            {
                await this.appointmentService.Delete(id);
                return this.Ok();
            }

            // Cancel slot
            await this.appointmentService.Cancel(id);

            var adminEmail = (await this.userManager.GetUsersInRoleAsync(GlobalValues.AdministratorRoleName)).FirstOrDefault().Email;
            var emailText = $"<div>Hello, </div> <div></div> <div>{AppointmentCancellationIntro}</div><div>Thank you.</div>";

            // Send email to user if admin cancels or vice versa
            if (this.User.IsInRole(GlobalValues.AdministratorRoleName))
            {
                await this.emailSender.SendEmailAsync(adminEmail, GlobalValues.SystemName, appointmentFromDb.User.Email, this.GetEmailSubject(appointmentFromDb.UtcStart), emailText);
            }
            else
            {
                await this.emailSender.SendEmailAsync(appointmentFromDb.User.Email, appointmentFromDb.User.UserName, adminEmail, this.GetEmailSubject(appointmentFromDb.UtcStart), emailText);
            }

            return this.Ok();
        }

        [HttpPost]
        [Authorize(Roles = GlobalValues.AdministratorRoleName)]
        [Route("SetWorkingHours")]
        public ActionResult SetWorkingHours([FromForm] string startHour, [FromForm] string endHour)
        {
            GlobalValues.WorkDayStart = int.Parse(startHour.Split(':')[0]);
            GlobalValues.WorkDayEnd = int.Parse(endHour.Split(':')[0]);
            return this.Ok();
        }

        [HttpGet]
        [Route("GetWorkingHours")]
        public ActionResult<int[]> GetWorkingHours() => new int[] { GlobalValues.WorkDayStart, GlobalValues.WorkDayEnd };

        private string GetEmailSubject(DateTime start) => $"{GlobalValues.SystemName} {AppointmentEmailSubject} on {start:dd MMMM HH:mm}";

        private async Task SendBookingEmails(
           string userEmail, string userUserName, DateTime userTimeAppointmentStart, string adminEmail, DateTime adminTimeAppointmentStart)
        {
            // TODO: this thing needs to be way more abstract.
            var scheme = this.HttpContext.Request.Scheme;
            var baseUrl = this.HttpContext.Request.Host.Value;

            // TODO: Translate messages into Bulgarian
            var urlElement = $"<a href=\"{scheme}://{baseUrl}/home/appointment\"><h3>Appointment Details and Approval</h3></a>";
            var adminEmailText = $"<div>Hello, </div> <div></div> <div>You have a new appointment. Have a look below:</div><div></div><div>{urlElement}</div><div></div><div>Thank you!</div>";
            var userEmailText = $"<div>Hello, </div> <div></div> <div>{AppointmentAwaitingApprovalText}</div><div>Thank you!</div>";

            // Send emails to admin & user
            await this.emailSender.SendEmailAsync(userEmail, userUserName, adminEmail, this.GetEmailSubject(userTimeAppointmentStart), adminEmailText);
            await this.emailSender.SendEmailAsync(adminEmail, GlobalValues.SystemName, userEmail, this.GetEmailSubject(adminTimeAppointmentStart), userEmailText);
        }

        private async Task UpdateUserTimezone()
        {
            string ianaTimezoneCookieValue = this.GetTimezoneCookieValue();

            if (ianaTimezoneCookieValue != string.Empty)
            {
                return;
            }

            var user = await this.userManager.GetUserAsync(this.User);
            string timezoneWindowsId = TZConvert.GetTimeZoneInfo(ianaTimezoneCookieValue).Id;

            if (user.WindowsTimezoneId == null ||
                user.WindowsTimezoneId.ToLower().CompareTo(timezoneWindowsId.ToLower()) != 0)
            {
                user.WindowsTimezoneId = timezoneWindowsId;
                await this.userManager.UpdateAsync(user);
            }
        }

        private string GetTimezoneCookieValue() => this.HttpContext.Request.Cookies[IANATimezoneCookieName];

        private DateTime GetUtcTimeSlot(DateTime timeSlot)
        {
            var userWindowsTimezone = TZConvert.GetTimeZoneInfo(this.GetTimezoneCookieValue());
            return TimeZoneInfo.ConvertTimeToUtc(timeSlot, userWindowsTimezone);
        }
    }
}
