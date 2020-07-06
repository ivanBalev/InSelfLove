namespace BDInSelfLove.Services.Data.User
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using BDInSelfLove.Data.Common.Repositories;
    using BDInSelfLove.Data.Models;
    using BDInSelfLove.Services.Models.User;
    using Microsoft.EntityFrameworkCore;

    public class UserService : IUserService
    {
        private const int UserBanThreshold = 3;
        private const int DefaultBanLength = 3;

        private readonly IDeletableEntityRepository<ApplicationUser> userRepository;

        public UserService(IDeletableEntityRepository<ApplicationUser> userRepository)
        {
            this.userRepository = userRepository;
        }

        public async Task<ApplicationUserServiceModel> GetProfileInfo(string username)
        {
            var user = await this.userRepository.All().Where(u => u.UserName == username)
                .Select(u => new ApplicationUserServiceModel
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    ProfilePhoto = u.ProfilePhoto,
                })
                .FirstOrDefaultAsync();

            return user;
        }

        public async Task<string> GetProfilePicture(string userId)
        {
            return await this.userRepository.All()
                .Where(u => u.Id == userId)
                .Select(u => u.ProfilePhoto)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> SetProfilePicture(ApplicationUser user, string profilePicture)
        {
            user.ProfilePhoto = profilePicture;

            this.userRepository.Update(user);
            int result = await this.userRepository.SaveChangesAsync();

            return result > 0;
        }
    }
}
