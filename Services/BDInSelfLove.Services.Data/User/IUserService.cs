namespace BDInSelfLove.Services.Data.User
{
    using System.Threading.Tasks;

    using BDInSelfLove.Data.Models;
    using BDInSelfLove.Services.Models.User;

    public interface IUserService
    {
        Task<string> GetProfilePicture(string userId);

        Task<bool> SetProfilePicture(ApplicationUser user, string profilePicture);

        Task<ApplicationUserServiceModel> GetProfileInfo(string username);
    }
}
