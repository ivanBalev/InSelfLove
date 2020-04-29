namespace BDInSelfLove.Web.ViewModels.Forum.Comment
{
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Services.Models.Comment;

    public class AssessCommentReportViewModel : IMapFrom<ReportServiceModel>
    {
        public int Id { get; set; }

        public string Reason { get; set; }

        public string SubmitterProfilePhoto { get; set; }

        public string SubmitterUserName { get; set; }
    }
}
