namespace BDInSelfLove.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;

    using BDInSelfLove.Common;
    using BDInSelfLove.Data.Models;
    using BDInSelfLove.Services.Data.Calendar;
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Services.Messaging;
    using BDInSelfLove.Services.Models.Appointment;
    using BDInSelfLove.Web.Areas.Administration;
    using BDInSelfLove.Web.InputModels.Appointment;
    using BDInSelfLove.Web.ViewModels.Appointment;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using TimeZoneConverter;

    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentController : ControllerBase
    {
        private const string AppointmentEmailSubject = "Appointment";
        private const string AppointmentCancellationIntro = "I'm deeply sorry but I'm going to have to cancel the appointment.";
        private const string AppointmentConfirmationString = "Your appointment has been confirmed. See you soon!";
        private const string AppointmentAwaitingApprovalText = "Your request for an appointment has been received. Please wait for approval.";
        private const string IANATimezoneCookieName = "timezoneIANA";

        private readonly IAppointmentService appointmentService;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IEmailSender emailSender;

        public AppointmentController(
            IAppointmentService appointmentService,
            UserManager<ApplicationUser> userManager,
            IEmailSender emailSender)
        {
            this.appointmentService = appointmentService;
            this.userManager = userManager;
            this.emailSender = emailSender;
        }

        [HttpPost]
        [Route("Create")]
        [Authorize(Roles = GlobalConstants.AdministratorRoleName)]
        public async Task<IActionResult> Create([FromForm] AvailabilityInputModel availabilityInput)
        {
            TimeZoneInfo windowsTimezone = TZConvert.GetTimeZoneInfo(availabilityInput.Timezone);

            List<AppointmentServiceModel> appointments = availabilityInput.TimeSlots.Select(ts =>
            {
                DateTime date = DateTime.ParseExact($"{availabilityInput.Date} {ts}", "MM-dd-yyyy H:m", CultureInfo.InvariantCulture);
                DateTime utcDate = TimeZoneInfo.ConvertTimeToUtc(date, windowsTimezone);

                return new AppointmentServiceModel { UtcStart = utcDate };
            })
            .ToList();

            await this.appointmentService.Create(appointments);
            return this.Ok();
        }

        [HttpGet]
        [Route("GetAll")]
        public async Task<ActionResult<AppointmentViewModel[]>> GetAll()
        {
            string userId;
            TimeZoneInfo windowsTimezone;
            string ianaTimezoneCookieValue = this.HttpContext.Request.Cookies[IANATimezoneCookieName];

            if (ianaTimezoneCookieValue != null)
            {
                // does this query DB?
                userId = this.userManager.GetUserId(this.User);
                windowsTimezone = TZConvert.GetTimeZoneInfo(ianaTimezoneCookieValue);
            }
            else
            {
                var user = await this.userManager.GetUserAsync(this.User);
                userId = user.Id;
                windowsTimezone = TZConvert.GetTimeZoneInfo(user.WindowsTimezoneId);
            }

            var appointments = (await this.appointmentService
                .GetAll(userId))
                .Select(a => AutoMapperConfig.MapperInstance.Map<AppointmentViewModel>(a))
                .Select(a =>
                {
                    a.Start = TimeZoneInfo.ConvertTimeFromUtc(a.Start, windowsTimezone);
                    return a;
                })
                .ToArray();

            return appointments;
        }

        [HttpPost]
        [Route("Book")]
        public async Task<IActionResult> Book([FromForm] AppointmentInputModel inputModel)
        {
            // Create model for service
            var serviceModel = AutoMapperConfig.MapperInstance.Map<AppointmentServiceModel>(inputModel);

            // Set up model for service
            var user = await this.userManager.GetUserAsync(this.User);
            serviceModel.UserId = user.Id;

            // Create appointment in server
            var appointmentId = await this.appointmentService.Book(serviceModel);

            var scheme = this.HttpContext.Request.Scheme;
            var baseUrl = this.HttpContext.Request.Host.Value;

            //TODO: Translate messages into Bulgarian
            var urlElement = $"<a href=\"{scheme}://{baseUrl}/home/appointment\"><h3>Appointment Details and Approval</h3></a>";
            var adminEmailText = $"<div>Hello, </div> <div></div> <div>You have a new appointment. Have a look below:</div><div></div><div>{urlElement}</div><div></div><div>Thank you!</div>";
            var userEmailText = $"<div>Hello, </div> <div></div> <div>{AppointmentAwaitingApprovalText}</div><div>Thank you!</div>";

            var adminEmail = (await this.userManager.GetUsersInRoleAsync(GlobalConstants.AdministratorRoleName)).FirstOrDefault().Email;

            // Send emails to admin and user
            await this.emailSender.SendEmailAsync(user.Email, user.UserName, adminEmail, this.GetEmailSubject(serviceModel.UtcStart), adminEmailText);

            await this.emailSender.SendEmailAsync(adminEmail, GlobalConstants.SystemName, user.Email, this.GetEmailSubject(serviceModel.UtcStart), userEmailText);
            return this.Ok();
        }

        //[HttpPost]
        //[Route("Approve")]
        //public async Task<ActionResult> Approve([FromForm] bool evaluation, [FromForm] int id, [FromForm] string declineReasoning)
        //{
        //    var appointmentFromDb = await this.appointmentService.GetById(id);
        //    var appointmentUser = await this.userManager.FindByIdAsync(appointmentFromDb.UserId);
        //    var currentUser = await this.userManager.GetUserAsync(this.User);

        //    var adminEmail = (await this.userManager.GetUsersInRoleAsync(GlobalConstants.AdministratorRoleName)).FirstOrDefault().Email;

        //    if (evaluation)
        //    {
        //        // Appointment is approved
        //        var emailContent = $"<div>Hello, </div> <div></div> <div>{AppointmentConfirmationString}</div><div>Thanks!</div>";
        //        await this.emailSender.SendEmailAsync(adminEmail, GlobalConstants.SystemName, appointmentUser.Email, this.GetEmailSubject(appointmentFromDb.Start), emailContent);
        //        await this.appointmentService.Approve(id);
        //        return this.Ok();
        //    }

        //    // Appointment is declined or cancelled
        //    // Allow only admin to cancel others' appointments
        //    if (appointmentFromDb.UserId != currentUser.Id && (!this.User.IsInRole(GlobalConstants.AdministratorRoleName)))
        //    {
        //        return this.BadRequest();
        //    }

        //    await this.appointmentService.Delete(id);

        //    // Don't send emails if admin cancels their own appointment(unavailability slot)
        //    if (currentUser.Id == appointmentFromDb.UserId && this.User.IsInRole(GlobalConstants.AdministratorRoleName))
        //    {
        //        return this.Ok();
        //    }

        //    var emailText = $"<div>Hello, </div> <div></div> <div>{AppointmentCancellationIntro}</div> <div>{declineReasoning?.Replace("\n", "<br>")}</div> <div>Thank you.</div>";

        //    // Send email to user if admin cancels or vice versa
        //    if (this.User.IsInRole(GlobalConstants.AdministratorRoleName))
        //    {
        //        await this.emailSender.SendEmailAsync(adminEmail, GlobalConstants.SystemName, appointmentUser.Email, this.GetEmailSubject(appointmentFromDb.Start), emailText);
        //    }
        //    else
        //    {
        //        await this.emailSender.SendEmailAsync(appointmentUser.Email, appointmentUser.UserName, adminEmail, this.GetEmailSubject(appointmentFromDb.Start), emailText);
        //    }

        //    return this.Ok();
        //}

        [HttpPost]
        [Authorize(Roles = GlobalConstants.AdministratorRoleName)]
        [Route("SetWorkingHours")]
        public ActionResult SetWorkingHours([FromForm] string startHour, [FromForm] string endHour)
        {
            GlobalAdminValues.WorkDayStart = int.Parse(startHour.Split(':')[0]);
            GlobalAdminValues.WorkDayEnd = int.Parse(endHour.Split(':')[0]);
            return this.Ok();
        }

        [HttpGet]
        [Route("GetWorkingHours")]
        public ActionResult<int[]> GetWorkingHours() => new int[] { GlobalAdminValues.WorkDayStart, GlobalAdminValues.WorkDayEnd };

        private string GetEmailSubject(DateTime start) => $"{GlobalConstants.SystemName} {AppointmentEmailSubject} on {start:dd MMMM HH:mm}";
    }
}
