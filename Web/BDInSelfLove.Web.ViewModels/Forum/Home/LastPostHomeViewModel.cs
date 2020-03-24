namespace BDInSelfLove.Web.ViewModels.Forum.Home
{
    using System;

    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Services.Models.Post;

    public class LastPostHomeViewModel : IMapFrom<PostServiceModel>
    {
        public string Title { get; set; }

        public int Id { get; set; }

        public string UserUserName { get; set; }

        public string UserId { get; set; }

        public DateTime CreatedOn { get; set; }
    }
}
