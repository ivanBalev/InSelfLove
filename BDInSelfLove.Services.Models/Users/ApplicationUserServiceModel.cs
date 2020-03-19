namespace BDInSelfLove.Services.Models.Users
{
    using BDInSelfLove.Data.Models;
    using BDInSelfLove.Services.Mapping;
    using Microsoft.AspNetCore.Identity;

    public class ApplicationUserServiceModel : IdentityUser, IMapFrom<ApplicationUser>, IMapTo<ApplicationUser>
    {
    }
}
