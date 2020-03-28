using BDInSelfLove.Data.Common.Repositories;
using BDInSelfLove.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDInSelfLove.Services.Data.User
{
    public class UserService : IUserService
    {
        private readonly IDeletableEntityRepository<ApplicationUser> userRepository;

        public UserService(IDeletableEntityRepository<ApplicationUser> userRepository)
        {
            this.userRepository = userRepository;
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
