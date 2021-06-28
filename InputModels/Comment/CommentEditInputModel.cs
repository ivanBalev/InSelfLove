using BDInSelfLove.Services.Mapping;
using BDInSelfLove.Services.Models.Comment;
using System.ComponentModel.DataAnnotations;

namespace BDInSelfLove.Web.InputModels.Comment
{
    public class CommentEditInputModel : IMapTo<CommentServiceModel>
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [MinLength(2, ErrorMessage = "Comment must be longer than 2 symbols.")]
        public string Content { get; set; }

        public int? ArticleId { get; set; }

        public int? VideoId { get; set; }

    }
}
