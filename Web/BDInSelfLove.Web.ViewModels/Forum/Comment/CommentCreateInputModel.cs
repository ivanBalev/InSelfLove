namespace BDInSelfLove.Web.ViewModels.Forum.Comment
{
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Services.Models.Comment;
    using System.ComponentModel.DataAnnotations;

    public class CommentCreateInputModel : IMapTo<CommentServiceModel>
    {
        [Required]
        public string Content { get; set; }

        [Required]
        public int ParentPostId { get; set; }

        public int? ParentCommentId { get; set; }
    }
}
