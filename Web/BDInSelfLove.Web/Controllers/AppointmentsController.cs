namespace BDInSelfLove.Web.Controllers
{
    using System;
    using System.Linq;
    using System.Runtime.CompilerServices;
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
    using Microsoft.EntityFrameworkCore;
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
            await this.UpdateUserTimezone();

            var timezoneId = await this.TimezoneId(timezone);
            string userId = this.userManager.GetUserId(this.User);
            bool userIsAdmin = this.User.IsInRole(GlobalValues.AdministratorRoleName);
            string adminId = (await this.GetUser(true)).Id;

            var appointments = (await this.appointmentService
                .GetAll(userIsAdmin, userId)
                .To<AppointmentViewModel>()
                .ToArrayAsync())
                .Select(a => this.SanitizeAppt(a, timezoneId, userIsAdmin, userId, adminId))
                .OrderBy(x => x.Start)
                .ToArray();

            // TODO: does grayed out appointment have a title?
            var viewModel = new AppointmentIndexViewModel
            {
                WorkdayStart = TimezoneHelper.ToLocalTime(GlobalValues.WorkDayStartUTC, timezoneId),
                WorkdayEnd = TimezoneHelper.ToLocalTime(GlobalValues.WorkDayEndUTC, timezoneId),
                Appointments = appointments,
            };

            return this.View(viewModel);
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
            await this.SendEmail(appointment, false, "NewAppointment");
            await this.SendEmail(appointment, true, "AwaitingApproval");
            return this.Ok();
        }

        [HttpPost]
        [Authorize(Roles = GlobalValues.AdministratorRoleName)]
        [Route("Approve")]
        public async Task<ActionResult> Approve([FromBody] AppointmentManipulateModel input)
        {
            Appointment appointment = await this.appointmentService.Approve(input.Id);
            await this.SendEmail(appointment, true, "Confirmation");
            return this.Ok();
        }

        [HttpPost]
        [Authorize(Roles = GlobalValues.AdministratorRoleName)]
        [Route("Occupy")]
        public async Task<ActionResult> Occupy([FromBody] AppointmentManipulateModel input)
        {
            var appt = await this.appointmentService.Occupy(input.Id, (await this.GetUser(true)).Id);
            return this.Ok();
        }

        [HttpPost]
        [Authorize]
        [Route("Cancel")]
        public async Task<ActionResult> Cancel([FromBody] AppointmentManipulateModel input)
        {
            var admin = await this.GetUser(true);
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

            // Cancel slot if admin removes dummy
            if (appointment.UserId == admin.Id && userIsAdmin)
            {
                await this.appointmentService.Cancel(appointment);
                return this.Ok();
            }

            await this.SendEmail(appointment, userIsAdmin, "Cancellation");
            await this.appointmentService.Cancel(appointment);
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

        private async Task SendEmail(Appointment apptmnt, bool fromAdmin, string status)
        {
            // Get admin
            var admin = await this.GetUser(true);

            // Get user
            var user = apptmnt.User ?? await this.GetUser();

            // Get current user timezone
            var recipientTimezoneId = fromAdmin ? admin.WindowsTimezoneId : await this.TimezoneId();

            // Define data
            var model = new AppointmentEmail()
            {
                Start = TimezoneHelper.ToLocalTime(apptmnt.UtcStart, recipientTimezoneId),
                Status = status,
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

        private async Task<string> TimezoneId(string queryTimezone = null)
        {
            if (this.TimezoneIdFromCookie == null)
            {
                var user = await this.userManager.GetUserAsync(this.User);
                return user?.WindowsTimezoneId ?? queryTimezone;
            }

            return this.TimezoneIdFromCookie;
        }

        private AppointmentViewModel SanitizeAppt(AppointmentViewModel a, string timezoneId, bool userIsAdmin, string userId, string adminId)
        {
            a.Start = TimezoneHelper.ToLocalTime(a.Start, timezoneId);
            if (userIsAdmin)
            {
                if (a.UserId == adminId)
                {
                    a.IsUnavailable = true;
                }

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
                        IsUnavailable = a.UserId != null ? true : false,
                    };
                }
            }
        }
    }
}
