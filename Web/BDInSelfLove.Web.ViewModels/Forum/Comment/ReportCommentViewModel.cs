namespace BDInSelfLove.Web.ViewModels.Forum.Comment
{
    using System.ComponentModel.DataAnnotations;

    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Services.Models.Comment;

    public class ReportCommentViewModel : IMapFrom<CommentServiceModel>
    {
        [Required]
        public int Id { get; set; }

        public string UserUsername { get; set; }

        [Required]
        [MinLength(30)]
        public string Reason { get; set; }
    }
}
