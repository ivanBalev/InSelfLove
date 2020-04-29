namespace BDInSelfLove.Web.ViewModels.Forum.Profile
{
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Services.Models.User;

    public class ProfileUserViewModel : IMapFrom<ApplicationUserServiceModel>
    {
        public string Id { get; set; }

        public string UserName { get; set; }

        public string ProfilePhoto { get; set; }

        public int PostsCount { get; set; }

        public int CommentsCount { get; set; }

    }
}
