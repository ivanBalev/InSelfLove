using BDInSelfLove.Services.Mapping;
using System.ComponentModel.DataAnnotations;

namespace BDInSelfLove.Web.InputModels.Comment
{
    public class CommentInputModel : IMapTo<Data.Models.Comment>
    {
        [Required]
        [MinLength(2, ErrorMessage = "Comment must be longer than 2 symbols")]
        public string Content { get; set; }

        public int? ArticleId { get; set; }

        public int? VideoId { get; set; }

        public int? ParentCommentId { get; set; }
    }
}
