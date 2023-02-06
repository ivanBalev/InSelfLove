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

        public async Task<int> HandlePayment(string json, string stripeSignature)
        {
            try
            {
                // Validation
                var stripeEvent = EventUtility.ConstructEvent(
                  json,
                  stripeSignature,
                  this.configuration["Stripe:TestSecret"]);

                // Rudimentary error handling
                if (stripeEvent.Type.Contains("fail"))
                {
                    this.logger.LogError("STRIPE PAYMENT FAILED" + json);
                    return 0;
                }

                // Payment intent.succeeded event instead of checkoutcompleted?
                // Handle the checkout.session.completed event
                if (stripeEvent.Type == Events.CheckoutSessionCompleted)
                {
                    var session = stripeEvent.Data.Object as Session;
                    await this.FulfillOrder(session);
                }
            }
            catch (StripeException e)
            {
                this.logger.LogError("STRIPE PAYMENT EXCEPTION" + e.Message + Environment.NewLine + "JSON: " + json);
                return 0;
            }

            return 1;
        }

        public async Task<int> StorePayment(Payment payment)
        {
            payment.AmountTotal = payment.AmountTotal / 100; // Just a quirk of the stripe api

            var user = await this.userRepository.All().Include(u => u.Courses).FirstOrDefaultAsync(u => u.Id.Equals(payment.ApplicationUserId));
            var appointment = await this.appointmentRepository.All().Where(a => a.Id == payment.AppointmentId).FirstOrDefaultAsync();

            if (user == null || appointment == null)
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

        private async Task FulfillOrder(Session session)
        {
            // Get user reference info set in BuyCourse Action
            var userCourseInfo = session.ClientReferenceId.Split(", ");

            var payment = new Payment
            {
                ApplicationUserId = userCourseInfo[0].Split(": ")[1],
                AppointmentId = int.Parse(userCourseInfo[1].Split(": ")[1]),
                StripeCustomerId = session.CustomerId,
                AmountTotal = (long)session.AmountTotal,
            };

            await this.StorePayment(payment);
        }
    }
}
