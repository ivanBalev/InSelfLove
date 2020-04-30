namespace BDInSelfLove.Services.Models.User
{
    using System;
    using System.Collections.Generic;
    using BDInSelfLove.Data.Models;
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Services.Models.Comment;
    using Microsoft.AspNetCore.Identity;

    public class ApplicationUserServiceModel : IdentityUser, IMapFrom<ApplicationUser>, IMapTo<ApplicationUser>
    {
        public string ProfilePhoto { get; set; }

        public int PostsCount { get; set; }

        public int CommentsCount { get; set; }

        public ICollection<ReportServiceModel> Reports { get; set; }

        public DateTime CreatedOn { get; set; }
    }
}
