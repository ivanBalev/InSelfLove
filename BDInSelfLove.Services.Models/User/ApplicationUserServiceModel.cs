namespace BDInSelfLove.Services.Models.User
{
    using BDInSelfLove.Data.Models;
    using BDInSelfLove.Services.Mapping;
    using Microsoft.AspNetCore.Identity;

    public class ApplicationUserServiceModel : IdentityUser, IMapFrom<ApplicationUser>, IMapTo<ApplicationUser>
    {
        public string ProfilePhoto { get; set; }
    }
}
