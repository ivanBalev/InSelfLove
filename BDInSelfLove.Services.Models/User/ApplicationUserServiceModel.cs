namespace BDInSelfLove.Services.Models.User
{
    using BDInSelfLove.Data.Models;
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Services.Models.Comment;
    using Microsoft.AspNetCore.Identity;
    using System.Collections.Generic;

    public class ApplicationUserServiceModel : IdentityUser, IMapFrom<ApplicationUser>, IMapTo<ApplicationUser>
    {
        public string ProfilePhoto { get; set; }

        public int PostsCount { get; set; }

        public int CommentsCount { get; set; }

        public ICollection<ReportServiceModel> Reports { get; set; }
    }
}
