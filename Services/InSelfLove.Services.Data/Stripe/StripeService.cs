namespace InSelfLove.Services.Data.Stripe
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using InSelfLove.Data.Common.Repositories;
    using InSelfLove.Data.Models;
    using global::Stripe;
    using global::Stripe.Checkout;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Linq;
    using InSelfLove.Services.Data.Helpers;

    public class StripeService : IStripeService
    {
        private readonly IDeletableEntityRepository<ApplicationUser> userRepository;
        private readonly IDeletableEntityRepository<Course> courseRepository;
        private readonly IDeletableEntityRepository<Payment> paymentRepository;
        private readonly IConfiguration configuration;
        private readonly ILogger<StripeService> logger;
        private readonly IDeletableEntityRepository<Appointment> appointmentRepository;

        public StripeService(
            IDeletableEntityRepository<ApplicationUser> userRepository,
            IDeletableEntityRepository<Course> courseRepository,
            IDeletableEntityRepository<Payment> paymentRepository,
            IDeletableEntityRepository<Appointment> appointmentRepository,
            IConfiguration configuration,
            ILogger<StripeService> logger)
        {
            this.userRepository = userRepository;
            this.courseRepository = courseRepository;
            this.paymentRepository = paymentRepository;
            this.configuration = configuration;
            this.logger = logger;
            this.appointmentRepository = appointmentRepository;
        }

        public async Task<string> CreateProduct(string productName, long priceAmount)
        {
            var options = new PriceCreateOptions
            {
                Currency = "bgn",
                UnitAmount = priceAmount * 100, // Stripe reqiures num value where last 2 digits = cents
                ProductData = new PriceProductDataOptions { Name = productName },
            };
            var service = new PriceService();
            var price = await service.CreateAsync(options);

            return price.Id;
        }

        public async Task<Session> CreateSession(string domain, string priceId, string successEndpoint,
            string cancelEndpoint, string clientReferenceInfo, int quantity = 1)
        {
            var options = new SessionCreateOptions
            {
                LineItems = new List<SessionLineItemOptions>
                {
                  new SessionLineItemOptions
                  {
                    Price = priceId,
                    Quantity = quantity,
                  },
                },
                Mode = "payment",
                SuccessUrl = domain + successEndpoint,
                CancelUrl = domain + cancelEndpoint,
                ClientReferenceId = clientReferenceInfo,
            };
            var service = new SessionService();
            Session session = await service.CreateAsync(options);
            return session;
        }

        public string CreatePaymentIntent(int objectId, string userId)
        {
            var paymentIntentService = new PaymentIntentService();

            var paymentIntent = paymentIntentService.Create(new PaymentIntentCreateOptions
            {
                Amount = 5000,
                Currency = "bgn",
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true,
                },
                Metadata = new Dictionary<string, string>()
                {
                    { nameof(PaymentResult.ObjectId), objectId.ToString() },
                    { nameof(userId), userId },
                },
            });

            return paymentIntent.ClientSecret;
        }

        public async Task<PaymentResult> HandlePayment(string json, string stripeSignature)
        {
            string status = "";
            int objectId = 0;

            try
            {
                // Validation
                var stripeEvent = EventUtility.ConstructEvent(
                  json,
                  stripeSignature,
                  this.configuration["Stripe:TestSecret"]);

                var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                int.TryParse(paymentIntent?.Metadata[nameof(PaymentResult.ObjectId)], out objectId);

                if (stripeEvent.Type == Events.PaymentIntentSucceeded)
                {
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
                    this.logger.LogError($"Unhandled event type: {stripeEvent.Type}");
                    status = "Failed";
                }
            }
            catch (StripeException e)
            {
                // Issue validating the webhook's signature
                this.logger.LogError("STRIPE PAYMENT EXCEPTION" + e.Message + Environment.NewLine + "JSON: " + json);
            }

            return new PaymentResult() { ObjectId = objectId, Status = status};
        }

        public async Task<int> StorePayment(Payment payment)
        {
            var user = await this.userRepository.All().Include(u => u.Courses).FirstOrDefaultAsync(u => u.Id.Equals(payment.ApplicationUserId));
            var appointment = await this.appointmentRepository.All().Where(a => a.Id == payment.AppointmentId).FirstOrDefaultAsync();

            if (user == null || appointment == null || user.Id != appointment.UserId)
            {
                throw new ArgumentNullException(nameof(user) + " " + nameof(appointment));
            }

            await this.paymentRepository.AddAsync(payment);
            appointment.IsPaid = true;
            this.appointmentRepository.Update(appointment);

            await this.appointmentRepository.SaveChangesAsync();
            await this.paymentRepository.SaveChangesAsync();

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
