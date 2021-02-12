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
    using BDInSelfLove.Services.Models.Appointment;
    using BDInSelfLove.Web.Areas.Administration;
    using BDInSelfLove.Web.InputModels.Appointment;
    using BDInSelfLove.Web.ViewModels.Appointment;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentController : ControllerBase
    {
        private const string AppointmentEmailSubject = "Appointment";
        private const string AppointmentCancellationIntro = "I'm deeply sorry but I'm going to have to cancel the appointment.";
        private const string AppointmentConfirmationString = "Your appointment has been confirmed. See you soon!";
        private const string AppointmentAwaitingApprovalText = "Your request for an appointment has been received. Please wait for approval.";

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

        [HttpGet]
        [Authorize(Roles = GlobalConstants.AdministratorRoleName)]
        [Route("AdminAppointments")]
        public async Task<ActionResult<ICollection<AppointmentViewModel>>> AdminAppointments()
        {
            // Is user admin?
            if (!this.User.IsInRole(GlobalConstants.AdministratorRoleName))
            {
                return this.BadRequest();
            }

            // Get all appointments and wrap for client
            var appointmentViewList = await this.appointmentService.GetAll(GlobalConstants.AdministratorRoleName)
                .To<AppointmentViewModel>()
                .ToListAsync();

            var currentUserUserName = this.userManager.GetUserName(this.User);
            this.MarkOwnAppointments(appointmentViewList, currentUserUserName);

            return appointmentViewList;
        }

        [HttpGet]
        [Route("AvailableAppointments")]
        public async Task<ActionResult<ICollection<AppointmentViewModel>>> AvailableAppointments()
        {
            var currentUserUserName = this.userManager.GetUserName(this.User);

            // Get all appointments from db
            var appointmentViewList = await this.appointmentService
                .GetAllForDaysAhead(GlobalAdminValues.AvailabilitySpanInDays, currentUserUserName)
                .To<AppointmentViewModel>()
                .ToListAsync();

            var today = DateTime.Today;
            this.MarkOwnAppointments(appointmentViewList, currentUserUserName);
            var appointments = this.GetAvailableSlots(appointmentViewList.Where(a => a.Start.Date >= today.Date && a.Start.Date <= today.AddDays(GlobalAdminValues.AvailabilitySpanInDays).Date).ToList());

            // Include user's own appointments in response
            appointments.AddRange(appointmentViewList.Where(a => a.IsOwn));

            return appointments;
        }


        [HttpPost]
        [Route("Save")]
        public async Task<IActionResult> Save([FromForm] AppointmentInputModel inputModel)
        {
            // Create model for service
            var serviceModel = AutoMapperConfig.MapperInstance.Map<AppointmentServiceModel>(inputModel);

            // Validate
            var appointmentSlotIsOccupied = await this.appointmentService
                                                      .GetAllByDate(serviceModel.Start)
                                                      .Where(a => a.Start.Hour == serviceModel.Start.Hour)
                                                      .FirstOrDefaultAsync();
            if (appointmentSlotIsOccupied != null)
            {
                return this.BadRequest();
            }

            // Set up model for service
            var user = await this.userManager.GetUserAsync(this.User);
            serviceModel.UserId = user.Id;

            // Create appointment in server
            var appointmentId = await this.appointmentService.Create(serviceModel);

            // Enter user phone number, if provided
            if (!string.IsNullOrEmpty(serviceModel.PhoneNumber) && (await this.userManager.GetPhoneNumberAsync(user)) == null)
            {
                await this.userManager.SetPhoneNumberAsync(user, serviceModel.PhoneNumber);
            }

            var urlElement = "<a href=\"https://localhost:44319/home/appointment\"><h3>Appointment Details and Approval</h3></a>";
            var adminEmailText = $"<div>Hello, </div> <div></div> <div>You have a new appointment. Have a look below:</div><div></div><div>{urlElement}</div><div></div><div>Thank you!</div>";
            var userEmailText = $"<div>Hello, </div> <div></div> <div>{AppointmentAwaitingApprovalText}</div><div>Thank you!</div>";

            // Send emails to admin
            await this.emailSender.SendEmailAsync(user.Email, user.UserName, GlobalConstants.SystemEmail, this.GetEmailSubject(serviceModel.Start), adminEmailText);

            await this.emailSender.SendEmailAsync(GlobalConstants.SystemEmail, GlobalConstants.SystemName, user.Email, this.GetEmailSubject(serviceModel.Start), userEmailText);
            return this.Ok();
        }

        [HttpPost]
        [Route("Approve")]
        public async Task<ActionResult> Approve([FromForm] bool evaluation, [FromForm] int id, [FromForm] string declineReasoning)
        {
            var appointmentFromDb = await this.appointmentService.GetById(id);
            var appointmentUser = await this.userManager.FindByIdAsync(appointmentFromDb.UserId);
            var currentUser = await this.userManager.GetUserAsync(this.User);

            if (evaluation)
            {
                // Appointment is approved
                var emailContent = $"<div>Hello, </div> <div></div> <div>{AppointmentConfirmationString}</div><div>Thanks!</div>";
                await this.emailSender.SendEmailAsync(GlobalConstants.SystemEmail, GlobalConstants.SystemName, appointmentUser.Email, this.GetEmailSubject(appointmentFromDb.Start), emailContent);
                await this.appointmentService.Approve(id);
                return this.Ok();
            }

            // Appointment is declined or cancelled
            // Allow only admin to cancel others' appointments
            if (appointmentFromDb.UserId != currentUser.Id && (!this.User.IsInRole(GlobalConstants.AdministratorRoleName)))
            {
                return this.BadRequest();
            }

            await this.appointmentService.Delete(id);

            // Don't send emails if admin cancels their own appointment(unavailability slot)
            if (currentUser.Id == appointmentFromDb.UserId && this.User.IsInRole(GlobalConstants.AdministratorRoleName))
            {
                return this.Ok();
            }
            // TODO: Below gives error when reasoning is NULL
            var emailText = $"<div>Hello, </div> <div></div> <div>{AppointmentCancellationIntro}</div> <div>{declineReasoning.Replace("\n", "<br>")}</div> <div>Thank you.</div>";

            // TODO: system email needs to be set by admin
            // Send email to user if admin cancels or vice versa
            if (this.User.IsInRole(GlobalConstants.AdministratorRoleName))
            {
                await this.emailSender.SendEmailAsync(GlobalConstants.SystemEmail, GlobalConstants.SystemName, appointmentUser.Email, this.GetEmailSubject(appointmentFromDb.Start), emailText);
            }
            else
            {
                await this.emailSender.SendEmailAsync(appointmentUser.Email, appointmentUser.UserName, GlobalConstants.SystemEmail, this.GetEmailSubject(appointmentFromDb.Start), emailText);
            }

            return this.Ok();
        }

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
        [Authorize(Roles = GlobalConstants.AdministratorRoleName)]
        [Route("GetWorkingHours")]
        public ActionResult<int[]> GetWorkingHours() => new int[] { GlobalAdminValues.WorkDayStart, GlobalAdminValues.WorkDayEnd };

        [HttpPost]
        [Route("SubmitDailyAvailability")]
        [Authorize(Roles = GlobalConstants.AdministratorRoleName)]
        public async Task<IActionResult> SubmitDailyAvailability([FromForm] AvailabilityInputModel availabilityInput)
        {
            // Get only the required part of the strings to create datetime objects for db
            List<string> timeSlotsInput = new List<string>();
            for (int i = 0; i < availabilityInput.TimeSlots?.Count; i++)
            {
                var currentSlot = availabilityInput.TimeSlots[i];
                var stringToAdd = string.Join(' ', currentSlot.Split(' ', StringSplitOptions.RemoveEmptyEntries).Skip(1).Take(4));
                timeSlotsInput.Add(stringToAdd);
            }

            var dateString = string.Join(' ', availabilityInput.Date.Split(' ', StringSplitOptions.RemoveEmptyEntries).Skip(1).Take(4));
            var date = DateTime.ParseExact(dateString, "MMM dd yyyy HH:mm:ss", CultureInfo.InvariantCulture);

            // Set up and parse to integers for comparison between old and new availability
            var newAvailabilityHours = timeSlotsInput.Select(ts => int.Parse(ts.Split(' ')[3].Split(':')[0]));
            var regularAvailabilityHours = Enumerable.Range(GlobalAdminValues.WorkDayStart, GlobalAdminValues.WorkDayEnd - GlobalAdminValues.WorkDayStart);

            var dummyAppointments = new List<AppointmentServiceModel>();
            var admin = await this.userManager.GetUserAsync(this.User);

            // Create dummy appointments for all unavailable slots
            foreach (var slot in regularAvailabilityHours.Where(rah => !newAvailabilityHours.Any(nah => nah == rah)))
            {
                var slotDateTime = new DateTime(date.Year, date.Month, date.Day, slot, 0, 0);
                dummyAppointments.Add(new AppointmentServiceModel()
                {
                    Start = slotDateTime,
                    UserId = admin.Id,
                });
            }

            await this.appointmentService.SubmitDailyWorkingHours(dummyAppointments, date, this.userManager.GetUserId(this.User));
            return this.Ok();
        }

        private string GetEmailSubject(DateTime start) => $"{GlobalConstants.SystemName} {AppointmentEmailSubject} on {start:dd MMMM HH:mm}";

        private void MarkOwnAppointments(List<AppointmentViewModel> appointmentViewList, string currentUserUserName)
        {
            for (int i = 0; i < appointmentViewList.Count; i++)
            {
                appointmentViewList[i].IsOwn = currentUserUserName == appointmentViewList[i].UserUserName;
            }
        }

        private List<AppointmentViewModel> GetAvailableSlots(List<AppointmentViewModel> appointmentViewList)
        {
            var currentLocalTime = DateTime.UtcNow.AddHours(2);
            var availableAppointmentSlots = new List<AppointmentViewModel>();

            for (int dayOfAvailability = 0; dayOfAvailability <= GlobalAdminValues.AvailabilitySpanInDays; dayOfAvailability++)
            {
                // Fix this to work for days that are not today(disregard hours)
                var currentAvailableDay = currentLocalTime.AddDays(dayOfAvailability);

                List<AppointmentViewModel> currentDayAppointments = appointmentViewList
                    .Where(a => a.Start.Date == currentAvailableDay.Date).ToList();

                // Create available slot for all unoccupied current day slots
                for (int currentHour = GlobalAdminValues.WorkDayStart; currentHour < GlobalAdminValues.WorkDayEnd; currentHour++)
                {
                    if (dayOfAvailability == 0 && currentHour <= currentLocalTime.Hour)
                    {
                        continue;
                    }

                    if (!currentDayAppointments.Any(a => a.Start.Hour == currentHour))
                    {
                        // Remove UTC indication for client
                        DateTime availableSlot = DateTime.Parse(currentAvailableDay.Date.ToString().Trim('Z'));
                        availableSlot = availableSlot.AddHours(currentHour);

                        availableAppointmentSlots.Add(new AppointmentViewModel()
                        {
                            Start = availableSlot,
                            IsApproved = true,
                        });
                    }
                }
            }

            return availableAppointmentSlots;
        }
    }
}
