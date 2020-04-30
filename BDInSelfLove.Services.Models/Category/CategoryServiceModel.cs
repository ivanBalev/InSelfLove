namespace BDInSelfLove.Services.Models.Category
{
    using System.Collections.Generic;
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Services.Models.Post;
    using BDInSelfLove.Services.Models.User;

    public class CategoryServiceModel : IMapTo<Data.Models.Category>, IMapFrom<Data.Models.Category>
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string UserId { get; set; }

        public ApplicationUserServiceModel User { get; set; }

        public ICollection<PostServiceModel> Posts { get; set; }

        public PostServiceModel LastPost { get; set; }

        public int PostsCount { get; set; }

        public int CommentsCount { get; set; }
    }
}
