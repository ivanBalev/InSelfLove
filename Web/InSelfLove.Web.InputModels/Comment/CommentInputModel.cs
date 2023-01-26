using InSelfLove.Services.Mapping;
using System.ComponentModel.DataAnnotations;

namespace InSelfLove.Web.InputModels.Comment
{
    public class CommentInputModel : IMapTo<Data.Models.Comment>
    {
        [Required]
        [MinLength(2, ErrorMessage = "Comment must be longer than 2 symbols")]
        public string Content { get; set; }

        public int? ArticleId { get; set; }

        public int? VideoId { get; set; }

        public int? ParentCommentId { get; set; }

        public string ResourceUrl { get; set; }
    }
}
