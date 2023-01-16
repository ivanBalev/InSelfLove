namespace BDInSelfLove.Web.Controllers
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using BDInSelfLove.Common;
    using BDInSelfLove.Data.Models;
    using BDInSelfLove.Services.Data.Appointments;
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Services.Messaging;
    using BDInSelfLove.Web.Controllers.Helpers;
    using BDInSelfLove.Web.InputModels.Appointment;
    using BDInSelfLove.Web.ViewModels.Appointment;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Localization;

    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentsController : BaseController
    {
        private readonly IStringLocalizer<AppointmentsController> localizer;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IAppointmentService appointmentService;
        private readonly IEmailSender emailSender;
        private readonly IViewRender viewRender;

        public AppointmentsController(
            IViewRender viewRender,
            IEmailSender emailSender,
            IAppointmentService appointmentService,
            UserManager<ApplicationUser> userManager,
            IStringLocalizer<AppointmentsController> localizer)
        {
            this.appointmentService = appointmentService;
            this.userManager = userManager;
            this.emailSender = emailSender;
            this.localizer = localizer;
            this.viewRender = viewRender;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string timezone)
        {
            // Update user timezone
            var userTimezone = await this.UpdateUserTimezone(timezone);

            // Gather data required by service
            string userId = this.userManager.GetUserId(this.User);
            string adminId = (await this.GetUser(admin: true)).Id;

            // Get appointments
            var appointments = await this.appointmentService.GetAll(userId, adminId);

            // Map db appointments to view model
            var appointmentsViewModel = appointments.Select(a =>
                {
                    var vm = AutoMapperConfig.MapperInstance.Map<Appointment, AppointmentViewModel>(a);

                    // Adjust appointments' start times for user
                    vm.Start = TimezoneHelper.ToLocalTime(vm.Start, userTimezone);
                    return vm;
                });

            var viewModel = new AppointmentIndexViewModel
            {
                // Get workday start & end for calendar + appointments
                WorkdayStart = TimezoneHelper.ToLocalTime(GlobalValues.WorkDayStartUTC, userTimezone),
                WorkdayEnd = TimezoneHelper.ToLocalTime(GlobalValues.WorkDayEndUTC, userTimezone),
                Appointments = appointmentsViewModel,
            };

            return this.View(viewModel);
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

            // Book appointment in db
            var appointment = await this.appointmentService.Book(
                inputModel.Id, inputModel.Description, inputModel.IsOnSite, this.userManager.GetUserId(this.User));

            // Notify user & admin
            await this.SendEmail(appointment, fromAdmin: false, "NewAppointment");
            await this.SendEmail(appointment, fromAdmin: true, "AwaitingApproval");

            return this.Ok();
        }

        [HttpPost]
        [Authorize]
        [Route("Cancel")]
        public async Task<ActionResult> Cancel([FromBody] AppointmentManipulateModel input)
        {
            // Gather data for email & service
            var appointment = await this.appointmentService.GetById(input.Id);
            var adminId = (await this.GetUser(admin: true)).Id;
            var userId = this.userManager.GetUserId(this.User);

            // Send cancellation email only if appointment is occupied by registered user,
            // not if admin has occupied the slot themselves after a client has booked via fb/insta/phone
            if (appointment.UserId != null && appointment.UserId != adminId)
            {
                await this.SendEmail(appointment, userId == adminId, "Cancellation");
            }

            await this.appointmentService.Cancel(appointment, userId, adminId);

            return this.Ok();
        }

        // Admin-only methods
        [HttpPost]
        [Route("Create")]
        [Authorize(Roles = GlobalValues.AdministratorRoleName)]
        public async Task<IActionResult> Create([FromBody] AvailabilityInputModel availabilityInput)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest();
            }

            var adminTimezone = await this.GetUserTimezoneId();

            // Convert appointment slots to utc time for db
            DateTime[] timeSlotsInUTC = availabilityInput.TimeSlots
                .Select(ts => TimezoneHelper.ToUTCTime(ts, adminTimezone)).ToArray();

            // Send slots and date to service
            await this.appointmentService.Create(timeSlotsInUTC, availabilityInput.Date);

            return this.Ok();
        }

        [HttpPost]
        [Authorize(Roles = GlobalValues.AdministratorRoleName)]
        [Route("Approve")]
        public async Task<ActionResult> Approve([FromBody] AppointmentManipulateModel input)
        {
            // Approve appointment in db
            var appointment = await this.appointmentService.Approve(input.Id);

            // Notify user
            await this.SendEmail(appointment, fromAdmin: true, "Confirmation");

            return this.Ok();
        }

        [HttpPost]
        [Authorize(Roles = GlobalValues.AdministratorRoleName)]
        [Route("Occupy")]
        public async Task<ActionResult> Occupy([FromBody] AppointmentManipulateModel input)
        {
            // Mark available appointment as occupied by admin.
            // Used for displaying appointments made via fb/insta/phone.
            await this.appointmentService.Occupy(input.Id, this.userManager.GetUserId(this.User));
            return this.Ok();
        }

        [HttpPost]
        [Authorize(Roles = GlobalValues.AdministratorRoleName)]
        [Route("SetOnSite")]
        public async Task<ActionResult> SetOnSite([FromBody] AppointmentManipulateModel input)
        {
            // Mark if appointment can be on site
            await this.appointmentService.SetOnSite(input.Id, input.CanBeOnSite);

            return this.Ok();
        }

        [HttpPost]
        [Authorize(Roles = GlobalValues.AdministratorRoleName)]
        [Route("SetWorkingHours")]
        public async Task<ActionResult> SetWorkingHours([FromBody] WorkingHoursInputModel input)
        {
            // TODO: store start/end times in file outside project so it doesn't get deleted after each website update

            // Get start & end time hours from input
            // Currently working only with 00 minutes
            var localStartHour = DateTime.Now.Date.AddHours(double.Parse(input.StartHour.Split(':')[0]));
            var localEndHour = DateTime.Now.Date.AddHours(double.Parse(input.EndHour.Split(':')[0]));

            var timezoneId = await this.GetUserTimezoneId();

            // Set workday start & end in global values
            GlobalValues.WorkDayStartUTC = TimezoneHelper.ToUTCTime(localStartHour, timezoneId);
            GlobalValues.WorkDayEndUTC = TimezoneHelper.ToUTCTime(localEndHour, timezoneId);

            return this.Ok();
        }

        // Helper methods
        private async Task<string> UpdateUserTimezone(string timezoneFromQuery)
        {
            var user = await this.GetUser();
            string userCurrentTimezone = this.UserTimezoneIdFromCookie ?? timezoneFromQuery;

            // If no data is sent by client, there's nothing to update with
            if (userCurrentTimezone == null)
            {
                // Try and return what's stored in db or return nothing
                return user.WindowsTimezoneId ?? null;
            }

            // Convert user's current timezone to windows timezone
            string userCurrentWindowsTimezoneId = TimezoneHelper.GetTimezone(userCurrentTimezone).Id;

            // Compare current timezone from cookie/query param with stored timezone in db
            if (user != null && user.WindowsTimezoneId.ToLower().CompareTo(userCurrentWindowsTimezoneId.ToLower()) != 0)
            {
                // Update if current & stored timezones don't match
                user.WindowsTimezoneId = userCurrentWindowsTimezoneId;
                await this.userManager.UpdateAsync(user);
            }

            // Return current timezone
            return userCurrentTimezone;
        }

        private async Task<string> GetUserTimezoneId(string timezoneFromQuery = null)
        {
            if (this.UserTimezoneIdFromCookie == null)
            {
                // Get timezone from db or query if user hasn't given cookie consent
                var user = await this.userManager.GetUserAsync(this.User);
                return user?.WindowsTimezoneId ?? timezoneFromQuery;
            }

            return this.UserTimezoneIdFromCookie;
        }

        private async Task SendEmail(Appointment apptmnt, bool fromAdmin, string status)
        {
            // Get admin
            var admin = await this.GetUser(true);

            // Get user
            var user = apptmnt.User ?? await this.GetUser();

            // Get current user timezone
            var recipientTimezoneId = fromAdmin ? await this.GetUserTimezoneId() : admin.WindowsTimezoneId;

            // Define data for email
            var model = new AppointmentEmail()
            {
                Start = TimezoneHelper.ToLocalTime(apptmnt.UtcStart, recipientTimezoneId),
                Status = status,
                Description = apptmnt.Description,
                IsOnSite = apptmnt.IsOnSite,
            };

            // Compose email
            var emailBody = await this.viewRender.RenderPartialViewToString("_EmailBody", model);

            // Send email
            await this.emailSender.SendEmailAsync(
                from: fromAdmin ? admin.Email : user.Email,
                fromName: fromAdmin ? GlobalValues.SystemName : user.UserName,
                to: fromAdmin ? user.Email : admin.Email,
                subject: "Терапевтична сесия",
                htmlContent: emailBody);
        }

        private async Task<ApplicationUser> GetUser(bool admin = false)
        {
            return admin ?
               (await this.userManager.GetUsersInRoleAsync(GlobalValues.AdministratorRoleName))
               .FirstOrDefault() :
                await this.userManager.GetUserAsync(this.User);
        }
    }
}
