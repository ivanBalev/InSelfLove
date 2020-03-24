namespace BDInSelfLove.Web.ViewModels.Forum.Post
{
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Services.Models.Post;

    public class PostCreateInputModel : IMapTo<PostServiceModel>
    {
        public string Title { get; set; }

        public string Content { get; set; }

        public int CategoryId { get; set; }
    }
}
