using BDInSelfLove.Data.Models;
using BDInSelfLove.Services.Models.User;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BDInSelfLove.Services.Data.User
{
    public interface IUserService
    {
        Task<string> GetProfilePicture(string userId);

        Task<bool> SetProfilePicture(ApplicationUser user, string profilePicture);

        Task<ApplicationUserServiceModel> GetProfileInfo(string username);
    }
}
