namespace InSelfLove.Services.Data.Stripe
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using InSelfLove.Data.Common.Repositories;
    using InSelfLove.Data.Models;
    using global::Stripe;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Linq;
    using InSelfLove.Services.Data.Helpers;
    using InSelfLove.Services.Data.Appointments;

    public class StripeService : IStripeService
    {
        private readonly IDeletableEntityRepository<ApplicationUser> userRepository;
        private readonly IDeletableEntityRepository<Payment> paymentRepository;
        private readonly IConfiguration configuration;
        private readonly ILogger<StripeService> logger;
        private readonly IAppointmentService appointmentService;

        public StripeService(
            IDeletableEntityRepository<ApplicationUser> userRepository,
            IDeletableEntityRepository<Payment> paymentRepository,
            IAppointmentService appointmentService,
            IConfiguration configuration,
            ILogger<StripeService> logger)
        {
            this.userRepository = userRepository;
            this.paymentRepository = paymentRepository;
            this.configuration = configuration;
            this.logger = logger;
            this.appointmentService = appointmentService;
        }

        public async Task<string> CreatePaymentIntent(int objectId, string userId)
        {
            // Validate
            // Appointment needs to exist and be unpaid
            var appointment = await this.appointmentService.GetById(objectId);
            if (appointment == null || appointment.IsPaid)
            {
                return null;
            }

            // Create payment intent
            var paymentIntentService = new PaymentIntentService();
            var paymentIntent = paymentIntentService.Create(new PaymentIntentCreateOptions
            {
                // 50,00. Stripe requires the format below
                Amount = 5000,
                Currency = "bgn",
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true,
                },

                // Attach appointment & user ids. Allows us to identify the user & appointment
                // in the POST request the Stripe server sends to our webhook to confirm/decline payment
                Metadata = new Dictionary<string, string>()
                {
                    { nameof(PaymentResult.ObjectId), objectId.ToString() },
                    { nameof(userId), userId },
                },
            });

            // Return the client secret. It's then attached to the payment form
            // And returned to our webhook, so we can validate it hasn't been tampered with
            return paymentIntent.ClientSecret;
        }

        public async Task<PaymentResult> HandlePayment(string json, string stripeSignature)
        {
            // Payment status and object id for controller
            string status = "";
            int objectId = 0;

            try
            {
                // Validate client secret
                var stripeEvent = EventUtility.ConstructEvent(
                  json,
                  stripeSignature,
                  this.configuration["Stripe:TestSecret"]);

                // Construct payment intent object
                var paymentIntent = stripeEvent.Data.Object as PaymentIntent;

                // Parse our target object's id (if one is given)
                int.TryParse(paymentIntent?.Metadata[nameof(PaymentResult.ObjectId)], out objectId);

                // If payment has succeeded
                if (stripeEvent.Type == Events.PaymentIntentSucceeded)
                {
                    // Store payment info in db, log result & set status
                    await this.FulfillOrder(paymentIntent.Metadata["userId"], objectId, paymentIntent.Amount);
                    this.logger.LogInformation($"A successful payment for {paymentIntent.Amount} was made.");
                    status = "Successful";
                }
                else if (stripeEvent.Type == Events.PaymentIntentProcessing)
                {
                    status = "Processing";
                }
                else if (stripeEvent.Type == Events.PaymentIntentPaymentFailed)
                {
                    // Log our error & set status
                    this.logger.LogError($"Unhandled event type: {stripeEvent.Type}");
                    status = "Failed";
                }
            }
            catch (StripeException e)
            {
                // Issue validating the webhook's signature (client secret)
                this.logger.LogError("STRIPE PAYMENT EXCEPTION" + e.Message + Environment.NewLine + "JSON: " + json);
            }

            // Return payment status to controller
            return new PaymentResult() { ObjectId = objectId, Status = status };
        }

        private async Task<int> StorePayment(Payment payment)
        {
            var user = await this.userRepository.All().Include(u => u.Courses).FirstOrDefaultAsync(u => u.Id.Equals(payment.ApplicationUserId));
            var appointment = await this.appointmentService.GetById(payment.AppointmentId ?? 0);

            // Confirm user & appointment exist and match
            if (user == null || appointment == null || user.Id != appointment.UserId)
            {
                throw new ArgumentNullException(nameof(user) + " " + nameof(appointment));
            }

            // Update db
            await this.paymentRepository.AddAsync(payment);
            await this.paymentRepository.SaveChangesAsync();
            await this.appointmentService.Pay(appointment);

            return 1;
        }

        private async Task FulfillOrder(string userId, int objectId, long amountTotal)
        {
            var payment = new Payment
            {
                ApplicationUserId = userId,
                AppointmentId = objectId,
                AmountTotal = amountTotal / 100,
                //StripeCustomerId = session.CustomerId,
            };

            await this.StorePayment(payment);
        }
    }
}
