namespace InSelfLove.Services.Data.Stripe
{
    using System.Threading.Tasks;
    using InSelfLove.Services.Data.Helpers;

    public interface IStripeService
    {
        //Task<string> CreateProduct(string productName, long priceAmount);

        //Task<Session> CreateSession(
        //    string domain, string priceId, string successEndpoint,
        //    string cancelEndpoint, string clientReferenceInfo, int quantity = 1);

        Task<PaymentResult> HandlePayment(string json, string stripeSignature);

        Task<string> CreatePaymentIntent(int appointmentId, string userId);
    }
}
