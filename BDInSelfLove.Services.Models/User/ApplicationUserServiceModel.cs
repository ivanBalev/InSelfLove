namespace BDInSelfLove.Services.Models.User
{
    using System;
    using BDInSelfLove.Data.Models;
    using BDInSelfLove.Services.Mapping;

    public class ApplicationUserServiceModel : IMapFrom<ApplicationUser>, IMapTo<ApplicationUser>
    {
        public string Id { get; set; }

        public string UserName { get; set; }

        public string Email { get; set; }

        public string ProfilePhoto { get; set; }

        public DateTime CreatedOn { get; set; }
    }
}
