using BDInSelfLove.Data.Common.Repositories;
using BDInSelfLove.Data.Models;
using BDInSelfLove.Services.Models.User;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BDInSelfLove.Services.Mapping;

namespace BDInSelfLove.Services.Data.User
{
    public class UserService : IUserService
    {
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
                    PostsCount = u.Posts.Count,
                    CommentsCount = u.Comments.Count,
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
