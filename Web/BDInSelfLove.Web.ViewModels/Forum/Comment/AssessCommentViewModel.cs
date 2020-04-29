namespace BDInSelfLove.Web.ViewModels.Forum.Comment
{
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Services.Models.Comment;
    using Ganss.XSS;

    public class AssessCommentViewModel : IMapFrom<CommentServiceModel>
    {
        public int ParentPostId { get; set; }

        public string Content { get; set; }

        public string SanitizedContent => new HtmlSanitizer().Sanitize(this.Content);

        public string UserUserName { get; set; }

        public string UserProfilePhoto { get; set; }

        public string UserId { get; set; }

        public AssessCommentReportViewModel Report { get; set; }
    }
}
