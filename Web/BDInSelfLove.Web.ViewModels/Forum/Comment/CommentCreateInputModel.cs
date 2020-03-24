namespace BDInSelfLove.Web.ViewModels.Forum.Comment
{
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Services.Models.Comment;

    public class CommentCreateInputModel : IMapTo<CommentServiceModel>
    {
        public string Content { get; set; }

        public int ParentPostId { get; set; }
    }
}
