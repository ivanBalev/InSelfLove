namespace BDInSelfLove.Web.ViewModels.Forum.Post
{
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Services.Models.Post;
    using System.ComponentModel.DataAnnotations;

    public class PostCreateInputModel : IMapTo<PostServiceModel>
    {
        [Required]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        public int CategoryId { get; set; }
    }
}
