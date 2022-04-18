namespace BDInSelfLove.Services.Data.Recaptcha
{
    using System.Threading.Tasks;

    public interface IRecaptchaService
    {
        Task<string> VerifyAsync(string recaptchaResponse, string expectedAction);
    }
}
