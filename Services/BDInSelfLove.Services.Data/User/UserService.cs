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
using BDInSelfLove.Services.Data.Comment;

namespace BDInSelfLove.Services.Data.User
{
    public class UserService : IUserService
    {
        private const int UserBanThreshold = 3;
        private const int DefaultBanLength = 3;

        private readonly IDeletableEntityRepository<ApplicationUser> userRepository;

        public UserService(IDeletableEntityRepository<ApplicationUser> userRepository)
        {
            this.userRepository = userRepository;
        }

        public async Task CheckIfUserNeedsToBeBanned(string userId, int reportsCount)
        {
            if (reportsCount < UserBanThreshold - 1)
            {
                return;
            }

            var user = await this.userRepository.All().FirstOrDefaultAsync(u => u.Id == userId);

            if (user.IsBanned)
            {
                return;
            }

            user.IsBanned = true;
            user.Bans.Add(new Ban());

            this.userRepository.Update(user);
            await this.userRepository.SaveChangesAsync();
        }

        public async Task<bool> CheckIfBanNeedsToBeLifted(string userId)
        {
            var user = await this.userRepository.All().Include(u => u.Bans).FirstOrDefaultAsync(u => u.Id == userId);

            var daysSinceBanned = (DateTime.UtcNow - user.Bans.FirstOrDefault().CreatedOn).TotalDays;

            if (!(daysSinceBanned > DefaultBanLength))
            {
                return false;
            }

            user.IsBanned = false;
            user.Bans.FirstOrDefault().IsDeleted = true;
            await this.userRepository.SaveChangesAsync();
            return true;
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
