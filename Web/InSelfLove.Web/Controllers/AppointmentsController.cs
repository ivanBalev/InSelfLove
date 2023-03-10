namespace InSelfLove.Web.Controllers
{
    using System.Linq;
    using System.Threading.Tasks;

    using InSelfLove.Data.Models;
    using InSelfLove.Services.Data.Appointments;
    using InSelfLove.Services.Data.Helpers;
    using InSelfLove.Services.Mapping;
    using InSelfLove.Web.Controllers.Helpers;
    using InSelfLove.Web.InputModels.Appointment;
    using InSelfLove.Web.ViewModels.Appointment;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentsController : BaseController
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IAppointmentService appointmentService;
        private readonly IAppointmentEmailHelper emailSender;

        public AppointmentsController(
            IAppointmentService appointmentService,
            IAppointmentEmailHelper appointmentEmailHelper,
            UserManager<ApplicationUser> userManager)
        {
            this.appointmentService = appointmentService;
            this.emailSender = appointmentEmailHelper;
            this.userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string timezone)
        {
            // If request is redirect from Stripe, wait for StripeService
            // to finish updating the appointment's status to IsPaid = true
            // before returning data to client
            if (this.Request.Query.Keys.Any(k => k.ToLower().Equals("payment_intent")))
            {
                // Otherwise, we get the appointment before it's been updated
                // and return inaccurate data to client
                await Task.Delay(500);
            }

            // Update user timezone
            var userTimezone = await this.UpdateUserTimezone(timezone);

            // Gather data required by service
            string userId = this.userManager.GetUserId(this.User);
            string adminId = (await this.GetUser(admin: true)).Id;

            // Get appointments
            var appointments = await this.appointmentService.GetAll(userId, adminId, userTimezone);

            // Map db appointments to view model
            var appointmentsViewModel = appointments.Select(a =>
                    AutoMapperConfig.MapperInstance.Map<Appointment, AppointmentViewModel>(a));

            var viewModel = new AppointmentIndexViewModel
            {
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

            var admin = await this.GetUser(true);
            var user = await this.GetUser();

            // Notify user & admin
            await this.emailSender.SendEmail(
                appointment, fromAdmin: false, "NewAppointment", admin, user);
            await this.emailSender.SendEmail(
                appointment, fromAdmin: true, "AwaitingApproval", admin, user);

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
                await this.emailSender.SendEmail(
                    appointment, userId == adminId, "Cancellation", await this.GetUser(true), await this.GetUser());
            }

            await this.appointmentService.Cancel(appointment, userId, adminId);

            return this.Ok();
        }

        // Admin-only methods
        [HttpPost]
        [Route("Create")]
        [Authorize(Roles = AppConstants.AdministratorRoleName)]
        public async Task<IActionResult> Create([FromBody] AvailabilityInputModel availabilityInput)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest();
            }

            var adminTimezone = (await this.GetUser(true)).Timezone;

            // Send slots and date to service
            await this.appointmentService.Create(availabilityInput.TimeSlots, availabilityInput.Date, adminTimezone);
            return this.Ok();
        }

        [HttpPost]
        [Authorize(Roles = AppConstants.AdministratorRoleName)]
        [Route("Approve")]
        public async Task<ActionResult> Approve([FromBody] AppointmentManipulateModel input)
        {
            // Approve appointment in db
            var appointment = await this.appointmentService.Approve(input.Id);

            // Notify user
            await this.emailSender.SendEmail(
                appointment, fromAdmin: true, "Confirmation", await this.GetUser(true), await this.GetUser());

            return this.Ok();
        }

        [HttpPost]
        [Authorize(Roles = AppConstants.AdministratorRoleName)]
        [Route("Occupy")]
        public async Task<ActionResult> Occupy([FromBody] AppointmentManipulateModel input)
        {
            // Mark available appointment as occupied by admin.
            // Used for displaying appointments made via fb/insta/phone.
            await this.appointmentService.Occupy(input.Id, this.userManager.GetUserId(this.User));
            return this.Ok();
        }

        [HttpPost]
        [Authorize(Roles = AppConstants.AdministratorRoleName)]
        [Route("SetOnSite")]
        public async Task<ActionResult> SetOnSite([FromBody] AppointmentManipulateModel input)
        {
            // Mark if appointment can be on site
            await this.appointmentService.SetOnSite(input.Id, input.CanBeOnSite);

            return this.Ok();
        }

        // Helper methods

        // Updates user's timezone and returns it
        private async Task<string> UpdateUserTimezone(string timezoneFromQuery)
        {
            // If user hasn't consented to cookies, we get their timezone from query string
            string userCurrentTimezone = this.UserTimezoneIdFromCookie ?? timezoneFromQuery;

            var user = await this.GetUser();

            // If no data is sent by client, there's nothing to update with
            if (userCurrentTimezone == null)
            {
                return user?.Timezone;
            }

            // Compare current timezone from cookie/query param with stored timezone in db
            if (user != null &&
                user.Timezone.ToLower().CompareTo(userCurrentTimezone.ToLower()) != 0)
            {
                // Update if current & stored timezones don't match
                user.Timezone = userCurrentTimezone;
                await this.userManager.UpdateAsync(user);
            }

            // Return current timezone
            return userCurrentTimezone;
        }

        private async Task<ApplicationUser> GetUser(bool admin = false)
        {
            return admin ?
               (await this.userManager.GetUsersInRoleAsync(AppConstants.AdministratorRoleName))
               .FirstOrDefault() :
                await this.userManager.GetUserAsync(this.User);
        }
    }
}
