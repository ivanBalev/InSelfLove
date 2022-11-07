namespace BDInSelfLove.Services.Data.Stripe
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using BDInSelfLove.Data.Common.Repositories;
    using BDInSelfLove.Data.Models;
    using global::Stripe;
    using global::Stripe.Checkout;
    using Microsoft.EntityFrameworkCore;

    public class StripeService : IStripeService
    {
        private readonly IDeletableEntityRepository<ApplicationUser> userRepository;
        private readonly IDeletableEntityRepository<Course> courseRepository;
        private readonly IDeletableEntityRepository<Payment> paymentRepository;

        public StripeService(
            IDeletableEntityRepository<ApplicationUser> userRepository,
            IDeletableEntityRepository<Course> courseRepository,
            IDeletableEntityRepository<Payment> paymentRepository)
        {
            this.userRepository = userRepository;
            this.courseRepository = courseRepository;
            this.paymentRepository = paymentRepository;
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

        public async Task<int> StorePayment(Payment payment)
        {
            payment.AmountTotal = payment.AmountTotal / 100; // Just a quirk of the stripe api

            var course = await this.courseRepository.All().Include(c => c.ApplicationUsers).FirstOrDefaultAsync(c => c.Id.Equals(payment.CourseId));
            var user = await this.userRepository.All().Include(u => u.Courses).FirstOrDefaultAsync(u => u.Id.Equals(payment.ApplicationUserId));

            user.Courses.Add(course);

            await this.userRepository.SaveChangesAsync();
            await this.courseRepository.SaveChangesAsync();

            await this.paymentRepository.AddAsync(payment);
            await this.paymentRepository.SaveChangesAsync();

            return 1;
        }
    }
}
