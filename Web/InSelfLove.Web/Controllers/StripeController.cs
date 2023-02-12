namespace InSelfLove.Web.Controllers
{
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using InSelfLove.Data.Models;
    using InSelfLove.Services.Data.Appointments;
    using InSelfLove.Services.Data.Helpers;
    using InSelfLove.Services.Data.Stripe;
    using InSelfLove.Web.Controllers.Helpers;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;

    public class StripeController : BaseController
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IStripeService stripeService;
        private readonly IAppointmentService appointmentService;
        private readonly IAppointmentEmailHelper emailSender;
        private readonly IConfiguration config;

        public StripeController(
            UserManager<ApplicationUser> userManager,
            IStripeService stripeService,
            IAppointmentService appointmentService,
            IAppointmentEmailHelper emailSender,
            IConfiguration config)
        {
            this.userManager = userManager;
            this.stripeService = stripeService;
            this.appointmentService = appointmentService;
            this.emailSender = emailSender;
            this.config = config;
        }

        [HttpGet]
        [Authorize]
        public IActionResult Config()
        {
            return this.Ok(new { publishableKey = this.config["Stripe:PublishableKey"] });
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<JsonResult> CreatePaymentIntent([FromBody] int appointmentId)
        {
            // TODO: allow online payment only for confirmed emails;
            var userId = this.userManager.GetUserId(this.User);

            // Return client secret to be attached to our payment form 
            return this.Json(new { clientSecret = await this.stripeService.CreatePaymentIntent(appointmentId, userId) });
        }

        // CSRF check is done in service
        // Standard practice with the Stripe API
        [IgnoreAntiforgeryToken]
        [HttpPost]
        public async Task<IActionResult> ConfirmPay()
        {
            var json = await new StreamReader(this.HttpContext.Request.Body).ReadToEndAsync();
            var stripeSignature = this.Request.Headers["Stripe-Signature"];

            // Provide necessary info to service for validation & confirmation/cancellation of payment
            var paymentResult = await this.stripeService.HandlePayment(json, stripeSignature);

            // Send email only if payment is successful
            if (!string.IsNullOrEmpty(paymentResult.Status) && paymentResult.ObjectId != 0)
            {
                var appointment = await this.appointmentService.GetById(paymentResult.ObjectId);

                var admin = (await this.userManager.GetUsersInRoleAsync(AppConstants.AdministratorRoleName)).FirstOrDefault();
                var user = await this.userManager.FindByIdAsync(appointment.UserId);

                await this.emailSender.SendEmail(appointment, true, paymentResult.Status, admin, user);
            }

            return this.Ok();
        }
    }
}
